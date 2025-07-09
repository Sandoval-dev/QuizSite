

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizSite.Data;

[Authorize(Roles = "Admin")]
public class QuestionController : Controller
{
    private readonly DataContext _context;

    public QuestionController(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResult> Index(string category, string q, int page = 1)
    {
        int pageSize = 15;

        // Sorguyu IQueryable olarak başlat
        var query = _context.Questions
            .Include(q => q.Options)
            .Include(q => q.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(q => q.Category.Name == category);
        }

        if (!string.IsNullOrEmpty(q))
        {
            query = query.Where(u => u.Text.ToLower().Contains(q.ToLower()));
        }

        var totalQuestions = await query.CountAsync();
        ViewBag.TotalQuestions = totalQuestions;

        var questions = await query
            .OrderBy(q => q.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuestionViewModel
            {
                Id = q.Id,
                Text = q.Text,
                CategoryName = q.Category.Name,
                CorrectOptionIndex = q.CorrectOptionIndex,
                Options = q.Options.Select(o => new AnswerOptionViewModel
                {
                    Text = o.Text
                }).ToList()
            })
            .ToListAsync();

        var categories = await _context.Categories
            .Select(c => new SelectListItem
            {
                Value = c.Name,
                Text = c.Name
            })
            .Distinct()
            .ToListAsync();

        ViewData["q"] = q;
        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = category;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalQuestions / (double)pageSize);

        return View(questions);
    }


    public ActionResult Create()
    {
        var model = new QuestionCreateModel
        {
            Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(QuestionCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = _context.Categories
           .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
           .ToList();
            return View(model);
        }

        //yeni question entitysi oluştur
        var question = new Question
        {
            Text = model.Text,
            Options = model.Options.Select((o, index) => new AnswerOption
            {
                Text = o.Text,
                IsCorrect = index == model.CorrectOptionIndex
            }).ToList(),
            CorrectOptionIndex = model.CorrectOptionIndex,
            CorrectAnswer = model.Options[model.CorrectOptionIndex].Text,
            CategoryId = model.CategoryId,

        };

        if (model.CorrectOptionIndex < 0 || model.CorrectOptionIndex >= question.Options.Count)
        {
            ModelState.AddModelError(nameof(model.CorrectOptionIndex), "Please select a valid correct answer");
            return View(model);
        }

        question.CorrectAnswer = question.Options[model.CorrectOptionIndex].Text;

        //veritabanına kaydet
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    public async Task<ActionResult> Edit(int id)
    {
        var question = await _context.Questions
        .Include(q => q.Options)
        .FirstOrDefaultAsync(u => u.Id == id);

        if (question == null)
        {
            TempData["Message"] = "Question not found.";
            return RedirectToAction("Index");
        }

        var model = new QuestionEditModel
        {
            Id = question.Id,
            Text = question.Text,
            CategoryId = question.CategoryId,
            CorrectOptionIndex = question.CorrectOptionIndex,
            Options = question.Options.Select(o => new AnswerOptionCreateModel
            {
                Text = o.Text
            }).ToList(),
            Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList()
        };

        // Eksik seçenek varsa 4'e tamamla
        while (model.Options.Count < 4)
            model.Options.Add(new AnswerOptionCreateModel());
        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Edit(int id, QuestionEditModel model)
    {
        if (id != model.Id)
        {
            TempData["Message"] = "Question not found.";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            model.Categories = _context.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

            return View(model);
        }

        if (ModelState.IsValid)
        {
            var question = _context.Questions
            .Include(q => q.Options)
            .FirstOrDefault(u => u.Id == id);

            if (question != null)
            {
                question.Text = model.Text;
                question.CorrectOptionIndex = model.CorrectOptionIndex;
                question.CategoryId = model.CategoryId;
                question.CorrectAnswer = model.Options[model.CorrectOptionIndex].Text;

                //seçenekleri güncelle
                question.Options.Clear();
                foreach (var option in model.Options)
                {
                    question.Options.Add(new AnswerOption
                    {
                        Text = option.Text
                    });
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Question updated successfully.";
            }
        }
        return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return RedirectToAction("Index");

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Question deleted successfully.";
        return RedirectToAction("Index");
    }
}