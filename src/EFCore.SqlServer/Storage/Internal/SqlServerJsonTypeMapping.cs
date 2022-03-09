// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    /// TODO
    /// </summary>
    public class SqlServerJsonTypeMapping : JsonTypeMapping
    {
        /// <summary>
        /// TODO
        /// </summary>
        public SqlServerJsonTypeMapping(string storeType)
            : base(storeType, typeof(JsonElement), System.Data.DbType.String)
        {
        }

        /// <summary>
        /// TODO
        /// </summary>
        protected SqlServerJsonTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        /// TODO
        /// </summary>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqlServerJsonTypeMapping(parameters); 
    }
}
