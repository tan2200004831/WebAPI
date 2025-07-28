using Microsoft.AspNetCore.Mvc;
using NIIEPay.Data;
using NIIEPay.Dto;
using System;
using System.Linq;

namespace NIIEPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly NiiepayContext _context;

        public TransactionController(NiiepayContext context)
        {
            _context = context;
        }

        // 🟢 Tạo giao dịch chuyển tiền
        [HttpPost("create")]
        public IActionResult CreateTransaction(CreateTransactionDto input)
        {
            var fromAccount = _context.Accounts.FirstOrDefault(a => a.AccountId == input.FromAccountId);
            var toAccount = _context.Accounts.FirstOrDefault(a => a.AccountId == input.ToAccountId);

            if (fromAccount == null || toAccount == null)
                return BadRequest("Tài khoản gửi hoặc nhận không tồn tại.");

            if (fromAccount.AvailableBalance < input.Amount)
                return BadRequest("Số dư không đủ.");

            // Trừ tiền người gửi, cộng tiền người nhận
            fromAccount.AvailableBalance -= input.Amount;
            toAccount.AvailableBalance += input.Amount;

            // Xác định giao dịch nội bộ
            bool isInternal = string.IsNullOrEmpty(input.ExternalBankCode);

            var transaction = new Transaction
            {
                FromAccountId = input.FromAccountId,
                ToAccountId = input.ToAccountId,
                Amount = input.Amount,
                Note = input.Note,
                ExternalBankCode = input.ExternalBankCode,
                IsInternal = isInternal,
                TransactionTime = DateTime.UtcNow,
                BalanceAfter = fromAccount.AvailableBalance,
                TransactionCode = Guid.NewGuid().ToString("N").ToUpper()
            };

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            var result = new TransactionDto
            {
                FromAccountId = transaction.FromAccountId ?? 0,
                ToAccountId = transaction.ToAccountId ?? 0,
                Amount = transaction.Amount,
                TransactionTime = transaction.TransactionTime,
                BalanceAfter = transaction.BalanceAfter,
                Note = transaction.Note,
                IsInternal = transaction.IsInternal,
                ExternalBankCode = transaction.ExternalBankCode,
                TransactionCode = transaction.TransactionCode
            };

            return Ok(result);
        }

        // 🟡 Lấy lịch sử giao dịch theo accountId
        [HttpGet("history/{accountId}")]
        public IActionResult GetTransactionHistory(int accountId)
        {
            var history = _context.Transactions
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.TransactionTime)
                .Select(t => new TransactionDto
                {
                    FromAccountId = t.FromAccountId ?? 0,
                    ToAccountId = t.ToAccountId ?? 0,
                    Amount = t.Amount,
                    TransactionTime = t.TransactionTime,
                    BalanceAfter = t.BalanceAfter,
                    Note = t.Note,
                    IsInternal = t.IsInternal,
                    ExternalBankCode = t.ExternalBankCode,
                    TransactionCode = t.TransactionCode
                })
                .ToList();

            return Ok(history);
        }
    }
}
