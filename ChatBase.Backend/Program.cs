
using ChatBase.Backend.Data.Identity;
using ChatBase.Backend.Domain.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using System;
using System.Security.Claims;

namespace ChatBase.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthentication()
                  .AddBearerToken(IdentityConstants.BearerScheme);

            builder.Services.AddAuthorizationBuilder();
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

            app.MapPost("/identity/logout", async (SignInManager<ApplicationUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Ok("Logged out");
            }).RequireAuthorization();
            app.MapGet("/identity/pingauth", async (ClaimsPrincipal user, ApplicationUserManager userManager) =>
            {
                var email = user.FindFirstValue(ClaimTypes.Email);
                var userDetails = await userManager.FindByEmailAsync(email);

                return Results.Json(new
                {
                    Id = userDetails.Id,
                    FirstName = userDetails.FirstName,
                    LastName = userDetails.LastName,
                    Email = email
                });
            }).RequireAuthorization();
            app.MapGroup("/identity").MapIdentityApi<ApplicationUser>();

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
