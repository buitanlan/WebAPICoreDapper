using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using WebAPICoreDapper.Filters;
using WebAPICoreDapper.Data.Models;
using WebAPICoreDapper.Utilities.Dtos;

namespace WebAPICoreDapper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FunctionController(IConfiguration configuration) : ControllerBase
{
    private readonly string _connectionString = configuration.GetConnectionString("DbConnectionString");

    // GET: api/Role
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            await conn.OpenAsync();

        var parameters = new DynamicParameters();
        var result = await conn.QueryAsync<Function>("Get_Function_All", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok(result);
    }

    // GET: api/Role/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);

        var result = await conn.QueryAsync<Function>("Get_Function_ById", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok(result.Single());
    }

    [HttpGet("paging")]
    public async Task<IActionResult> GetPaging(string keyword, int pageIndex, int pageSize)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            await conn.OpenAsync();

        var parameters = new DynamicParameters();
        parameters.Add("@keyword", keyword);
        parameters.Add("@pageIndex", pageIndex);
        parameters.Add("@pageSize", pageSize);
        parameters.Add("@totalRow", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        var result = await conn.QueryAsync<Function>("Get_Function_AllPaging", parameters, null, null, System.Data.CommandType.StoredProcedure);

        var totalRow = parameters.Get<int>("@totalRow");

        var pagedResult = new PagedResult<Function>()
        {
            Items = result.ToList(),
            TotalRow = totalRow,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
        return Ok(pagedResult);
    }

    // POST: api/Role
    [HttpPost]
    [ValidateModel]
    public async Task<IActionResult> Post([FromBody] Function function)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", function.Id);
        parameters.Add("@name", function.Name);
        parameters.Add("@url", function.Url);
        parameters.Add("@parentId", function.ParentId);
        parameters.Add("@sortOrder", function.SortOrder);
        parameters.Add("@cssClass", function.CssClass);
        parameters.Add("@isActive", function.IsActive);
        await conn.ExecuteAsync("Create_Function", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok();
    }

    // PUT: api/Role/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put([Required] Guid id, [FromBody] Function function)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", function.Id);
        parameters.Add("@name", function.Name);
        parameters.Add("@url", function.Url);
        parameters.Add("@parentId", function.ParentId);
        parameters.Add("@sortOrder", function.SortOrder);
        parameters.Add("@cssClass", function.CssClass);
        parameters.Add("@isActive", function.IsActive);

        await conn.ExecuteAsync("Update_Function", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok();
    }

    // DELETE: api/ApiWithActions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        await conn.ExecuteAsync("Delete_Function_ById", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok();
    }
}