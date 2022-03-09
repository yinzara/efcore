// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query
{
    public class JsonQuerySqliteTest : JsonQueryTestBase<JsonQuerySqliteTest.JsonQuerySqliteFixture>
    {
        public JsonQuerySqliteTest(JsonQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public class JsonQuerySqliteFixture : JsonQueryFixtureBase
        {
            protected override string StoreName => throw new NotImplementedException();

            protected override ITestStoreFactory TestStoreFactory => throw new NotImplementedException();
        }
    }
}
