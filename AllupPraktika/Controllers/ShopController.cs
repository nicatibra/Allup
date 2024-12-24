using AllupPraktika.DAL;
using AllupPraktika.Models;
using AllupPraktika.Utilities.Enums;
using AllupPraktika.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllupPraktika.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int key = 1, int page = 1)
        {
            IQueryable<Product> query = _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null)).
                Include(p => p.Category).
                Include(p => p.Brand).
                Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null));


            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            switch (key)
            {
                case (int)ESortType.Name:
                    query = query.OrderBy(p => p.Name);
                    break;

                case (int)ESortType.Price:
                    query = query.OrderByDescending(p => p.DiscountPrice);
                    break;

                case (int)ESortType.Date:
                    query = query.OrderBy(p => p.CreatedAt);
                    break;

                default:
                    break;
            }

            int count = query.Count();
            double totalPage = Math.Ceiling((double)count / 3);

            query = query.Skip((page - 1) * 3).Take(3);

            ShopVM shopVM = new ShopVM
            {
                Products = await query.Select(p => new GetProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                    SecondaryImage = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false).Image,
                    Price = p.Price,
                    DiscountPercentage = p.DiscountPercentage,
                    DiscountPrice = p.DiscountPrice,
                    Description = p.Description,
                }).ToListAsync(),


                Search = search,
                Key = key,
                TotalPage = totalPage,
                CurrentPage = page
            };


            return View(shopVM);
        }

        public async Task<IActionResult> Detail(int? id)
        {

            if (id == null || id < 1) { return BadRequest(); }

            Product? product = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) { return NotFound(); }

            DetailVM detailVM = new()
            {
                Product = product,

                RelatedProducts = await _context.Products
                .Take(8)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p => p.ProductImages)
                .ToListAsync()
            };

            return View(detailVM);
        }
    }
}
