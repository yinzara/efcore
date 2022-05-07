// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    ///// <summary>
    ///// TODO
    ///// </summary>
    //public class JsonPathExpression : SqlExpression
    //{
    //    private readonly List<string> _jsonPath;

    //    /// <summary>
    //    /// TODO
    //    /// </summary>
    //    public virtual ColumnExpression JsonColumn { get; }

    //    /// <summary>
    //    /// TODO
    //    /// </summary>
    //    public virtual IReadOnlyDictionary<IProperty, SqlExpression> KeyPropertyExpressionMap { get; }

    //    /// <summary>
    //    /// TODO
    //    /// </summary>
    //    public JsonPathExpression(
    //        ColumnExpression jsonColumn,
    //        Type type,
    //        RelationalTypeMapping? typeMapping,
    //        IReadOnlyDictionary<IProperty, SqlExpression> keyPropertyExpressionMap)
    //        : this(jsonColumn, type, typeMapping, keyPropertyExpressionMap, new List<string>())
    //    {
    //        JsonColumn = jsonColumn;
    //        KeyPropertyExpressionMap = keyPropertyExpressionMap;
    //    }

    //    /// <summary>
    //    /// TODO
    //    /// </summary>
    //    public JsonPathExpression(
    //        ColumnExpression jsonColumn,
    //        Type type,
    //        RelationalTypeMapping? typeMapping,
    //        IReadOnlyDictionary<IProperty, SqlExpression> keyPropertyExpressionMap,
    //        List<string> jsonPath)
    //        : base(type, typeMapping)
    //    {
    //        JsonColumn = jsonColumn;
    //        KeyPropertyExpressionMap = keyPropertyExpressionMap;
    //        _jsonPath = jsonPath;
    //    }

    //    /// <summary>
    //    /// TODO
    //    /// </summary>
    //    protected override void Print(ExpressionPrinter expressionPrinter)
    //    {
    //        expressionPrinter.Append("SqlPathExpression(column: " + JsonColumn.Name + "  Path: " + string.Join(".", _jsonPath) + ")");
    //    }



    //    /// <inheritdoc />
    //    public override bool Equals(object? obj)
    //    {
    //        if (obj is JsonPathExpression jsonPathExpression)
    //        {
    //            var result = true;
    //            result = result && JsonColumn.Equals(jsonPathExpression.JsonColumn);
    //            result = result && _jsonPath.Count == jsonPathExpression._jsonPath.Count;

    //            if (result)
    //            {
    //                result = result && _jsonPath.Zip(jsonPathExpression._jsonPath, (l, r) => l == r).All(x => true);
    //            }

    //            return result;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    /// <inheritdoc />
    //    public override int GetHashCode()
    //        => HashCode.Combine(base.GetHashCode(), JsonColumn, _jsonPath);
    //}

    /// <summary>
    /// TODO
    /// </summary>
    public class JsonEntityExpression : SqlExpression
    {
        private readonly List<string> _jsonPath;

        private readonly Dictionary<INavigation, RelationalEntityShaperExpression> _ownedNavigationMap = new();

        /// <summary>
        /// TODO
        /// </summary>
        public virtual IReadOnlyDictionary<IProperty, SqlExpression> KeyPropertyExpressionMap { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual ColumnExpression JsonColumn { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsCollection { get; init; }

        /// <summary>
        /// TODO
        /// </summary>
        public JsonEntityExpression(
            ColumnExpression jsonColumn,
            IEntityType entityType,
            Type type,
            RelationalTypeMapping? typeMapping,
            IReadOnlyDictionary<IProperty, SqlExpression> keyPropertyExpressionMap,
            bool isCollection)
            : this(jsonColumn, entityType, type, typeMapping, keyPropertyExpressionMap, new List<string>(), isCollection)
        {
        }

        /// <summary>
        /// TODO
        /// </summary>
        public JsonEntityExpression(
            ColumnExpression jsonColumn,
            IEntityType entityType,
            Type type,
            RelationalTypeMapping? typeMapping,
            IReadOnlyDictionary<IProperty, SqlExpression> keyPropertyExpressionMap,
            List<string> jsonPath,
            bool isCollection)
            : base(type, typeMapping)
        {
            JsonColumn = jsonColumn;
            EntityType = entityType;
            _jsonPath = jsonPath;
            KeyPropertyExpressionMap = keyPropertyExpressionMap;
            IsCollection = isCollection;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public RelationalEntityShaperExpression BindNavigation(INavigation navigation)
        {
            if (_ownedNavigationMap.TryGetValue(navigation, out var result))
            {
                return result;
            }

            var pathSegment = navigation.Name;
            var entityType = navigation.TargetEntityType;

            var newPath = _jsonPath.ToList();
            newPath.Add(pathSegment);

            var newKeyPropertyExpressionMap = new Dictionary<IProperty, SqlExpression>();

            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null || primaryKey.Properties.Count != KeyPropertyExpressionMap.Count)
            {
                throw new InvalidOperationException("shouldnt happen");
            }

            // TODO: do this properly this is sooooo hacky rn
            var oldValues = KeyPropertyExpressionMap.Values.ToList();
            for (var i = 0; i < primaryKey.Properties.Count; i++)
            {
                newKeyPropertyExpressionMap[primaryKey.Properties[i]] = oldValues[i];
            }

            var jsonEntityExpression = new JsonEntityExpression(JsonColumn, entityType, Type, TypeMapping, newKeyPropertyExpressionMap, newPath, IsCollection);

            return new RelationalEntityShaperExpression(entityType, jsonEntityExpression, nullable: true);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void AddNavigationBinding(INavigation navigation, RelationalEntityShaperExpression entityShaperExpression)
        {
            _ownedNavigationMap[navigation] = entityShaperExpression;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public SqlExpression BindProperty(IProperty property)
        {
            if (KeyPropertyExpressionMap.TryGetValue(property, out var keyMapping))
            {
                return keyMapping;
            }

            var newJsonPath = _jsonPath.ToList();
            newJsonPath.Add(property.Name);

            return new JsonMappedPropertyExpression(JsonColumn, property.ClrType, null, newJsonPath);
        }

        /// <summary>
        /// TODO
        /// </summary>
        public IReadOnlyList<string> GetPath()
            => _jsonPath.AsReadOnly();

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

        /// <summary>
        /// TODO
        /// </summary>
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("JsonEntityExpression(entity: " + EntityType.Name + "  Path: " + string.Join(".", _jsonPath) + ")");
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is JsonEntityExpression jsonEntityExpression)
            {
                var result = true;
                result = result && JsonColumn.Equals(jsonEntityExpression.JsonColumn);
                result = result && _jsonPath.Count == jsonEntityExpression._jsonPath.Count;

                if (result)
                {
                    result = result && _jsonPath.Zip(jsonEntityExpression._jsonPath, (l, r) => l == r).All(x => true);
                }

                return result;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => base.GetHashCode(); // TODO
    }
}
