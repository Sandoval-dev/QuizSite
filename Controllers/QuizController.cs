using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizSite.Data;


public class QuizController : Controller
{
    private readonly DataContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public QuizController(DataContext context, UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        _context = context;
    }
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Index(int page = 1)
    {
        int pageSize = 5; // Her sayfada 10 quiz gösterilecek
        var totalQuizzes = await _context.Quizzes.CountAsync();


        var quizzes = await _context.Quizzes
        .Include(q => q.category) //Quiz ile ilişkili category nesnesini getirir.
        .Include(c => c.QuizQuestions) //Quiz ile ilişkili QuizQuestions ları getirir.
            .ThenInclude(qq => qq.Question) //Her bir QuizQuestion içindeki Question nesnesini getirir.
            .OrderBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
        .ToListAsync();

        //tüm quize ait question sayısı
        var totalQuestions = quizzes.Sum(q => q.QuizQuestions.Count());
        ViewBag.TotalQuestions = totalQuestions;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalQuizzes / pageSize);
        ViewBag.CurrentPage = page;
        return View(quizzes);
    }

    [Authorize(Roles = "Admin")]
    public ActionResult Create()
    {
        var allQuestions = _context.Questions
        .Include(q => q.Options)
        .ToList();

        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        ViewBag.Questions = new MultiSelectList(allQuestions, "Id", "Text");

        ViewBag.QuestionList = allQuestions.Select(q => new
        {
            Id = q.Id,
            text = q.Text,
            categoryId = q.CategoryId,
            correctAnswer = q.Options.Count > q.CorrectOptionIndex ? q.Options[q.CorrectOptionIndex].Text : ""
        }).ToList();

        return View(new QuizFormModel());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Create(QuizFormModel model)
    {
        if (!ModelState.IsValid)
        {
            // ViewBag'leri tekrar dolduruyoruz çünkü post sırasında kaybolurlar
            var allQuestions = _context.Questions.Include(q => q.Options).ToList();
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Questions = new MultiSelectList(allQuestions, "Id", "Text");
            ViewBag.QuestionList = allQuestions.Select(q => new
            {
                Id = q.Id,
                text = q.Text,
                categoryId = q.CategoryId,
                correctAnswer = q.Options.Count > q.CorrectOptionIndex ? q.Options[q.CorrectOptionIndex].Text : ""
            }).ToList();
        }

        var newQuiz = new Quiz
        {
            Title = model.Title,
            Description = model.Description,
            CategoryId = model.CategoryId,
            DurationInMinutes = model.DurationInMinutes
        };

        //seçilen sorulara göre ilişkilendirme
        newQuiz.QuizQuestions = model.SelectedQuestionIds.Select(qid => new QuizQuestion
        {
            QuizId = newQuiz.Id, //EF henüz ID'yi atamamış olacak ama sorun değil
            QuestionId = qid
        }).ToList();

        _context.Quizzes.Add(newQuiz);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Details(int id, int page = 1)
    {
        const int pageSize = 4; // her sayfada 4 soru göster

        var quiz = await _context.Quizzes
            .Include(q => q.category)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
            return RedirectToAction("Index");

        // Sayfalama için QuizQuestions'ı sırala ve sayfaya göre filtrele
        var totalQuestions = quiz.QuizQuestions.Count;
        var paginatedQuestions = quiz.QuizQuestions
            .OrderBy(qq => qq.Question.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalQuestions / pageSize);
        ViewBag.QuizId = id;
        ViewBag.TotalQuestionCount = totalQuestions;
        quiz.QuizQuestions = paginatedQuestions;

        return View(quiz);
    }

    [Authorize(Roles = "Admin")]
    public ActionResult Delete(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Index");
        }

        var quizzes = _context.Quizzes
        .Include(qq => qq.QuizQuestions)
           .ThenInclude(q => q.Question)
        .FirstOrDefault(q => q.Id == id);
        if (quizzes != null)
        {
            return View(quizzes);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public ActionResult DeleteConfirm(int id)
    {
        // Quiz'i ve ilişkili QuizQuestions ile birlikte çekiyoruz
        var quiz = _context.Quizzes
        .Include(q => q.QuizQuestions)
        .FirstOrDefault(q => q.Id == id);

        if (quiz == null)
        {
            // Quiz bulunamazsa anasayfaya yönlendir
            return RedirectToAction("Index");
        }

        _context.Quizzes.Remove(quiz);
        _context.SaveChanges();
        TempData["Success"] = "Quiz başarıyla silindi.";
        return RedirectToAction("Index");
    }


    public ActionResult List(string category, string search)
    {
        var userId = _userManager.GetUserId(User); // bu GUID olur, veritabanıyla eşleşir

        var quizzes = _context.Quizzes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            quizzes = quizzes.Where(q => q.category.Name == category);

        if (!string.IsNullOrWhiteSpace(search))
            quizzes = quizzes.Where(q => q.Title.Contains(search));

        var quizList = quizzes.Select(q => new QuizViewModel
        {
            Id = q.Id,
            Title = q.Title,
            Description = q.Description,
            DurationInMinutes = q.DurationInMinutes,
            CategoryName = q.category.Name,
            QuestionCount = q.QuizQuestions.Count,
        }).ToList();

        var categories = _context.Categories.Select(c => c.Name).Distinct().ToList();
        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = category;
        ViewBag.SearchText = search;

        //Kullanıcının çözdüğü quiz ID'leri
        var userResults = _context.UserQuizResults
        .Where(u => u.UserId == userId)
        .GroupBy(r => r.QuizId)
        .Select(g => new
        {
            QuizId = g.Key,
            LastTakenAt = g.Max(r => r.TakenAt)
        })
        .ToList();

        var solvedQuizIds = userResults.Select(r => r.QuizId).ToList();
        ViewBag.SolvedQuizzes = solvedQuizIds;
        var lastTakenDict = userResults.ToDictionary(x => x.QuizId, x => x.LastTakenAt);
        ViewBag.LastTakenDict = lastTakenDict;

        return View(quizList);
    }

    [Authorize(Roles = "Admin")]
    public ActionResult Edit(int id)
    {
        var quiz = _context.Quizzes
            .Include(q => q.QuizQuestions)
            .FirstOrDefault(q => q.Id == id);

        if (quiz == null)
        {
            TempData["Message"] = "Quiz güncellenemedi.";
            return RedirectToAction("Index");
        }

        var model = new QuizEditModel
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            DurationInMinutes = quiz.DurationInMinutes,
            CategoryId = quiz.CategoryId,
            SelectedQuestionIds = quiz.QuizQuestions.Select(qq => qq.QuestionId).ToList(),
            Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList(),

            // Sadece quizin kategorisindeki sorular yüklensin
            AllQuestions = _context.Questions
                .Where(q => q.CategoryId == quiz.CategoryId)
                .Select(q => new SelectListItem
                {
                    Value = q.Id.ToString(),
                    Text = q.Text
                }).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public JsonResult GetQuestionsByCategory(int categoryId)
    {
        var questions = _context.Questions
            .Where(q => q.CategoryId == categoryId)
            .Select(q => new
            {
                id = q.Id,
                text = q.Text
            }).ToList();

        return Json(questions);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(QuizEditModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            model.AllQuestions = _context.Questions.Select(q => new SelectListItem
            {
                Value = q.Id.ToString(),
                Text = q.Text
            }).ToList();

            return View(model);
        }

        var quiz = _context.Quizzes
        .Include(q => q.QuizQuestions)
        .FirstOrDefault(q => q.Id == model.Id);

        if (quiz == null)
        {
            TempData["Message"] = "Quiz bulunamadı.";
            return RedirectToAction("Index");
        }

        quiz.Title = model.Title;
        quiz.Description = model.Description;
        quiz.CategoryId = model.CategoryId;
        quiz.DurationInMinutes = model.DurationInMinutes;

        // Eski ilişkileri temizle
        _context.QuizQuestion.RemoveRange(quiz.QuizQuestions);

        //Yeni ilişkiler ekle
        if (model.SelectedQuestionIds != null && model.SelectedQuestionIds.Any())
        {
            quiz.QuizQuestions = model.SelectedQuestionIds.Select(qid => new QuizQuestion
            {
                QuizId = quiz.Id,
                QuestionId = qid
            }).ToList();
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Quiz başarıyla güncellendi.";
        return RedirectToAction("Index");
    }

    [Authorize]
    public ActionResult StartIntro(int id)
    {
        var userId = _userManager.GetUserId(User);
        var quiz = _context.Quizzes
        .Include(q => q.QuizQuestions)
        .Include(q => q.category)
        .FirstOrDefault(q => q.Id == id);

        var lastTaken = _context.UserQuizResults
        .Where(r => r.UserId == userId && r.QuizId == id)
        .OrderByDescending(r => r.TakenAt)
        .Select(r => r.TakenAt)
        .FirstOrDefault();

        var now = DateTime.UtcNow;
        var waitSeconds = 180;
        int secondsLeft = 0;
        bool canRetry = true;

        HttpContext.Session.Remove($"Quiz_{id}_StartTime");
        HttpContext.Session.Remove($"Quiz_{id}_Answers");
        HttpContext.Session.Remove($"Quiz_{id}_Questions");

        if (quiz == null)
        {
            return RedirectToAction("List");
        }
        if (lastTaken != default)
        {
            var secondsPassed = (now - lastTaken).TotalSeconds;
            if (secondsPassed < waitSeconds)
            {
                canRetry = false;
                secondsLeft = waitSeconds - (int)secondsPassed;
            }
        }
        var model = new QuizViewModel
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            DurationInMinutes = quiz.DurationInMinutes,
            CategoryName = quiz.category.Name,
            QuestionCount = quiz.QuizQuestions.Count
        };

        // ViewBag ile View'a gönder
        ViewBag.CanRetry = canRetry;
        ViewBag.SecondsLeft = secondsLeft;
        return View("StartIntro", model);
    }

    private DateTime? GetSessionDateTime(string key)
    {
        var str = HttpContext.Session.GetString(key);
        if (string.IsNullOrEmpty(str)) return null;

        if (DateTime.TryParse(str, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            return dt;

        return null;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Start(int id, int page = 1)
    {
        const int pageSize = 2;

        var quiz = await _context.Quizzes
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
            return RedirectToAction("List");

        // Süre kontrolü
        string timeKey = $"Quiz_{id}_StartTime";
        var startTimeStr = HttpContext.Session.GetString(timeKey);

        if (string.IsNullOrEmpty(startTimeStr))
        {
            HttpContext.Session.Remove($"Quiz_{id}_Answers");
            HttpContext.Session.Remove($"Quiz_{id}_Questions");

            var now = DateTime.UtcNow;
            HttpContext.Session.SetString(timeKey, now.ToString("o"));
            startTimeStr = now.ToString("o");
        }

        var startTime = DateTime.Parse(startTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var timePassed = (DateTime.UtcNow - startTime).TotalSeconds;
        var remainingSeconds = quiz.DurationInMinutes * 60 - (int)timePassed;

        if (remainingSeconds <= 0)
        {
            HttpContext.Session.Remove(timeKey);
            HttpContext.Session.Remove($"Quiz_{id}_Answers");
            return RedirectToAction("SubmitAnswers", new { id });
        }

        ViewBag.RemainingSeconds = remainingSeconds;

        // Cevapları session'dan al
        string answerKey = $"Quiz_{id}_Answers";
        Dictionary<int, int> selectedAnswers = new();
        var answerJson = HttpContext.Session.GetString(answerKey);
        if (!string.IsNullOrEmpty(answerJson))
            selectedAnswers = JsonSerializer.Deserialize<Dictionary<int, int>>(answerJson)!;

        var allQuestions = quiz.QuizQuestions
            .OrderBy(qq => qq.Question.Id)
            .Select(qq => qq.Question)
            .ToList();

        // Tüm soruların ViewModel listesi
        var questionViewModelList = allQuestions.Select(q => new QuestionSolveViewModel
        {
            Id = q.Id,
            Text = q.Text,
            Options = q.Options.Select(o => new AnswerOptionViewModel
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.IsCorrect
            }).ToList(),
            SelectedOptionId = selectedAnswers.TryGetValue(q.Id, out var selectedId) ? selectedId : (int?)null
        }).ToList();

        // Sadece burada 1 defa yazmamız yeterli
        HttpContext.Session.SetString($"Quiz_{id}_Questions", JsonSerializer.Serialize(questionViewModelList));

        int totalQuestions = allQuestions.Count;
        int totalPages = (int)Math.Ceiling(totalQuestions / (double)pageSize);
        page = Math.Max(1, Math.Min(page, totalPages));

        var pagedQuestions = allQuestions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var model = new QuizSolveViewModel
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalQues = totalQuestions,
            DurationInMinutes = quiz.DurationInMinutes,
            Questions = pagedQuestions.Select(q => new QuestionSolveViewModel
            {
                Id = q.Id,
                Text = q.Text,
                Options = q.Options.Select(o => new AnswerOptionViewModel
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList(),
                SelectedOptionId = selectedAnswers.TryGetValue(q.Id, out var selectedId) ? selectedId : (int?)null
            }).ToList()
        };

        return View(model);
    }


    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id, int page, Dictionary<int, int>? Answers, string? action)
    {
        string answerKey = $"Quiz_{id}_Answers";
        string questionKey = $"Quiz_{id}_Questions";
        string timeKey = $"Quiz_{id}_StartTime";

        var quiz = await _context.Quizzes
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            TempData["ErrorMessage"] = "Quiz bulunamadı.";
            return RedirectToAction("List");
        }

        // Önceki cevapları al
        Dictionary<int, int> existingAnswers = new();
        var answerJson = HttpContext.Session.GetString(answerKey);
        if (!string.IsNullOrEmpty(answerJson))
            existingAnswers = JsonSerializer.Deserialize<Dictionary<int, int>>(answerJson)!;

        if (Answers != null)
        {
            foreach (var kvp in Answers)
                existingAnswers[kvp.Key] = kvp.Value;

            HttpContext.Session.SetString(answerKey, JsonSerializer.Serialize(existingAnswers));
        }

        // Soruları tekrar session'a kaydet (submit durumunda özellikle önemli)
        var allQuestions = quiz.QuizQuestions
            .OrderBy(qq => qq.Question.Id)
            .Select(qq => qq.Question)
            .ToList();

        var allQuestionsViewModel = allQuestions.Select(q => new QuestionSolveViewModel
        {
            Id = q.Id,
            Text = q.Text,
            Options = q.Options.Select(o => new AnswerOptionViewModel
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.IsCorrect
            }).ToList(),
            SelectedOptionId = existingAnswers.TryGetValue(q.Id, out var selectedId) ? selectedId : (int?)null
        }).ToList();

        HttpContext.Session.SetString(questionKey, JsonSerializer.Serialize(allQuestionsViewModel));

        if (action == "submit")
        {
            Console.WriteLine("AnswerJson saved: " + JsonSerializer.Serialize(existingAnswers));
            return RedirectToAction("SubmitAnswers", new { id });
        }

        // Süre kontrolü
        var startTimeStr = HttpContext.Session.GetString(timeKey);
        if (string.IsNullOrEmpty(startTimeStr))
        {
            var now = DateTime.UtcNow;
            HttpContext.Session.SetString(timeKey, now.ToString("o"));
            startTimeStr = now.ToString("o");
        }

        var startTime = DateTime.Parse(startTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var timePassed = (DateTime.UtcNow - startTime).TotalSeconds;
        var remainingSeconds = quiz.DurationInMinutes * 60 - (int)timePassed;

        if (remainingSeconds <= 0)
        {
            return RedirectToAction("SubmitAnswers", new { id });
        }

        ViewBag.RemainingSeconds = remainingSeconds;

        if (action == "prev") page--;
        else if (action == "next") page++;

        int totalQuestions = allQuestions.Count;
        int totalPages = (int)Math.Ceiling(totalQuestions / (double)2);
        page = Math.Max(1, Math.Min(page, totalPages));

        var pagedQuestions = allQuestions
            .Skip((page - 1) * 2)
            .Take(2)
            .ToList();

        var model = new QuizSolveViewModel
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalQues = totalQuestions,
            DurationInMinutes = quiz.DurationInMinutes,
            Questions = pagedQuestions.Select(q => new QuestionSolveViewModel
            {
                Id = q.Id,
                Text = q.Text,
                Options = q.Options.Select(o => new AnswerOptionViewModel
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList(),
                SelectedOptionId = existingAnswers.TryGetValue(q.Id, out var sel) ? sel : (int?)null
            }).ToList()
        };

        return View(model);
    }


    [Authorize]
    public async Task<IActionResult> SubmitAnswers(int id)
    {
        var userId = _userManager.GetUserId(User);

        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null)
        {
            TempData["ErrorMessage"] = "Quiz bulunamadı.";
            return RedirectToAction("List");
        }

        string answerKey = $"Quiz_{id}_Answers";
        string questionKey = $"Quiz_{id}_Questions";

        var answerJson = HttpContext.Session.GetString(answerKey);
        var questionJson = HttpContext.Session.GetString(questionKey);

        if (string.IsNullOrEmpty(answerJson) || string.IsNullOrEmpty(questionJson))
        {
            TempData["Message"] = "Lütfen en az bir soru işaretleyin.";
            return RedirectToAction("Start", new { id });
        }

        var userAnswers = JsonSerializer.Deserialize<Dictionary<int, int>>(answerJson)!;
        var questions = JsonSerializer.Deserialize<List<QuestionSolveViewModel>>(questionJson)!;

        int correctCount = 0;
        int blankCount = 0;
        foreach (var question in questions)
        {
            if (!userAnswers.TryGetValue(question.Id, out var selectedOptionId))
            {
                blankCount++;
                continue;
            }
            var correctOptionId = question.Options.FirstOrDefault(o => o.IsCorrect)?.Id;
            if (correctOptionId != null && selectedOptionId == correctOptionId)
            {
                correctCount++;
            }
        }

        int totalQuestions = questions.Count;
        int wrongCount = totalQuestions - correctCount - blankCount;

        var existingResult = _context.UserQuizResults
        .Where(r => r.QuizId == id && r.UserId == userId)
        .OrderBy(r => r.TakenAt)
        .ToList();

        if (existingResult.Count >= 3)
        {
            var oldestResult = existingResult.First();
            _context.UserQuizResults.Remove(oldestResult);
        }

        var result = new UserQuizResult
        {
            QuizId = id,
            UserId = userId!,
            WrongCount = wrongCount,
            BlankCount = blankCount,
            TakenAt = DateTime.UtcNow,
            CorrectCount = correctCount
        };

        _context.UserQuizResults.Add(result);
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove(answerKey);
        HttpContext.Session.Remove(questionKey);
        HttpContext.Session.Remove($"Quiz_{id}_StartTime");

        return RedirectToAction("Result", new { id });
    }


    [Authorize]
    public IActionResult Result(int id)
    {
        var userId = _userManager.GetUserId(User);

        var quiz = _context.Quizzes
            .Include(q => q.QuizQuestions)
            .FirstOrDefault(q => q.Id == id);

        var result = _context.UserQuizResults
            .Where(r => r.QuizId == id && r.UserId == userId)
            .OrderByDescending(r => r.TakenAt)
            .FirstOrDefault();

        if (quiz == null || result == null)
        {
            TempData["Message"] = "Sonuç bulunamadı.";
            return RedirectToAction("List");
        }

        var totalQuestions = quiz.QuizQuestions.Count;
        var correct = result.CorrectCount;
        var percentage = totalQuestions > 0 ? (int)((double)correct / totalQuestions * 100) : 0;

        var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        var localTakenAt = TimeZoneInfo.ConvertTimeFromUtc(result.TakenAt, turkeyTimeZone);

        var model = new QuizResultViewModel
        {
            QuizTitle = quiz.Title,
            TotalQuestions = totalQuestions,
            CorrectAnswers = correct,
            Percentage = percentage,
            TakenAt = localTakenAt,
            BlankAnswers = result.BlankCount
        };

        return View(model);
    }
}