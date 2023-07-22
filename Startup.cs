using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SourceforqualityAPI.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//using SourceforqualityAPI.Content;
//using SourceforqualityAPI.Contracts;
using Microsoft.AspNetCore.Authentication;
//using SourceforqualityAPI.Entity;
using SourceforqualityAPI.Interfaces;
using Microsoft.AspNetCore.Builder;
using SourceforqualityAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using UserModuleApi.Filters;
using System.IO;

namespace SourceforqualityAPI
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Enable JWT authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "admin",
                    ValidAudience = "user",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySecret4839745dsklf2023255jsdjf2345lk")),
                    ClockSkew = TimeSpan.Zero // No tolerance for the token expiration time
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Program>>();
            services.AddSingleton(typeof(ILogger), logger);

            //services.AddAuthentication().AddFacebook(facebookOptions =>
            //{
            //    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
            //    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            //});
            // Configure Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SourceForQuality API", Version = "v1" });
                //c.CustomSchemaIds(type => type.ToString());
                // Define the Bearer token authentication scheme
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                // Add the security requirement to each operation in Swagger UI
                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                };

                c.AddSecurityDefinition("Bearer", securityScheme);
                c.AddSecurityRequirement(securityRequirement);
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                policy.Requirements.Add(new RoleRequirement("Admin")));
                options.AddPolicy("RequireEndUserRole", policy =>
                policy.Requirements.Add(new RoleRequirement("EndUser")));
                options.AddPolicy("RequireFoodSupplierRole", policy =>
                policy.Requirements.Add(new RoleRequirement("FoodSupplier")));
                options.AddPolicy("RequireAnyRole", policy =>
                policy.RequireRole("Admin", "EndUser", "FoodSupplier"));
                options.AddPolicy("RequireFoodSupplierAdmin", policy =>
                policy.RequireRole("FoodSupplier", "Admin"));
                options.AddPolicy("RequireEndUserAdmin", policy =>
                policy.RequireRole("EndUser", "Admin"));
                options.AddPolicy("RequireEndUserFoodSupplier", policy =>
                policy.RequireRole("EndUser", "FoodSupplier"));
            });

            services.AddSingleton<IAuthorizationHandler, RoleHandler>();

            services.AddControllers();
            Global.ConnectionString = Configuration.GetConnectionString("SqlConnection");

            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IContactService, ContactServices>();
            services.AddTransient<IUserManagementServices, UserManagementServices>();
            services.AddTransient<IFileUpload, FileUploadService>();
            services.AddTransient<IFileFormatConverter, FileFormatConverter>();
            services.AddTransient<ISupplierProfileServices, SupplierProfileServices>();
            services.AddTransient<IDropdown, DropdownService>();
            services.AddTransient<ISupplierSearchService, SupplierSearchService>();
            services.AddTransient<IChangePasswordServices, ChangePasswordServices>();
            services.AddTransient<IUpdateRoleOnSubscriptionServices, UpdateRoleOnSubscriptionServices>();
            services.AddTransient<IFAQManagementServices, FAQManagementServices>();
            services.AddTransient<ISubscriptionManagementServices, SubscriptionManagementServices>();
            services.AddTransient<IUserFavouriteSupplierServices, UserFavouriteSupplierServices>();
            services.AddTransient<IAdminConfirmationToSuppliersServices, AdminConfirmationToSuppliersServices>();


        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var path = Directory.GetCurrentDirectory();
            loggerFactory.AddFile($"{path}\\Logs\\Log.txt");
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c =>
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi V1"));
            //}
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
            });

            Global.LoadSmtpSettings();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });



            //private readonly List<string> Hub_Url = null;
            //public Startup(IConfiguration configuration)
            //{
            //    Configuration = configuration;
            //    string allowedCorsHosts = Configuration.GetValue<string>("AllowedCorsHosts");
            //    if (!string.IsNullOrEmpty(allowedCorsHosts))
            //    {
            //        Hub_Url = new List<string>(allowedCorsHosts.Split(','));
            //    }
            //    else
            //    {
            //        Hub_Url = new List<string>();
            //    }
            //}

            //public IConfiguration Configuration { get; }

            //// This method gets called by the runtime. Use this method to add services to the container.
            //public void ConfigureServices(IServiceCollection services)
            //{

            //    services.AddControllers();
            //    //services.AddSingleton<DapperContext>();
            //    //services.AddScoped<IUserLoginRepo, UserLoginRepo>();
            //    //services.AddScoped<IPasswordHasher<UserLogin>, PasswordHasher<UserLogin>>();
            //    services.AddTransient<IAccountService, AccountService>();
            //    services.AddSwaggerGen(c =>
            //        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SourceforqualityAPI", Description = "SourceforqualityAPI", Version = "v1" }));

            //    services.AddCors(options =>
            //    {
            //        options.AddPolicy("CorsPolicy", builder => builder
            //            .WithOrigins(Hub_Url.ToArray())
            //            .AllowAnyMethod()
            //            .AllowAnyHeader()
            //            .AllowCredentials()
            //            .WithMethods("PUT", "DELETE", "GET"));
            //    });
            //}

            //// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            //public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            //{
            //    //if (env.IsDevelopment())
            //    //{
            //    //    app.UseDeveloperExceptionPage();
            //    //}

            //    app.UseSwagger();
            //    app.UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SourceForQuality API V1");
            //    });

            //    app.UseHttpsRedirection();

            //    app.UseRouting();
            //    app.UseCors("CorsPolicy");
            //    app.UseAuthorization();
            //    app.UseEndpoints(endpoints =>
            //    {
            //        endpoints.MapControllers();
            //    });


            //}
        }
    }
}