using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizSite.Models;

namespace QuizSite.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var latestQuizzes = _context.Quizzes
        .OrderByDescending(q => q.Id)
        .Take(3)
        .Select(q => new QuizViewModel
        {
            Id = q.Id,
            Title = q.Title,
            Description = q.Description,
            CategoryName = q.category.Name
        }).ToList();

        var categories = _context.Categories.ToList();
        var totalQuizzes = _context.Quizzes.Count();
        var totalQuestions = _context.Questions.Count();
        var activeUsers = _userManager.Users.Count();
        var quizzesSolved = _context.UserQuizResults.Count();

        ViewBag.LatestQuizzes = latestQuizzes;
        ViewBag.Categories = categories;
        ViewBag.TotalQuizzes = totalQuizzes;
        ViewBag.TotalQuestions = totalQuestions;
        ViewBag.ActiveUsers = activeUsers;
        ViewBag.QuizzesSolved = quizzesSolved;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult SetSession()
    {
        HttpContext.Session.SetString("TestKey", "TestValue");
        return Content("Session yazıldı.");
    }

    public IActionResult GetSession()
    {
        string val = HttpContext.Session.GetString("TestKey") ?? "Bulunamadı";
        return Content("Session değeri: " + val);
    }



}
