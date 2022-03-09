// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalShapedQueryCompilingExpressionVisitor
{
    private sealed class JsonCollectionResultInternalExpression : Expression
    {
        private readonly Type _type;

        public JsonCollectionResultInternalExpression(
            JsonValueBufferExpression valueBufferExpression,
            INavigationBase? navigation,
            Type elementType,
            Type type)
        {
            ValueBufferExpression = valueBufferExpression;
            Navigation = navigation;
            ElementType = elementType;
            _type = type;
        }

        public JsonValueBufferExpression ValueBufferExpression { get; }

        public INavigationBase? Navigation { get; }

        public Type ElementType { get; }

        public override Type Type => _type;
    }

    private sealed class JsonValueBufferExpression : Expression
    {
        public JsonValueBufferExpression(
            ParameterExpression keyValuesParameter,
            ParameterExpression jsonElementParameter,
            Expression entityExpression,
            INavigationBase? navigation)
        {
            KeyValuesParameter = keyValuesParameter;
            JsonElementParameter = jsonElementParameter;
            EntityExpression = entityExpression;
            Navigation = navigation;
        }

        public ParameterExpression KeyValuesParameter { get; }
        public ParameterExpression JsonElementParameter { get; }
        public Expression EntityExpression { get; }
        public INavigationBase? Navigation { get; }

        public override Type Type => typeof(ValueBuffer);
    }

    private sealed class JsonMappedEntityCompilingExpressionVisitor : ExpressionVisitor
    {
        private readonly MethodInfo _materializeIncludedJsonEntityMethodInfo = typeof(JsonMappedEntityCompilingExpressionVisitor).GetMethod(nameof(MaterializeIncludedJsonEntity))!;
        private readonly MethodInfo _materializeIncludedJsonEntityCollectionMethodInfo = typeof(JsonMappedEntityCompilingExpressionVisitor).GetMethod(nameof(MaterializeIncludedJsonEntityCollection))!;
        private readonly MethodInfo _materializeRootJsonEntityMethodInfo = typeof(JsonMappedEntityCompilingExpressionVisitor).GetMethod(nameof(MaterializeRootJsonEntity))!;
        private readonly MethodInfo _materializeRootJsonEntityCollectionMethodInfo = typeof(JsonMappedEntityCompilingExpressionVisitor).GetMethod(nameof(MaterializeRootJsonEntityCollection))!;

        private readonly MethodInfo _extractJsonPropertyMethodInfo = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(ShaperProcessingExpressionVisitor.ExtractJsonProperty))!;
        private readonly MethodInfo _jsonElementGetPropertyMethod = typeof(JsonElement).GetMethod(nameof(JsonElement.GetProperty), new[] { typeof(string) })!;

        private readonly RelationalShapedQueryCompilingExpressionVisitor _relationalShapedQueryCompilingExpressionVisitor;
        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)> _valueBufferParameterMapping = new();
        private readonly Dictionary<ParameterExpression, (ParameterExpression, ParameterExpression)> _materializationContextParameterMapping = new();

        public JsonMappedEntityCompilingExpressionVisitor(RelationalShapedQueryCompilingExpressionVisitor relationalShapedQueryCompilingExpressionVisitor)
        {
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
            if (extensionExpression is JsonCollectionResultInternalExpression or RelationalEntityShaperExpression)
            {
                var valueBufferExpression = (extensionExpression as JsonCollectionResultInternalExpression)?.ValueBufferExpression
                    ?? (JsonValueBufferExpression)((RelationalEntityShaperExpression)extensionExpression).ValueBufferExpression;

                var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer));
                var keyValuesShaperLambdaParameter = Expression.Parameter(typeof(object[]));
                var jsonElementShaperLambdaParameter = Expression.Parameter(typeof(JsonElement));

                _valueBufferParameterMapping[valueBufferParameter] = (keyValuesShaperLambdaParameter, jsonElementShaperLambdaParameter);

                var targetEntityType = extensionExpression is JsonCollectionResultInternalExpression
                    ? ((JsonCollectionResultInternalExpression)extensionExpression).Navigation!.TargetEntityType
                    : ((RelationalEntityShaperExpression)extensionExpression).EntityType;

                var nullable = (extensionExpression as RelationalEntityShaperExpression)?.IsNullable ?? false;
                var shaperExpression = new RelationalEntityShaperExpression(
                    targetEntityType,
                    valueBufferParameter,//TODO!!!
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
                    var innerJsonElement = Expression.Variable(
                        typeof(JsonElement));

                    shaperBlockVariables.Add(innerJsonElement);

                    // TODO: do TryGetProperty and short circuit if failed instead
                    var innerJsonElementAssignment = Expression.Assign(
                        innerJsonElement,
                        Expression.Call(jsonElementShaperLambdaParameter, _jsonElementGetPropertyMethod, Expression.Constant(ownedNavigation.Name)));

                    shaperBlockExpressions.Add(innerJsonElementAssignment);

                    if (ownedNavigation.IsCollection)
                    {
                        var nestedJsonCollectionShaper = new JsonCollectionResultInternalExpression(
                            new JsonValueBufferExpression(keyValuesShaperLambdaParameter, innerJsonElement, visited.Result, ownedNavigation),
                            ownedNavigation,
                            ownedNavigation.TargetEntityType.ClrType,
                            ownedNavigation.ClrType);

                        var nestedResult = Visit(nestedJsonCollectionShaper);
                        shaperBlockExpressions.Add(nestedResult);
                    }
                    else
                    {
                        var nestedJsonEntityShaper = new RelationalEntityShaperExpression(
                            ownedNavigation.TargetEntityType,
                            new JsonValueBufferExpression(keyValuesShaperLambdaParameter, innerJsonElement, visited.Result, ownedNavigation),
                            nullable: false);// TODO: fix nullability

                        var nestedResult = Visit(nestedJsonEntityShaper);
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

                var jsonValueBufferExpression = (extensionExpression as JsonCollectionResultInternalExpression)?.ValueBufferExpression
                    ?? (JsonValueBufferExpression)((RelationalEntityShaperExpression)extensionExpression).ValueBufferExpression;

                if (valueBufferExpression.Navigation != null)
                {
                    var fixup = ShaperProcessingExpressionVisitor.GenerateFixup(
                        valueBufferExpression.EntityExpression.Type,
                        valueBufferExpression.Navigation.TargetEntityType.ClrType,
                        valueBufferExpression.Navigation,
                        valueBufferExpression.Navigation.Inverse);

                    if (valueBufferExpression.Navigation.IsCollection)
                    {
                        var materializeIncludedJsonEntityCollectionMethodCall = Expression.Call(
                            null,
                            _materializeIncludedJsonEntityCollectionMethodInfo.MakeGenericMethod(
                                valueBufferExpression.EntityExpression.Type,
                                valueBufferExpression.Navigation.TargetEntityType.ClrType),
                            QueryCompilationContext.QueryContextParameter,
                            jsonValueBufferExpression.JsonElementParameter,
                            jsonValueBufferExpression.KeyValuesParameter,
                            valueBufferExpression.EntityExpression,
                            innerShaperLambda,
                            fixup);

                        return materializeIncludedJsonEntityCollectionMethodCall;
                    }

                    var entityType = valueBufferExpression.Navigation.DeclaringEntityType;
                    var table = entityType.GetViewOrTableMappings().SingleOrDefault()?.Table
                        ?? entityType.GetDefaultMappings().Single().Table;

                    var optionalDependent = table.IsOptional(entityType);
                    var materializeIncludedJsonEntityMethodCall = Expression.Call(
                        null,
                        _materializeIncludedJsonEntityMethodInfo.MakeGenericMethod(
                            valueBufferExpression.EntityExpression.Type,
                            valueBufferExpression.Navigation.TargetEntityType.ClrType),
                        QueryCompilationContext.QueryContextParameter,
                        jsonValueBufferExpression.JsonElementParameter,
                        jsonValueBufferExpression.KeyValuesParameter,
                        valueBufferExpression.EntityExpression,
                        Expression.Constant(optionalDependent),
                        innerShaperLambda,
                        fixup);

                    return materializeIncludedJsonEntityMethodCall;
                }
                else
                {
                    if (extensionExpression is JsonCollectionResultInternalExpression jsonCollectionResultInternalExpression)
                    {
                        var materializedRootJsonEntityCollection = Expression.Call(
                            null,
                            _materializeRootJsonEntityCollectionMethodInfo.MakeGenericMethod(
                                jsonCollectionResultInternalExpression.ElementType,
                                jsonCollectionResultInternalExpression.Navigation!.ClrType),
                            QueryCompilationContext.QueryContextParameter,
                            jsonValueBufferExpression.JsonElementParameter,
                            jsonValueBufferExpression.KeyValuesParameter,
                            Expression.Constant(jsonCollectionResultInternalExpression.Navigation),
                            innerShaperLambda);

                        return materializedRootJsonEntityCollection;
                    }

                    // TODO: just remap the shaper and return it instead
                    var materializedRootJsonEntity = Expression.Call(
                        null,
                        _materializeRootJsonEntityMethodInfo.MakeGenericMethod(valueBufferExpression.EntityExpression.Type),
                        QueryCompilationContext.QueryContextParameter,
                        jsonValueBufferExpression.JsonElementParameter,
                        jsonValueBufferExpression.KeyValuesParameter,
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
            bool optionalDependent,
            Func<QueryContext, object[], JsonElement, TIncludedEntity> innerShaper,
            Action<TIncludingEntity, TIncludedEntity> fixup)
            where TIncludingEntity : class
            where TIncludedEntity : class
        {
            if (jsonElement.ValueKind == JsonValueKind.Null)
            {
                if (optionalDependent)
                {
                    return;
                }
                else
                {
                    throw new InvalidOperationException("Required Json entity not found.");
                }
            }
            else
            {
                var included = innerShaper(queryContext, keyPropertyValues, jsonElement);
                fixup(entity, included);
            }
        }

        public static void MaterializeIncludedJsonEntityCollection<TIncludingEntity, TIncludedCollectionElement>(
            QueryContext queryContext,
            JsonElement jsonElement,
            object[] keyPropertyValues,
            TIncludingEntity entity,
            Func<QueryContext, object[], JsonElement, TIncludedCollectionElement> innerShaper,
            Action<TIncludingEntity, TIncludedCollectionElement> fixup)
            where TIncludingEntity : class
            where TIncludedCollectionElement : class
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
