using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebAPICoreDapper.Filters;
using Microsoft.Extensions.Logging;
using WebAPICoreDapper.Extensions;
using System.Globalization;
using Microsoft.Extensions.Localization;
using WebAPICoreDapper.Resources;
using WebAPICoreDapper.Data.Models;
using WebAPICoreDapper.Data.Repositories;
using WebAPICoreDapper.Utilities.Dtos;

namespace WebAPICoreDapper.Controllers
{
    [Route("api/{culture}/[controller]")]
    [ApiController]
    [MiddlewareFilter(typeof(LocalizationPipeline))]
    public class ProductController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<ProductController> _logger;
        private readonly IStringLocalizer<ProductController> _localizer;
        private readonly LocService _locService;
        private readonly ProductRepository _productRepository;
        public ProductController(IConfiguration configuration, ILogger<ProductController> logger,
        IStringLocalizer<ProductController> localizer, LocService locService, ProductRepository productRepository)
        {
            _connectionString = configuration.GetConnectionString("DbConnectionString");
            _logger = logger;
            _localizer = localizer;
            _locService = locService;
            _productRepository = new ProductRepository(configuration);
        }
        // GET: api/Product
        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            // var culture = CultureInfo.CurrentCulture.Name;
            // string text = _localizer["test"];
            // string text1 = _locService.GetLocalizedHtmlString("ForgotPassword");
            return await _productRepository.GetAllAsync(CultureInfo.CurrentCulture.Name);
          
        }

        // GET: api/Product/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<Product> Get(int id)
        {
            return await _productRepository.GetByIdAsync(id,CultureInfo.CurrentCulture.Name);
        }

        [HttpGet("paging", Name = "GetPaging")]
        public async Task<PagedResult<Product>> GetPaging(string keyword, int categoryId, int pageIndex, int pageSize)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                return await _productRepository.GetPaging(CultureInfo.CurrentCulture.Name, keyword, categoryId, pageIndex, pageSize);
            }
        }
        // POST: api/Product
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            var newId = await _productRepository.Create(CultureInfo.CurrentCulture.Name, product);
            return Ok(newId);

        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            await _productRepository.Update(CultureInfo.CurrentCulture.Name, id, product);
            return Ok();
        }
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productRepository.Delete(id);
            return Ok();
        }
    }
}
