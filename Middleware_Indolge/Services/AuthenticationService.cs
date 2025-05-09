using Azure.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.SqlClient;
using Middleware_Indolge.Models;
using Middleware_Indolge.Services.Interfaces;
using Newtonsoft.Json;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Middleware_Indolge.Services
{


    public class AuthenticationService : IAuthenticationService
    {

        private string _connectionString_CHZ_MIDDLEWARE;
        private string _connectionString;
        private string _connectionString_KFC;
        //private InlineQueryResponse lastRecordResponse;
        private string _connectionString_RSSU;
        private readonly IConfiguration _configuration;
        private string _terminalId;
        private string _receiptId;
        private string _suspendedId;
        private string _transactionId;
        private string _fbRInvoiceNo;
        private long _channle = 0;
        private long _batchId = 0;
        private decimal? _totalBillAmount;
        private DataTable dataTable;
        private decimal _taxPrice;
        private decimal _orignalprice;
        private string inventLocationId = string.Empty;
        private decimal _grossAmountCustom;
        private string _staffId = string.Empty;
        private bool _isFBRFail = false;
        private string _isTaxImplemented = string.Empty;
        private string _itemName = string.Empty;
        private string _fbrInvoiceNumber = string.Empty;
        private string _srbInvoiceNumber = string.Empty;
        private decimal _taxValue;
        //private readonly ISender _mediator;
        private string _terminalIdOverride;

        private readonly IHttpContextAccessor _httpContextAccessor;

        
        public AuthenticationService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _connectionString_CHZ_MIDDLEWARE = configuration.GetConnectionString("AppDbConnection");
            _connectionString = configuration.GetConnectionString("AppDbConnection");
            _connectionString_KFC = configuration.GetConnectionString("RSSUConnection");
            _connectionString_RSSU = configuration.GetConnectionString("RSSUConnection");
            _terminalId = _configuration.GetSection("Keys:TerminalId").Value;
            _terminalId = _configuration.GetSection("Keys:TerminalId").Value;
            _isTaxImplemented = _configuration.GetSection("Keys:TaxApplied").Value;
            _terminalIdOverride = _configuration.GetSection("Keys:_terminalIdOverride").Value;
        }

        public async Task<LoginResponse> Login(LoginModel request)
        {
            var response = new LoginResponse();

            if (IsLoginValid(request.username, request.password))
            {
                string token = Guid.NewGuid().ToString();
                DateTime expiry = DateTime.UtcNow.AddHours(2);
                string ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

                await InsertTokenAsync(request.username, token, expiry, ipAddress, userAgent);

                response.messageType = 1; // success
                response.message = "Login successful";
                response.httpStatusCode = 200;
                response.result = new LoginResult
                {
                    token = token,
                    expiry = expiry.ToString("o") // ISO 8601 format
                };
            }
            else
            {
                response.messageType = 0; // error
                response.message = "Invalid username or password";
                response.httpStatusCode = 401;
                response.result = null;
            }

            return response;
        }


        public bool IsLoginValid(string inputUserId, string inputPassword)
        {
            //string connectionString = "your_connection_string_here";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT PasswordHash FROM Users WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", inputUserId);

                string storedHash = cmd.ExecuteScalar() as string;

                if (string.IsNullOrEmpty(storedHash))
                    return false; // user not found

                // Hash the input password
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(inputPassword);
                    byte[] hash = sha.ComputeHash(bytes);
                    string inputHash = Convert.ToBase64String(hash);

                    return inputHash == storedHash;
                }
            }
        }

        public async Task InsertTokenAsync(string userId, string token, DateTime expiry, string ipAddress, string userAgent)
        {
            //string connectionString = "your_connection_string_here";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("InsertToken", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.Parameters.AddWithValue("@TokenExpiry", expiry);
                    cmd.Parameters.AddWithValue("@IpAddress", ipAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserAgent", userAgent ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


    }
}