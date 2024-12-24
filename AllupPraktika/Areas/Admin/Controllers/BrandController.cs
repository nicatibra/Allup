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
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;

        public BrandController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<GetBrandVM> brandVMs = await _context.Brands
                .Where(b => b.IsDeleted == false)
                .Include(b => b.Products)
                .Select(b => new GetBrandVM
                {
                    Id = b.Id,
                    Name = b.Name,
                    ProductCount = b.Products.Count
                })
                .ToListAsync();

            return View(brandVMs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateBrandVM brandVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.Brands.AnyAsync(b => b.Name.Trim() == brandVM.Name.Trim()); //Any() avtomatik ToLower edir

            if (result)
            {
                ModelState.AddModelError("Name", "Brand already exists!");
                return View();
            }

            Brand brand = new()
            {
                Name = brandVM.Name,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Brand brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null)
            {
                return NotFound();
            }

            UpdateBrandVM brandVM = new()
            {
                Name = brand.Name
            };

            return View(brandVM);
        }



        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateBrandVM brandVM)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Brand existedBrand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);

            if (existedBrand == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(brandVM);
            }

            bool result = await _context.Brands.AnyAsync(b => b.Name.Trim() == brandVM.Name.Trim() && b.Id != id);
            if (result)
            {
                ModelState.AddModelError(nameof(UpdateBrandVM.Name), "Brand adlready exists!");
                return View(brandVM);
            }

            existedBrand.Name = brandVM.Name;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Brand brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null) { return NotFound(); }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
