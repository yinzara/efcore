// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class JsonColumn : Column
    {
        private readonly Dictionary<IForeignKey, Dictionary<string, Column>> _containedColumns = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public JsonColumn(string name, string type, Table table)
            : base(name, type, table)
        {
        }

        ///// <summary>
        /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
        /////     any release. You should only use it directly in your code with extreme caution and knowing that
        /////     doing so can result in application failures when updating to a new Entity Framework Core release.
        ///// </summary>
        //public Dictionary<IForeignKey, Dictionary<string, Column>> ContainedColumns = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Column? FindColumn(IForeignKey ownership, string columnName)
        {
            if (_containedColumns.TryGetValue(ownership, out var inner))
            {
                if (inner.TryGetValue(columnName, out var result))
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public void AddColumn(IForeignKey ownership, string columnName, Column column)
        {
            if (!_containedColumns.TryGetValue(ownership, out var inner))
            {
                inner = new Dictionary<string, Column>();
                _containedColumns.Add(ownership, inner);
            }

            if (!inner.TryGetValue(columnName, out var existingColumn))
            {
                inner.Add(columnName, column);
            }
            else
            {
                // TODO: resource string
                throw new InvalidOperationException("column already exists");
            }
        }
    }
}
