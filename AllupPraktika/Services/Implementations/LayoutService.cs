﻿using AllupPraktika.DAL;
using AllupPraktika.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AllupPraktika.Services.Implementations
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;

        public LayoutService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.LayoutSettings.ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }


    }
}
