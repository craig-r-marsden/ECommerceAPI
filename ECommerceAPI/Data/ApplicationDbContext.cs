using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.HasData(
                new Product { Id = 1, Name = "Wireless Bluetooth Headphones", Description = "Premium over-ear headphones with active noise cancellation, 30-hour battery life, and superior sound quality. Perfect for music lovers and professionals." },
                new Product { Id = 2, Name = "Smart Fitness Watch", Description = "Track your health and fitness goals with this advanced smartwatch. Features heart rate monitoring, GPS, sleep tracking, and 50+ workout modes." },
                new Product { Id = 3, Name = "USB-C Charging Cable 6ft", Description = "Durable braided charging cable with fast charging support. Compatible with most USB-C devices including smartphones, tablets, and laptops." },
                new Product { Id = 4, Name = "Mechanical Gaming Keyboard", Description = "RGB backlit mechanical keyboard with customizable keys, anti-ghosting technology, and durable switches rated for 50 million keystrokes." },
                new Product { Id = 5, Name = "Portable Power Bank 20000mAh", Description = "High-capacity power bank with dual USB ports and USB-C input/output. Charges multiple devices simultaneously with fast charging technology." },
                new Product { Id = 6, Name = "Wireless Computer Mouse", Description = "Ergonomic wireless mouse with adjustable DPI settings, programmable buttons, and long-lasting battery. Ideal for work and gaming." },
                new Product { Id = 7, Name = "4K Webcam", Description = "Professional-grade webcam with 4K resolution, autofocus, built-in microphone, and wide-angle lens. Perfect for video conferencing and streaming." },
                new Product { Id = 8, Name = "Laptop Stand Aluminum", Description = "Adjustable ergonomic laptop stand made from premium aluminum. Improves posture and airflow with multiple angle settings." },
                new Product { Id = 9, Name = "Wireless Earbuds Pro", Description = "True wireless earbuds with active noise cancellation, transparency mode, and 24-hour battery life with charging case. IPX4 water resistant." },
                new Product { Id = 10, Name = "External SSD 1TB", Description = "Ultra-fast portable SSD with USB 3.2 Gen 2 interface. Read speeds up to 1050MB/s. Compact, durable design with hardware encryption." }
            );
        });
    }
}
