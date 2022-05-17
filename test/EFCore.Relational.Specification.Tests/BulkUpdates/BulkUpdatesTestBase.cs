// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates
{
    public abstract class BulkUpdatesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : class, IBulkUpdatesFixtureBase, new()
    {
        private readonly Func<DbContext> _contextCreator;
        private readonly Func<DbContext, ISetSource> _setSourceCreator;

        protected BulkUpdatesTestBase(TFixture fixture)
        {
            Fixture = fixture;
            _contextCreator = Fixture.GetContextCreator();
            _setSourceCreator = Fixture.GetSetSourceCreator();
        }

        protected TFixture Fixture { get; }

        protected virtual Expression RewriteServerQueryExpression(Expression serverQueryExpression)
            => serverQueryExpression;

        public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

        public async Task AssertDelete<TResult>(
            bool async,
            Func<ISetSource, IQueryable<TResult>> query,
            int rowsAffectedCount)
        {
            using var context = _contextCreator();
            var processedQuery = RewriteServerQuery(query(_setSourceCreator(context)));
            var result = async
                ? await processedQuery.BulkDeleteAsync()
                : processedQuery.BulkDelete();

            Assert.Equal(rowsAffectedCount, result);
        }

        private IQueryable<T> RewriteServerQuery<T>(IQueryable<T> query)
            => query.Provider.CreateQuery<T>(RewriteServerQueryExpression(query.Expression));
    }

    public interface IBulkUpdatesFixtureBase
    {
        Func<DbContext> GetContextCreator();
        Func<DbContext, ISetSource> GetSetSourceCreator()
            => context => new DefaultSetSource(context);

        private class DefaultSetSource : ISetSource
        {
            private readonly DbContext _context;

            public DefaultSetSource(DbContext context)
            {
                _context = context;
            }

            public IQueryable<TEntity> Set<TEntity>()
                where TEntity : class
                => _context.Set<TEntity>();
        }

    }
}
