// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    /// TODO
    /// </summary>
    public class JsonScalarExpression : SqlExpression
    {
        /// <summary>
        /// TODO
        /// </summary>
        public JsonScalarExpression(
            ColumnExpression jsonColumn,
            Type type,
            RelationalTypeMapping? typeMapping)
            : this(jsonColumn, type, typeMapping, new List<string>())
        {
        }

        /// <summary>
        /// TODO
        /// </summary>
        public JsonScalarExpression(
            ColumnExpression jsonColumn,
            Type type,
            RelationalTypeMapping? typeMapping,
            List<string> jsonPath)
            : base(type, typeMapping)
        {
            JsonColumn = jsonColumn;
            JsonPath = jsonPath.AsReadOnly();
        }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual ColumnExpression JsonColumn { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual IReadOnlyList<string> JsonPath { get; }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("JsonScalarExpression(column: " + JsonColumn.Name + "  Path: " + string.Join(".", JsonPath) + ")");
        }
    }
}
