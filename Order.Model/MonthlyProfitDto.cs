namespace Order.Model
{
    // DTO representing the calculated profit breakdown by month for completed orders
    public class MonthlyProfitDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal TotalProfit { get; set; }
        public int CompletedOrdersCount { get; set; }
    }
}
