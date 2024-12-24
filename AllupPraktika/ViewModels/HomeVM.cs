﻿using AllupPraktika.Models;

namespace AllupPraktika.ViewModels
{
    public class HomeVM
    {
        public List<Slide> Slides { get; set; }

        public List<Product> Products { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public List<Product> NewProducts { get; set; }


        public List<Category> Categories { get; set; }


    }
}
