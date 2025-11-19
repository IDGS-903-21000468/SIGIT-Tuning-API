using Microsoft.EntityFrameworkCore;
using SigitTuning.API.Models;

namespace SigitTuning.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - Representan las tablas
        public DbSet<User> Users { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<MarketplaceListing> MarketplaceListings { get; set; }
        public DbSet<MarketplaceBid> MarketplaceBids { get; set; }
        public DbSet<MarketplaceChat> MarketplaceChats { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<SocialPost> SocialPosts { get; set; }
        public DbSet<SocialLike> SocialLikes { get; set; }
        public DbSet<SocialComment> SocialComments { get; set; }
        public DbSet<AssistantConsultation> AssistantConsultations { get; set; }

        // Agregar estos DbSets en ApplicationDbContext.cs

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<ReturnDetail> ReturnDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de relaciones para evitar ciclos de cascada

            // Usuarios - Evitar eliminación en cascada circular
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Usuario)
                .WithMany(u => u.Pedidos)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketplaceListing>()
                .HasOne(m => m.Vendedor)
                .WithMany(u => u.PublicacionesMarketplace)
                .HasForeignKey(m => m.UserID_Vendedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketplaceListing>()
                .HasOne(m => m.Comprador)
                .WithMany()
                .HasForeignKey(m => m.CompradorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketplaceChat>()
                .HasOne(mc => mc.Vendedor)
                .WithMany()
                .HasForeignKey(mc => mc.UserID_Vendedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MarketplaceChat>()
                .HasOne(mc => mc.Comprador)
                .WithMany()
                .HasForeignKey(mc => mc.UserID_Comprador)
                .OnDelete(DeleteBehavior.Restrict);

            // SocialLikes - Constraint único (un usuario solo puede dar 1 like por post)
            modelBuilder.Entity<SocialLike>()
                .HasIndex(sl => new { sl.PostID, sl.UserID })
                .IsUnique();

            // Índices para mejorar rendimiento
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryID);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Stock);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Estatus);

            modelBuilder.Entity<SocialPost>()
                .HasIndex(sp => sp.FechaPublicacion);
        }
    }
}