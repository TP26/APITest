using Microsoft.EntityFrameworkCore;

namespace APITest.Models
{
    public class CoOrdinatesContext : DbContext
    {
        public CoOrdinatesContext(DbContextOptions<CoOrdinatesContext> options) : base(options) { }

        public DbSet<CoOrdinates> CoOrdinates { get; set; } = null!;
    }
}
