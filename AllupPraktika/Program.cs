using AllupPraktika.DAL;
using AllupPraktika.Models;
using AllupPraktika.Services.Implementations;
using AllupPraktika.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace AllupPraktika
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //View islemesi ucundur
            builder.Services.AddControllersWithViews();


            //SQL serverine json ile qosulmaq ucun
            builder.Services.AddDbContext<AppDbContext>(option =>
            option.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


            //AppUser ucun
            builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
            {
                opt.Password.RequiredLength = 8; //parolun min uzunlugu
                opt.Password.RequireNonAlphanumeric = false; //herf ve reqemden basqa simvol olmalidirmi

                opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._@";//username-de istifade oluna bilecek simvollar
                opt.User.RequireUniqueEmail = true; //1 mail 1 profil ucun olur

                opt.Lockout.AllowedForNewUsers = true; //sehv giris edenden sora lock ola bilermi
                opt.Lockout.MaxFailedAccessAttempts = 4; //sehv girislerin max sayi
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3); //nece deq sonra yeniden cehd etmek olar

            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            //LayoutService ucun
            builder.Services.AddScoped<ILayoutService, LayoutService>();


            //Basket ucun
            builder.Services.AddScoped<IBasketService, BasketService>();


            builder.Services.AddHttpContextAccessor();

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            //wwwroot ucun
            app.UseStaticFiles();


            //Login ve Register ucun
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "admin",
                pattern: "{area:exists}/{controller=home}/{action=index}/{id?}"
                );


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=home}/{action=index}/{id?}"
                );

            app.Run();
        }
    }
}

