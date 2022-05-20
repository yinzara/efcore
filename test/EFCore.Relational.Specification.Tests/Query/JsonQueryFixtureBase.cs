// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonMappedEntitiesModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class JsonQueryFixtureBase : SharedStoreFixtureBase<JsonMappedEntitiesContext>, IQueryFixtureBase
    {
        public Func<DbContext> GetContextCreator()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<Type, object> GetEntityAsserters()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<Type, object> GetEntitySorters()
        {
            throw new NotImplementedException();
        }

        public ISetSource GetExpectedData()
        {
            throw new NotImplementedException();
        }
    }
}
