
using ChatBase.Backend.Data.Chat;
using ChatBase.Backend.Data.Identity;
using ChatBase.Backend.Data.Profile;
using ChatBase.Backend.Domain.Identity;
using ChatBase.Backend.Helper.OpenApiUI;
using ChatBase.Backend.Infrastructure.Profile;
using ChatBase.Backend.Lib;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace ChatBase.Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);
            string defualtConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");


            #region swagger
            builder.Services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Zelvor.Services", Version = "v1" });
                    c.AddEnumsWithValuesFixFilters(o =>
                    {
                        // add schema filter to fix enums (add 'x-enumNames' for NSwag or its alias from XEnumNamesAlias) in schema
                        o.ApplySchemaFilter = true;

                        // alias for replacing 'x-enumNames' in swagger document
                        o.XEnumNamesAlias = "x-enum-varnames";

                        // alias for replacing 'x-enumDescriptions' in swagger document
                        o.XEnumDescriptionsAlias = "x-enum-descriptions";

                        // add parameter filter to fix enums (add 'x-enumNames' for NSwag or its alias from XEnumNamesAlias) in schema parameters
                        o.ApplyParameterFilter = true;

                        // add document filter to fix enums displaying in swagger document
                        o.ApplyDocumentFilter = true;

                        // add descriptions from DescriptionAttribute or xml-comments to fix enums (add 'x-enumDescriptions' or its alias from XEnumDescriptionsAlias for schema extensions) for applied filters
                        o.IncludeDescriptions = true;

                        // add remarks for descriptions from xml-comments
                        o.IncludeXEnumRemarks = true;

                        // get descriptions from DescriptionAttribute then from xml-comments
                        o.DescriptionSource = DescriptionSources.DescriptionAttributesThenXmlComments;

                        // new line for enum values descriptions
                        // o.NewLine = Environment.NewLine;
                        o.NewLine = "\n";


                    });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = @"Enter 'Bearer' [space] and your token",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    },
                    Scheme="oauth2",
                    Name="Bearer",
                    In=ParameterLocation.Header
                },
                new List<string>()
            }

                    });
                    try
                    {
                        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                        c.IncludeXmlComments(xmlPath);

                        c.AddSignalRSwaggerGen(_ =>
                        {
                            _.UseHubXmlCommentsSummaryAsTagDescription = true;
                            _.UseHubXmlCommentsSummaryAsTag = true;
                            _.UseXmlComments(xmlPath);
                        });

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            #endregion

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

            builder.Services.AddTransient<IProfileRepository, ProfileRepository>();
            builder.Services.AddDbContext<ChatDataContext>(
                options => options.UseSqlServer(defualtConnectionString)
            );
            builder.Services.AddBusinessServices();
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
                 options.LoginPath = PathString.Empty;
                 options.LogoutPath = PathString.Empty;
                 options.Events = new CookieAuthenticationEvents
                 {
                     OnRedirectToLogin = context =>
                     {
                         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                         return Task.CompletedTask;
                     }
                 };
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

            #region Chat

            builder.Services.AddSingleton<HubExceptionFilter>();
            builder.Services.AddScoped<PresenceTracker>();
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.AddFilter<HubExceptionFilter>();
            });
            #endregion

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.AllowAnyOrigin();
                                      policy.AllowAnyMethod();
                                      policy.AllowAnyHeader();
                                  });
            });

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

                    var chatContext = services.GetRequiredService<ChatDataContext>();
                    await chatContext.Database.MigrateAsync();
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

            }
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.EnableDeepLinking();
                options.InjectJavascript("/js/swaggerUIModelLinkSupport.js");
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
