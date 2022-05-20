// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query
{
    public class JsonQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : JsonQueryFixtureBase, new()
    {

        protected JsonQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }
    }
}
