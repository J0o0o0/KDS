using KDS.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KDS.Server.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var db = services.GetRequiredService<AppDbContext>();

            string[] roles = { "Admin", "Cashier", "Cook", "Expediter" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed default admin
            var adminEmail = "admin@kds.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ── Stations
            if (!await db.Stations.AnyAsync())
            {
                db.Stations.AddRange(
                    new Station { Name = "Grill", Color = "#ef4444", SortOrder = 1 },
                    new Station { Name = "Fry", Color = "#f59e0b", SortOrder = 2 },
                    new Station { Name = "Salad", Color = "#10b981", SortOrder = 3 },
                    new Station { Name = "Drinks", Color = "#3b82f6", SortOrder = 4 }
                );
                await db.SaveChangesAsync();
            }

            // ── Components (only if empty)
            if (!await db.Components.AnyAsync())
            {
                var grill = await db.Stations.FirstAsync(s => s.Name == "Grill");
                var fry = await db.Stations.FirstAsync(s => s.Name == "Fry");
                var salad = await db.Stations.FirstAsync(s => s.Name == "Salad");
                var drinks = await db.Stations.FirstAsync(s => s.Name == "Drinks");

                var components = new List<Component>
            {
                new() { Name = "Beef Burger", DefaultStationId = grill.Id },
                new() { Name = "Chicken Burger", DefaultStationId = grill.Id },
                new() { Name = "Fried Chicken Piece", DefaultStationId = fry.Id },
                new() { Name = "French Fries", DefaultStationId = fry.Id },
                new() { Name = "Coleslaw", DefaultStationId = salad.Id },
                new() { Name = "Cola", DefaultStationId = drinks.Id },
                new() { Name = "Orange Juice", DefaultStationId = drinks.Id },
                new() { Name = "Shawerma Wrap", DefaultStationId = grill.Id },
            };
                db.Components.AddRange(components);
                await db.SaveChangesAsync();

                // Variants
                db.ComponentVariants.AddRange(
                    // Beef Burger, Chicken Burger, Fried Chicken, Shawerma — Spicy/Not Spicy
                    new ComponentVariant { ComponentId = components[0].Id, Name = "Not Spicy", IsDefault = true },
                    new ComponentVariant { ComponentId = components[0].Id, Name = "Spicy" },
                    new ComponentVariant { ComponentId = components[1].Id, Name = "Not Spicy", IsDefault = true },
                    new ComponentVariant { ComponentId = components[1].Id, Name = "Spicy" },
                    new ComponentVariant { ComponentId = components[2].Id, Name = "Not Spicy", IsDefault = true },
                    new ComponentVariant { ComponentId = components[2].Id, Name = "Spicy" },
                    new ComponentVariant { ComponentId = components[7].Id, Name = "Not Spicy", IsDefault = true },
                    new ComponentVariant { ComponentId = components[7].Id, Name = "Spicy" },
                    // Fries — Small/Medium/Large
                    new ComponentVariant { ComponentId = components[3].Id, Name = "Small", IsDefault = true },
                    new ComponentVariant { ComponentId = components[3].Id, Name = "Medium", PriceDelta = 1 },
                    new ComponentVariant { ComponentId = components[3].Id, Name = "Large", PriceDelta = 2 },
                    // Coleslaw — Small/Medium/Large
                    new ComponentVariant { ComponentId = components[4].Id, Name = "Small", IsDefault = true },
                    new ComponentVariant { ComponentId = components[4].Id, Name = "Medium", PriceDelta = 1 },
                    new ComponentVariant { ComponentId = components[4].Id, Name = "Large", PriceDelta = 2 },
                    // Cola — Can / 1.5L Bottle
                    new ComponentVariant { ComponentId = components[5].Id, Name = "Can", IsDefault = true },
                    new ComponentVariant { ComponentId = components[5].Id, Name = "1.5L Bottle", PriceDelta = 2 },
                    // Orange Juice — Glass / Bottle
                    new ComponentVariant { ComponentId = components[6].Id, Name = "Glass", IsDefault = true },
                    new ComponentVariant { ComponentId = components[6].Id, Name = "Bottle", PriceDelta = 1.5m }
                );
                await db.SaveChangesAsync();

                // AddOns
                var addons = new List<AddOn>
            {
                new() { Name = "Extra Cheese", Price = 1.00m },
                new() { Name = "Bacon", Price = 1.50m },
                new() { Name = "Extra Sauce", Price = 0.50m },
                new() { Name = "No Tomato", IsRemoval = true },
                new() { Name = "No Onion", IsRemoval = true },
                new() { Name = "No Lettuce", IsRemoval = true },
                new() { Name = "Garlic Sauce", Price = 0.50m },
            };
                db.AddOns.AddRange(addons);
                await db.SaveChangesAsync();

                // Allowed AddOns — burgers and shawerma share most addons
                foreach (var compId in new[] { components[0].Id, components[1].Id, components[7].Id })
                {
                    db.ComponentAllowedAddOns.AddRange(
                        new ComponentAllowedAddOn { ComponentId = compId, AddOnId = addons[0].Id, MaxQuantity = 3 },
                        new ComponentAllowedAddOn { ComponentId = compId, AddOnId = addons[1].Id, MaxQuantity = 2 },
                        new ComponentAllowedAddOn { ComponentId = compId, AddOnId = addons[2].Id, MaxQuantity = 1 },
                        new ComponentAllowedAddOn { ComponentId = compId, AddOnId = addons[3].Id, MaxQuantity = 1 },
                        new ComponentAllowedAddOn { ComponentId = compId, AddOnId = addons[4].Id, MaxQuantity = 1 },
                        new ComponentAllowedAddOn { ComponentId = compId, AddOnId = addons[5].Id, MaxQuantity = 1 }
                    );
                }
                // Shawerma also gets garlic sauce
                db.ComponentAllowedAddOns.Add(new ComponentAllowedAddOn
                { ComponentId = components[7].Id, AddOnId = addons[6].Id, MaxQuantity = 2 });
                await db.SaveChangesAsync();

                // Swap Pairs — bidirectional
                db.SwapPairs.AddRange(
                    new SwapPair { ComponentAId = components[0].Id, ComponentBId = components[1].Id }, // Beef↔Chicken Burger
                    new SwapPair { ComponentAId = components[3].Id, ComponentBId = components[4].Id }, // Fries↔Coleslaw
                    new SwapPair { ComponentAId = components[5].Id, ComponentBId = components[6].Id }  // Cola↔OJ
                );
                await db.SaveChangesAsync();

                // Menu Items — each just lists components + quantities
                var classicBurger = new MenuItem
                {
                    Name = "Classic Burger",
                    BasePrice = 8.99m,
                    Category = "Burgers",
                    PrepTimeMinutes = 10
                };
                classicBurger.Components.Add(new MenuItemComponent { ComponentId = components[0].Id, Quantity = 1 });

                var boxOf4 = new MenuItem
                {
                    Name = "Box of 4 Burgers",
                    BasePrice = 20.00m,
                    Category = "Boxes",
                    PrepTimeMinutes = 20
                };
                boxOf4.Components.Add(new MenuItemComponent { ComponentId = components[0].Id, Quantity = 4 });
                // Beef Burger can swap to Chicken Burger via SwapPair

                var burgerCombo = new MenuItem
                {
                    Name = "Burger Combo",
                    BasePrice = 12.99m,
                    Category = "Combos",
                    PrepTimeMinutes = 15
                };
                burgerCombo.Components.Add(new MenuItemComponent { ComponentId = components[0].Id, Quantity = 1 });
                burgerCombo.Components.Add(new MenuItemComponent { ComponentId = components[3].Id, Quantity = 1 });
                burgerCombo.Components.Add(new MenuItemComponent { ComponentId = components[5].Id, Quantity = 1 });

                var familyBucket = new MenuItem
                {
                    Name = "Family Bucket 6 PCs",
                    BasePrice = 34.99m,
                    Category = "Buckets",
                    PrepTimeMinutes = 25
                };
                familyBucket.Components.Add(new MenuItemComponent { ComponentId = components[2].Id, Quantity = 6 });
                familyBucket.Components.Add(new MenuItemComponent { ComponentId = components[3].Id, Quantity = 1 });
                familyBucket.Components.Add(new MenuItemComponent { ComponentId = components[4].Id, Quantity = 1 });
                familyBucket.Components.Add(new MenuItemComponent { ComponentId = components[5].Id, Quantity = 1 });

                // Shawerma items — one component reused 3 times
                var shawermaWrap = new MenuItem
                {
                    Name = "Shawerma Wrap",
                    BasePrice = 9.99m,
                    Category = "Shawerma",
                    PrepTimeMinutes = 8
                };
                shawermaWrap.Components.Add(new MenuItemComponent { ComponentId = components[7].Id, Quantity = 1 });

                var shawermaCombo = new MenuItem
                {
                    Name = "Shawerma Combo",
                    BasePrice = 13.99m,
                    Category = "Combos",
                    PrepTimeMinutes = 12
                };
                shawermaCombo.Components.Add(new MenuItemComponent { ComponentId = components[7].Id, Quantity = 1 });
                shawermaCombo.Components.Add(new MenuItemComponent { ComponentId = components[3].Id, Quantity = 1 });
                shawermaCombo.Components.Add(new MenuItemComponent { ComponentId = components[5].Id, Quantity = 1 });

                var doubleShawerma = new MenuItem
                {
                    Name = "Double Shawerma",
                    BasePrice = 19.99m,
                    Category = "Combos",
                    PrepTimeMinutes = 15
                };
                doubleShawerma.Components.Add(new MenuItemComponent { ComponentId = components[7].Id, Quantity = 2 });
                doubleShawerma.Components.Add(new MenuItemComponent { ComponentId = components[3].Id, Quantity = 1 });
                doubleShawerma.Components.Add(new MenuItemComponent { ComponentId = components[5].Id, Quantity = 2 });

                db.MenuItems.AddRange(classicBurger, boxOf4, burgerCombo, familyBucket,
                                      shawermaWrap, shawermaCombo, doubleShawerma);
                await db.SaveChangesAsync();
            }
        }
    }
}
