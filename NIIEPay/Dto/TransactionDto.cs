namespace NIIEPay.Dto
{
    public class TransactionDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionTime { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Note { get; set; }
        public bool IsInternal { get; set; }
        public string? ExternalBankCode { get; set; }
        public string TransactionCode { get; set; } = null!;
        public decimal TransactionFee { get; set; }  
    }
}
