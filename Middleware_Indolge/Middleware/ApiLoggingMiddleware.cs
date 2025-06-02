using Microsoft.Data.SqlClient;

namespace Middleware_Indolge.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _connectionString;

        public ApiLoggingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _connectionString = configuration.GetConnectionString("YourDbConnection"); // from appsettings.json
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestTime = DateTime.UtcNow;

            context.Request.EnableBuffering();
            var requestBody = await ReadRequestBody(context.Request);

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            var responseTime = DateTime.UtcNow;
            var responseText = await ReadResponseBody(context.Response);

            await responseBody.CopyToAsync(originalBodyStream);

            // Insert into DB using ADO.NET
            await LogToDatabase(
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString.ToString(),
                requestBody,
                responseText,
                context.Response.StatusCode,
                requestTime,
                responseTime
            );
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Position = 0;
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Position = 0;
            return text;
        }

        private async Task LogToDatabase(string method, string path, string query, string reqBody, string resBody, int statusCode, DateTime reqTime, DateTime resTime)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
            INSERT INTO ApiLogs 
            (RequestMethod, RequestPath, RequestQuery, RequestBody, ResponseBody, ResponseStatusCode, RequestTimestamp, ResponseTimestamp)
            VALUES 
            (@RequestMethod, @RequestPath, @RequestQuery, @RequestBody, @ResponseBody, @ResponseStatusCode, @RequestTimestamp, @ResponseTimestamp)", conn);

            cmd.Parameters.AddWithValue("@RequestMethod", method);
            cmd.Parameters.AddWithValue("@RequestPath", path);
            cmd.Parameters.AddWithValue("@RequestQuery", (object)query ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RequestBody", (object)reqBody ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ResponseBody", (object)resBody ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ResponseStatusCode", statusCode);
            cmd.Parameters.AddWithValue("@RequestTimestamp", reqTime);
            cmd.Parameters.AddWithValue("@ResponseTimestamp", resTime);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }

}
