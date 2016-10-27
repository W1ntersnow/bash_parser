using WebApplication2.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace WebApplication2.DAL
{
    public class PostContext : DbContext
    {

        public PostContext() : base("MyConnectionString")
        {
        }

        public DbSet<BashPost> Posts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public void Add(BashPost post) {
            this.Posts.Add(post);
        }
        public void Commit() {
            this.SaveChanges();
        }
    }
}