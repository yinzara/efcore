// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
/// TODO
/// </summary>
public class SqlServerJsonTypeMapping : JsonTypeMapping
{
    /// <summary>
    /// TODO
    /// </summary>
    public SqlServerJsonTypeMapping(
        string storeType = "nvarchar(max)",
        DbType? dbType = System.Data.DbType.String)
        : base(storeType, dbType)
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

