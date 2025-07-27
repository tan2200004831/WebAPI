using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NIIEPay.Data;

public partial class NiiepayContext : DbContext
{
    public NiiepayContext()
    {
    }

    public NiiepayContext(DbContextOptions<NiiepayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<InterestRate> InterestRates { get; set; }

    public virtual DbSet<Saving> Savings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=LENHATTAN\\SQLEXPRESS;Initial Catalog=NIIEPay;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA5A62A996048");

            entity.HasIndex(e => e.AccountNumber, "UQ__Accounts__BE2ACD6F4691BA02").IsUnique();

            entity.Property(e => e.AccountHolder).HasMaxLength(100);
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AvailableBalance).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.CitizenId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<InterestRate>(entity =>
        {
            entity.HasKey(e => e.TermMonths).HasName("PK__Interest__25E3983318AC82D3");

            entity.Property(e => e.TermMonths).ValueGeneratedNever();
            entity.Property(e => e.InterestRate1)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("InterestRate");
        });

        modelBuilder.Entity<Saving>(entity =>
        {
            entity.HasKey(e => e.SavingId).HasName("PK__Savings__E3D384B946BFD6D3");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.InterestRate).HasColumnType("decimal(4, 2)");

            entity.HasOne(d => d.Account).WithMany(p => p.Savings)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__Savings__Account__5165187F");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A6B29C2F321");

            entity.HasIndex(e => e.TransactionCode, "UQ__Transact__D85E7026860431D2").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ExternalBankCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.FromAccount).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.FromAccountId)
                .HasConstraintName("FK__Transacti__FromA__4D94879B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
