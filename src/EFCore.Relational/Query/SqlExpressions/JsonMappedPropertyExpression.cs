// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    /// TODO
    /// </summary>
    public class JsonMappedPropertyExpression : SqlExpression
    {
        private readonly List<string> _jsonPath;

        /// <summary>
        /// TODO
        /// </summary>
        public ColumnExpression JsonColumn { get; init; }

        /// <summary>
        /// TODO
        /// </summary>
        public JsonMappedPropertyExpression(
            ColumnExpression jsonColumn,
            Type type,
            RelationalTypeMapping? typeMapping,
            List<string> jsonPath)
            : base(type, typeMapping)
        {
            JsonColumn = jsonColumn;
            _jsonPath = jsonPath;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public IReadOnlyList<string> GetPath()
            => _jsonPath.AsReadOnly();

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("JsonMappedPropertyExpression(entity: " + JsonColumn.Name + "  Path: " + string.Join(".", _jsonPath) + ")");
        }
    }
}
