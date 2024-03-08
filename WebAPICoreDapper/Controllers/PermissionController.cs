using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using WebAPICoreDapper.Extensions;
using WebAPICoreDapper.Data.ViewModels;

namespace WebAPICoreDapper.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PermissionController(IConfiguration configuration) : ControllerBase
{
    private readonly string _connectionString = configuration.GetConnectionString("DbConnectionString");

    [HttpGet("function-actions")]
    public async Task<IActionResult> GetAllWithPermission()
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == ConnectionState.Closed)
            await conn.OpenAsync();

        var result = await conn.QueryAsync<FunctionActionViewModel>("Get_Function_WithActions", null, null, null, System.Data.CommandType.StoredProcedure);

        return Ok(result);
    }

    [HttpGet("{role}/role-permissions")]
    public async Task<IActionResult> GetAllRolePermissions(Guid? role)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == ConnectionState.Closed)
            conn.Open();

        var parameters = new DynamicParameters();
        parameters.Add("@roleId", role);

        var result = await conn.QueryAsync<PermissionViewModel>("Get_Permission_ByRoleId", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok(result);
    }

    [HttpPost("{role}/save-permissions")]
    public async Task<IActionResult> SavePermissions(Guid role, [FromBody] List<PermissionViewModel> permissions)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == ConnectionState.Closed)
            conn.Open();

        var dt = new DataTable();
        dt.Columns.Add("RoleId", typeof(Guid));
        dt.Columns.Add("FunctionId", typeof(string));
        dt.Columns.Add("ActionId", typeof(string));
        foreach (var item in permissions)
        {
            dt.Rows.Add(role, item.FunctionId, item.ActionId);
        }
        var parameters = new DynamicParameters();
        parameters.Add("@roleId", role);
        parameters.Add("@permissions", dt.AsTableValuedParameter("dbo.Permission"));
        await conn.ExecuteAsync("Create_Permission", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok();
    }

    [HttpGet("functions-view")]
    public async Task<IActionResult> GetAllFunctionByPermission()
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == ConnectionState.Closed)
            conn.Open();

        var parameters = new DynamicParameters();
        parameters.Add("@userId", User.GetUserId());

        var result = await conn.QueryAsync<FunctionViewModel>("Get_Function_ByPermission", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return Ok(result);
    }
}