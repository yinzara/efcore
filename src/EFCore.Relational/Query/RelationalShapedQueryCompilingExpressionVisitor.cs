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
            shapedQueryExpression.ShaperExpression,
            out var relationalCommandCache, out var readerColumns, out var relatedDataLoaders, ref collectionCount);

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
                Expression.Constant(readerColumns, typeof(IReadOnlyList<ReaderColumn?>)),
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
                Expression.Constant(readerColumns, typeof(IReadOnlyList<ReaderColumn?>)),
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
            Expression.Constant(readerColumns, typeof(IReadOnlyList<ReaderColumn?>)),
            Expression.Constant(shaper.Compile()),
            Expression.Constant(_contextType),
            Expression.Constant(
                QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
            Expression.Constant(_detailedErrorsEnabled),
            Expression.Constant(_threadSafetyChecksEnabled));
    }

    private class JsonMappedEntityCompilingExpressionVisitor : ExpressionVisitor
    {
        private readonly MethodInfo _materializeIncludedJsonEntityMethodInfo = typeof(JsonMappedEntityCompilingExpressionVisitor).GetMethod(nameof(MaterializeIncludedJsonEntity))!;
        private readonly MethodInfo _materializeRootJsonEntityMethodInfo = typeof(JsonMappedEntityCompilingExpressionVisitor).GetMethod(nameof(MaterializeRootJsonEntity))!;
        private readonly MethodInfo _materializeRootJsonEntityCollectionMethodInfo = typeof(JsonMappedEntityCompilingExpressionVisitor).GetMethod(nameof(MaterializeRootJsonEntityCollection))!;
        private readonly MethodInfo _extractJsonPropertyMethodInfo = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(ShaperProcessingExpressionVisitor.ExtractJsonProperty))!;
        private readonly MethodInfo _jsonElementGetPropertyMethod = typeof(JsonElement).GetMethod(nameof(JsonElement.GetProperty), new[] { typeof(string) })!;

        private readonly ParameterExpression _keyValuesParameter;
        private readonly Expression _jsonElement;
        private readonly Expression _includingEntity;
        private readonly INavigationBase? _navigation;

        private readonly RelationalShapedQueryCompilingExpressionVisitor _relationalShapedQueryCompilingExpressionVisitor;
        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)> _valueBufferParameterMapping = new();
        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)> _materializationContextParameterMapping = new();

        public JsonMappedEntityCompilingExpressionVisitor(
            ParameterExpression keyValuesParameter,
            Expression jsonElement,
            Expression includingEntity,
            INavigationBase? navigation,
            RelationalShapedQueryCompilingExpressionVisitor relationalShapedQueryCompilingExpressionVisitor)
        {
            _keyValuesParameter = keyValuesParameter;
            _jsonElement = jsonElement;
            _includingEntity = includingEntity;
            _navigation = navigation;

            _relationalShapedQueryCompilingExpressionVisitor = relationalShapedQueryCompilingExpressionVisitor;
        }

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
                        _extractJsonPropertyMethodInfo,
                        jsonElementParameter,
                        Expression.Constant(property.Name),
                        Expression.Constant(property.ClrType)),
                    property.ClrType);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is CollectionResultExpression or RelationalEntityShaperExpression)
            {
                var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer));
                var keyValuesShaperLambdaParameter = Expression.Parameter(typeof(object[]));
                var jsonElementShaperLambdaParameter = Expression.Parameter(typeof(JsonElement));

                _valueBufferParameterMapping[valueBufferParameter] = (keyValuesShaperLambdaParameter, jsonElementShaperLambdaParameter);

                var targetEntityType = extensionExpression is CollectionResultExpression
                    ? ((CollectionResultExpression)extensionExpression).Navigation!.TargetEntityType
                    : ((RelationalEntityShaperExpression)extensionExpression).EntityType;

                // TODO: fix nullability 
                var nullable = (extensionExpression as RelationalEntityShaperExpression)?.IsNullable ?? false;
                var shaperExpression = new RelationalEntityShaperExpression(
                    targetEntityType,
                    valueBufferParameter,
                    nullable);

                var shaperBlockVariables = new List<ParameterExpression>();
                var shaperBlockExpressions = new List<Expression>();

                var injectedMaterializer = _relationalShapedQueryCompilingExpressionVisitor.InjectEntityMaterializers(shaperExpression);

                var visited = (BlockExpression)Visit(injectedMaterializer);

                // the result of the visitation (i.e. the owner entity) will be added to the block at the very end, once we process all it's owned navigations
                var visitedExpressionsArray = visited.Expressions.ToArray();
                shaperBlockVariables.AddRange(visited.Variables);
                shaperBlockExpressions.AddRange(visitedExpressionsArray[..^1]);
                var shaperBlockResult = visitedExpressionsArray[^1];

                foreach (var ownedNavigation in targetEntityType.GetNavigations().Where(
                    n => n.TargetEntityType.MappedToJson() && n.ForeignKey.IsOwnership && n == n.ForeignKey.PrincipalToDependent))
                {
                    var jsonElement = Expression.Variable(
                        typeof(JsonElement));

                    shaperBlockVariables.Add(jsonElement);

                    // TODO: do TryGetProperty and short circuit if failed instead
                    var jsonElementAssignment = Expression.Assign(
                        jsonElement,
                        Expression.Call(jsonElementShaperLambdaParameter, _jsonElementGetPropertyMethod, Expression.Constant(ownedNavigation.Name)));

                    shaperBlockExpressions.Add(jsonElementAssignment);

                    var nestedVisitor = new JsonMappedEntityCompilingExpressionVisitor(
                        keyValuesShaperLambdaParameter,
                        jsonElement,
                        visited.Result,
                        ownedNavigation, _relationalShapedQueryCompilingExpressionVisitor);

                    if (ownedNavigation.IsCollection)
                    {
                        // we don't use projection binding at this level, so we can just use a dummy
                        // TODO: alternative: rewrite the collection result expression on the level above this into a new structure that doesn't have the unnecessary stuff
                        var dummyProjectionBinding = new ProjectionBindingExpression(Expression.Constant(ValueBuffer.Empty), 0, typeof(object));
                        var fubson = new CollectionResultExpression(dummyProjectionBinding, ownedNavigation, ownedNavigation.TargetEntityType.ClrType);

                        var nestedResult = nestedVisitor.Visit(fubson);
                        shaperBlockExpressions.Add(nestedResult);
                    }
                    else
                    {
                        var other = new RelationalEntityShaperExpression(
                            ownedNavigation.TargetEntityType,
                            Expression.Constant(ValueBuffer.Empty), // dummy, gets replaced when visited
                            nullable: false);// TODO: fix nullability

                        var nestedResult = nestedVisitor.Visit(other);
                        shaperBlockExpressions.Add(nestedResult);
                    }
                }

                shaperBlockExpressions.Add(shaperBlockResult);

                var shaperBlock = Expression.Block(
                    shaperBlockVariables,
                    shaperBlockExpressions);

                var innerShaperLambda = Expression.Lambda(
                    shaperBlock,
                    QueryCompilationContext.QueryContextParameter,
                    keyValuesShaperLambdaParameter,
                    jsonElementShaperLambdaParameter);

                if (_navigation != null)
                {
                    var fixup = ShaperProcessingExpressionVisitor.GenerateFixup(
                        _includingEntity.Type,
                        _navigation.TargetEntityType.ClrType,
                        _navigation,
                        _navigation.Inverse);

                    var materializeIncludedJsonEntityMethodCall = Expression.Call(
                        null,
                        _materializeIncludedJsonEntityMethodInfo.MakeGenericMethod(_includingEntity.Type, _navigation.TargetEntityType.ClrType),
                        QueryCompilationContext.QueryContextParameter,
                        _jsonElement,
                        _keyValuesParameter,
                        _includingEntity,
                        Expression.Constant(_navigation),
                        innerShaperLambda,
                        fixup);

                    return materializeIncludedJsonEntityMethodCall;
                }
                else
                {
                    if (extensionExpression is CollectionResultExpression collectionResultExpression)
                    {
                        var materializedRootJsonEntityCollection = Expression.Call(
                            null,
                            _materializeRootJsonEntityCollectionMethodInfo.MakeGenericMethod(collectionResultExpression.ElementType, collectionResultExpression.Navigation!.ClrType),
                            QueryCompilationContext.QueryContextParameter,
                            _jsonElement,
                            _keyValuesParameter,
                            Expression.Constant(collectionResultExpression.Navigation),
                            innerShaperLambda);

                        return materializedRootJsonEntityCollection;
                    }

                    // TODO: just remap the shaper and return it instead
                    var materializedRootJsonEntity = Expression.Call(
                        null,
                        _materializeRootJsonEntityMethodInfo.MakeGenericMethod(_includingEntity.Type),
                        QueryCompilationContext.QueryContextParameter,
                        _jsonElement,
                        _keyValuesParameter,
                        innerShaperLambda);

                    return materializedRootJsonEntity;
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        public static void MaterializeIncludedJsonEntity<TIncludingEntity, TIncludedEntity>(
            QueryContext queryContext,
            JsonElement jsonElement,
            object[] keyPropertyValues,
            TIncludingEntity entity,
            INavigationBase navigation,
            Func<QueryContext, object[], JsonElement, TIncludedEntity> innerShaper,
            Action<TIncludingEntity, TIncludedEntity> fixup)
            where TIncludingEntity : class
            where TIncludedEntity : class
        {
            if (navigation.IsCollection)
            {
                var newKeyPropertyValues = new object[keyPropertyValues.Length + 1];
                Array.Copy(keyPropertyValues, newKeyPropertyValues, keyPropertyValues.Length);

                var i = 0;
                foreach (var jsonArrayElement in jsonElement.EnumerateArray())
                {
                    newKeyPropertyValues[^1] = ++i;

                    var resultElement = innerShaper(queryContext, newKeyPropertyValues, jsonArrayElement);

                    fixup(entity, resultElement);
                }
            }
            else
            {
                var included = innerShaper(queryContext, keyPropertyValues, jsonElement);
                fixup(entity, included);
            }
        }

        public static TEntity MaterializeRootJsonEntity<TEntity>(
            QueryContext queryContext,
            JsonElement jsonElement,
            object[] keyPropertyValues,
            Func<QueryContext, object[], JsonElement, TEntity> shaper)
            where TEntity : class
        {
            var result = shaper(queryContext, keyPropertyValues, jsonElement);

            return result;
        }

        public static TResult MaterializeRootJsonEntityCollection<TEntity, TResult>(
            QueryContext queryContext,
            JsonElement jsonElement,
            object[] keyPropertyValues,
            INavigationBase navigation,
            Func<QueryContext, object[], JsonElement, TEntity> elementShaper)
            where TEntity : class
            where TResult : ICollection<TEntity>
        {
            var collectionAccessor = navigation.GetCollectionAccessor();
            var result = (TResult)collectionAccessor!.Create();

            var newKeyPropertyValues = new object[keyPropertyValues.Length + 1];
            Array.Copy(keyPropertyValues, newKeyPropertyValues, keyPropertyValues.Length);

            var i = 0;
            foreach (var jsonArrayElement in jsonElement.EnumerateArray())
            {
                newKeyPropertyValues[^1] = ++i;

                var resultElement = elementShaper(queryContext, newKeyPropertyValues, jsonArrayElement);

                result.Add(resultElement);
            }

            return result;
        }
    }
}
