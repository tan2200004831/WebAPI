namespace NIIEPay.Model
{
    public class AccountDto
    {
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}
