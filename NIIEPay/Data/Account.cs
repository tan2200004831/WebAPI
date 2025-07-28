using System;
using System.Collections.Generic;

namespace NIIEPay.Data;

public partial class Account
{
    public int AccountId { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string AccountHolder { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string CitizenId { get; set; } = null!;

    public DateOnly ExpiryDate { get; set; }

    public decimal AvailableBalance { get; set; }

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<Saving> Savings { get; set; } = new List<Saving>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
