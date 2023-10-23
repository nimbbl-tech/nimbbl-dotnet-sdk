using System.Threading.Tasks;
namespace Nimbbl.Sdk.Rest;

public interface IOrders
{
    Task<Order> CreateAsync(OrderRequest orderRequest);
    Task<Order> GetByIdAsync(string orderId);
}