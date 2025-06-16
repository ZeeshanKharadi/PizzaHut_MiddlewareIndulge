using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.SqlClient;
using Middleware_Indolge.Helper;
using Middleware_Indolge.Models;
using Middleware_Indolge.Services.Interfaces;
using Newtonsoft.Json;
using POS_Integration_CommonCore.Helpers;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Middleware_Indolge.Services
{
    public class CreateOrderPOSService : ICreateOrderPosService
    {

        private string _connectionString;
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
        private string _dynamicsURL;
        private string _dynamicsLoginGrantType;
        private string _dynamicsLoginResource;
        private string _dynamicsLoginClientid;
        private string _dynamicsLoginClientsecret;
        private string _dynamicsLoginUrl;
        private readonly ILogger<CreateOrderPOSService> _logger;

        public CreateOrderPOSService(IConfiguration configuration, ILogger<CreateOrderPOSService> logger)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("AppDbConnection");
            _connectionString_RSSU = configuration.GetConnectionString("RSSUConnection");
            _terminalId = _configuration.GetSection("Keys:TerminalId").Value;
            _isTaxImplemented = _configuration.GetSection("Keys:TaxApplied").Value;
            _terminalIdOverride = _configuration.GetSection("Keys:_terminalIdOverride").Value;
            _dynamicsURL = _configuration.GetSection("AppInformation:UrlTemplate").Value;
            _dynamicsLoginGrantType = _configuration.GetSection("AppInformation:LoginGrantType").Value;
            _dynamicsLoginResource = _configuration.GetSection("AppInformation:LoginResource").Value;
            _dynamicsLoginClientid = _configuration.GetSection("AppInformation:LoginClientId").Value;
            _dynamicsLoginClientsecret = _configuration.GetSection("AppInformation:LoginClientSecret").Value;
            _dynamicsLoginUrl = _configuration.GetSection("AppInformation:LoginUrl").Value;
            _logger = logger;
        }

        public async Task<CreateOrderResponse> CreateOrder(CreateOrderModel request)
        {
            try
            {
                CreateOrderResponse response = new CreateOrderResponse();
                if (!IsOrderAlreadyExist(request.ThirdPartyOrderId))
                {
                    // Insert into DynamicPosOrder Table
                    InsertDynamicPOSOrder(request);
                    // Get store URL 
                    StoreInfo getStoreinfo = await GetStoreInfoFromDynamicsAsync(request.Store);
                    _logger.LogInformation("Call SendOrderToExternalApiV2");
                    _logger.LogInformation("Request   {method}", System.Text.Json.JsonSerializer.Serialize(request));
                    string apiResult = await SendOrderToExternalApiV2(request, getStoreinfo);
                    _logger.LogInformation("Response   {method}", System.Text.Json.JsonSerializer.Serialize(apiResult));

                    var deserializedResult = JsonConvert.DeserializeObject<dynamic>(apiResult);

                    if (deserializedResult?.status == "ok")
                    {
                        response.Message = "Order Created Successfully";
                        response.HttpStatusCode = 200;
                    }
                    else
                    {
                        DeleteCurrentRecord(request.ThirdPartyOrderId);
                        response.Message = "Failed to create order By DT";
                    }
                    response = JsonConvert.DeserializeObject<CreateOrderResponse>(apiResult);
                    return response;
                }
                else
                {
                    response.Message = "Order Already Exist";
                }
                return response;
            }
            catch (Exception ex)
            {

                DeleteCurrentRecord(request.ThirdPartyOrderId);
                throw ex;
            }

        }

        public async Task<CreateOrderResponse> UpdateOrder(UpdateOrderModel request, string ThirdPartyOrderId)
        {

            CreateOrderResponse response = new CreateOrderResponse();
            var json = await GetRequestJsonByThirdPartyOrderId(ThirdPartyOrderId);
            string PreviousOrderStatus;
            if (request.OrderStatus == "01") // Cancelled Kds Order
            {
                PreviousOrderStatus = GetOrderStatus(ThirdPartyOrderId);
                if (PreviousOrderStatus == "0")
                {
                    if (!string.IsNullOrEmpty(json))
                    {
                        var requestObject = JsonConvert.DeserializeObject<CreateOrderModel>(json);
                        StoreInfo getStoreinfo = await GetStoreInfoFromDynamicsAsync(requestObject.Store);
                        _logger.LogInformation("Call SendCancelKDSOrderToExternalApiV2");
                        _logger.LogInformation("Request   {method}", System.Text.Json.JsonSerializer.Serialize(request));
                        string apiResult = await SendCancelKDSOrderToExternalApiV2(ThirdPartyOrderId, getStoreinfo);
                        _logger.LogInformation("Response   {method}", System.Text.Json.JsonSerializer.Serialize(apiResult));

                        response = JsonConvert.DeserializeObject<CreateOrderResponse>(apiResult);
                        UpdateOrderStatus(ThirdPartyOrderId, request.OrderStatus);
                    }
                }
                else
                {
                    response.Message = "Order Can't cancelled";
                }
            }
            else if (request.OrderStatus == "02") // Sale
            {
                PreviousOrderStatus = GetOrderStatus(ThirdPartyOrderId);
                if (PreviousOrderStatus == "0")
                {
                    if (!string.IsNullOrEmpty(json))
                    {
                        var storedOrder = JsonConvert.DeserializeObject<CreateOrderModel>(json);
                        // Assign ExtItemId to ItemId for each sales line
                        storedOrder.SalesLines.ForEach(line => line.ItemId = line.ExtItemId);
                        _logger.LogInformation("Call SendSaleOrderToExternalApi");
                        _logger.LogInformation("Request   {method}", System.Text.Json.JsonSerializer.Serialize(request));
                        string apiResult = await SendSaleOrderToExternalApi(storedOrder);
                        _logger.LogInformation("Response   {method}", System.Text.Json.JsonSerializer.Serialize(apiResult));

                        response = JsonConvert.DeserializeObject<CreateOrderResponse>(apiResult);
                        UpdateOrderStatus(ThirdPartyOrderId, request.OrderStatus);
                    }
                }
                else
                {
                    response.Message = "Order Already marked as Sale/ Order can't be marked as sale ";
                }
            }
            else if (request.OrderStatus == "03") // Return
            {
                PreviousOrderStatus = GetOrderStatus(ThirdPartyOrderId);
                if (PreviousOrderStatus == "02")
                {
                    if (!string.IsNullOrEmpty(json))
                    {
                        var storedOrder = JsonConvert.DeserializeObject<CreateOrderModel>(json);
                        // Assign ExtItemId to ItemId for each sales line
                        storedOrder.SalesLines.ForEach(line => line.ItemId = line.ExtItemId);
                        // mapping update Order Tender Type
                        storedOrder.TenderTypeId = request.TenderTypeId;
                        _logger.LogInformation("Call SendReturnOrderToExternalApi");
                        _logger.LogInformation("Request   {method}", System.Text.Json.JsonSerializer.Serialize(request));
                        string apiResult = await SendReturnOrderToExternalApi(storedOrder);
                        _logger.LogInformation("Response   {method}", System.Text.Json.JsonSerializer.Serialize(apiResult));

                        response = JsonConvert.DeserializeObject<CreateOrderResponse>(apiResult);
                        UpdateOrderStatus(ThirdPartyOrderId, request.OrderStatus);
                    }
                }
                else
                {
                    response.Message = "Order can't returned ";
                }
            }
            else
            {
                response = new CreateOrderResponse();
            }
            return response;
        }

        public async Task<string> GetRequestJsonByThirdPartyOrderId(string thirdPartyOrderId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT RequestJson FROM DynamicPosOrders WHERE ThirdPartyOrderId = @ThirdPartyOrderId";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ThirdPartyOrderId", thirdPartyOrderId);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return result?.ToString();
            }
        }
        public void InsertDynamicPOSOrder(CreateOrderModel order)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Serialize SalesLines to JSON
                var salesLinesJson = JsonConvert.SerializeObject(order);

                // Prepare the SQL command for stored procedure
                using (var command = new SqlCommand("InsertDynamicPOSOrder", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@StoreId", order.Store);
                    command.Parameters.AddWithValue("@ThirdPartyOrderId", order.ThirdPartyOrderId);
                    command.Parameters.AddWithValue("@TransDate", order.TransDate);
                    command.Parameters.AddWithValue("@OrderSource", order.OrderSource ?? "");
                    command.Parameters.AddWithValue("@OrderStatus", (int)OrderStatus.Created); // Default status as 'Created'
                    command.Parameters.AddWithValue("@RequestJson", salesLinesJson);

                    // Execute the command
                    command.ExecuteNonQuery();
                }
            }
        }

        public enum OrderStatus
        {
            Created = 0,
            Cancelled = 1,
            Sale = 2,
            Return = 3
        }



        public async Task<string> SendOrderToExternalApi(CreateOrderModel request)
        {
            var externalRequest = new CreateKDSOrderExt
            {
                thirdPartyOrderId = request.ThirdPartyOrderId,
                storeid = request.Store,
                salesLines = request.SalesLines.Select(line => new ExternalOrderLine
                {
                    itemId = line.ExtItemId,
                    itemName = $"{line.Size} {line.Crust}".Trim(),
                    quantity = line.Qty,
                    description = line.LineComment ?? line.ExtItemId,
                    storeId = request.Store,
                    posId = "000012" // You can make this dynamic if needed
                }).ToList()
            };

            string apiUrl = "http://localhost:1638/api/OrderKDS/CreateKDSOrder";
            return await ApiHelper.PostAsync(apiUrl, externalRequest);
        }

        public async Task<string> SendOrderToExternalApiV2(CreateOrderModel request, StoreInfo storeinfo)
        {
            // Safely combine base URL and endpoint
            string apiUrl = $"{storeinfo.RSSUUrl.TrimEnd('/')}/api/OrderKDS/createOrderForDragonTail";
            return await ApiHelper.PostAsync(apiUrl, request);
        }
        public async Task<string> SendSaleOrderToExternalApi(CreateOrderModel request)
        {
            request.Company = "php";
            request.BusinessDateCustom = request.TransDate;
            string apiUrl = "http://localhost:1638/api/OrderPOS/complete-order";
            return await ApiHelper.PostAsync(apiUrl, request);
        }

        public async Task<string> SendReturnOrderToExternalApi(CreateOrderModel request)
        {
            request.Company = "php";
            request.BusinessDateCustom = request.TransDate;
            string apiUrl = "http://localhost:1638/api/OrderPOS/return-order";
            return await ApiHelper.PostAsync(apiUrl, request);
        }

        public async Task<string> SendCancelKDSOrderToExternalApi(string ThirdPartyOrderId)
        {
            string apiUrl = "http://localhost:1638/api/OrderKDS/cancelKDSOrder/?OrderId=" + ThirdPartyOrderId;
            return await ApiHelper.DeleteAsync(apiUrl);
        }
        public async Task<string> SendCancelKDSOrderToExternalApiV2(string ThirdPartyOrderId, StoreInfo storeinfo)
        {
            // Safely combine base URL and endpoint
            string apiUrl = $"{storeinfo.RSSUUrl.TrimEnd('/')}/api/OrderKDS/cancelOrderForDragonTail";
            return await ApiHelper.PutAsync(apiUrl, ThirdPartyOrderId);
        }


        public bool IsOrderAlreadyExist(string thirdPartyOrderId)
        {
            string query = "SELECT COUNT(*) FROM dbo.DynamicPOSOrders WHERE thirdPartyOrderId = @thirdPartyOrderId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add(new SqlParameter("@thirdPartyOrderId", SqlDbType.VarChar) { Value = thirdPartyOrderId });

                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        public string GetOrderStatus(string thirdPartyOrderId)
        {
            string query = "SELECT orderstatus FROM dbo.DynamicPOSOrders WHERE thirdPartyOrderId = @thirdPartyOrderId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add(new SqlParameter("@thirdPartyOrderId", SqlDbType.VarChar) { Value = thirdPartyOrderId });

                connection.Open();
                var result = command.ExecuteScalar();
                return result?.ToString(); // Returns null if no record found
            }
        }
        public void UpdateOrderStatus(string thirdPartyOrderId, string orderStatus)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("UpdateDynamicPOSOrder", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@thirdPartyOrderId", SqlDbType.VarChar) { Value = thirdPartyOrderId });
                command.Parameters.Add(new SqlParameter("@orderStatus", SqlDbType.VarChar) { Value = orderStatus });

                connection.Open();
                command.ExecuteNonQuery(); // Use ExecuteNonQuery for UPDATE
            }
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            using var client = new HttpClient();

            var values = new Dictionary<string, string>
    {
        { "grant_type", _dynamicsLoginGrantType},
        { "client_id", _dynamicsLoginClientid },
        { "client_secret", _dynamicsLoginClientsecret },
        { "resource", _dynamicsLoginResource }
    };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(_dynamicsLoginUrl, content);
            if (!response.IsSuccessStatusCode) return null;

            var responseString = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
            return tokenData?["access_token"]?.ToString();
        }

        public async Task<StoreInfo?> GetStoreInfoFromDynamicsAsync(string defaultCustAccount)
        {

            string url = StoreUrlCache.GetStoreUrl(defaultCustAccount);

            if (!string.IsNullOrWhiteSpace(url))
            {
                StoreInfo testResult = new StoreInfo
                {
                    RSSUUrl = url
                };
                return testResult;
            }
            string? token = await GetAccessTokenAsync();
            if (token == null) return null;

            string urlTemplate = _configuration["AppInformation:UrlTemplate"];
            string finalUrl = string.Format(urlTemplate, defaultCustAccount);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("Call {api}", finalUrl);
            _logger.LogInformation("Request   " );
            var response = await client.GetAsync(finalUrl);
            _logger.LogInformation("Response  {response}", System.Text.Json.JsonSerializer.Serialize(response));

            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<StoreInfoWrapper>(responseContent);
            StoreUrlCache.SetStoreUrl(defaultCustAccount, result?.Value?.FirstOrDefault()?.RSSUUrl ?? string.Empty);


            // After: Check if DTCreateUrl is null or empty, then set to your default data
            var RSSUUrl = result?.Value?.FirstOrDefault()?.RSSUUrl;

            if (string.IsNullOrEmpty(RSSUUrl))
            {
                RSSUUrl = "your_default_data_here"; // Replace with your actual default value
            }
            StoreUrlCache.SetStoreUrl(defaultCustAccount, RSSUUrl);
            return result?.Value?.FirstOrDefault(); // Return the first matched store info
        }

        private bool DeleteCurrentRecord(string thirdPartyOrderId)
        {
            bool result = false;
            string deleteFrom_DynamicPOSOrders = "Delete from DynamicPOSOrders where ThirdPartyOrderId='" + thirdPartyOrderId + "'";
            int IsdeleteFrom_DynamicPOSOrders = 0;
            IsdeleteFrom_DynamicPOSOrders = SqlHelper.ExecuteNonQuery(_connectionString, deleteFrom_DynamicPOSOrders.ToString(), CommandType.Text, null);
            if (IsdeleteFrom_DynamicPOSOrders > 0)
            {
                result = true;
            }
            return result;

        }

    }

}
