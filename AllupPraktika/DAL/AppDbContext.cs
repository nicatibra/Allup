using AllupPraktika.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AllupPraktika.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        //Slide
        public DbSet<Slide> Slides { get; set; }


        //Category (one-to-many)
        public DbSet<Category> Categories { get; set; }


        //Brand (one-to-many)
        public DbSet<Brand> Brands { get; set; }


        //Tag (many-to-many)
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }


        public DbSet<LayoutSetting> LayoutSettings { get; set; }


        public DbSet<BasketItem> BasketItems { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Order> Orders { get; set; }
    }
}
