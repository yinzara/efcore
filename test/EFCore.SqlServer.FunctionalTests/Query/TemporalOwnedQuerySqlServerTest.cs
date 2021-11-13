// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TemporalOwnedQuerySqlServerTest : OwnedQueryRelationalTestBase<TemporalOwnedQuerySqlServerTest.TemporalOwnedQuerySqlServerFixture>
    {
        public TemporalOwnedQuerySqlServerTest(TemporalOwnedQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        {
            var temporalEntityTypes = new List<Type>
            {
                typeof(Barton),
                typeof(Star),
            };

            var rewriter = new TemporalPointInTimeQueryRewriter(Fixture.ChangesDate, temporalEntityTypes);

            return rewriter.Visit(serverQueryExpression);
        }

        public override async Task Query_with_owned_entity_equality_operator(bool async)
        {
            await base.Query_with_owned_entity_equality_operator(async);

            AssertSql(
                @"");
        }

        public override async Task Query_loads_reference_nav_automatically_in_projection(bool async)
        {
            await base.Query_loads_reference_nav_automatically_in_projection(async);

            AssertSql(
                @"SELECT TOP(2) [b].[Id], [b].[PeriodEnd], [b].[PeriodStart], [b].[Simple], [b].[Throned_Property], [b].[Throned_Value]
FROM [Fink] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [f]
LEFT JOIN [Barton] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [b] ON [f].[BartonId] = [b].[Id]");
        }

        public override async Task Simple_query_entity_with_owned_collection(bool async)
        {
            await base.Simple_query_entity_with_owned_collection(async);

            AssertSql(
                @"SELECT [s].[Id], [s].[Name], [s].[PeriodEnd], [s].[PeriodStart], [e].[Id], [e].[Name], [e].[PeriodEnd], [e].[PeriodStart], [e].[StarId]
FROM [Star] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [s]
LEFT JOIN [Element] FOR SYSTEM_TIME AS OF '2010-01-01T00:00:00.0000000' AS [e] ON [s].[Id] = [e].[StarId]
ORDER BY [s].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class TemporalOwnedQuerySqlServerFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override string StoreName { get; } = "TemporalOwnedQueryTest";

            public DateTime ChangesDate { get; private set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<OwnedPerson>(
                    eb =>
                    {
                        eb.IndexerProperty<string>("Name");
                        var ownedPerson = new OwnedPerson { Id = 1 };
                        ownedPerson["Name"] = "Mona Cy";
                        eb.HasData(ownedPerson);

                        eb.OwnsOne(
                            p => p.PersonAddress, ab =>
                            {
                                ab.IndexerProperty<string>("AddressLine");
                                ab.IndexerProperty(typeof(int), "ZipCode");
                                ab.HasData(
                                    new
                                    {
                                        OwnedPersonId = 1,
                                        PlaceType = "Land",
                                        AddressLine = "804 S. Lakeshore Road",
                                        ZipCode = 38654
                                    },
                                    new
                                    {
                                        OwnedPersonId = 2,
                                        PlaceType = "Land",
                                        AddressLine = "7 Church Dr.",
                                        ZipCode = 28655
                                    },
                                    new
                                    {
                                        OwnedPersonId = 3,
                                        PlaceType = "Land",
                                        AddressLine = "72 Hickory Rd.",
                                        ZipCode = 07728
                                    },
                                    new
                                    {
                                        OwnedPersonId = 4,
                                        PlaceType = "Land",
                                        AddressLine = "28 Strawberry St.",
                                        ZipCode = 19053
                                    });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 1,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 2,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 3,
                                                PlanetId = 1,
                                                Name = "USA"
                                            },
                                            new
                                            {
                                                OwnedAddressOwnedPersonId = 4,
                                                PlanetId = 1,
                                                Name = "USA"
                                            });

                                        cb.HasOne(cc => cc.Planet).WithMany().HasForeignKey(ee => ee.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);
                                    });
                            });

                        eb.OwnsMany(
                            p => p.Orders, ob =>
                            {
                                ob.IndexerProperty<DateTime>("OrderDate");
                                ob.HasData(
                                    new
                                    {
                                        Id = -10,
                                        ClientId = 1,
                                        OrderDate = Convert.ToDateTime("2018-07-11 10:01:41")
                                    },
                                    new
                                    {
                                        Id = -11,
                                        ClientId = 1,
                                        OrderDate = Convert.ToDateTime("2015-03-03 04:37:59")
                                    },
                                    new
                                    {
                                        Id = -20,
                                        ClientId = 2,
                                        OrderDate = Convert.ToDateTime("2015-05-25 20:35:48")
                                    },
                                    new
                                    {
                                        Id = -30,
                                        ClientId = 3,
                                        OrderDate = Convert.ToDateTime("2014-11-10 04:32:42")
                                    },
                                    new
                                    {
                                        Id = -40,
                                        ClientId = 4,
                                        OrderDate = Convert.ToDateTime("2016-04-25 19:23:56")
                                    }
                                );

                                ob.OwnsMany(e => e.Details, odb =>
                                {
                                    odb.HasData(
                                        new
                                        {
                                            Id = -100,
                                            OrderId = -10,
                                            OrderClientId = 1,
                                            Detail = "Discounted Order"
                                        },
                                        new
                                        {
                                            Id = -101,
                                            OrderId = -10,
                                            OrderClientId = 1,
                                            Detail = "Full Price Order"
                                        },
                                        new
                                        {
                                            Id = -200,
                                            OrderId = -20,
                                            OrderClientId = 2,
                                            Detail = "Internal Order"
                                        },
                                        new
                                        {
                                            Id = -300,
                                            OrderId = -30,
                                            OrderClientId = 3,
                                            Detail = "Bulk Order"
                                        });
                                });
                            });
                    });

                modelBuilder.Entity<Branch>(
                    eb =>
                    {
                        eb.HasData(new { Id = 2, Name = "Antigonus Mitul" });

                        eb.OwnsOne(
                            p => p.BranchAddress, ab =>
                            {
                                ab.IndexerProperty<string>("BranchName").IsRequired();
                                ab.HasData(
                                    new
                                    {
                                        BranchId = 2,
                                        PlaceType = "Land",
                                        BranchName = "BranchA"
                                    },
                                    new
                                    {
                                        BranchId = 3,
                                        PlaceType = "Land",
                                        BranchName = "BranchB"
                                    });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressBranchId = 2,
                                                PlanetId = 1,
                                                Name = "Canada"
                                            },
                                            new
                                            {
                                                OwnedAddressBranchId = 3,
                                                PlanetId = 1,
                                                Name = "Canada"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<LeafA>(
                    eb =>
                    {
                        var leafA = new LeafA { Id = 3 };
                        leafA["Name"] = "Madalena Morana";
                        eb.HasData(leafA);

                        eb.OwnsOne(
                            p => p.LeafAAddress, ab =>
                            {
                                ab.IndexerProperty<int>("LeafType");

                                ab.HasData(
                                    new
                                    {
                                        LeafAId = 3,
                                        PlaceType = "Land",
                                        LeafType = 1
                                    });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasOne(c => c.Planet).WithMany().HasForeignKey(c => c.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);

                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressLeafAId = 3,
                                                PlanetId = 1,
                                                Name = "Mexico"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<LeafB>(
                    eb =>
                    {
                        var leafB = new LeafB { Id = 4 };
                        leafB["Name"] = "Vanda Waldemar";
                        eb.HasData(leafB);

                        eb.OwnsOne(
                            p => p.LeafBAddress, ab =>
                            {
                                ab.IndexerProperty<string>("LeafBType").IsRequired();
                                ab.HasData(
                                    new
                                    {
                                        LeafBId = 4,
                                        PlaceType = "Land",
                                        LeafBType = "Green"
                                    });

                                ab.OwnsOne(
                                    a => a.Country, cb =>
                                    {
                                        cb.HasOne(c => c.Planet).WithMany().HasForeignKey(c => c.PlanetId)
                                            .OnDelete(DeleteBehavior.Restrict);

                                        cb.HasData(
                                            new
                                            {
                                                OwnedAddressLeafBId = 4,
                                                PlanetId = 1,
                                                Name = "Panama"
                                            });
                                    });
                            });
                    });

                modelBuilder.Entity<Planet>(pb => pb.HasData(new Planet { Id = 1, StarId = 1 }));

                modelBuilder.Entity<Moon>(
                    mb => mb.HasData(
                        new Moon
                        {
                            Id = 1,
                            PlanetId = 1,
                            Diameter = 3474
                        }));

                modelBuilder.Entity<Star>(
                    sb =>
                    {
                        sb.ToTable(tb => tb.IsTemporal(ttb =>
                        {
                            ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                            ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                        }));
                        sb.HasData(new Star { Id = 1, Name = "Sol" });
                        sb.OwnsMany(
                            s => s.Composition, ob =>
                            {
                                ob.ToTable(tb => tb.IsTemporal(ttb =>
                                {
                                    ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                    ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                }));
                                ob.HasKey(e => e.Id);
                                ob.HasData(
                                    new
                                    {
                                        Id = "H",
                                        Name = "Hydrogen",
                                        StarId = 1
                                    },
                                    new
                                    {
                                        Id = "He",
                                        Name = "Helium",
                                        StarId = 1
                                    });
                            });
                    });

                modelBuilder.Entity<Barton>(
                    b =>
                    {
                        b.ToTable(tb => tb.IsTemporal(ttb =>
                        {
                            ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                            ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                        }));
                        b.OwnsOne(
                            e => e.Throned, b =>
                            {
                                b.ToTable(tb => tb.IsTemporal(ttb =>
                                {
                                    ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                                    ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                                }));
                                b.HasData(
                                    new
                                    {
                                        BartonId = 1,
                                        Property = "Property",
                                        Value = 42
                                    });
                            });
                        b.HasData(
                            new Barton { Id = 1, Simple = "Simple" },
                            new Barton { Id = 2, Simple = "Not" });
                    });

                modelBuilder.Entity<Fink>()
                    .ToTable(tb => tb.IsTemporal())
                    .HasData(new { Id = 1, BartonId = 1 });
            }

            protected override void Seed(PoolableDbContext context)
            {
                base.Seed(context);

                ChangesDate = new DateTime(2010, 1, 1);

                var stars = context.Set<Star>().AsTracking().ToList();
                foreach (var star in stars)
                {
                    star.Name = "Modified" + star.Name;
                    if (star.Composition.Any())
                    {
                        foreach (var comp in star.Composition)
                        {
                            comp.Name = "Modified" + comp.Name;
                        }
                    }
                }

                var finks = context.Set<Fink>().AsTracking().ToList();
                context.Set<Fink>().RemoveRange(finks);

                var bartons = context.Set<Barton>().Include(x => x.Throned).AsTracking().ToList();
                foreach (var barton in bartons)
                {
                    barton.Simple = "Modified" + barton.Simple;
                    if (barton.Throned != null)
                    {
                        barton.Throned.Property = "Modified" + barton.Throned.Property;
                    }
                }    

                context.SaveChanges();

                var tableNames = new List<string>
                {
                    nameof(Barton),
                    nameof(Fink),
                    nameof(Star),
                    nameof(Element),
                };

                foreach (var tableName in tableNames)
                {
                    context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
                    context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] DROP PERIOD FOR SYSTEM_TIME");

                    context.Database.ExecuteSqlRaw($"UPDATE [{tableName + "History"}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
                    context.Database.ExecuteSqlRaw($"UPDATE [{tableName + "History"}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

                    context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
                    context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName + "History"}]))");
                }
            }
        }
    }
}
