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
        public async Task<IActionResult> CreateOrder(CreateOrderModel request)
        {
            _logger.LogInformation("Call CreateOrder");
            _logger.LogInformation("Request   {method}", System.Text.Json.JsonSerializer.Serialize(request));

            CreateOrderResponse response = new CreateOrderResponse();

            try
            {
                response = await _createOrderPOSService.CreateOrder(request);
                _logger.LogInformation("Response CreateOrder  {method}", System.Text.Json.JsonSerializer.Serialize(response));

                // Use the HttpStatusCode from the response if set, otherwise default to 200
                int statusCode = response.HttpStatusCode > 0 ? response.HttpStatusCode : StatusCodes.Status200OK;
                return StatusCode(statusCode, response);
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.HttpStatusCode = StatusCodes.Status500InternalServerError;
                response.MessageType = 0;
                response.Message = "server error msg: " + ex.Message + " | Inner exception:  " + ex.InnerException;
                _logger.LogError(ex, "An error occurred while creating the order.");

                return StatusCode(response.HttpStatusCode, response);
            }
        }

        [HttpPut]
        [Route("updateOrder")]
        public async Task<CreateOrderResponse> UpdateOrder(string thirdPartyOrderId, [FromBody] UpdateOrderModel request)
        {
            _logger.LogInformation("Call UpdateOrder");
            _logger.LogInformation("Request   {method}", System.Text.Json.JsonSerializer.Serialize(request));
            CreateOrderResponse response = new CreateOrderResponse();

            try
            {
                response = await _createOrderPOSService.UpdateOrder(request, thirdPartyOrderId);
                _logger.LogInformation("Response UpdateOrder  {method}", System.Text.Json.JsonSerializer.Serialize(response));

                return response;
            }
            catch (Exception ex)
            {
                response.Result = null;
                response.HttpStatusCode = 0; // Consider using actual status codes
                response.MessageType = 0;
                response.Message = $"server error msg: {ex.Message} | Inner exception: {ex.InnerException}";
                _logger.LogError(ex, "An error occurred while updating the order.");
               

                return response;
            }
        }
    }
}
