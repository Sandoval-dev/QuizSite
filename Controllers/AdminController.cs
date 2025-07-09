

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly DataContext _context;

    public AdminController(DataContext context, UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<ActionResult> Index()
    {
        ViewBag.QuizCount = await _context.Quizzes.CountAsync();
        ViewBag.QuestionCount = await _context.Questions.CountAsync();
        ViewBag.UserCount = await _userManager.Users.CountAsync();
        ViewBag.ResultCount = await _context.UserQuizResults.CountAsync();


        var topQuizzes = await _context.UserQuizResults
        .GroupBy(r => r.QuizId)
        .OrderByDescending(g => g.Count())
        .Take(5)
        .Select(g => new QuizSummaryViewModel
        {
            Title = _context.Quizzes.FirstOrDefault(q => q.Id == g.Key)!.Title,
            CategoryName = _context.Quizzes.FirstOrDefault(q => q.Id == g.Key)!.category.Name,
            SolveCount = g.Count()
        }).ToListAsync();

        ViewBag.ChartLabels = topQuizzes.Select(q => q.Title).ToList();
        ViewBag.ChartData = topQuizzes.Select(q => q.SolveCount).ToList();

        var recentQuizzes = await _context.Quizzes
        .OrderByDescending(q => q.Id)
        .Take(5)
        .Select(q => new QuizSummaryViewModel
        {
            Title = q.Title,
            CategoryName = q.category.Name,
            SolveCount = 0
        }).ToListAsync();

        var lastestUsers = await _context.Users
        .OrderByDescending(u => u.Id)
        .Take(1)
        .Select(q => new UserViewModel
        {
            Id = q.Id,
            Email = q.Email!,
            UserName = q.UserName!

        }).ToListAsync();

        ViewBag.TopQuizzes = topQuizzes;
        ViewBag.RecentQuizzes = recentQuizzes;
        ViewBag.LatestUsers = lastestUsers;

        return View();

    }
}