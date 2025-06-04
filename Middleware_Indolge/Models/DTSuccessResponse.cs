namespace Middleware_Indolge.Models
{
    public class DTSuccessResponse
    {
        public string Time { get; set; }
        public long UpdateStamp { get; set; }
        public int RowCount { get; set; }
        public string Status { get; set; }
        public List<string> Warnings { get; set; }
        public List<Order> Orders { get; set; }


        public class Order
        {
            public int StoreNo { get; set; }
            public string OrderId { get; set; }
            public string TrackUrl { get; set; }
            public int PromiseTimeSeconds { get; set; }
        }

       

    }
}
