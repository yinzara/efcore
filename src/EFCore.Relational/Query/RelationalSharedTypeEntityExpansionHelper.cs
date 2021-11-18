// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <inheritdoc/>
    public class RelationalSharedTypeEntityExpansionHelper : IRelationalSharedTypeEntityExpansionHelper
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalSharedTypeEntityExpansionHelper" /> class.
        /// </summary>
        /// <param name="dependencies">Dependencies for this service.</param>
        public RelationalSharedTypeEntityExpansionHelper(RelationalSharedTypeEntityExpansionHelperDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual RelationalSharedTypeEntityExpansionHelperDependencies Dependencies { get; }

        /// <inheritdoc/>
        public virtual TableExpressionBase CreateRelatedTableExpression(
            TableExpressionBase sourceTable,
            IEntityType targetEntityType)
        {
            var table = targetEntityType.GetTableMappings().Single().Table;

            return new TableExpression(table);

            //Dependencies.SqlExpressionFactory.Select(targetEntityType)

            //throw new System.NotImplementedException();
        }

        ///// <inheritdoc/>
        //public virtual SelectExpression CreateInnerSelectExpression(
        //    TableExpressionBase sourceTable,
        //    IEntityType targetEntityType)
        //    => Dependencies.SqlExpressionFactory.Select(targetEntityType);
    }
}
