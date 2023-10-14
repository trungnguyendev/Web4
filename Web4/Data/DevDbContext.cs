using Microsoft.EntityFrameworkCore;
using Web4.Models.Domain;

namespace Web4.Data
{
    public class DevDbContext : DbContext
    {
        public DevDbContext(DbContextOptions<DevDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }public DbSet<Subject> Subjects { get; set; }
    }
}
