﻿using AllupPraktika.Areas.Admin.ViewModels;
using AllupPraktika.DAL;
using AllupPraktika.Models;
using AllupPraktika.Utilities.Enums;
using AllupPraktika.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AllupPraktika.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    //[Authorize(Roles = "Admin,Moderator")]

    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;


        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.ToListAsync();
            return View(slides);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM slideVM)
        {
            if (!ModelState.IsValid) { return View(); };

            if (!slideVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "File type must be image!");
                return View();
            }
            if (!slideVM.Photo.ValidateSize(Utilities.Enums.FileSize.MB, 5))
            {
                ModelState.AddModelError("Photo", "File size must be less than 5 mb");
                return View();
            }

            Slide slide = new Slide
            {
                Title = slideVM.Title,
                Subtitle = slideVM.Subtitle,
                Description = slideVM.Description,
                Order = slideVM.Order,
                Image = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images"),
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };


            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));


        }


        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (slide == null) { return NotFound(); }

            UpdateSlideVM slideVM = new UpdateSlideVM
            {
                Title = slide.Title,
                Subtitle = slide.Subtitle,
                Description = slide.Description,
                Order = slide.Order,
                Image = slide.Image
            };

            return View(slideVM);
        }


        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateSlideVM slideVM)
        {
            if (id == null || id < 1) { return BadRequest(); }

            if (!ModelState.IsValid)
            {
                return View(slideVM);
            }

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (existed == null) { return NotFound(); }

            if (slideVM.Photo is not null)
            {
                if (!slideVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File type is incorrect");
                    return View(slideVM);
                }

                if (!slideVM.Photo.ValidateSize(FileSize.MB, 5))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File size is incorrect");
                    return View(slideVM);
                }

                string filename = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images");

                existed.Image.DeleteFile(_env.WebRootPath, "assets", "images");
                existed.Image = filename;
            }

            existed.Title = slideVM.Title;
            existed.Description = slideVM.Description;
            existed.Subtitle = slideVM.Subtitle;
            existed.Order = slideVM.Order;


            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id < 1) { return BadRequest(); }

            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (slide == null) { return NotFound(); }

            slide.Image.DeleteFile(_env.WebRootPath, "assets", "images");



            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
