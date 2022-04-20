﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using WebAPICoreDapper.Filters;
using WebAPICoreDapper.Utilities.Dtos;
using WebAPICoreDapper.Data.Models;

namespace WebAPICoreDapper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly string _connectionString;

    public RoleController(RoleManager<AppRole> roleManager, IConfiguration configuration)
    {
        _roleManager = roleManager;
        _connectionString = configuration.GetConnectionString("DbConnectionString");
    }

    // GET: api/Role
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            await conn.OpenAsync();

        var parameters = new DynamicParameters();
        var result = await conn.QueryAsync<AppRole>("Get_Role_All", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok(result);
    }

    // GET: api/Role/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        return Ok(await _roleManager.FindByIdAsync(id));
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

        var result = await conn.QueryAsync<AppRole>("Get_Role_AllPaging", parameters, null, null, System.Data.CommandType.StoredProcedure);

        int totalRow = parameters.Get<int>("@totalRow");

        var pagedResult = new PagedResult<AppRole>()
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
    public async Task<IActionResult> Post([FromBody] AppRole role)
    {
        var result = await _roleManager.CreateAsync(role);
        if (result.Succeeded)
            return Ok();
        return BadRequest();
    }

    // PUT: api/Role/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put([Required] Guid id, [FromBody] AppRole role)
    {
        role.Id = id;
        var result = await _roleManager.UpdateAsync(role);
        if (result.Succeeded)
            return Ok();
        return BadRequest();
    }

    // DELETE: api/ApiWithActions/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
            return Ok();
        return BadRequest();
    }
}