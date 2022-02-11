// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// TODO
/// </summary>
public class JsonTypeMapping : RelationalTypeMapping
{
    /// <summary>
    /// TODO
    /// </summary>
    public JsonTypeMapping(
        string storeType,
        DbType? dbType)
        : base(storeType, typeof(bool), dbType)
    {
    }

    /// <summary>
    /// TODO
    /// </summary>
    protected JsonTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    /// TODO
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new JsonTypeMapping(parameters);

    /// <summary>
    /// TODO
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
        // TODO: how to smuggle in the serialization options here, should this be part of the mapping options?
        => JsonSerializer.Serialize(value);
}
