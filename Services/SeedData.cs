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

        string adminEmail = "admin@foodinspection2.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        
        string inspectorEmail = "inspector@foodinspection2.com";
        if (await userManager.FindByEmailAsync(inspectorEmail) == null)
        {
            var user = new IdentityUser { UserName = inspectorEmail, Email = inspectorEmail, EmailConfirmed = true };
            await userManager.CreateAsync(user, "Inspector@123");
            await userManager.AddToRoleAsync(user, "Inspector");
        }

        string viewerEmail = "viewer@foodinspection2.com";
        if (await userManager.FindByEmailAsync(viewerEmail) == null)
        {
            var user = new IdentityUser { UserName = viewerEmail, Email = viewerEmail, EmailConfirmed = true };
            await userManager.CreateAsync(user, "Viewer@123");
            await userManager.AddToRoleAsync(user, "Viewer");
        }

        if (context.Premises.Any()) return;

        var premisesFaker = new Faker<Premises>()
            .RuleFor(p => p.Name, f => f.Company.CompanyName() + " Restaurant")
            .RuleFor(p => p.Address, f => f.Address.StreetAddress())
            .RuleFor(p => p.Town, f => f.PickRandom("Dublin", "Cork", "Limerick", "Galway", "Tralee"))
            .RuleFor(p => p.RiskRating, f => f.PickRandom("Low", "Medium", "High"));

        var premises = premisesFaker.Generate(12);
        await context.Premises.AddRangeAsync(premises);
        await context.SaveChangesAsync();

        var inspections = new List<Inspection>();
        foreach (var premise in premises)
        {
            var inspectionFaker = new Faker<Inspection>()
                .RuleFor(i => i.PremisesId, premise.Id)
                .RuleFor(i => i.InspectionDate, f => f.Date.Past(180))
                .RuleFor(i => i.Score, f => f.Random.Int(50, 100))
                .RuleFor(i => i.Outcome, (f, i) => i.Score >= 70 ? "Pass" : "Fail")
                .RuleFor(i => i.Notes, f => f.Lorem.Sentence());

            var premiseInspections = inspectionFaker.Generate(new Random().Next(1, 4));
            inspections.AddRange(premiseInspections);
        }
        
        var selectedInspections = inspections.Take(25).ToList();
        await context.Inspections.AddRangeAsync(selectedInspections);
        await context.SaveChangesAsync();

        var followUps = new List<FollowUp>();
        for (int i = 0; i < 10; i++)
        {
            var faker = new Faker();
            var inspection = faker.PickRandom(selectedInspections);
            
            var followUp = new FollowUp
            {
                InspectionId = inspection.Id,
                DueDate = faker.Date.Future(30),
                Status = faker.PickRandom("Open", "Closed")
            };
            
            if (followUp.Status == "Closed")
            {
                followUp.ClosedDate = faker.Date.Recent(10);
            }
            
            followUps.Add(followUp);
        }
        
        await context.FollowUps.AddRangeAsync(followUps);
        await context.SaveChangesAsync();
    }
}