using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebAPICoreDapper.Data;
using WebAPICoreDapper.Data.Models;
using WebAPICoreDapper.Data.Repositories;
using WebAPICoreDapper.Data.Repositories.Interfaces;
using WebAPICoreDapper.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IUserStore<AppUser>, UserStore>();
builder.Services.AddTransient<IRoleStore<AppRole>, RoleStore>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();

builder.Services.AddIdentity<AppUser, AppRole>()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(opt =>
{
    // Default Password settings.
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 6;
    opt.Password.RequiredUniqueChars = 1;
});
builder.Services.AddControllers();
//     .AddJsonOptions(options =>

// {
//     // Use the default property (Pascal) casing.
//     options.JsonSerializerOptions.PropertyNamingPolicy = null;
// });
// // .AddNewtonsoftJson(options =>
// // {
// //     options.SerializerSettings.ContractResolver = new DefaultContractResolver();
// // });
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
    new RouteDataRequestCultureProvider() { Options = options }
};
builder.Services.AddSingleton(options);
builder.Services.AddSingleton<LocService>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

//Add authentication to get claims
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Token:Issuer"],
        ValidAudience = builder.Configuration["Token:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:Key"]))
    };
});

builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
        {
            var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
            return factory.Create("SharedResource", assemblyName.Name);
        };
    });
builder.Services.AddSwaggerGen(c =>
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
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
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
var app = builder.Build();
var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
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
        context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { builder.Configuration["AllowedHosts"] });
        // using Newton.Json
        // using (var writer = new StreamWriter(context.Response.Body))
        // {
        //     new JsonSerializer().Serialize(writer, error);
        //     await writer.FlushAsync().ConfigureAwait(false);
        // }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var json = JsonSerializer.Serialize(error, options);

        await context.Response.WriteAsync(json);
    });
});
if (app.Environment.IsDevelopment())
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
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.Run();
