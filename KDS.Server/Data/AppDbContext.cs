using KDS.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KDS.Server.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Station> Stations => Set<Station>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<MenuItemComponent> MenuItemComponents => Set<MenuItemComponent>();
        public DbSet<Component> Components => Set<Component>();
        public DbSet<ComponentVariant> ComponentVariants => Set<ComponentVariant>();
        public DbSet<SwapPair> SwapPairs => Set<SwapPair>();
        public DbSet<AddOn> AddOns => Set<AddOn>();
        public DbSet<ComponentAllowedAddOn> ComponentAllowedAddOns => Set<ComponentAllowedAddOn>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderItemComponent> OrderItemComponents => Set<OrderItemComponent>();
        public DbSet<OrderItemComponentAddOn> OrderItemComponentAddOns => Set<OrderItemComponentAddOn>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Station>(e =>
            {
                e.HasIndex(s => s.Name).IsUnique();
                e.Property(s => s.Color).HasDefaultValue("#6b7280");
            });

            builder.Entity<MenuItem>(e =>
            {
                e.Property(m => m.BasePrice).HasColumnType("decimal(10,2)");
                e.HasIndex(m => m.Category);
            });

            builder.Entity<MenuItemComponent>(e =>
            {
                e.HasOne(mc => mc.MenuItem)
                 .WithMany(m => m.Components)
                 .HasForeignKey(mc => mc.MenuItemId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(mc => mc.Component)
                 .WithMany()
                 .HasForeignKey(mc => mc.ComponentId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(mc => new { mc.MenuItemId, mc.ComponentId }).IsUnique();
            });

            builder.Entity<Component>(e =>
            {
                e.HasOne(c => c.DefaultStation)
                 .WithMany()
                 .HasForeignKey(c => c.DefaultStationId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(c => c.Name);
            });

            builder.Entity<ComponentVariant>(e =>
            {
                e.HasOne(v => v.Component)
                 .WithMany(c => c.Variants)
                 .HasForeignKey(v => v.ComponentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(v => v.PriceDelta).HasColumnType("decimal(10,2)");
                e.HasIndex(v => new { v.ComponentId, v.Name }).IsUnique();
            });

            builder.Entity<SwapPair>(e =>
            {
                e.HasOne(s => s.ComponentA)
                 .WithMany()
                 .HasForeignKey(s => s.ComponentAId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(s => s.ComponentB)
                 .WithMany()
                 .HasForeignKey(s => s.ComponentBId)
                 .OnDelete(DeleteBehavior.Restrict);
                // Prevent duplicate pairs (always store smaller ID first)
                e.HasIndex(s => new { s.ComponentAId, s.ComponentBId }).IsUnique();
            });

            builder.Entity<AddOn>(e =>
            {
                e.Property(a => a.Price).HasColumnType("decimal(10,2)");
                e.HasIndex(a => a.Name);
            });

            builder.Entity<ComponentAllowedAddOn>(e =>
            {
                e.HasOne(a => a.Component)
                 .WithMany(c => c.AllowedAddOns)
                 .HasForeignKey(a => a.ComponentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(a => a.AddOn)
                 .WithMany()
                 .HasForeignKey(a => a.AddOnId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(a => new { a.ComponentId, a.AddOnId }).IsUnique();
            });

            builder.Entity<AppUser>(e =>
            {
                e.HasOne(u => u.Station)
                 .WithMany(s => s.AssignedCooks)
                 .HasForeignKey(u => u.StationId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Order>(e =>
            {
                e.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
                e.HasIndex(o => o.OrderNumber).IsUnique();
                e.HasIndex(o => o.Status);
                e.HasIndex(o => o.CreatedAt);
            });

            builder.Entity<OrderItem>(e =>
            {
                e.HasOne(i => i.Order)
                 .WithMany(o => o.Items)
                 .HasForeignKey(i => i.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
                e.Property(i => i.LineTotal).HasColumnType("decimal(10,2)");
            });

            builder.Entity<OrderItemComponent>(e =>
            {
                e.HasOne(c => c.OrderItem)
                 .WithMany(i => i.Components)
                 .HasForeignKey(c => c.OrderItemId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(c => c.StationId);
                e.HasIndex(c => c.Status);
            });

            builder.Entity<OrderItemComponentAddOn>(e =>
            {
                e.HasOne(a => a.OrderItemComponent)
                 .WithMany(c => c.AddOns)
                 .HasForeignKey(a => a.OrderItemComponentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(a => a.Price).HasColumnType("decimal(10,2)");
            });
        }
    }
}