using System.Data.SqlClient;

namespace Middleware_Indolge.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public TokenValidationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("AppDbConnection");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Apply token validation only to protected routes
            if (path.Contains("/api/orderpos/createorder") || path.Contains("/api/orderpos/updateorder"))
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token) || !IsTokenValid(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Invalid or missing token.");
                    return;
                }
            }

            await _next(context);
        }

        private bool IsTokenValid(string token)
        {
            //string connectionString = _configuration.GetConnectionString("YourDb");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Tokens WHERE Token = @Token AND TokenExpiry > GETUTCDATE()";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Token", token);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }

}
