using Microsoft.AspNetCore.Identity;
using oop_s2_2_mvc_78853.Models;
using oop_s2_2_mvc_78853.Data;
using Bogus;

namespace oop_s2_2_mvc_78853.Services;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "Inspector", "Viewer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await CreateUser(userManager, "admin@foodinspection2.com", "Admin@123", "Admin");
        await CreateUser(userManager, "inspector@foodinspection2.com", "Inspector@123", "Inspector");
        await CreateUser(userManager, "viewer@foodinspection2.com", "Viewer@123", "Viewer");

        if (context.Premises.Any()) return;

        var premisesFaker = new Faker<Premises>()
            .RuleFor(p => p.Name, f => f.Company.CompanyName() + " Restaurant")
            .RuleFor(p => p.Address, f => f.Address.StreetAddress())
            .RuleFor(p => p.Town, f => f.PickRandom("Dublin", "Cork", "Galway")) 
            .RuleFor(p => p.RiskRating, f => f.PickRandom("Low", "Medium", "High"));

        var premises = premisesFaker.Generate(12);
        await context.Premises.AddRangeAsync(premises);
        await context.SaveChangesAsync();

        var inspections = new List<Inspection>();
        var faker = new Faker();

        for (int i = 0; i < 25; i++)
        {
            var p = faker.PickRandom(premises);
            var score = faker.Random.Int(40, 100);
            
            inspections.Add(new Inspection
            {
                PremisesId = p.Id,
                Score = score,
                Outcome = score >= 70 ? "Pass" : "Fail",
                Notes = faker.Lorem.Sentence(),
                InspectionDate = i < 10 ? DateTime.Now.AddDays(-faker.Random.Int(1, 25)) 
                                        : faker.Date.Past(1)
            });
        }
        await context.Inspections.AddRangeAsync(inspections);
        await context.SaveChangesAsync();

        var followUps = new List<FollowUp>();
        for (int i = 0; i < 10; i++)
        {
            var inspection = faker.PickRandom(inspections);
            var status = faker.PickRandom("Open", "Closed");
            
            var dueDate = i < 4 ? DateTime.Now.AddDays(-10) : DateTime.Now.AddDays(14);

            followUps.Add(new FollowUp
            {
                InspectionId = inspection.Id,
                DueDate = dueDate,
                Status = status,
                ClosedDate = status == "Closed" ? DateTime.Now.AddDays(-2) : null
            });
        }
        
        await context.FollowUps.AddRangeAsync(followUps);
        await context.SaveChangesAsync();
    }

    private static async Task CreateUser(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }
    }
}