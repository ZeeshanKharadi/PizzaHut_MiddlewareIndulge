namespace Middleware_Indolge.Models
{
    public class CreateOrderResponse
    {
        public int MessageType { get; set; }
        public string Message { get; set; }
        public int HttpStatusCode { get; set; }
        public CreateOrderResult Result { get; set; }
    }

    public class CreateOrderResult
    {
        public string? FBRInvoiceNo { get; set; }
        public string? ReceiptId { get; set; }
        public string? OrderId { get; set; }
        public string? trackUrl { get; set; }
    }
}
