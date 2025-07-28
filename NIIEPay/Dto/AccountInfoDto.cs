namespace NIIEPay.Dto
{
    public class AccountInfoDto
    {
        public string? AccountNumber { get; set; }
        public string? AccountHolder { get; set; }
        public string? Phone { get; set; }
        public string? CitizenId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal AvailableBalance { get; set; }
    }

}
