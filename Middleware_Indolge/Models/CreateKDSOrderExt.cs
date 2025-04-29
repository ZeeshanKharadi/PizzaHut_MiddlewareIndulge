namespace Middleware_Indolge.Models
{

    public class CreateKDSOrderExt
    {
        public string thirdPartyOrderId { get; set; }
        public string storeid { get; set; }
        public List<ExternalOrderLine> salesLines { get; set; }
    }
    public class ExternalOrderLine
    {
        public string itemId { get; set; }
        public string itemName { get; set; }
        public int quantity { get; set; }
        public string description { get; set; }
        public string storeId { get; set; }
        public string posId { get; set; }
    }

    public class CancelKDSOrderExt
    {
        public string OrderId { get; set; }
    }



}
