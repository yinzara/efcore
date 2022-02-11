// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    /// TODO
    /// </summary>
    public class SqlServerJsonTranslator : IMemberTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="sqlExpressionFactory">TODO</param>
        public SqlServerJsonTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="instance">TODO</param>
        /// <param name="member">TODO</param>
        /// <param name="returnType">TODO</param>
        /// <param name="logger">TODO</param>
        /// <returns>TODO</returns>
        public SqlExpression? Translate(
            SqlExpression? instance,
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            return null;
        }
    }
}
