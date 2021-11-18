// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TemporalComplexNavigationsCollectionsSharedTypeQuerySqlServerTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<
        TemporalComplexNavigationsSharedTypeQuerySqlServerFixture>
    {
        public TemporalComplexNavigationsCollectionsSharedTypeQuerySqlServerTest(
            TemporalComplexNavigationsSharedTypeQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        {
            var temporalEntityTypes = new List<Type>
            {
                typeof(Level1),
                typeof(Level2),
                typeof(Level3),
                typeof(Level4),
            };

            var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

            return rewriter.Visit(serverQueryExpression);
        }

        [Xunit.ConditionalTheory]
        [Xunit.MemberData(nameof(IsAsyncData))]
        public virtual async Task Fubarson(bool async)
        {
            await AssertQuery(async, ss => ss.Set<Level1>().Include(x => x.OneToMany_Optional1));

            AssertSql("");
        }

        [Xunit.ConditionalTheory]
        [Xunit.MemberData(nameof(IsAsyncData))]
        public virtual async Task Fubarson2(bool async)
        {
            await AssertQuery(
                async,
                ss => from e1 in ss.Set<Level1>().Include(x => x.OneToMany_Required1)
                      join e2 in ss.Set<Level1>().Include(x => x.OneToMany_Optional1) on e1.Id equals e2.Id
                      select new { e1, e2 });

            AssertSql("");
        }

        public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        {
            await base.Complex_multi_include_with_order_by_and_paging(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Id00]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
) AS [t0] ON [t].[Id] = [t0].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
    ) AS [t2] ON [l2].[Id] = [t2].[Id]
) AS [t1] ON CASE
    WHEN ((([t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL) AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL) AND [t0].[PeriodEnd] IS NOT NULL) AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [l7].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7] ON [l6].[Id] = [l7].[Id]
    ) AS [t4] ON [l5].[Id] = [t4].[Id]
) AS [t3] ON CASE
    WHEN ((([t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL) AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL) AND [t0].[PeriodEnd] IS NOT NULL) AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t3].[OneToMany_Required_Inverse3Id]
ORDER BY [t].[Name], [t].[Id], [t0].[Id], [t0].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Id0]");
        }

        public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool async)
        {
            await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Id00]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
) AS [t0] ON [t].[Id] = [t0].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
    ) AS [t2] ON [l2].[Id] = [t2].[Id]
) AS [t1] ON CASE
    WHEN ((([t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL) AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL) AND [t0].[PeriodEnd] IS NOT NULL) AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [l7].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7] ON [l6].[Id] = [l7].[Id]
    ) AS [t4] ON [l5].[Id] = [t4].[Id]
) AS [t3] ON CASE
    WHEN ((([t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL) AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL) AND [t0].[PeriodEnd] IS NOT NULL) AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t3].[OneToMany_Required_Inverse3Id]
ORDER BY [t].[Name], [t].[Id], [t0].[Id], [t0].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Id0]");
        }

        public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool async)
        {
            await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t0].[Id0], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Level3_Optional_Id], [t3].[Level3_Required_Id], [t3].[Level4_Name], [t3].[OneToMany_Optional_Inverse4Id], [t3].[OneToMany_Required_Inverse4Id], [t3].[OneToOne_Optional_PK_Inverse4Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Id00], [t3].[Id000]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
) AS [t0] ON [t].[Id] = [t0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
    ) AS [t2] ON [l2].[Id] = [t2].[Id]
) AS [t1] ON CASE
    WHEN ((([t0].[OneToOne_Required_PK_Date] IS NOT NULL AND [t0].[Level1_Required_Id] IS NOT NULL) AND [t0].[OneToMany_Required_Inverse2Id] IS NOT NULL) AND [t0].[PeriodEnd] IS NOT NULL) AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
END = [t1].[Level2_Required_Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00], [t4].[Id00] AS [Id000]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
        INNER JOIN (
            SELECT [l7].[Id], [l8].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
        ) AS [t5] ON [l6].[Id] = [t5].[Id]
    ) AS [t4] ON [l5].[Id] = [t4].[Id]
) AS [t3] ON CASE
    WHEN (([t1].[Level2_Required_Id] IS NOT NULL AND [t1].[OneToMany_Required_Inverse3Id] IS NOT NULL) AND [t1].[PeriodEnd] IS NOT NULL) AND [t1].[PeriodStart] IS NOT NULL THEN [t1].[Id]
