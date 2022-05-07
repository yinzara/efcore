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
        // TODO - maybe add caching
        //private readonly Dictionary<INavigation, JsonProjectionExpression> _jsonNavigationMap = new();

        /// <summary>
        /// TODO
        /// </summary>
        public JsonProjectionExpression(IEntityType entityType, JsonPathExpression jsonPathExpression)
        {
            //_jsonNavigationMap = new Dictionary<INavigation, JsonProjectionExpression>();

            EntityType = entityType;
            JsonPathExpression = jsonPathExpression;
        }

        /// <summary>
        ///     The entity type being projected out.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual JsonPathExpression JsonPathExpression { get; }

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type
            => EntityType.ClrType;

        /// <summary>
        ///     Binds a property with this entity projection to get the SQL representation.
        /// </summary>
        /// <param name="property">A property to bind.</param>
        /// <returns>A column which is a SQL representation of the property.</returns>
        public virtual ColumnExpression BindKeyProperty(IProperty property)
        {
            if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
                && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
            }

            return JsonPathExpression.KeyPropertyMap[property];
        }

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
            if (primaryKey == null || primaryKey.Properties.Count != JsonPathExpression.KeyPropertyMap.Count)
            {
                // or should it? (collection)
                throw new InvalidOperationException("shouldnt happen");
            }

            var oldValues = JsonPathExpression.KeyPropertyMap.Values.ToList();
            for (var i = 0; i < primaryKey.Properties.Count; i++)
            {
                newKeyPropertyMap[primaryKey.Properties[i]] = oldValues[i];
            }

            var newJsonPathExpression = new JsonPathExpression(
                JsonPathExpression.JsonColumn,
                JsonPathExpression.Type,
                JsonPathExpression.TypeMapping,
                newKeyPropertyMap,
                newPath);

            return new JsonProjectionExpression(entityType, newJsonPathExpression);
        }

        ///// <summary>
        ///// TODO
        ///// </summary>
        //public virtual JsonProjectionExpression BindNavigation(INavigation navigation)
        //{
        //    var pathSegment = navigation.Name;
        //    var entityType = navigation.TargetEntityType;

        //    var newPath = JsonPathExpression.JsonPath.ToList();
        //    newPath.Add(pathSegment);

        //    var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
        //    var primaryKey = entityType.FindPrimaryKey();
        //    if (primaryKey == null || primaryKey.Properties.Count != JsonPathExpression.KeyPropertyMap.Count)
        //    {
        //        // or should it? (collection)
        //        throw new InvalidOperationException("shouldnt happen");
        //    }

        //    var oldValues = JsonPathExpression.KeyPropertyMap.Values.ToList();
        //    for (var i = 0; i < primaryKey.Properties.Count; i++)
        //    {
        //        newKeyPropertyMap[primaryKey.Properties[i]] = oldValues[i];
        //    }

        //    var newJsonPathExpression = new JsonPathExpression(
        //        JsonPathExpression.JsonColumn,
        //        JsonPathExpression.Type,
        //        JsonPathExpression.TypeMapping,
        //        newKeyPropertyMap,
        //        newPath);

        //    return new JsonProjectionExpression(entityType, newJsonPathExpression);
        //}
    }
}
