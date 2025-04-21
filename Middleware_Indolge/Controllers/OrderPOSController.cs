using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware_Indolge.Models;
using Middleware_Indolge.Services.Interfaces;
using System.Net;

namespace Middleware_Indolge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderPOSController : ControllerBase
    {

        private readonly IConfiguration Iconfig;
        private string cs;
        private readonly ILogger<OrderPOSController> _logger;
        private readonly ICreateOrderPosService _createOrderPOSService;

        public OrderPOSController(IConfiguration iconfig, ILogger<OrderPOSController> logger, ICreateOrderPosService createOrderPOSService)
        {
            Iconfig = iconfig;
            _createOrderPOSService = createOrderPOSService;
            _logger = logger;
        }

        [HttpPost]
        [Route("createOrder")]
        public async Task<CreateOrderResponse> CreateOrder(CreateOrderModel request)
        {
            CreateOrderResponse response = new CreateOrderResponse();

            try
            {
                return await _createOrderPOSService.CreateOrder(request);
            }
            catch (Exception ex)
            {
                response.Result = null;
                // response.HttpStatusCode = (int)HttpStatusCode.InternalServerError;
                response.HttpStatusCode = 0;
                //response.MessageType = (int)MessageType.Error;
                response.MessageType = 0;
                response.Message = "server error msg: " + ex.Message + " | Inner exception:  " + ex.InnerException;
                return response;
            }
        }

        [HttpPut]
        [Route("updateOrder")]
        public async Task<CreateOrderResponse> UpdateOrder(string thirdPartyOrderId, [FromBody] UpdateOrderModel request)
        {
            CreateOrderResponse response = new CreateOrderResponse();

            try
            {

                return await _createOrderPOSService.UpdateOrder(request, thirdPartyOrderId);
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.HttpStatusCode = 0; // Consider using actual status codes
                response.MessageType = 0;
                response.Message = $"server error msg: {ex.Message} | Inner exception: {ex.InnerException}";
                return response;
            }
        }
    }
}
