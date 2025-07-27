using System;
using System.Collections.Generic;

namespace NIIEPay.Data;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int? FromAccountId { get; set; }

    public int? ToAccountId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionTime { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? Note { get; set; }

    public bool IsInternal { get; set; }

    public string? ExternalBankCode { get; set; }

    public string TransactionCode { get; set; } = null!;

    public virtual Account? FromAccount { get; set; }
}
