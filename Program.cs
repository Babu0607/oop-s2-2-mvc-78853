using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_78853.Data;
using oop_s2_2_mvc_78853.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FoodInspectionTracker")
    .Enrich.WithEnvironmentUserName()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.File("logs/foodinspection-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Food Inspection Tracker application");

    var builder = WebApplication.CreateBuilder(args);

   
    builder.Host.UseSerilog();

  
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    
    builder.Services.AddDefaultIdentity<IdentityUser>(options => 
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();

    var app = builder.Build();
    
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await SeedData.Initialize(services);
    }


    
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapRazorPages()
        .WithStaticAssets();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}