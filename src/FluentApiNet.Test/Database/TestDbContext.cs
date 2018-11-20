using FluentApiNet.Test.Entities;
using System.Data.Common;
using System.Data.Entity;

namespace FluentApiNet.Test.Database
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbConnection connection) : base(connection, true)
        {
            this.Database.CreateIfNotExists();
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(x => x.Id);
        }

    }
}
