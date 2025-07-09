

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizSite.Models;


public class UserController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DataContext _context;

    public UserController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, DataContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();

        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "User",
                RegisteredDate = user is IdentityUser u && u.LockoutEnd.HasValue
                    ? u.LockoutEnd.Value.DateTime // ya da başka bir kayıt tarihi için custom property kullan
                    : DateTime.Now // burada örnek olarak şimdi koyduk
            });
        }
        return View(userViewModels);
    }


    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return RedirectToAction("Index");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "User deleted successfully";
        return RedirectToAction("Index");
    }


}