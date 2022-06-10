// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// TODO
    /// </summary>
    public class JsonQueryExpression : Expression, IPrintableExpression
    {
        /// <summary>
        /// TODO
        /// </summary>
        public JsonQueryExpression(IEntityType entityType, ColumnExpression jsonColumn, bool isCollection, List<(IProperty, ColumnExpression)> keyPropertyMap)
            : this(entityType, jsonColumn, isCollection, keyPropertyMap, new List<string>())
        {
        }

        /// <summary>
        /// TODO
        /// </summary>
        public JsonQueryExpression(IEntityType entityType, ColumnExpression jsonColumn, bool isCollection, List<(IProperty, ColumnExpression)> keyPropertyMap, List<string> jsonPath)
        {
            // or just store type instead?
            EntityType = entityType;
            JsonColumn = jsonColumn;
            IsCollection = isCollection;
            KeyPropertyMap = keyPropertyMap;
            JsonPath = jsonPath;
        }

        /// <summary>
        ///     The entity type being projected out.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual ColumnExpression JsonColumn { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsCollection { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual IReadOnlyList<(IProperty, ColumnExpression)> KeyPropertyMap { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual IReadOnlyList<string> JsonPath { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Extension;

        /// <inheritdoc />
        public override Type Type
            => IsCollection ? typeof(IEnumerable<>).MakeGenericType(EntityType.ClrType) : EntityType.ClrType;

        /// <inheritdoc />
        public void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append($"JsonQueryExpression({JsonColumn.Name}, \"{string.Join(".", JsonPath)}\")");
        }
    }
}
