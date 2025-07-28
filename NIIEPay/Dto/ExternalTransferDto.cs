namespace NIIEPay.Dto
{
    public class ExternalTransferDto
    {
        public string FromAccount { get; set; } = null!;
        public string ToBankCode { get; set; } = null!;
        public string ToAccount { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
