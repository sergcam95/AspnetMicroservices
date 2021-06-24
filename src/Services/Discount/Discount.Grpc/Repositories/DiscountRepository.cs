using System.Threading.Tasks;
using Dapper;
using Discount.Grpc.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Discount.Grpc.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<Coupon> GetDiscount(string productName)
        {
            // NpgsqlConnection implements IAsyncDisposable so we can use "await using"
            // to not block the cleanup of the object. C# 8
            await using var connection = new NpgsqlConnection
                (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>
            ("SELECT * FROM Coupon WHERE ProductName = @ProductName",
                new {ProductName = productName});

            if (coupon == null)
            {
                return new Coupon
                {
                    ProductName = "No discount",
                    Amount = 0,
                    Description = "No Discount Desc"
                };
            }

            return coupon;
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            await using var connection = new NpgsqlConnection
                (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected =
                await connection.ExecuteAsync(
                    "INSERT INTO Coupon (ProductName, Description, Amount)" +
                    "VALUES (@ProductName, @Description, @Amount)",
                    new
                    {
                        ProductName = coupon.ProductName,
                        Description = coupon.Description,
                        Amount = coupon.Amount,
                    });
            
            if (affected == 0)
                return false;

            return true;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            await using var connection = new NpgsqlConnection
                (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync
            ("UPDATE Coupon SET ProductName=@ProductName, Description=@Description," +
             "Amount=@Amount, Id=@Id WHERE Id=@Id", 
            new
                {
                    Id = coupon.Id,
                    ProductName = coupon.ProductName,
                    Description = coupon.Description,
                    Amount = coupon.Amount
                });
            
            if (affected == 0)
                return false;

            return true;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            await using var connection = new NpgsqlConnection
                (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync(
                "DELETE FROM Coupon WHERE ProductName = @ProductName",
                new {ProductName = productName});
            
            if (affected == 0)
                return false;

            return true;
        }
    }
}