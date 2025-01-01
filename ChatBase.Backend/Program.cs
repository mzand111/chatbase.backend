
using ChatBase.Backend.Data.Identity;
using ChatBase.Backend.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace ChatBase.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            string defualtConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            // Add services to the container.
            #region identity
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(defualtConnectionString)
                );



            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                }
                )
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<ApplicationUserManager>()
                .AddDefaultTokenProviders()
                .AddApiEndpoints();
            //builder.Services.AddIdentityApiEndpoints<ApplicationUser>();
            //builder.Services.Addapi<ApplicationUser>();
            #endregion identity
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();
            app.MapIdentityApi<ApplicationUser>();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
