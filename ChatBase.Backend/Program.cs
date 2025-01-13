
using ChatBase.Backend.Data.Identity;
using ChatBase.Backend.Data.Profile;
using ChatBase.Backend.Domain.Identity;
using ChatBase.Backend.Helper.OpenApiUI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Scalar.AspNetCore;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ChatBase.Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);
            string defualtConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            #region identity
            builder.Services.AddDbContext<IdentityDbContext>(options =>
              options.UseSqlServer(defualtConnectionString)
              );

            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(
                 options =>
                 {
                     options.SignIn.RequireConfirmedAccount = true;
                 }
             )
             .AddEntityFrameworkStores<IdentityDbContext>()
             .AddUserManager<ApplicationUserManager>()
             .AddDefaultTokenProviders();
            // .AddApiEndpoints();

            builder.Services.AddControllers();

            builder.Services.AddAuthentication(
                 options =>
                 {
                     options.DefaultScheme = "JWT_OR_COOKIE";
                     options.DefaultChallengeScheme = "JWT_OR_COOKIE";
                     options.DefaultAuthenticateScheme = "JWT_OR_COOKIE";
                 }
             ).AddCookie(options =>
             {
                 options.LoginPath = "/identity/account/login";
                 options.LogoutPath = "/logout";
             })
             // Adding Jwt Bearer
             .AddJwtBearer(options =>
             {
                 options.SaveToken = true;
                 options.RequireHttpsMetadata = false;
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateIssuer = true,
                     ValidateAudience = false,

                     ValidAudience = builder.Configuration["JWT:ValidAudience"],
                     ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                 };
             })// this is the key piece to have both cookie and jwt login working togethers!
             .AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
             {
                 // runs on each request
                 options.ForwardDefaultSelector = context =>
                 {
                     // filter by auth type
                     string authorization = context.Request.Headers[HeaderNames.Authorization];
                     if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                         return JwtBearerDefaults.AuthenticationScheme;

                     // otherwise always check for cookie auth
                     return IdentityConstants.ApplicationScheme;
                 };
             });


            #endregion identity

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

            builder.Services.AddDbContext<ProfileDbContext>(
             options => options.UseSqlServer(defualtConnectionString)
             );

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<IdentityDbContext>();
                    await context.Database.MigrateAsync();

                    var profileContext = services.GetRequiredService<ProfileDbContext>();
                    await profileContext.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options
                    .WithPreferredScheme(IdentityConstants.ApplicationScheme);
                });
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
