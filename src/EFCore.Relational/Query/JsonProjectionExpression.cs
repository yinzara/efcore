// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// TODO
    /// </summary>
    public class JsonProjectionExpression : Expression
    {
        /// <summary>
        /// TODO
        /// </summary>
        public JsonProjectionExpression(IEntityType entityType, JsonPathExpression jsonPathExpression, bool isCollection)
        {
            EntityType = entityType;
            JsonPathExpression = jsonPathExpression;
            IsCollection = isCollection;
        }

        /// <summary>
        ///     The entity type being projected out.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual JsonPathExpression JsonPathExpression { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsCollection { get; }

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type
            => IsCollection ? typeof(IEnumerable<>).MakeGenericType(EntityType.ClrType) : EntityType.ClrType;

        /// <summary>
        /// TODO
        /// </summary>
        public virtual JsonProjectionExpression BuildJsonProjectionExpressionForNavigation(INavigation navigation)
        {
            var pathSegment = navigation.Name;
            var entityType = navigation.TargetEntityType;

            var newPath = JsonPathExpression.JsonPath.ToList();
            newPath.Add(pathSegment);

            var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null || primaryKey.Properties.Count < JsonPathExpression.KeyPropertyMap.Count)
            {
                // TODO: debug or remove
                throw new InvalidOperationException("shouldnt happen");
            }

            // TODO: fix this (i.e. key property map should be sorted in the first place List<KVP> ??)
            var oldValues = JsonPathExpression.KeyPropertyMap.Values.ToList();

            for (var i = 0; i < oldValues.Count; i++)
            {
                newKeyPropertyMap[primaryKey.Properties[i]] = oldValues[i];
            }

            var newJsonPathExpression = new JsonPathExpression(
                JsonPathExpression.JsonColumn,
                JsonPathExpression.Type,
                JsonPathExpression.TypeMapping,
                newKeyPropertyMap,
                newPath);

            return new JsonProjectionExpression(entityType, newJsonPathExpression, navigation.IsCollection);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual SqlExpression BindProperty(IProperty property)
        {
            if (JsonPathExpression.KeyPropertyMap.TryGetValue(property, out var keyColumn))
            {
                return keyColumn;
            }

            var pathSegment = property.Name;
            var newPath = JsonPathExpression.JsonPath.ToList();
            newPath.Add(pathSegment);

            return new JsonPathExpression(
                JsonPathExpression.JsonColumn,
                property.ClrType,
                property.FindRelationalTypeMapping(), // TODO: use column information we should have somewhere
                new Dictionary<IProperty, ColumnExpression>(),
                newPath);
        }
    }
}
