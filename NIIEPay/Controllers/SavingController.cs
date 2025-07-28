using Microsoft.AspNetCore.Mvc;
using NIIEPay.Data;

namespace NIIEPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingController : Controller
    {
        private readonly NiiepayContext _context;

        public SavingController(NiiepayContext context)
        {
            _context = context;
        }

        // 🟢 Tạo sổ tiết kiệm
        [HttpPost("create")]
        public IActionResult CreateSaving(Saving saving)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == saving.AccountId);
            if (account == null)
                return BadRequest("Tài khoản không tồn tại.");

            if (account.AvailableBalance < saving.Amount)
                return BadRequest("Số dư không đủ để gửi tiết kiệm.");

            account.AvailableBalance -= saving.Amount;
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            saving.MaturityDate = saving.StartDate.AddMonths(saving.TermMonths);

            _context.Savings.Add(saving);
            _context.SaveChanges();

            return Ok(saving);
        }

        // 🟡 Lấy danh sách sổ tiết kiệm theo accountId
        [HttpGet("list/{accountId}")]
        public IActionResult GetSavings(int accountId)
        {
            var savings = _context.Savings
                .Where(s => s.AccountId == accountId)
                .OrderByDescending(s => s.StartDate)
                .ToList();

            return Ok(savings);
        }
    }
}
