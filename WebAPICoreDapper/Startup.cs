using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebAPICoreDapper.Data;
using WebAPICoreDapper.Data.Models;
using WebAPICoreDapper.Resources;

namespace WebAPICoreDapper
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
            services.AddTransient<IUserStore<AppUser>, UserStore>();
            services.AddTransient<IRoleStore<AppRole>, RoleStore>();

            services.AddIdentity<AppUser, AppRole>()
            .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(opt =>
            {
                // Default Password settings.
                opt.Password.RequireDigit = true;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequiredLength = 6;
                opt.Password.RequiredUniqueChars = 1;
            });
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("vi-VN")
            };
            var options = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(culture: "vi-VN", uiCulture: "vi-VN"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            options.RequestCultureProviders = new[]
            {
                new RouteDataRequestCultureProvider() { Options = options}
            };
            services.AddSingleton(options);
            services.AddSingleton<LocService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    {
                        var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
                        return factory.Create("SharedResource", assemblyName.Name);
                    };
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "E-commerce API",
                    Description = "A simple e-commerce ASP.NET Core Web API",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Bui Tan Lan",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/BuiTanLan"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        //Url = new Uri("https://example.com/license"),
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile(Configuration.GetSection("Logging"));
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                 {
                     context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                     var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                     if (ex == null) return;

                     var error = new
                     {
                         message = ex.Message
                     };

                     context.Response.ContentType = "application/json";
                     context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
                     context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { Configuration["AllowedHosts"] });

                     using (var writer = new StreamWriter(context.Response.Body))
                     {
                         new JsonSerializer().Serialize(writer, error);
                         await writer.FlushAsync().ConfigureAwait(false);
                     }
                 });
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((document, request) =>
                {
                    var paths = document.Paths.ToDictionary(item => item.Key.ToLowerInvariant(), item => item.Value);
                    document.Paths.Clear();
                    foreach (var pathItem in paths)
                    {
                        document.Paths.Add(pathItem.Key, pathItem.Value);
                    }
                });
            });
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

