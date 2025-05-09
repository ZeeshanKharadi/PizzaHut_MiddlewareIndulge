namespace Middleware_Indolge.Models
{
    public class LoginResponse
    {
        public int messageType { get; set; }
        public string message { get; set; }
        public int httpStatusCode { get; set; }
        public LoginResult result { get; set; }
    }

    public class LoginResult
    {
        public string token { get; set; }
        public string expiry { get; set; }
    }
}
