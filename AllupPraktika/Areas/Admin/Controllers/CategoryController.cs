using AllupPraktika.Areas.Admin.ViewModels;
using AllupPraktika.DAL;
using AllupPraktika.Models;
using AllupPraktika.Utilities.Enums;
using AllupPraktika.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllupPraktika.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin,Moderator")]
    [AutoValidateAntiforgeryToken]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<GetCategoryAdminVM> categoryVMs = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .Include(c => c.Products)
                .Select(c => new GetCategoryAdminVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products.Count,
                    Image = c.Image
                })
                .ToListAsync();

            return View(categoryVMs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (!categoryVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "File type must be image!");
                return View();
            }
            if (!categoryVM.Photo.ValidateSize(Utilities.Enums.FileSize.MB, 5))
            {
                ModelState.AddModelError("Photo", "File size must be less than 5 mb");
                return View();
            }

            bool result = await _context.Categories.AnyAsync(c => c.Name.Trim() == categoryVM.Name.Trim()); //Any() avtomatik ToLower edir

            if (result)
            {
                ModelState.AddModelError("Name", "Category already exists!");
                return View();
            }

            Category category = new()
            {
                Name = categoryVM.Name,
                Image = await categoryVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            UpdateCategoryVM categoryVM = new()
            {
                Name = category.Name,
                Image = category.Image
            };

            return View(categoryVM);
        }



        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateCategoryVM categoryVM)
        {
            if (id == null || id < 1) { return BadRequest(); }

            if (!ModelState.IsValid)
            {
                return View(categoryVM);
            }

            Category existedCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (existedCategory == null)
            {
                return NotFound();
            }

            if (categoryVM.Photo is not null)
            {
                if (!categoryVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateCategoryVM.Photo), "File type is incorrect");
                    return View(categoryVM);
                }

                if (!categoryVM.Photo.ValidateSize(FileSize.MB, 5))
                {
                    ModelState.AddModelError(nameof(UpdateCategoryVM.Photo), "File size is incorrect");
                    return View(categoryVM);
                }

                string filename = await categoryVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images");

                existedCategory.Image.DeleteFile(_env.WebRootPath, "assets", "images");
                existedCategory.Image = filename;
            }


            bool result = await _context.Categories.AnyAsync(c => c.Name.Trim() == categoryVM.Name.Trim() && c.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateCategoryVM.Name), "Category adlready exists!");
                return View(categoryVM);
            }

            existedCategory.Name = categoryVM.Name;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) { return NotFound(); }

            category.Image.DeleteFile(_env.WebRootPath, "assets", "images");


            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
