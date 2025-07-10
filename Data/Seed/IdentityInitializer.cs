
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
public class IdentityInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, string adminMail, string adminPassword)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        //check roles if not create
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminUser = await userManager.FindByEmailAsync(adminMail);
        if (adminUser == null)
        {
            var newAdmin = new IdentityUser
            {
                UserName = adminMail,
                Email = adminMail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newAdmin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
        else
        {
            // Admin zaten varsa ama güncellemek istiyorsan
            adminUser.Email = adminMail;
            adminUser.UserName = adminMail;
            await userManager.UpdateAsync(adminUser);

            // Şifreyi sıfırla (önce token oluştur)
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);
        }
    }
}