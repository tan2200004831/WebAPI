using System;
using System.Collections.Generic;

namespace NIIEPay.Data;

public partial class Deposit
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public decimal Amount { get; set; }

    public DateTime DepositDate { get; set; }

    public virtual Account Account { get; set; } = null!;
}
