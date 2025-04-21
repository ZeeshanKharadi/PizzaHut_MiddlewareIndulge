using Middleware_Indolge.Models;

namespace Middleware_Indolge.Services.Interfaces
{
    public interface ICreateOrderPosService
    {
        Task<CreateOrderResponse> CreateOrder(CreateOrderModel request);
        Task<CreateOrderResponse> UpdateOrder(UpdateOrderModel request, string thirdPartyOrderId);
    }
}
