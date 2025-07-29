namespace NIIEPay.Dto
{
    public class CreateTransactionDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public string? ExternalBankCode { get; set; }

        // Thêm TransactionFee
        public decimal TransactionFee { get; set; } = 0;
    }
}
