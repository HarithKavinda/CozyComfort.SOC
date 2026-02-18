using CozyComfort.API.Middleware;
using CozyComfort.Data.Context;
using CozyComfort.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CozyComfort.Data")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// JWT Authentication - must match AuthController signing key
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKey123!SuperSecretKey123!";
var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ----- SAMPLE DATA SEEDING (runs once when DB is empty) -----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Apply migrations and create DB if needed
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        // Basic demo users for each role
        var admin = new User
        {
            Email = "admin@cozycomfort.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin
        };
        var manufacturer = new User
        {
            Email = "mfg@cozycomfort.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mfg@123"),
            Role = UserRole.Manufacturer
        };
        var distributor = new User
        {
            Email = "dist@cozycomfort.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dist@123"),
            Role = UserRole.Distributor
        };
        var seller = new User
        {
            Email = "seller@cozycomfort.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Seller@123"),
            Role = UserRole.Seller
        };
        var customer = new User
        {
            Email = "customer@cozycomfort.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Role = UserRole.Customer
        };

        db.Users.AddRange(admin, manufacturer, distributor, seller, customer);
        db.SaveChanges();

        // Manufacturer base stock
        var mfgStock = new[]
        {
            new Inventory { ProductName = "CloudSoft Blanket", Quantity = 120, Price = 79.99m, OwnerUserId = manufacturer.UserId, OwnerRole = "Manufacturer" },
            new Inventory { ProductName = "ArcticWarm Duvet",  Quantity = 80,  Price = 109.50m, OwnerUserId = manufacturer.UserId, OwnerRole = "Manufacturer" },
            new Inventory { ProductName = "Bamboo Breeze Throw", Quantity = 150, Price = 49.90m, OwnerUserId = manufacturer.UserId, OwnerRole = "Manufacturer" }
        };

        // Distributor stock
        var distStock = new[]
        {
            new Inventory { ProductName = "CloudSoft Blanket", Quantity = 40, Price = 84.99m, OwnerUserId = distributor.UserId, OwnerRole = "Distributor" },
            new Inventory { ProductName = "Bamboo Breeze Throw", Quantity = 30, Price = 54.90m, OwnerUserId = distributor.UserId, OwnerRole = "Distributor" }
        };

        // Seller stock
        var sellerStock = new[]
        {
            new Inventory { ProductName = "CloudSoft Blanket", Quantity = 12, Price = 99.99m, OwnerUserId = seller.UserId, OwnerRole = "Seller" },
            new Inventory { ProductName = "Bamboo Breeze Throw", Quantity = 20, Price = 69.90m, OwnerUserId = seller.UserId, OwnerRole = "Seller" }
        };

        db.Inventories.AddRange(mfgStock);
        db.Inventories.AddRange(distStock);
        db.Inventories.AddRange(sellerStock);
        db.SaveChanges();

        // Example orders in different stages
        var distToMfgOrder = new Order
        {
            ProductName = "CloudSoft Blanket",
            Quantity = 25,
            UnitPrice = 79.99m,
            Status = "Pending",
            FromUserId = distributor.UserId,
            ToUserId = manufacturer.UserId,
            OrderType = "DistributorToManufacturer"
        };

        var sellerToDistOrder = new Order
        {
            ProductName = "CloudSoft Blanket",
            Quantity = 8,
            UnitPrice = 84.99m,
            Status = "Pending",
            FromUserId = seller.UserId,
            ToUserId = distributor.UserId,
            OrderType = "SellerToDistributor"
        };

        var customerToSellerOrder = new Order
        {
            ProductName = "Bamboo Breeze Throw",
            Quantity = 2,
            UnitPrice = 69.90m,
            Status = "Completed",
            FromUserId = customer.UserId,
            ToUserId = seller.UserId,
            OrderType = "CustomerToSeller"
        };

        db.Orders.AddRange(distToMfgOrder, sellerToDistOrder, customerToSellerOrder);
        db.SaveChanges();

        // Notifications so dashboards aren't empty
        db.Notifications.AddRange(
            new Notification
            {
                UserId = manufacturer.UserId,
                Title = "New Distributor Order",
                Message = $"Distributor requested {distToMfgOrder.Quantity} x {distToMfgOrder.ProductName}.",
                RelatedEntityType = "Order",
                RelatedEntityId = distToMfgOrder.Id
            },
            new Notification
            {
                UserId = distributor.UserId,
                Title = "New Seller Order",
                Message = $"Seller ordered {sellerToDistOrder.Quantity} x {sellerToDistOrder.ProductName}.",
                RelatedEntityType = "Order",
                RelatedEntityId = sellerToDistOrder.Id
            },
            new Notification
            {
                UserId = customer.UserId,
                Title = "Welcome to CozyComfort",
                Message = "Start shopping from your Customer dashboard.",
                RelatedEntityType = null,
                RelatedEntityId = null
            }
        );

        // Simple sample message between distributor and seller
        var msg = new Message
        {
            FromUserId = distributor.UserId,
            ToUserId = seller.UserId,
            Content = "Your recent order has been shipped and will arrive in 2–3 days."
        };
        db.Messages.Add(msg);
        db.SaveChanges();
    }
}

// Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Avoid http->https redirect in Development, because the Web app uses HttpClient
// with a configured HTTP base URL and redirects can drop Authorization headers.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

// Middleware should be before MapControllers
app.UseRoleAuthorization();

app.MapControllers();

app.Run();
