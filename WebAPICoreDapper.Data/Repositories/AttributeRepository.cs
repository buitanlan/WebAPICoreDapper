using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebAPICoreDapper.Data.Repositories.Interfaces;
using WebAPICoreDapper.Data.ViewModels;

namespace WebAPICoreDapper.Data.Repositories;

public class AttributeRepository(IConfiguration configuration) : IAttributeRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DbConnectionString");

    public async Task Add(string culture, AttributeViewModel attribute)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@code", attribute.Code);
        parameters.Add("@name", attribute.Name);
        parameters.Add("@sortOrder", attribute.SortOrder);
        parameters.Add("@backendType", attribute.BackendType);
        parameters.Add("@isActive", attribute.IsActive);
        parameters.Add("@hasOption", attribute.HasOption);
        parameters.Add("@values", attribute.Values);
        await conn.ExecuteAsync("Create_Attribute", parameters, null, null, System.Data.CommandType.StoredProcedure);
    }

    public async Task Delete(int id)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        await conn.ExecuteAsync("Delete_Attribute_ById", parameters, null, null, System.Data.CommandType.StoredProcedure);
    }

    public async Task<List<AttributeViewModel>> GetAll(string culture)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();

        var parameters = new DynamicParameters();
        parameters.Add("@language", culture);

        var result = await conn.QueryAsync<AttributeViewModel>("Get_Attribute_All", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return result.ToList();
    }

    public async Task<AttributeViewModel> GetById(int id, string culture)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        parameters.Add("@language", culture);

        var result = await conn.QueryAsync<AttributeViewModel>("Get_Attribute_ById", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return result.SingleOrDefault();
    }

    public async Task Update(int id, string culture, AttributeViewModel attribute)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        parameters.Add("@name", attribute.Name);
        parameters.Add("@sortOrder", attribute.SortOrder);
        parameters.Add("@backendType", attribute.BackendType);
        parameters.Add("@isActive", attribute.IsActive);
        parameters.Add("@hasOption", attribute.HasOption);
        parameters.Add("@values", attribute.Values);
        await conn.ExecuteAsync("Update_Attribute", parameters, null, null, System.Data.CommandType.StoredProcedure);
    }
}