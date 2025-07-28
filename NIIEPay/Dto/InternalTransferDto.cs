namespace NIIEPay.Dto
{
    public class InternalTransferDto
    {
        public string FromAccount { get; set; } = null!;
        public string ToAccountOrPhone { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
