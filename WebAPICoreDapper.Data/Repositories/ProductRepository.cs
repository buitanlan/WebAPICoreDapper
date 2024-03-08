using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using WebAPICoreDapper.Data.Models;
using WebAPICoreDapper.Utilities.Dtos;
using WebAPICoreDapper.Data.Repositories.Interfaces;
using WebAPICoreDapper.Data.ViewModels;

namespace WebAPICoreDapper.Data.Repositories;

public class ProductRepository(IConfiguration configuration) : IProductRepository

{
    private readonly string _connectionString = configuration.GetConnectionString("DbConnectionString");

    public async Task<IEnumerable<Product>> GetAllAsync(string culture)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();

        var parameters = new DynamicParameters();
        parameters.Add("@language", culture);

        var result = await conn.QueryAsync<Product>("Get_Product_All", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return result;
    }

    public async Task<Product> GetByIdAsync(int id, string culture)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        parameters.Add("@language", culture);

        var result = await conn.QueryAsync<Product>("Get_Product_ById", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return result.Single();
    }

    public async Task<PagedResult<Product>> GetPaging(string keyword, string culture, int categoryId, int pageIndex, int pageSize)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@keyword", keyword);
        parameters.Add("@categoryId", categoryId);
        parameters.Add("@pageIndex", pageIndex);
        parameters.Add("@pageSize", pageSize);
        parameters.Add("@language", culture);

        parameters.Add("@totalRow", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        var result = await conn.QueryAsync<Product>("Get_Product_AllPaging", parameters, null, null, System.Data.CommandType.StoredProcedure);

        var totalRow = parameters.Get<int>("@totalRow");

        var pagedResult = new PagedResult<Product>()
        {
            Items = result.ToList(),
            TotalRow = totalRow,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
        return pagedResult;
    }

    public async Task<int> Create(string culture, Product product)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@name", product.Name);
        parameters.Add("@description", product.Description);
        parameters.Add("@content", product.Content);
        parameters.Add("@seoDescription", product.SeoDescription);
        parameters.Add("@seoAlias", product.SeoAlias);
        parameters.Add("@seoTitle", product.SeoTitle);
        parameters.Add("@seoKeyword", product.SeoKeyword);
        parameters.Add("@sku", product.Sku);
        parameters.Add("@price", product.Price);
        parameters.Add("@isActive", product.IsActive);
        parameters.Add("@imageUrl", product.ImageUrl);
        parameters.Add("@language", culture);
        parameters.Add("@categoryIds", product.CategoryIds);
        parameters.Add("@id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
        var result = await conn.ExecuteAsync("Create_Product", parameters, null, null, System.Data.CommandType.StoredProcedure);

        var newId = parameters.Get<int>("@id");
        return newId;
    }

    public async Task Update(string culture, int id, Product product)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        parameters.Add("@name", product.Name);
        parameters.Add("@description", product.Description);
        parameters.Add("@content", product.Content);
        parameters.Add("@seoDescription", product.SeoDescription);
        parameters.Add("@seoAlias", product.SeoAlias);
        parameters.Add("@seoTitle", product.SeoTitle);
        parameters.Add("@seoKeyword", product.SeoKeyword);
        parameters.Add("@sku", product.Sku);
        parameters.Add("@price", product.Price);
        parameters.Add("@isActive", product.IsActive);
        parameters.Add("@imageUrl", product.ImageUrl);
        parameters.Add("@language", culture);
        parameters.Add("@categoryIds", product.CategoryIds);
        await conn.ExecuteAsync("Update_Product", parameters, null, null, System.Data.CommandType.StoredProcedure);
    }

    public async Task Delete(int id)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        await conn.ExecuteAsync("Delete_Product_ById", parameters, null, null, System.Data.CommandType.StoredProcedure);
    }
    public async Task<List<ProductAttributeViewModel>> GetAttributes(int id, string culture)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();
        var parameters = new DynamicParameters();
        parameters.Add("@id", id);
        parameters.Add("@language", culture);

        var result = await conn.QueryAsync<ProductAttributeViewModel>("Get_Product_Attributes", parameters, null, null, System.Data.CommandType.StoredProcedure);
        return result.ToList();
    }

    public async Task<PagedResult<Product>> SearchByAttributes(string keyword, string culture,
        int categoryId, string size, int pageIndex, int pageSize)
    {
        await using var conn = new SqlConnection(_connectionString);
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();

        var parameters = new DynamicParameters();
        parameters.Add("@keyword", keyword);
        parameters.Add("@categoryId", categoryId);
        parameters.Add("@pageIndex", pageIndex);
        parameters.Add("@pageSize", pageSize);
        parameters.Add("@language", culture);
        parameters.Add("@size", size);

        parameters.Add("@totalRow", dbType: System.Data.DbType.Int32,
            direction: System.Data.ParameterDirection.Output);

        var result = await conn.QueryAsync<Product>("[Search_Product_ByAttributes]",
            parameters, null, null, System.Data.CommandType.StoredProcedure);

        var totalRow = parameters.Get<int>("@totalRow");

        var pagedResult = new PagedResult<Product>()
        {
            Items = result.ToList(),
            TotalRow = totalRow,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
        return pagedResult;
    }
}