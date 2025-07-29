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

        // 🟢 Tạo giao dịch chuyển tiền (có TransactionFee = 0)
        [HttpPost("create")]
        public IActionResult CreateTransaction(CreateTransactionDto input)
        {
            var fromAccount = _context.Accounts.FirstOrDefault(a => a.AccountId == input.FromAccountId);
            var toAccount = _context.Accounts.FirstOrDefault(a => a.AccountId == input.ToAccountId);

            if (fromAccount == null || toAccount == null)
                return BadRequest("Tài khoản gửi hoặc nhận không tồn tại.");

            if (fromAccount.AvailableBalance - input.Amount < 50000)
                return BadRequest("Số dư không đủ sau khi trừ (phải ≥ 50,000đ).");

            fromAccount.AvailableBalance -= input.Amount;
            toAccount.AvailableBalance += input.Amount;

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
                TransactionCode = Guid.NewGuid().ToString("N").ToUpper(),
                TransactionFee = 0 // miễn phí
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
                TransactionCode = transaction.TransactionCode,
                TransactionFee = transaction.TransactionFee
            };

            return Ok(result);
        }

        // 🟡 Lịch sử giao dịch
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
                    TransactionCode = t.TransactionCode,
                    TransactionFee = t.TransactionFee
                })
                .ToList();

            return Ok(history);
        }

        // 🔍 Lọc theo ngày và số tài khoản
        [HttpGet]
        public IActionResult GetTransactionsByDate(
            [FromQuery] string accountNumber,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
                return NotFound(new { status = "FAIL", message = "Không tìm thấy tài khoản." });

            int accId = account.AccountId;

            var transactions = _context.Transactions
                .Where(t => (t.FromAccountId == accId || t.ToAccountId == accId)
                            && t.TransactionTime >= fromDate && t.TransactionTime <= toDate)
                .OrderByDescending(t => t.TransactionTime)
                .Select(t => new
                {
                    transactionId = t.TransactionCode,
                    accountHolder = account.AccountHolder,
                    accountNumber = account.AccountNumber,
                    amount = t.Amount,
                    transactionTime = t.TransactionTime,
                    balanceAfter = t.BalanceAfter,
                    note = t.Note,
                    transactionFee = t.TransactionFee
                })
                .ToList();

            return Ok(transactions);
        }

        //  Chuyển khoản nội bộ
        [HttpPost("transfers/internal")]
        public IActionResult TransferInternal(InternalTransferDto dto)
        {
            var from = _context.Accounts.FirstOrDefault(a => a.AccountNumber == dto.FromAccount);
            if (from == null) return NotFound("Tài khoản gửi không tồn tại.");

            if (from.AvailableBalance - dto.Amount < 50000)
                return BadRequest("Không đủ số dư để thực hiện giao dịch. Cần giữ lại tối thiểu 50,000đ.");

            var to = _context.Accounts.FirstOrDefault(a =>
                a.AccountNumber == dto.ToAccountOrPhone || a.Phone == dto.ToAccountOrPhone);
            if (to == null) return NotFound("Tài khoản nhận không tồn tại.");

            from.AvailableBalance -= dto.Amount;
            to.AvailableBalance += dto.Amount;

            var now = DateTime.Now;
            var transactionId1 = $"TXN{now:yyyyMMddHHmmssfff}";
            var transactionId2 = $"TXN{now.AddMilliseconds(1):yyyyMMddHHmmssfff}";
            dto.Note ??= "Chuyển khoản nội bộ (miễn phí)";

            _context.Transactions.Add(new Transaction
            {
                FromAccountId = from.AccountId,
                ToAccountId = to.AccountId,
                Amount = -dto.Amount,
                TransactionTime = now,
                BalanceAfter = from.AvailableBalance,
                Note = dto.Note,
                TransactionCode = transactionId1,
                IsInternal = true,
                TransactionFee = 0
            });

            _context.Transactions.Add(new Transaction
            {
                FromAccountId = from.AccountId,
                ToAccountId = to.AccountId,
                Amount = dto.Amount,
                TransactionTime = now,
                BalanceAfter = to.AvailableBalance,
                Note = dto.Note,
                TransactionCode = transactionId2,
                IsInternal = true,
                TransactionFee = 0
            });

            _context.SaveChanges();

            return Ok(new
            {
                Status = "SUCCESS",
                SenderTransactionId = transactionId1,
                ReceiverTransactionId = transactionId2,
                Timestamp = now,
                RemainingBalance = from.AvailableBalance
            });
        }

        // 🔁 Chuyển khoản liên ngân hàng
        [HttpPost("transfers/external")]
        public IActionResult TransferExternal(ExternalTransferDto dto)
        {
            var from = _context.Accounts.FirstOrDefault(a => a.AccountNumber == dto.FromAccount);
            if (from == null) return NotFound("Tài khoản gửi không tồn tại.");

            if (from.AvailableBalance - dto.Amount < 50000)
                return BadRequest("Không đủ số dư để thực hiện giao dịch. Cần giữ lại tối thiểu 50,000đ.");

            from.AvailableBalance -= dto.Amount;

            var transactionId = $"TXN{DateTime.Now:yyyyMMddHHmmssfff}";
            var now = DateTime.Now;
            dto.Note ??= "Chuyển khoản liên ngân hàng";

            _context.Transactions.Add(new Transaction
            {
                FromAccountId = from.AccountId,
                ToAccountId = null,
                Amount = -dto.Amount,
                TransactionTime = now,
                BalanceAfter = from.AvailableBalance,
                Note = dto.Note,
                IsInternal = false,
                ExternalBankCode = dto.ToBankCode,
                TransactionCode = transactionId,
                TransactionFee = 0
            });

            _context.SaveChanges();

            return Ok(new
            {
                Status = "SUCCESS",
                TransactionId = transactionId,
                Timestamp = now,
                RemainingBalance = from.AvailableBalance
            });
        }

        // 🔍 Lọc giao dịch theo khoảng thời gian và accountId
        [HttpGet("history/{accountId}/filter")]
        public IActionResult GetTransactionHistoryFiltered(int accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            if (fromDate == null || toDate == null)
                return BadRequest(new { status = "FAIL", message = "Phải cung cấp fromDate và toDate." });

            var history = _context.Transactions
                .Where(t => (t.FromAccountId == accountId || t.ToAccountId == accountId)
                            && t.TransactionTime.Date >= fromDate.Value.Date
                            && t.TransactionTime.Date <= toDate.Value.Date)
                .OrderByDescending(t => t.TransactionTime)
                .Select(t => new
                {
                    t.TransactionCode,
                    t.TransactionTime,
                    t.Amount,
                    t.BalanceAfter,
                    t.Note,
                    t.IsInternal,
                    t.ExternalBankCode,
                    t.TransactionFee
                })
                .ToList();

            return Ok(history);
        }
    }
}
