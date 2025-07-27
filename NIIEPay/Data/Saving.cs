using System;
using System.Collections.Generic;

namespace NIIEPay.Data;

public partial class Saving
{
    public int SavingId { get; set; }

    public int? AccountId { get; set; }

    public decimal Amount { get; set; }

    public int TermMonths { get; set; }

    public decimal InterestRate { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly MaturityDate { get; set; }

    public bool AutoRenew { get; set; }

    public virtual Account? Account { get; set; }
}
