// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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

        public override async Task Query_with_owned_entity_equality_operator(bool async)
        {
            await base.Query_with_owned_entity_equality_operator(async);

            AssertSql(
                @"");
        }

















        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class TemporalOwnedQuerySqlServerFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override string StoreName { get; } = "TemporalOwnedQueryTest";

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
                        sb.HasData(new Star { Id = 1, Name = "Sol" });
                        sb.OwnsMany(
                            s => s.Composition, ob =>
                            {
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
                        b.ToTable(ttb => ttb.IsTemporal());
                        b.OwnsOne(
                            e => e.Throned, b =>
                            {
                                b.ToTable(ttb => ttb.IsTemporal());
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

                modelBuilder.Entity<Fink>().HasData(
                    new { Id = 1, BartonId = 1 });
            }
        }
    }
}