END = [t3].[OneToMany_Optional_Inverse4Id]
ORDER BY [t].[Name], [t].[Id], [t0].[Id], [t0].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id00], [t3].[Id], [t3].[Id0], [t3].[Id00]");
        }

        public override async Task Complex_query_issue_21665(bool async)
        {
            await base.Complex_query_issue_21665(async);

            AssertSql("");
        }

        public override async Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
        {
            await base.Complex_query_with_let_collection_projection_FirstOrDefault(async);

            AssertSql(
                @"SELECT [l].[Id], [t0].[Id], [t0].[Id0], [t1].[Name], [t1].[Id], [t0].[c]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[c], [t].[Id], [t].[Id0], [t].[OneToOne_Required_PK_Date], [t].[Level1_Required_Id], [t].[OneToMany_Required_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l1].[Id] AS [Id0], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id], [l1].[Id]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE ([l0].[Level2_Name] <> N'Foo') OR [l0].[Level2_Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
OUTER APPLY (
    SELECT [l2].[Name], [l2].[Id]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
    WHERE EXISTS (
        SELECT 1
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE ([l2].[Id] = [l3].[OneToMany_Optional_Inverse2Id]) AND ([l3].[Id] = [t0].[Id]))
) AS [t1]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
        {
            await base.Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(async);

            AssertSql("");
        }

        public override async Task Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
        {
            await base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(async);

            AssertSql("");
        }

        public override async Task Filtered_include_after_different_filtered_include_different_level(bool async)
        {
            await base.Filtered_include_after_different_filtered_include_different_level(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t3].[Id], [t3].[OneToOne_Required_PK_Date], [t3].[Level1_Optional_Id], [t3].[Level1_Required_Id], [t3].[Level2_Name], [t3].[OneToMany_Optional_Inverse2Id], [t3].[OneToMany_Required_Inverse2Id], [t3].[OneToOne_Optional_PK_Inverse2Id], [t3].[PeriodEnd], [t3].[PeriodStart], [t3].[Id0], [t3].[Id1], [t3].[Level2_Optional_Id], [t3].[Level2_Required_Id], [t3].[Level3_Name], [t3].[OneToMany_Optional_Inverse3Id], [t3].[OneToMany_Required_Inverse3Id], [t3].[OneToOne_Optional_PK_Inverse3Id], [t3].[PeriodEnd0], [t3].[PeriodStart0], [t3].[Id00], [t3].[Id000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t1].[Id] AS [Id1], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t1].[Id0] AS [Id00], [t1].[Id00] AS [Id000]
    FROM (
        SELECT TOP(3) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE ([l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]) AND (([l0].[Level2_Name] <> N'Foo') OR [l0].[Level2_Name] IS NULL)
        ORDER BY [l0].[Level2_Name]
    ) AS [t]
    LEFT JOIN (
        SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
        FROM (
            SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Required_Inverse3Id] ORDER BY [l2].[Level3_Name] DESC) AS [row]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            INNER JOIN (
                SELECT [l3].[Id], [l4].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            ) AS [t2] ON [l2].[Id] = [t2].[Id]
            WHERE ([l2].[Level3_Name] <> N'Bar') OR [l2].[Level3_Name] IS NULL
        ) AS [t0]
        WHERE 1 < [t0].[row]
    ) AS [t1] ON [t].[Id] = [t1].[OneToMany_Required_Inverse3Id]
) AS [t3]
ORDER BY [l].[Id], [t3].[Level2_Name], [t3].[Id], [t3].[Id0], [t3].[OneToMany_Required_Inverse3Id], [t3].[Level3_Name] DESC, [t3].[Id1], [t3].[Id00]");
        }

        public override async Task Filtered_include_after_different_filtered_include_same_level(bool async)
        {
            await base.Filtered_include_after_different_filtered_include_same_level(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t1].[Id], [t1].[OneToOne_Required_PK_Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Level2_Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE ([l0].[Level2_Name] <> N'Foo') OR [l0].[Level2_Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[OneToOne_Required_PK_Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Level2_Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[PeriodEnd], [t2].[PeriodStart], [t2].[Id0]
    FROM (
        SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[PeriodEnd], [l2].[PeriodStart], [l3].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Required_Inverse2Id] ORDER BY [l2].[Level2_Name] DESC) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3] ON [l2].[Id] = [l3].[Id]
        WHERE ([l2].[Level2_Name] <> N'Bar') OR [l2].[Level2_Name] IS NULL
    ) AS [t2]
    WHERE 1 < [t2].[row]
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Level2_Name], [t0].[Id], [t0].[Id0], [t1].[OneToMany_Required_Inverse2Id], [t1].[Level2_Name] DESC, [t1].[Id]");
        }

        public override async Task Filtered_include_after_reference_navigation(bool async)
        {
            await base.Filtered_include_after_reference_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[PeriodEnd], [t1].[PeriodStart], [t1].[Id0], [t1].[Id00]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Level3_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        ) AS [t2] ON [l2].[Id] = [t2].[Id]
        WHERE ([l2].[Level3_Name] <> N'Foo') OR [l2].[Level3_Name] IS NULL
    ) AS [t0]
    WHERE (1 < [t0].[row]) AND ([t0].[row] <= 4)
) AS [t1] ON CASE
    WHEN ((([t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL) AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL) AND [t].[PeriodEnd] IS NOT NULL) AND [t].[PeriodStart] IS NOT NULL THEN [t].[Id]
END = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t1].[OneToMany_Optional_Inverse3Id], [t1].[Level3_Name], [t1].[Id], [t1].[Id0]");
        }

        public override async Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
        {
            await base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t5].[Id], [t5].[OneToOne_Required_PK_Date], [t5].[Level1_Optional_Id], [t5].[Level1_Required_Id], [t5].[Level2_Name], [t5].[OneToMany_Optional_Inverse2Id], [t5].[OneToMany_Required_Inverse2Id], [t5].[OneToOne_Optional_PK_Inverse2Id], [t5].[PeriodEnd], [t5].[PeriodStart], [t5].[Id0], [t5].[Level2_Optional_Id], [t5].[Level2_Required_Id], [t5].[Level3_Name], [t5].[OneToMany_Optional_Inverse3Id], [t5].[OneToMany_Required_Inverse3Id], [t5].[OneToOne_Optional_PK_Inverse3Id], [t5].[PeriodEnd0], [t5].[PeriodStart0], [t5].[Id00], [t5].[Id01], [t5].[Id000], [t5].[Id1], [t5].[Level3_Optional_Id], [t5].[Level3_Required_Id], [t5].[Level4_Name], [t5].[OneToMany_Optional_Inverse4Id], [t5].[OneToMany_Required_Inverse4Id], [t5].[OneToOne_Optional_PK_Inverse4Id], [t5].[PeriodEnd1], [t5].[PeriodStart1], [t5].[Id02], [t5].[Id001], [t5].[Id0000]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd] AS [PeriodEnd0], [t0].[PeriodStart] AS [PeriodStart0], [t].[Id0] AS [Id00], [t0].[Id0] AS [Id01], [t0].[Id00] AS [Id000], [t2].[Id] AS [Id1], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id], [t2].[PeriodEnd] AS [PeriodEnd1], [t2].[PeriodStart] AS [PeriodStart1], [t2].[Id0] AS [Id02], [t2].[Id00] AS [Id001], [t2].[Id000] AS [Id0000], [t].[c]
    FROM (
        SELECT TOP(1) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [l0].[Id] AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE ([l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]) AND (([l0].[Level2_Name] <> N'Foo') OR [l0].[Level2_Name] IS NULL)
        ORDER BY [l0].[Id]
    ) AS [t]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
        INNER JOIN (
            SELECT [l3].[Id], [l4].[Id] AS [Id0]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
            INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
        ) AS [t1] ON [l2].[Id] = [t1].[Id]
    ) AS [t0] ON [t].[Id] = [t0].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
        INNER JOIN (
            SELECT [l6].[Id], [t4].[Id] AS [Id0], [t4].[Id0] AS [Id00]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
            INNER JOIN (
                SELECT [l7].[Id], [l8].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
            ) AS [t4] ON [l6].[Id] = [t4].[Id]
        ) AS [t3] ON [l5].[Id] = [t3].[Id]
        WHERE [l5].[Id] > 1
    ) AS [t2] ON CASE
        WHEN (([t0].[Level2_Required_Id] IS NOT NULL AND [t0].[OneToMany_Required_Inverse3Id] IS NOT NULL) AND [t0].[PeriodEnd] IS NOT NULL) AND [t0].[PeriodStart] IS NOT NULL THEN [t0].[Id]
    END = [t2].[OneToMany_Optional_Inverse4Id]
) AS [t5]
ORDER BY [l].[Id], [t5].[c], [t5].[Id], [t5].[Id00], [t5].[Id0], [t5].[Id01], [t5].[Id000], [t5].[Id1], [t5].[Id02], [t5].[Id001]");
        }

        public override async Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
        {
            await base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row], [l0].[Id] AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE ([l0].[Level2_Name] <> N'Foo') OR [l0].[Level2_Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
        }

        public override async Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
        {
            await base.Filtered_include_and_non_filtered_include_on_same_navigation2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row], [l0].[Id] AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE ([l0].[Level2_Name] <> N'Foo') OR [l0].[Level2_Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
        }

        public override async Task Filtered_include_basic_OrderBy_Skip(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Skip(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Level2_Name], [t0].[Id]");
        }

        public override async Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Skip_Take(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    ) AS [t]
    WHERE (1 < [t].[row]) AND ([t].[row] <= 4)
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Level2_Name], [t0].[Id]");
        }

        public override async Task Filtered_include_basic_OrderBy_Take(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Take(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Level2_Name], [t0].[Id]");
        }

        public override async Task Filtered_include_basic_Where(bool async)
        {
            await base.Filtered_include_basic_Where(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[Id] > 5
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id]");
        }

        public override async Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
        {
            await base.Filtered_include_complex_three_level_with_middle_having_filter1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t8].[Id], [t8].[OneToOne_Required_PK_Date], [t8].[Level1_Optional_Id], [t8].[Level1_Required_Id], [t8].[Level2_Name], [t8].[OneToMany_Optional_Inverse2Id], [t8].[OneToMany_Required_Inverse2Id], [t8].[OneToOne_Optional_PK_Inverse2Id], [t8].[PeriodEnd], [t8].[PeriodStart], [t8].[Id0], [t8].[Id1], [t8].[Level2_Optional_Id], [t8].[Level2_Required_Id], [t8].[Level3_Name], [t8].[OneToMany_Optional_Inverse3Id], [t8].[OneToMany_Required_Inverse3Id], [t8].[OneToOne_Optional_PK_Inverse3Id], [t8].[PeriodEnd0], [t8].[PeriodStart0], [t8].[Id00], [t8].[Id000], [t8].[Id10], [t8].[Level3_Optional_Id], [t8].[Level3_Required_Id], [t8].[Level4_Name], [t8].[OneToMany_Optional_Inverse4Id], [t8].[OneToMany_Required_Inverse4Id], [t8].[OneToOne_Optional_PK_Inverse4Id], [t8].[PeriodEnd00], [t8].[PeriodStart00], [t8].[Id01], [t8].[Id0000], [t8].[Id00000], [t8].[Id2], [t8].[Level3_Optional_Id0], [t8].[Level3_Required_Id0], [t8].[Level4_Name0], [t8].[OneToMany_Optional_Inverse4Id0], [t8].[OneToMany_Required_Inverse4Id0], [t8].[OneToOne_Optional_PK_Inverse4Id0], [t8].[PeriodEnd1], [t8].[PeriodStart1], [t8].[Id02], [t8].[Id001], [t8].[Id0001]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [t7].[Id] AS [Id1], [t7].[Level2_Optional_Id], [t7].[Level2_Required_Id], [t7].[Level3_Name], [t7].[OneToMany_Optional_Inverse3Id], [t7].[OneToMany_Required_Inverse3Id], [t7].[OneToOne_Optional_PK_Inverse3Id], [t7].[PeriodEnd] AS [PeriodEnd0], [t7].[PeriodStart] AS [PeriodStart0], [t7].[Id0] AS [Id00], [t7].[Id00] AS [Id000], [t7].[Id1] AS [Id10], [t7].[Level3_Optional_Id], [t7].[Level3_Required_Id], [t7].[Level4_Name], [t7].[OneToMany_Optional_Inverse4Id], [t7].[OneToMany_Required_Inverse4Id], [t7].[OneToOne_Optional_PK_Inverse4Id], [t7].[PeriodEnd0] AS [PeriodEnd00], [t7].[PeriodStart0] AS [PeriodStart00], [t7].[Id01], [t7].[Id000] AS [Id0000], [t7].[Id0000] AS [Id00000], [t7].[Id2], [t7].[Level3_Optional_Id0], [t7].[Level3_Required_Id0], [t7].[Level4_Name0], [t7].[OneToMany_Optional_Inverse4Id0], [t7].[OneToMany_Required_Inverse4Id0], [t7].[OneToOne_Optional_PK_Inverse4Id0], [t7].[PeriodEnd1], [t7].[PeriodStart1], [t7].[Id02], [t7].[Id001], [t7].[Id0001], [t7].[c]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    OUTER APPLY (
        SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00], [t1].[Id] AS [Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000], [t1].[Id000] AS [Id0000], [t4].[Id] AS [Id2], [t4].[Level3_Optional_Id] AS [Level3_Optional_Id0], [t4].[Level3_Required_Id] AS [Level3_Required_Id0], [t4].[Level4_Name] AS [Level4_Name0], [t4].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [t4].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [t4].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [t4].[PeriodEnd] AS [PeriodEnd1], [t4].[PeriodStart] AS [PeriodStart1], [t4].[Id0] AS [Id02], [t4].[Id00] AS [Id001], [t4].[Id000] AS [Id0001], [t0].[c]
        FROM (
            SELECT TOP(1) [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00], [l2].[Id] AS [c]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            INNER JOIN (
                SELECT [l3].[Id], [l4].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            ) AS [t] ON [l2].[Id] = [t].[Id]
            WHERE ([l0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]) AND (([l2].[Level3_Name] <> N'Foo') OR [l2].[Level3_Name] IS NULL)
            ORDER BY [l2].[Id]
        ) AS [t0]
        LEFT JOIN (
            SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
            INNER JOIN (
                SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
                INNER JOIN (
                    SELECT [l7].[Id], [l8].[Id] AS [Id0]
                    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                ) AS [t3] ON [l6].[Id] = [t3].[Id]
            ) AS [t2] ON [l5].[Id] = [t2].[Id]
        ) AS [t1] ON [t0].[Id] = [t1].[OneToMany_Optional_Inverse4Id]
        LEFT JOIN (
            SELECT [l9].[Id], [l9].[Level3_Optional_Id], [l9].[Level3_Required_Id], [l9].[Level4_Name], [l9].[OneToMany_Optional_Inverse4Id], [l9].[OneToMany_Required_Inverse4Id], [l9].[OneToOne_Optional_PK_Inverse4Id], [l9].[PeriodEnd], [l9].[PeriodStart], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00], [t5].[Id00] AS [Id000]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9]
            INNER JOIN (
                SELECT [l10].[Id], [t6].[Id] AS [Id0], [t6].[Id0] AS [Id00]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
                INNER JOIN (
                    SELECT [l11].[Id], [l12].[Id] AS [Id0]
                    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11]
                    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12] ON [l11].[Id] = [l12].[Id]
                ) AS [t6] ON [l10].[Id] = [t6].[Id]
            ) AS [t5] ON [l9].[Id] = [t5].[Id]
        ) AS [t4] ON [t0].[Id] = [t4].[OneToMany_Required_Inverse4Id]
    ) AS [t7]
) AS [t8] ON [l].[Id] = [t8].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t8].[Id], [t8].[Id0], [t8].[c], [t8].[Id1], [t8].[Id00], [t8].[Id000], [t8].[Id10], [t8].[Id01], [t8].[Id0000], [t8].[Id00000], [t8].[Id2], [t8].[Id02], [t8].[Id001]");
        }

        public override async Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
        {
            await base.Filtered_include_complex_three_level_with_middle_having_filter2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t8].[Id], [t8].[OneToOne_Required_PK_Date], [t8].[Level1_Optional_Id], [t8].[Level1_Required_Id], [t8].[Level2_Name], [t8].[OneToMany_Optional_Inverse2Id], [t8].[OneToMany_Required_Inverse2Id], [t8].[OneToOne_Optional_PK_Inverse2Id], [t8].[PeriodEnd], [t8].[PeriodStart], [t8].[Id0], [t8].[Id1], [t8].[Level2_Optional_Id], [t8].[Level2_Required_Id], [t8].[Level3_Name], [t8].[OneToMany_Optional_Inverse3Id], [t8].[OneToMany_Required_Inverse3Id], [t8].[OneToOne_Optional_PK_Inverse3Id], [t8].[PeriodEnd0], [t8].[PeriodStart0], [t8].[Id00], [t8].[Id000], [t8].[Id10], [t8].[Level3_Optional_Id], [t8].[Level3_Required_Id], [t8].[Level4_Name], [t8].[OneToMany_Optional_Inverse4Id], [t8].[OneToMany_Required_Inverse4Id], [t8].[OneToOne_Optional_PK_Inverse4Id], [t8].[PeriodEnd00], [t8].[PeriodStart00], [t8].[Id01], [t8].[Id0000], [t8].[Id00000], [t8].[Id2], [t8].[Level3_Optional_Id0], [t8].[Level3_Required_Id0], [t8].[Level4_Name0], [t8].[OneToMany_Optional_Inverse4Id0], [t8].[OneToMany_Required_Inverse4Id0], [t8].[OneToOne_Optional_PK_Inverse4Id0], [t8].[PeriodEnd1], [t8].[PeriodStart1], [t8].[Id02], [t8].[Id001], [t8].[Id0001]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], [t7].[Id] AS [Id1], [t7].[Level2_Optional_Id], [t7].[Level2_Required_Id], [t7].[Level3_Name], [t7].[OneToMany_Optional_Inverse3Id], [t7].[OneToMany_Required_Inverse3Id], [t7].[OneToOne_Optional_PK_Inverse3Id], [t7].[PeriodEnd] AS [PeriodEnd0], [t7].[PeriodStart] AS [PeriodStart0], [t7].[Id0] AS [Id00], [t7].[Id00] AS [Id000], [t7].[Id1] AS [Id10], [t7].[Level3_Optional_Id], [t7].[Level3_Required_Id], [t7].[Level4_Name], [t7].[OneToMany_Optional_Inverse4Id], [t7].[OneToMany_Required_Inverse4Id], [t7].[OneToOne_Optional_PK_Inverse4Id], [t7].[PeriodEnd0] AS [PeriodEnd00], [t7].[PeriodStart0] AS [PeriodStart00], [t7].[Id01], [t7].[Id000] AS [Id0000], [t7].[Id0000] AS [Id00000], [t7].[Id2], [t7].[Level3_Optional_Id0], [t7].[Level3_Required_Id0], [t7].[Level4_Name0], [t7].[OneToMany_Optional_Inverse4Id0], [t7].[OneToMany_Required_Inverse4Id0], [t7].[OneToOne_Optional_PK_Inverse4Id0], [t7].[PeriodEnd1], [t7].[PeriodStart1], [t7].[Id02], [t7].[Id001], [t7].[Id0001], [t7].[c]
    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
    OUTER APPLY (
        SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0], [t0].[Id00], [t1].[Id] AS [Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Level4_Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[PeriodEnd] AS [PeriodEnd0], [t1].[PeriodStart] AS [PeriodStart0], [t1].[Id0] AS [Id01], [t1].[Id00] AS [Id000], [t1].[Id000] AS [Id0000], [t4].[Id] AS [Id2], [t4].[Level3_Optional_Id] AS [Level3_Optional_Id0], [t4].[Level3_Required_Id] AS [Level3_Required_Id0], [t4].[Level4_Name] AS [Level4_Name0], [t4].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [t4].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [t4].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [t4].[PeriodEnd] AS [PeriodEnd1], [t4].[PeriodStart] AS [PeriodStart1], [t4].[Id0] AS [Id02], [t4].[Id00] AS [Id001], [t4].[Id000] AS [Id0001], [t0].[c]
        FROM (
            SELECT TOP(1) [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[PeriodEnd], [l2].[PeriodStart], [t].[Id] AS [Id0], [t].[Id0] AS [Id00], [l2].[Id] AS [c]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            INNER JOIN (
                SELECT [l3].[Id], [l4].[Id] AS [Id0]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l3]
                INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l4] ON [l3].[Id] = [l4].[Id]
            ) AS [t] ON [l2].[Id] = [t].[Id]
            WHERE ([l0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]) AND (([l2].[Level3_Name] <> N'Foo') OR [l2].[Level3_Name] IS NULL)
            ORDER BY [l2].[Id]
        ) AS [t0]
        LEFT JOIN (
            SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l5].[PeriodEnd], [l5].[PeriodStart], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00], [t2].[Id00] AS [Id000]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l5]
            INNER JOIN (
                SELECT [l6].[Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l6]
                INNER JOIN (
                    SELECT [l7].[Id], [l8].[Id] AS [Id0]
                    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l7]
                    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l8] ON [l7].[Id] = [l8].[Id]
                ) AS [t3] ON [l6].[Id] = [t3].[Id]
            ) AS [t2] ON [l5].[Id] = [t2].[Id]
        ) AS [t1] ON [t0].[Id] = [t1].[OneToMany_Optional_Inverse4Id]
        LEFT JOIN (
            SELECT [l9].[Id], [l9].[Level3_Optional_Id], [l9].[Level3_Required_Id], [l9].[Level4_Name], [l9].[OneToMany_Optional_Inverse4Id], [l9].[OneToMany_Required_Inverse4Id], [l9].[OneToOne_Optional_PK_Inverse4Id], [l9].[PeriodEnd], [l9].[PeriodStart], [t5].[Id] AS [Id0], [t5].[Id0] AS [Id00], [t5].[Id00] AS [Id000]
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l9]
            INNER JOIN (
                SELECT [l10].[Id], [t6].[Id] AS [Id0], [t6].[Id0] AS [Id00]
                FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l10]
                INNER JOIN (
                    SELECT [l11].[Id], [l12].[Id] AS [Id0]
                    FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l11]
                    INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l12] ON [l11].[Id] = [l12].[Id]
                ) AS [t6] ON [l10].[Id] = [t6].[Id]
            ) AS [t5] ON [l9].[Id] = [t5].[Id]
        ) AS [t4] ON [t0].[Id] = [t4].[OneToMany_Required_Inverse4Id]
    ) AS [t7]
) AS [t8] ON [l].[Id] = [t8].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t8].[Id], [t8].[Id0], [t8].[c], [t8].[Id1], [t8].[Id00], [t8].[Id000], [t8].[Id10], [t8].[Id01], [t8].[Id0000], [t8].[Id00000], [t8].[Id2], [t8].[Id02], [t8].[Id001]");
        }

        public override async Task Filtered_include_context_accessed_inside_filter(bool async)
        {
            await base.Filtered_include_context_accessed_inside_filter(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]",
                //
                @"@__p_0='True'

SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row], [l0].[Id] AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE @__p_0 = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
        }

        public override async Task Filtered_include_context_accessed_inside_filter_correlated(bool async)
        {
            await base.Filtered_include_context_accessed_inside_filter_correlated(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[PeriodEnd], [l].[PeriodStart], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[PeriodEnd], [t0].[PeriodStart], [t0].[Id0]
FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[PeriodEnd], [t].[PeriodStart], [t].[Id0], [t].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[PeriodEnd], [l0].[PeriodStart], [l1].[Id] AS [Id0], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row], [l0].[Id] AS [c]
        FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l0]
        INNER JOIN [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE (
            SELECT COUNT(*)
            FROM [Level1] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [l2]
            WHERE [l2].[Id] <> [l0].[Id]) > 1
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[c], [t0].[Id]");
        }

        public override async Task Filtered_include_is_considered_loaded(bool async)
        {
            await base.Filtered_include_is_considered_loaded(async);

            AssertSql("");
        }

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        //public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        //{
        //    await base.Complex_multi_include_with_order_by_and_paging(async);

        //    AssertSql("");
        //}

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
