using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware_Indolge.Models;
using Middleware_Indolge.Services.Interfaces;

namespace Middleware_Indolge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly IConfiguration Iconfig;
        private string cs;
        private readonly ILogger<OrderPOSController> _logger;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IConfiguration iconfig, ILogger<OrderPOSController> logger, IAuthenticationService authenticationService)
        {
            Iconfig = iconfig;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<LoginResponse> Login(LoginModel request)
        {

            LoginResponse response = new LoginResponse();

            try
            {
                _logger.LogInformation("call authentication");

                return await _authenticationService.Login(request);
            }
            catch (Exception ex)
            {
                response.result = null;
                response.httpStatusCode = 0;
                response.messageType = 0;
                response.message = "server error msg: " + ex.Message + " | Inner exception:  " + ex.InnerException;
                _logger.LogInformation("Resonse   {method}", System.Text.Json.JsonSerializer.Serialize(response));

                return response;
            }
        }
    
    }
}
