// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    /// TODO
    /// </summary>
    public class JsonQueryExpression : Expression
    {
        /// <summary>
        /// TODO
        /// </summary>
        public JsonQueryExpression(ColumnExpression jsonColumn)
        {
            JsonColumn = jsonColumn;
            JsonPath = new List<string>();
            KeyPropertyMap = new();
        }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual List<(IProperty, ColumnExpression)> KeyPropertyMap { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual ColumnExpression JsonColumn { get; }

        /// <summary>
        /// TODO
        /// </summary>
        public virtual IReadOnlyList<string> JsonPath { get; }
    }
}
