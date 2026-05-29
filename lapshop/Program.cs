using lapshop.Bl;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<LapShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services Configuration
builder.Services.AddScoped<ICategories, ClsCategories>();
builder.Services.AddScoped<IItems, ClsItems>();
builder.Services.AddScoped<IItemTypes, ClsItemTypes>();
builder.Services.AddScoped<IOs, ClsOs>();
builder.Services.AddScoped<IItemImages, ClsItemImages>();
builder.Services.AddScoped<ISalesInvoice, ClsSalesInvoice>();
builder.Services.AddScoped<ISalesInvoiceItems, ClsSalesInvoiceItems>();
builder.Services.AddScoped<ISliders, ClsSliders>();
builder.Services.AddScoped<ISettings, ClsSettings>();
builder.Services.AddScoped<IPages, ClsPages>();

builder.Services.AddSession();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LapShop API",
        Version = "v1",
        Description = "API documentation for LapShop",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "info!gmail.com",
            Url = new Uri("https://example.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "Use under LICX",
            Url = new Uri("https://example.com/license")
        }

    });
    //var xmlComments = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //var fullPath = Path.Combine(AppContext.BaseDirectory, xmlComments);
    //options.IncludeXmlComments(fullPath);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

// Identity Setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<LapShopContext>()
  .AddDefaultTokenProviders();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Users/AccessDenied";
    options.Cookie.Name = "Cookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(720);
    options.LoginPath = "/Users/Login";
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Seed Roles and Admin User
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Ensure roles exist
        string[] roleNames = { "Admin", "Customer" };
        foreach (var roleName in roleNames)
        {
            var roleExist = roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
            if (!roleExist)
            {
                roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
            }
        }

        // Seed default Admin user
        var adminEmail = "admin@lapshop.com";
        var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            var createAdmin = userManager.CreateAsync(adminUser, "Admin@123456").GetAwaiter().GetResult();
            if (createAdmin.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
            }
        }
        else
        {
            if (!userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult())
            {
                userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding Identity roles and users.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseCors();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "admin",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();