using Microsoft.EntityFrameworkCore;
using D.A.sneaker.Models;

namespace D.A.sneaker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ====== TABLES ======

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<UserChatState> UserChatStates { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ChatHistory> ChatHistories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
    new User
    {
        Id = 999,
        Name = "Admin",
        Email = "admin@gmail.com",
        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
        Role = "Admin"
    }
);
            modelBuilder.Entity<Size>().HasData(

    new Size { Id = 1, Number = 40 },
    new Size { Id = 2, Number = 41 },
    new Size { Id = 3, Number = 42 },
    new Size { Id = 4, Number = 43 }

);
            modelBuilder.Entity<Color>().HasData(

    new Color { Id = 1, Name = "White" },
    new Color { Id = 2, Name = "Black" },
    new Color { Id = 3, Name = "Grey" }

);
            modelBuilder.Entity<Category>().HasData(

    new Category
    {
        Id = 1,
        Name = "Running",
        Description = "Running shoes"
    },

    new Category
    {
        Id = 2,
        Name = "Casual",
        Description = "Casual sneakers"
    }

);



            // ===== TABLE NAME MAP =====
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages");
            modelBuilder.Entity<ProductVariant>().ToTable("ProductVariants");
            modelBuilder.Entity<Color>().ToTable("Colors");
            modelBuilder.Entity<Size>().ToTable("Sizes");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<ChatHistory>().ToTable("ChatHistories");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");

            modelBuilder.Entity<Wishlist>().ToTable("Wishlists");

            modelBuilder.Entity<Review>().ToTable("Reviews");
            modelBuilder.Entity<Promotion>().ToTable("Promotions");
            // ===== DECIMAL FIX =====
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.CostPrice)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<Order>()
                .Property(p => p.TotalAmount)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(p => p.Price)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<Customer>()
                .Property(p => p.TotalSpent)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<Promotion>()
                .Property(p => p.DiscountAmount)
                .HasColumnType("decimal(12,2)");
        }
        public DbSet<D.A.sneaker.Models.Category> Category { get; set; } = default!;
    }

}