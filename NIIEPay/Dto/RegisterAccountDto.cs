namespace NIIEPay.Dto
{
    public class RegisterAccountDto
    {
            public string AccountNumber { get; set; }
            public string Password { get; set; }
            public string AccountHolder { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string CitizenId { get; set; }
            public DateOnly ExpiryDate { get; set; }
            public decimal InitialBalance { get; set; }
    }
}
