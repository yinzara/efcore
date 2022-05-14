// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public partial class RelationalShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private readonly Type _contextType;
    private readonly ISet<string> _tags;
    private readonly bool _threadSafetyChecksEnabled;
    private readonly bool _detailedErrorsEnabled;
    private readonly bool _useRelationalNulls;

    /// <summary>
    /// TODO
    /// </summary>
    public class MyJsonShaperVisitor : ExpressionVisitor
    {
        private readonly MethodInfo _myMagicMethodInfo = typeof(MyJsonShaperVisitor).GetMethod(nameof(MyMagicMethod))!;

        private readonly ParameterExpression _keyValuesParameter;
        private readonly Expression _jsonElement;
        private readonly Expression _includingEntity;
        private readonly INavigationBase _navigation;

        private readonly RelationalShapedQueryCompilingExpressionVisitor _relationalShapedQueryCompilingExpressionVisitor;



        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)> _valueBufferParameterMapping = new();
        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)> _materializationContextParameterMapping = new();

        /// <summary>
        /// TODO
        /// </summary>
        public MyJsonShaperVisitor(
            ParameterExpression keyValuesParameter,
            Expression jsonElement,
            Expression includingEntity,
            INavigationBase navigation,
            RelationalShapedQueryCompilingExpressionVisitor relationalShapedQueryCompilingExpressionVisitor)
        {
            _keyValuesParameter = keyValuesParameter;
            _jsonElement = jsonElement;
            _includingEntity = includingEntity;
            _navigation = navigation;

            _relationalShapedQueryCompilingExpressionVisitor = relationalShapedQueryCompilingExpressionVisitor;
            //QueryCompilationContext.QueryContextParameter
        }

        /// <summary>
        /// TODO
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Assign
                && binaryExpression.Left is ParameterExpression parameterExpression
                && parameterExpression.Type == typeof(MaterializationContext))
            {
                var newExpression = (NewExpression)binaryExpression.Right;
                var valueBufferParameter = (ParameterExpression)newExpression.Arguments[0];
                _materializationContextParameterMapping[parameterExpression] = _valueBufferParameterMapping[valueBufferParameter];

                var updatedExpression = newExpression.Update(
                    new[] { Expression.Constant(ValueBuffer.Empty), newExpression.Arguments[1] });

                return Expression.Assign(binaryExpression.Left, updatedExpression);
            }

            return base.VisitBinary(binaryExpression);
        }

        private readonly MethodInfo _extractJsonPropertyMethodInfo2 = typeof(MyJsonShaperVisitor).GetMethod(nameof(ExtractJsonProperty2))!;

        /// <summary>
        /// TODO
        /// </summary>
        public static object? ExtractJsonProperty2(JsonElement element, string propertyName, Type returnType)
        {
            var jsonElementProperty = element.GetProperty(propertyName);

            if (returnType == typeof(int))
            {
                return jsonElementProperty.GetInt32();
            }
            if (returnType == typeof(DateTime))
            {
                return jsonElementProperty.GetDateTime();
            }
            if (returnType == typeof(bool))
            {
                return jsonElementProperty.GetBoolean();
            }
            if (returnType == typeof(decimal))
            {
                return jsonElementProperty.GetDecimal();
            }
            if (returnType == typeof(string))
            {
                return jsonElementProperty.GetString();
            }
            else
            {
                throw new InvalidOperationException("unsupported type");
            }
        }



        /// <summary>
        /// TODO
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition()
                == Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                var index = methodCallExpression.Arguments[1].GetConstantValue<int>();
                var property = methodCallExpression.Arguments[2].GetConstantValue<IProperty?>()!;
                var mappingParameter = (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object!;

                (var keyPropertyValuesParameter, var jsonElementParameter) = _materializationContextParameterMapping[mappingParameter];

                if (property.IsPrimaryKey())
                {
                    return Expression.MakeIndex(
                        keyPropertyValuesParameter,
                        keyPropertyValuesParameter.Type.GetProperty("Item"),
                        new[] { Expression.Constant(index) });
                }

                return
                    Expression.Convert(
                        Expression.Call(
                        null,
                        _extractJsonPropertyMethodInfo2,
                        jsonElementParameter,
                        Expression.Constant(property.Name),
                        Expression.Constant(property.ClrType)),
                    property.ClrType);





            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private readonly MethodInfo _objectListAddMethod = typeof(List<object>).GetMethod(nameof(List<object>.Add))!;


        /// <summary>
        /// TODO
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is CollectionResultExpression collectionResultExpression)
            {
                var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer));
                //var previousKeysParameter = Expression.Parameter(typeof(object[]));
                var jsonElementParameter = Expression.Parameter(typeof(JsonElement));

                //_valueBufferParameterMapping[valueBufferParameter] = (previousKeysParameter, jsonElementParameter);
                _valueBufferParameterMapping[valueBufferParameter] = (_keyValuesParameter, jsonElementParameter);

                var relationalEntityShaperExpression = new RelationalEntityShaperExpression(
                    collectionResultExpression.Navigation!.TargetEntityType,
                    valueBufferParameter,
                    nullable: false);


                //var keyValuesInitialization = Expression.ListInit(
                //    Expression.New(typeof(List<object>)),
                //    previousKeyValues.Select(x => Expression.ElementInit(_objectListAddMethod, x)));

                //var keyValuesAssignment = Expression.Assign(previousKeysParameter, keyValuesInitialization);

                var injectedMaterializer = _relationalShapedQueryCompilingExpressionVisitor.InjectEntityMaterializers(relationalEntityShaperExpression);
                var visited = Visit(injectedMaterializer);

                //var innerShaperLambda = Expression.Lambda(visited, QueryCompilationContext.QueryContextParameter, previousKeysParameter, jsonElementParameter);
                var innerShaperLambda = Expression.Lambda(visited, QueryCompilationContext.QueryContextParameter, _keyValuesParameter, jsonElementParameter);




                // TODO: build previous PK values




                var myMagicMethodCall = Expression.Call(
                    null,
                    _myMagicMethodInfo.MakeGenericMethod(_includingEntity.Type, _navigation.TargetEntityType.ClrType),
                    QueryCompilationContext.QueryContextParameter,
                    _jsonElement,

                    _keyValuesParameter,

                    //previousKeysParameter,
                    _includingEntity,
                    Expression.Constant(_navigation),
                    innerShaperLambda);

                return Expression.Block(
                    new List<ParameterExpression> { valueBufferParameter, /*previousKeysParameter*/ jsonElementParameter },
                    //keyValuesAssignment,
                    myMagicMethodCall);

                //return myMagicMethodCall;
            }

            return base.VisitExtension(extensionExpression);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public static void MyMagicMethod<TIncludingEntity, TIncludedEntity>(
            QueryContext queryContext,
            JsonElement jsonElement,
            object[] keyPropertyValues,
            TIncludingEntity entity,
            INavigationBase navigation,
            Func<QueryContext, object[], JsonElement, TIncludedEntity> innerShaper)
            where TIncludingEntity : class
            where TIncludedEntity : class
        {
            var collecionAccessor = navigation.GetCollectionAccessor();
            if (collecionAccessor != null)
            {
                collecionAccessor.GetOrCreate(entity, forMaterialization: true);

                var newKeyPropertyValues = new object[keyPropertyValues.Length + 1];
                Array.Copy(keyPropertyValues, newKeyPropertyValues, keyPropertyValues.Length);

                var i = 0;
                foreach (var jsonArrayElement in jsonElement.EnumerateArray())
                {
                    newKeyPropertyValues[^1] = ++i;

                    var resultElement = innerShaper(queryContext, newKeyPropertyValues, jsonArrayElement);

                    //fixup(entity, resultElement);
                    collecionAccessor.Add(entity, resultElement, forMaterialization: true);
                }
            }
        }
    }































    /// <summary>
    ///     Creates a new instance of the <see cref="ShapedQueryCompilingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    public RelationalShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext)
        : base(dependencies, queryCompilationContext)
    {
        RelationalDependencies = relationalDependencies;

        _contextType = queryCompilationContext.ContextType;
        _tags = queryCompilationContext.Tags;
        _threadSafetyChecksEnabled = dependencies.CoreSingletonOptions.AreThreadSafetyChecksEnabled;
        _detailedErrorsEnabled = dependencies.CoreSingletonOptions.AreDetailedErrorsEnabled;
        _useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalShapedQueryCompilingExpressionVisitorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        var selectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;

        VerifyNoClientConstant(shapedQueryExpression.ShaperExpression);
        var nonComposedFromSql = selectExpression.IsNonComposedFromSql();
        var querySplittingBehavior = ((RelationalQueryCompilationContext)QueryCompilationContext).QuerySplittingBehavior;
        var splitQuery = querySplittingBehavior == QuerySplittingBehavior.SplitQuery;
        var collectionCount = 0;

        var shaperProcessingExpressionVisitor = new ShaperProcessingExpressionVisitor(this, selectExpression, _tags, splitQuery, nonComposedFromSql);
        var shaper = shaperProcessingExpressionVisitor.ProcessShaper(
            shapedQueryExpression.ShaperExpression, out var relationalCommandCache, out var relatedDataLoaders, ref collectionCount);

        //var shaper = new ShaperProcessingExpressionVisitor(this, selectExpression, _tags, splitQuery, nonComposedFromSql).ProcessShaper(
        //    shapedQueryExpression.ShaperExpression, out var relationalCommandCache, out var relatedDataLoaders, ref collectionCount);
        if (querySplittingBehavior == null
            && collectionCount > 1)
        {
            QueryCompilationContext.Logger.MultipleCollectionIncludeWarning();
        }

        if (nonComposedFromSql)
        {
            return Expression.New(
                typeof(FromSqlQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors()[0],
                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(relationalCommandCache),
                Expression.Constant(
                    selectExpression.Projection.Select(pe => ((ColumnExpression)pe.Expression).Name).ToList(),
                    typeof(IReadOnlyList<string>)),
                Expression.Constant(shaper.Compile()),
                Expression.Constant(_contextType),
                Expression.Constant(
                    QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                Expression.Constant(_detailedErrorsEnabled),
                Expression.Constant(_threadSafetyChecksEnabled));
        }

        if (splitQuery)
        {
            var relatedDataLoadersParameter = Expression.Constant(
                QueryCompilationContext.IsAsync ? null : relatedDataLoaders?.Compile(),
                typeof(Action<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator>));

            var relatedDataLoadersAsyncParameter = Expression.Constant(
                QueryCompilationContext.IsAsync ? relatedDataLoaders?.Compile() : null,
                typeof(Func<QueryContext, IExecutionStrategy, SplitQueryResultCoordinator, Task>));

            return Expression.New(
                typeof(SplitQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors().Single(),
                Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
                Expression.Constant(relationalCommandCache),
                Expression.Constant(shaper.Compile()),
                relatedDataLoadersParameter,
                relatedDataLoadersAsyncParameter,
                Expression.Constant(_contextType),
                Expression.Constant(
                    QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                Expression.Constant(_detailedErrorsEnabled),
                Expression.Constant(_threadSafetyChecksEnabled));
        }

        return Expression.New(
            typeof(SingleQueryingEnumerable<>).MakeGenericType(shaper.ReturnType).GetConstructors()[0],
            Expression.Convert(QueryCompilationContext.QueryContextParameter, typeof(RelationalQueryContext)),
            Expression.Constant(relationalCommandCache),
            Expression.Constant(shaper.Compile()),
            Expression.Constant(_contextType),
            Expression.Constant(
                QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
            Expression.Constant(_detailedErrorsEnabled),
            Expression.Constant(_threadSafetyChecksEnabled));
    }
}
