using AllupPraktika.Areas.Admin.ViewModels;
using AllupPraktika.DAL;
using AllupPraktika.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllupPraktika.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    //[Authorize(Roles = "Admin,Moderator")]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<GetTagVM> tagVMs = await _context.Tags
                .Where(t => t.IsDeleted == false)
                .Include(t => t.ProductTags)
                .Select(t => new GetTagVM
                {
                    Id = t.Id,
                    Name = t.Name,
                    ProductCount = t.ProductTags.Count()
                })
                .ToListAsync();

            return View(tagVMs);
        }

        public IActionResult Create(int id)
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM tagVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.Tags.AnyAsync(t => t.Name.Trim() == tagVM.Name.Trim());
            if (result == true)
            {
                ModelState.AddModelError("Name", $"{tagVM.Name} already exist");
                return View();
            }


            Tag tag = new()
            {
                Name = tagVM.Name,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }



        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }

            Tag tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            UpdateTagVM tagVM = new()
            {
                Name = tag.Name
            };


            return View(tagVM);
        }


        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateTagVM tagVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            Tag existed = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (existed is null) return NotFound();


            existed.Name = tagVM.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }
            Tag tag = _context.Tags.FirstOrDefault(t => t.Id == id);
            if (tag == null) { return NotFound(); }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
