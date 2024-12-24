using AllupPraktika.Areas.Admin.ViewModels;
using AllupPraktika.DAL;
using AllupPraktika.Models;
using AllupPraktika.Utilities.Enums;
using AllupPraktika.Utilities.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllupPraktika.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    [Authorize(Roles = "Admin,Moderator")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1)

        {
            if (page < 1) return BadRequest();

            int count = await _context.Products.CountAsync();

            double total = Math.Ceiling((double)count / 2);

            if (page > total) return BadRequest();


            List<GetProductAdminVM> productsVMs = await _context.Products
                .Skip((page - 1) * 2)
                .Take(2)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Select(p => new GetProductAdminVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    DiscountPercentage = p.DiscountPercentage,
                    CategoryName = p.Category.Name,
                    Image = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image

                }).ToListAsync();

            PaginatedVM<GetProductAdminVM> paginatedVM = new()
            {
                TotalPage = total,
                CurrentPage = page,
                Items = productsVMs
            };

            return View(paginatedVM);
        }

        public async Task<IActionResult> Create()
        {

            CreateProductVM productVM = new CreateProductVM()
            {
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Brands = await _context.Brands.ToListAsync()
            };
            return View(productVM);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Brands = await _context.Brands.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(productVM.MainPhoto), "File format is not image");
                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError(nameof(productVM.MainPhoto), "File size must be less than 2 MB");
                return View(productVM);
            }


            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError("HoverPhoto", "File fornat is not image");
                return View(productVM);
            }

            if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 2))
            {
                ModelState.AddModelError("HoverPhoto", "File size must be less than 2 MB");
                return View(productVM);
            }

            bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateProductVM.CategoryId), "Category does not exist");
                return View(productVM);
            }

            if (productVM.TagIds is not null)
            {
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));

                if (tagResult)
                {
                    ModelState.AddModelError(nameof(CreateProductVM.TagIds), "Tags are wrong");
                    return View();
                }
            }

            ProductImage mainImage = new()
            {
                Image = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                IsPrimary = true,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            ProductImage hoverImage = new()
            {
                Image = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                IsPrimary = false,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            Product product = new()
            {
                Name = productVM.Name,
                Description = productVM.Description,
                ProductCode = productVM.ProductCode,
                Price = productVM.Price.Value,
                DiscountPercentage = productVM.DiscountPercentage,
                DiscountPrice = productVM.Price.Value - (productVM.DiscountPercentage * productVM.Price.Value / 100),
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                Availablity = true,
                CategoryId = productVM.CategoryId,
                BrandId = productVM.BrandId,
                ProductImages = new List<ProductImage>
                {
                    mainImage, hoverImage
                }
            };

            if (productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(tId => new ProductTag { TagId = tId }).ToList();
            }

            if (productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;

                foreach (IFormFile file in productVM.AdditionalPhotos)
                {

                    if (!file.ValidateType("image/"))
                    {
                        text +=
                            $"<p class=\"text-danger\">Type of {file.FileName} must be image!</p>";
                        continue;
                    }

                    if (!file.ValidateSize(FileSize.MB, 2))
                    {
                        text += $"<p class=\"text-danger\">Size of {file.FileName} must be less than 2 MB!</p>";
                        continue;
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null
                    });
                }
                TempData["FileWarning"] = text;
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            UpdateProductVM productVM = new()
            {
                Name = product.Name,
                ProductCode = product.ProductCode,
                Description = product.Description,
                Price = product.Price,
                DiscountPercentage = product.DiscountPercentage,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList(),
                ProductImages = product.ProductImages,

                Categories = await _context.Categories.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                Tags = await _context.Tags.ToListAsync()

            };

            return View(productVM);
        }


        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Product existed = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existed is null) return NotFound();

            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Brands = await _context.Brands.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.ProductImages = await _context.ProductImages.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(productVM);
            }




            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("MainPhoto", "File type is incorrect");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError("MainPhoto", "File type is incorrect");
                    return View(productVM);
                }
            }

            if (productVM.HoverPhoto is not null)
            {
                if (!productVM.HoverPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError("HoverPhoto", "File type is incorrect");
                    return View(productVM);
                }
                if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 1))
                {
                    ModelState.AddModelError("HoverPhoto", "File type is incorrect");
                    return View(productVM);
                }
            }




            if (existed.CategoryId != productVM.CategoryId)
            {
                bool result = productVM.Categories.Any(c => c.Id == productVM.CategoryId);
                if (!result)
                {
                    return View(productVM);
                }
            }

            if (existed.BrandId != productVM.BrandId)
            {
                bool result = productVM.Brands.Any(b => b.Id == productVM.BrandId);
                if (!result)
                {
                    return View(productVM);
                }
            }

            if (productVM.TagIds is not null)
            {
                bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));

                if (tagResult)
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.TagIds), "Tags are wrong");

                    return View(productVM);
                }
            }

            if (productVM.TagIds is null)
            {
                productVM.TagIds = new();
            }
            else
            {
                productVM.TagIds = productVM.TagIds.Distinct().ToList();
            }
            _context.ProductTags.RemoveRange(existed.ProductTags
            .Where(pTag => !productVM.TagIds.Exists(tId => tId == pTag.TagId))
            .ToList());

            _context.ProductTags.AddRange(productVM.TagIds
           .Where(tId => !existed.ProductTags.Exists(pTag => pTag.TagId == tId))
           .ToList()
           .Select(tId => new ProductTag { TagId = tId, ProductId = existed.Id }));




            if (productVM.MainPhoto is not null)
            {
                string fileName = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images");

                ProductImage main = existed.ProductImages.FirstOrDefault(p => p.IsPrimary == true);
                main.Image.DeleteFile(_env.WebRootPath, "assets", "images");
                existed.ProductImages.Remove(main);
                existed.ProductImages.Add(new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = true,
                    Image = fileName
                });
            }

            if (productVM.HoverPhoto is not null)
            {
                string fileName = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images");

                ProductImage hover = existed.ProductImages.FirstOrDefault(p => p.IsPrimary == false);
                hover.Image.DeleteFile(_env.WebRootPath, "assets", "images");
                existed.ProductImages.Remove(hover);
                existed.ProductImages.Add(new ProductImage
                {
                    CreatedAt = DateTime.Now,
                    IsDeleted = false,
                    IsPrimary = false,
                    Image = fileName
                });
            }

            if (productVM.ImageIds is null)
            {
                productVM.ImageIds = new List<int>();
            }
            var deletedImages = existed.ProductImages.Where(pi => !productVM.ImageIds.Exists(imgId => imgId == pi.Id) && pi.IsPrimary == null).ToList();

            deletedImages.ForEach(di => di.Image.DeleteFile(_env.WebRootPath, "assets", "images"));


            _context.ProductImages.RemoveRange(deletedImages);


            if (productVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} type was not correct</p>";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        text += $"<p class=\"text-warning\">{file.FileName} size was not correct</p>";
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null,
                    });
                }

                TempData["FileWarning"] = text;
            }

            existed.ProductCode = productVM.ProductCode;
            existed.Price = productVM.Price.Value;
            existed.DiscountPercentage = productVM.DiscountPercentage;
            existed.Description = productVM.Description;
            existed.Name = productVM.Name;
            existed.CategoryId = productVM.CategoryId.Value;
            existed.BrandId = productVM.BrandId.Value;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            foreach (var image in product.ProductImages)
            {
                image.Image.DeleteFile(_env.WebRootPath, "assets", "images");
            }

            _context.ProductTags.RemoveRange(product.ProductTags);

            _context.ProductImages.RemoveRange(product.ProductImages);

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
