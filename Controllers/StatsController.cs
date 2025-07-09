
namespace QuizSite.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


public class StatsController : Controller
{
    private readonly DataContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public StatsController(DataContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var quizzes = await _context.Quizzes
        .Include(q => q.QuizQuestions)
        .ToListAsync();

        var results = await _context.UserQuizResults
        .Where(r => r.UserId == userId)
        .GroupBy(r => r.QuizId)
        .Select(g => g.OrderByDescending(r => r.TakenAt).FirstOrDefault())
        .ToListAsync();

        var stats = quizzes.Select(quiz =>
        {
            var result = results.FirstOrDefault(r => r!.QuizId == quiz.Id);
            var totalQuestions = quiz.QuizQuestions.Count;

            return new QuizStatsViewModel
            {
                QuizId = quiz.Id,
                LastTakenAt = result?.TakenAt,
                TotalQuestions = totalQuestions,
                QuizTitle = quiz.Title,
                Correct = result?.CorrectCount ?? 0,
                Blank = result?.BlankCount ?? 0,
                Wrong = result != null ? totalQuestions - result.CorrectCount - result.BlankCount : 0
            };
        }).ToList();
        return View(stats);
    }
    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (userId == null) return RedirectToAction("Index", "Home");

        var results = await _context.UserQuizResults
        .Where(r => r.QuizId == id && r.UserId == userId)
        .OrderBy(r => r.TakenAt)
        .ToListAsync();

        var quiz = await _context.Quizzes.FindAsync(id);
        ViewBag.QuizTitle = quiz?.Title ?? "Quiz";

        return View(results);
    }


    public async Task<IActionResult> List()
    {
        var quizzes = await _context.Quizzes.ToListAsync();

        var solversPerQuiz = await _context.UserQuizResults
        .GroupBy(r => r.QuizId)
        .Select(g => new
        {
            QuizId = g.Key,
            SolverCount = g.Select(r => r.UserId).Distinct().Count()
        }).ToDictionaryAsync(x => x.QuizId, x => x.SolverCount);

        var model = quizzes.Select(q => new QuizStatsListViewModel
        {
            QuizId = q.Id,
            QuizTitle = q.Title,
            SolverCount = solversPerQuiz.ContainsKey(q.Id) ? solversPerQuiz[q.Id] : 0
        }).ToList();

        return View(model);
    }

    public async Task<IActionResult> ListDetail(int id)
    {
        var quiz = await _context.Quizzes
        .Include(q => q.QuizQuestions)
        .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            TempData["Message"] = "Quiz bulunamadı.";
            return RedirectToAction("List");
        }

        var totalQuestions = quiz.QuizQuestions.Count;

        //Her kullanıcının en son çözümünü al
        var lastResults = await _context.UserQuizResults
        .Where(r => r.QuizId == id)
        .GroupBy(r => r.UserId)
        .Select(g => g.OrderByDescending(r => r.TakenAt).FirstOrDefault())
        .ToListAsync();

        var userIds = lastResults.Select(r => r!.UserId).Distinct().ToList();

        var users = await Task.WhenAll(userIds.Select(async userId =>
        {
            var user = await _userManager.FindByIdAsync(userId);
            return new { userId, userName = user?.UserName ?? "Bilinmiyor" };
        }));

        var userMap = users.ToDictionary(x => x.userId, x => x.userName);

        var model = lastResults.Select(r => new QuizStatsViewModel
        {
            QuizId = id,
            QuizTitle = quiz.Title,
            LastTakenAt = r?.TakenAt,
            Correct = r!.CorrectCount,
            Blank = r!.BlankCount,
            Wrong = totalQuestions - r.BlankCount - r.CorrectCount,
            TotalQuestions = totalQuestions,
            UserName = userMap.ContainsKey(r.UserId) ? userMap[r.UserId] : "Bilinmiyor"
        })
        .OrderByDescending(r => r.Correct)
        .ThenBy(r => r.Wrong)
        .ThenBy(r => r.Blank)
        .ThenByDescending(r => r.LastTakenAt)
        .ToList();

        return View(model);
    }
}
