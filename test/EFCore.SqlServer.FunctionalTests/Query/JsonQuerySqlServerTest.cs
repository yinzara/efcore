// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query
{
    public class JsonQuerySqlServerTest
    {
    }






    public class JsonQuerySqlServerFixture : SharedStoreFixtureBase<PoolableDbContext>
    {

    }

    public class JsonQueryContext : PoolableDbContext
    {
        public DbSet<JsonEntity> JsonEntities { get; set; }

        public JsonQueryContext(DbContextOptions options) : base(options) { }

        public static void Seed(JsonQueryContext context)
        {
            context.JsonEntities.AddRange(
                new JsonEntity { Id = 1, Customer = CreateCustomer1() },
                new JsonEntity { Id = 2, Customer = CreateCustomer2() });
            context.SaveChanges();

            static JsonCustomer CreateCustomer1() => new JsonCustomer
            {
                Name = "Joe",
                Age = 25,
                IsVip = false,
                Statistics = new JsonStatistics
                {
                    Visits = 4,
                    Purchases = 3,
                    Nested = new JsonNestedStatistics
                    {
                        SomeProperty = 10,
                        IntArray = new[] { 3, 4 }
                    }
                },
                Orders = new[]
                {
                        new JsonOrder
                        {
                            Price = 99.5m,
                            ShippingAddress = "Some address 1",
                            ShippingDate = new DateTime(2019, 10, 1)
                        },
                        new JsonOrder
                        {
                            Price = 23,
                            ShippingAddress = "Some address 2",
                            ShippingDate = new DateTime(2019, 10, 10)
                        }
                    }
            };

            static JsonCustomer CreateCustomer2() => new JsonCustomer
            {
                Name = "Moe",
                Age = 35,
                IsVip = true,
                Statistics = new JsonStatistics
                {
                    Visits = 20,
                    Purchases = 25,
                    Nested = new JsonNestedStatistics
                    {
                        SomeProperty = 20,
                        IntArray = new[] { 5, 6 }
                    }
                },
                Orders = new[]
                {
                        new JsonOrder
                        {
                            Price = 5,
                            ShippingAddress = "Moe's address",
                            ShippingDate = new DateTime(2019, 11, 3)
                        }
                    }
            };
        }
    }

    public class JsonEntity
    {
        public int Id { get; set; }

        //[Column(TypeName = "json")]
        public JsonCustomer Customer { get; set; }
    }

    public class JsonCustomer
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public bool IsVip { get; set; }
        public JsonStatistics Statistics { get; set; }
        public JsonOrder[] Orders { get; set; }
    }

    public class JsonStatistics
    {
        public int Visits { get; set; }
        public int Purchases { get; set; }
        public JsonNestedStatistics Nested { get; set; }
    }

    public class JsonNestedStatistics
    {
        public int SomeProperty { get; set; }
        public int[] IntArray { get; set; }
    }

    public class JsonOrder
    {
        public decimal Price { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime ShippingDate { get; set; }
    }
}
