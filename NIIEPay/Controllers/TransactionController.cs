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

            // kiểm tra số dư khả dụng >= 50,000đ sau giao dịch (ĐĂNG FIX)
            if (fromAccount.AvailableBalance - input.Amount < 50000)
                return BadRequest("Số dư không đủ sau khi trừ (phải ≥ 50,000đ).");


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


        // ĐĂNG thêm lấy giao dịch theo khoảng thời gian
        [HttpGet]
        public IActionResult GetTransactionsByDate(
        [FromQuery] string accountNumber,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
                return NotFound(new { status = "FAIL", message = "Không tìm thấy tài khoản." });

            // Lấy id để so sánh vì Transaction chỉ chứa AccountId
            int accId = account.AccountId;

            var transactions = _context.Transactions
                .Where(t => t.FromAccountId == accId || t.ToAccountId == accId)
                .Where(t => t.TransactionTime >= fromDate && t.TransactionTime <= toDate)
                .OrderByDescending(t => t.TransactionTime)
                .Select(t => new
                {
                    transactionId = t.TransactionCode,
                    accountHolder = account.AccountHolder,
                    accountNumber = account.AccountNumber,
                    amount = t.Amount,
                    transactionTime = t.TransactionTime,
                    balanceAfter = t.BalanceAfter,
                    note = t.Note
                })
                .ToList();

            return Ok(transactions);
        }

        // ĐĂNG thêm chuyển khoản nội bộ
        [HttpPost("transfers/internal")]
        public IActionResult TransferInternal(InternalTransferDto dto)
        {
            var from = _context.Accounts.FirstOrDefault(a => a.AccountNumber == dto.FromAccount);
            if (from == null) return NotFound("Tài khoản gửi không tồn tại.");

            if (from.AvailableBalance - dto.Amount < 50000)
                return BadRequest("Không đủ số dư để thực hiện giao dịch. Cần giữ lại tối thiểu 50,000đ.");

            // Tìm tài khoản nhận theo số tài khoản hoặc số điện thoại
            var to = _context.Accounts.FirstOrDefault(a =>
                a.AccountNumber == dto.ToAccountOrPhone || a.Phone == dto.ToAccountOrPhone);
            if (to == null) return NotFound("Tài khoản nhận không tồn tại.");

            // Trừ tiền người gửi
            from.AvailableBalance -= dto.Amount;

            // Cộng tiền người nhận
            to.AvailableBalance += dto.Amount;

            // Tạo mã giao dịch đơn giản
            var transactionId1 = $"TXN{DateTime.Now:yyyyMMddHHmmssfff}";
            var transactionId2 = $"TXN{DateTime.Now.AddMilliseconds(1):yyyyMMddHHmmssfff}";


            // Lưu 2 giao dịch (ghi nhận vào bảng Transaction)
            var now = DateTime.Now;
            dto.Note ??= "Chuyển khoản nội bộ (không phí)";

            // Giao dịch người gửi
            _context.Transactions.Add(new Transaction
            {
                FromAccountId = from.AccountId,
                ToAccountId = to.AccountId,
                Amount = -dto.Amount,
                TransactionTime = now,
                BalanceAfter = from.AvailableBalance,
                Note = dto.Note,
                TransactionCode = transactionId1,
                IsInternal = true
            });

            // Giao dịch người nhận
            _context.Transactions.Add(new Transaction
            {
                FromAccountId = from.AccountId,
                ToAccountId = to.AccountId,
                Amount = dto.Amount,
                TransactionTime = now,
                BalanceAfter = to.AvailableBalance,
                Note = dto.Note,
                TransactionCode = transactionId2,
                IsInternal = true
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

        // ĐĂNG thêm chuyển khoản liên ngân hàng
        [HttpPost("transfers/external")]
        public IActionResult TransferExternal(ExternalTransferDto dto)
        {
            var from = _context.Accounts.FirstOrDefault(a => a.AccountNumber == dto.FromAccount);
            if (from == null) return NotFound("Tài khoản gửi không tồn tại.");

            if (from.AvailableBalance - dto.Amount < 50000)
                return BadRequest("Không đủ số dư để thực hiện giao dịch. Cần giữ lại tối thiểu 50,000đ.");

            // Trừ tiền
            from.AvailableBalance -= dto.Amount;

            // Tạo mã giao dịch
            var transactionId = $"TXN{DateTime.Now:yyyyMMddHHmmssfff}";
            var now = DateTime.Now;
            dto.Note ??= "Chuyển khoản liên ngân hàng";

            // Ghi log giao dịch (chỉ 1 chiều)
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
                TransactionCode = transactionId
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
                .ToList();

            return Ok(history);
        }
    }
}
