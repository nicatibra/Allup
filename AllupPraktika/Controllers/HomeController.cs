using AllupPraktika.DAL;
using AllupPraktika.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllupPraktika.Controllers
{
    public class HomeController : Controller
    {
        public readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            HomeVM homeVM = new HomeVM
            {
                Slides = await _context.Slides
                .Where(p => p.IsDeleted == false)//gelecekde heresine aid bele tek tek IsDeleted False serti qoyulmayacaq(silinecek)
                .OrderBy(s => s.Order)
                .Take(3)
                .ToListAsync(),

                NewProducts = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToListAsync()
            };

            return View(homeVM);
        }
    }
}
