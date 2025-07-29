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
        // ĐĂNG fix tạo sổ tiết kiệm (ràng buộc số dư khả dụng sau gửi ≥ 50.000đ, kỳ hạn hợp lệ)
        [HttpPost("create")]
        public IActionResult CreateSaving(Saving saving)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == saving.AccountId);
            if (account == null)
                return BadRequest(new { status = "FAIL", message = "Tài khoản không tồn tại." });

            var validTerms = new[] { 1, 2, 3, 6, 9, 12, 18, 24, 36 };
            if (!validTerms.Contains(saving.TermMonths))
                return BadRequest(new { status = "FAIL", message = "Kỳ hạn gửi không hợp lệ." });

            if (account.AvailableBalance - saving.Amount < 50000)
                return BadRequest(new { status = "FAIL", message = "Số dư sau khi gửi phải còn tối thiểu 50.000đ." });

            account.AvailableBalance -= saving.Amount;

            // Gán InterestRate từ hàm GetRate
            saving.InterestRate = GetRate(saving.TermMonths);

            saving.MaturityDate = saving.StartDate.AddMonths(saving.TermMonths);

            _context.Savings.Add(saving);
            _context.SaveChanges();

            return Ok(new
            {
                status = "SUCCESS",
                savingId = saving.SavingId,
                termMonths = saving.TermMonths,
                interestRate = saving.InterestRate,
                startDate = saving.StartDate,
                maturityDate = saving.MaturityDate
            });
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

        [HttpGet("rates")]
        public IActionResult GetSavingRates()
        {
            var rates = new List<SavingRateDto>
            {
        new SavingRateDto { TermMonths = 1, InterestRate = 3.5m },
        new SavingRateDto { TermMonths = 2, InterestRate = 3.7m },
        new SavingRateDto { TermMonths = 3, InterestRate = 3.8m },
        new SavingRateDto { TermMonths = 6, InterestRate = 4.8m },
        new SavingRateDto { TermMonths = 9, InterestRate = 4.9m },
        new SavingRateDto { TermMonths = 12, InterestRate = 5.2m },
        new SavingRateDto { TermMonths = 18, InterestRate = 5.5m },
        new SavingRateDto { TermMonths = 24, InterestRate = 5.8m },
        new SavingRateDto { TermMonths = 36, InterestRate = 5.8m }
            };

            return Ok(rates);
        }



        // ĐĂNG thêm GetRate
        private decimal GetRate(int termMonths)
        {
            return termMonths switch
            {
                1 => 3.5m,
                2 => 3.7m,
                3 => 3.8m,
                6 => 4.8m,
                9 => 4.9m,
                12 => 5.2m,
                18 => 5.5m,
                24 => 5.8m,
                36 => 5.8m,
                _ => 0
            };
        }
    }
}
