

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizSite.Data;
using QuizSite.Models;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly DataContext _context;

    public CategoryController(DataContext context)
    {
        _context = context;
    }
    public ActionResult Index(int page=1)
    {
        int pagesize = 8;
        var totalCategories = _context.Categories.Count();
        var categories = _context.Categories.OrderBy(i => i.Id)
        .Skip((page - 1) * pagesize)
        .Take(pagesize)
        .Select(i => new CategoryGetModel
        {
            Id = i.Id,
            Name = i.Name,
            QuizCount = i.Quizzes.Count(),
            QuestionCount = i.Questions.Count()
        }).ToList();
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCategories / pagesize);
        ViewBag.CurrentPage = page;
        return View(categories);
    }

    public ActionResult Create()
    {
        return View();
    }


    [HttpPost]
    public ActionResult Create(CategoryCreateModel model)
    {
        if (ModelState.IsValid)
        {
            var category = new Category
            {
                Name = model.Name
            };
            _context.Categories.Add(category);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Category created successfully!";
            return RedirectToAction("Index");
        }
        return View(model);
    }

    public ActionResult Delete(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Index");
        }
        var category = _context.Categories.FirstOrDefault(i => i.Id == id);
        if (category != null)
        {
            return View(category);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult DeleteConfirm(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("Index");
        }
        var category = _context.Categories.FirstOrDefault(i => i.Id == id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["SuccessMessage"] = $"{category.Name} Category deleted successfully.";
            return RedirectToAction("Index");
        }
        return RedirectToAction("Index");
    }

    public ActionResult Edit(int id)
    {
        var category = _context.Categories.Select(i => new CategoryEditModel
        {
            Id = i.Id,
            Name = i.Name
        }).FirstOrDefault(i => i.Id == id);
        return View(category);
    }

    [HttpPost]
    public ActionResult Edit(int id, CategoryEditModel model)
    {
        if (id != model.Id)
        {
            return RedirectToAction("Index");
        }

        if (ModelState.IsValid)
        {
            var category = _context.Categories.FirstOrDefault(i => i.Id == id);
            if (category != null)
            {
                category.Name = model.Name;
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"{category.Name} Category updated successfully.";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        return View(model);
    }

    public async Task<ActionResult> Details(int id,int page=1)
    {
        int pageSize = 8;
        var category = await _context.Categories
        .Include(c => c.Questions)
        .Include(c => c.Quizzes)
        .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return RedirectToAction("Index");
        }

        //Paginate questions
        var paginatedQuestions = category.Questions
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

        ViewBag.TotalPages = (int)Math.Ceiling((double)category.Questions.Count / pageSize);
        ViewBag.CurrentPage = page;
        ViewBag.PaginatedQuestions = paginatedQuestions;

        return View(category);
    }



}