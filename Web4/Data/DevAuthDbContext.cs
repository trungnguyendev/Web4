using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Web4.Data
{
    public class DevAuthDbContext : IdentityDbContext
    {
        public DevAuthDbContext(DbContextOptions<DevAuthDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var readerRoleId = "c5472bd6-328d-437f-9afd-7abc5699fb4c";
            var writerRoleId = "22e14eb5-5190-4600-84f5-4ad7d3750a8f";
            var roles = new List<IdentityRole>()
                {
                     new IdentityRole
                     {
                         Id = readerRoleId,
                         ConcurrencyStamp = readerRoleId,
                         Name = "Reader",
                         NormalizedName = "Reader".ToUpper()
                     },
                      new IdentityRole
                     {
                         Id = writerRoleId,
                         ConcurrencyStamp = writerRoleId,
                         Name = "Writer",
                         NormalizedName = "Writer".ToUpper()
                     }
                };
            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
