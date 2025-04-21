using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Data.SqlClient;
using Middleware_Indolge.Models;
using Middleware_Indolge.Services.Interfaces;
using Newtonsoft.Json;
using System.Data;

namespace Middleware_Indolge.Services
{
    public class CreateOrderPOSService : ICreateOrderPosService
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

        public CreateOrderPOSService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString_CHZ_MIDDLEWARE = configuration.GetConnectionString("AppDbConnection");
            _connectionString = configuration.GetConnectionString("AppDbConnection");
            _connectionString_KFC = configuration.GetConnectionString("RSSUConnection");
            _connectionString_RSSU = configuration.GetConnectionString("RSSUConnection");
            _terminalId = _configuration.GetSection("Keys:TerminalId").Value;
            _terminalId = _configuration.GetSection("Keys:TerminalId").Value;
            _isTaxImplemented = _configuration.GetSection("Keys:TaxApplied").Value;
            _terminalIdOverride = _configuration.GetSection("Keys:_terminalIdOverride").Value;
        }

        public async Task<CreateOrderResponse> CreateOrder(CreateOrderModel request)
        {
            CreateOrderResponse response = new CreateOrderResponse();

            // Insert into DynamicPosOrder Table
            InsertDynamicPOSOrder(request);
            string apiResult = await SendOrderToExternalApi(request);
            response = JsonConvert.DeserializeObject<CreateOrderResponse>(apiResult);
            return response;

        }

        public async Task<CreateOrderResponse> UpdateOrder(UpdateOrderModel request, string ThirdPartyOrderId)
        {
            CreateOrderResponse response = new CreateOrderResponse();
            var json = await GetRequestJsonByThirdPartyOrderId(ThirdPartyOrderId);

            if (!string.IsNullOrEmpty(json))
            {
                var storedOrder = JsonConvert.DeserializeObject<CreateOrderModel>(json);
                // Assign ExtItemId to ItemId for each sales line
                storedOrder.SalesLines.ForEach(line => line.ItemId = line.ExtItemId);
                string apiResult = await SendSaleOrderToExternalApi(storedOrder);
                response = JsonConvert.DeserializeObject<CreateOrderResponse>(apiResult);
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
        public async Task<string> SendSaleOrderToExternalApi(CreateOrderModel request)
        {
            request.Company = "Piz";
            request.BusinessDateCustom = request.TransDate;
            string apiUrl = "http://localhost:1638/api/OrderPOS/complete-order";
            return await ApiHelper.PostAsync(apiUrl, request);
        }
    }
}
