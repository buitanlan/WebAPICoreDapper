using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using WebAPICoreDapper.Filters;
using WebAPICoreDapper.Extensions;
using System.Globalization;
using Microsoft.Extensions.Localization;
using WebAPICoreDapper.Resources;
using WebAPICoreDapper.Data.Models;
using WebAPICoreDapper.Utilities.Dtos;
using WebAPICoreDapper.Data.Repositories.Interfaces;
using WebAPICoreDapper.Data.ViewModels;

namespace WebAPICoreDapper.Controllers;

[Route("api/{culture}/[controller]")]
[ApiController]
[MiddlewareFilter(typeof(LocalizationPipeline))]
public class ProductController(
    IConfiguration configuration,
    ILogger<ProductController> logger,
    IStringLocalizer<ProductController> localizer,
    LocService locService,
    IProductRepository productRepository)
    : ControllerBase
{
    private readonly string _connectionString = configuration.GetConnectionString("DbConnectionString");
    private readonly ILogger<ProductController> _logger = logger;
    private readonly IStringLocalizer<ProductController> _localizer = localizer;
    private readonly LocService _locService = locService;

    // GET: api/Product
    [HttpGet]
    public async Task<IEnumerable<Product>> Get()
    {
        // var culture = CultureInfo.CurrentCulture.Name;
        // string text = _localizer["test"];
        // string text1 = _locService.GetLocalizedHtmlString("ForgotPassword");
        return await productRepository.GetAllAsync(CultureInfo.CurrentCulture.Name);
          
    }

    // GET: api/Product/5
    [HttpGet("{id}", Name = "Get")]
    public async Task<Product> Get(int id)
    {
        return await productRepository.GetByIdAsync(id,CultureInfo.CurrentCulture.Name);
    }

    [HttpGet("paging", Name = "GetPaging")]
    public async Task<PagedResult<Product>> GetPaging(string keyword, int categoryId, int pageIndex, int pageSize)
    {
        await using var conn = new SqlConnection(_connectionString);
        return await productRepository.GetPaging(CultureInfo.CurrentCulture.Name, keyword, categoryId, pageIndex, pageSize);
    }
    // POST: api/Product
    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Post([FromBody] Product product)
    {
        var newId = await productRepository.Create(CultureInfo.CurrentCulture.Name, product);
        return Ok(newId);

    }

    [HttpPut("{id}")]
    [ValidateModel]
    public async Task<IActionResult> Put(int id, [FromBody] Product product)
    {
        await productRepository.Update(CultureInfo.CurrentCulture.Name, id, product);
        return Ok();
    }
    // DELETE: api/ApiWithActions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await productRepository.Delete(id);
        return Ok();
    }
    [HttpGet("{id}/attributes")]
    public async Task<List<ProductAttributeViewModel>> GetProductAttributes(int id)
    {
        return await productRepository.GetAttributes(id, CultureInfo.CurrentCulture.Name);
    }

    [HttpPost("search-attribute")]
    public async Task<PagedResult<Product>> SearchProductByAttributes(string keyword,
        int categoryId, string size, int pageIndex, int pageSize)
    {
        return await productRepository.SearchByAttributes(keyword, CultureInfo.CurrentCulture.Name, categoryId, size, pageIndex, pageSize);
    }
}