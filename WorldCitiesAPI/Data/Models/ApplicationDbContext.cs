using Microsoft.EntityFrameworkCore;

namespace WorldCitiesAPI.Data.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base()
        {

        }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<City> Citys { get; set; }
        public DbSet<Country> Countrys { get; set; }
    }
}
