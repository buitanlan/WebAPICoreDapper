using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebAPICoreDapper.Data.Repositories.Interfaces;
using WebAPICoreDapper.Data.ViewModels;
using WebAPICoreDapper.Extensions;
using WebAPICoreDapper.Filters;

namespace WebAPICoreDapper.Controllers;

[Route("api/{culture}/[controller]")]
[ApiController]
[MiddlewareFilter(typeof(LocalizationPipeline))]
public class AttributeController(IAttributeRepository attributeRepository) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<AttributeViewModel> Get(int id)
    {
        return await attributeRepository.GetById(id, CultureInfo.CurrentCulture.Name);
    }

    [HttpGet]
    public async Task<List<AttributeViewModel>> GetAll()
    {
        return await attributeRepository.GetAll(CultureInfo.CurrentCulture.Name);
    }

    [HttpPost]
    [ValidateModel]
    public async Task AddAttribute([FromBody] AttributeViewModel attribute)
    {
        await attributeRepository.Add(CultureInfo.CurrentCulture.Name, attribute);
    }

    [HttpPut("{id}")]
    [ValidateModel]

    public async Task Update(int id, [FromBody] AttributeViewModel attribute)
    {
        await attributeRepository.Update(id, CultureInfo.CurrentCulture.Name, attribute);
    }

    [HttpDelete("{id}")]
    public async Task Delete(int id)
    {
        await attributeRepository.Delete(id);
    }
}
