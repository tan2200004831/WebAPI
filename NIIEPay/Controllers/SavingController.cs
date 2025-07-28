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

            // 🟡 BỔ SUNG KIỂM TRA 1: kỳ hạn hợp lệ
            var validTerms = new[] { 1, 2, 3, 6, 9, 12, 18, 24, 36 };
            if (!validTerms.Contains(saving.TermMonths))
                return BadRequest(new { status = "FAIL", message = "Kỳ hạn gửi không hợp lệ." });

            // 🟡 BỔ SUNG KIỂM TRA 2: số dư sau gửi còn ≥ 50.000
            if (account.AvailableBalance - saving.Amount < 50000)
                return BadRequest(new { status = "FAIL", message = "Số dư sau khi gửi phải còn tối thiểu 50.000đ." });

            // Nếu đạt yêu cầu → trừ tiền và lưu
            account.AvailableBalance -= saving.Amount;
            saving.MaturityDate = saving.StartDate.AddMonths(saving.TermMonths);

            _context.Savings.Add(saving);
            _context.SaveChanges();

            return Ok(new
            {
                status = "SUCCESS",
                savingId = saving.SavingId,
                termMonths = saving.TermMonths,
                interestRate = GetRate(saving.TermMonths),
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
            new SavingRateDto { TermMonths = 1, InterestRate = 3.5 },
            new SavingRateDto { TermMonths = 3, InterestRate = 4.0 },
            new SavingRateDto { TermMonths = 6, InterestRate = 5.2 },
            new SavingRateDto { TermMonths = 12, InterestRate = 6.8 },
            new SavingRateDto { TermMonths = 24, InterestRate = 7.1 },
            new SavingRateDto { TermMonths = 36, InterestRate = 7.5 }
        };

            return Ok(rates);
        }

        // ĐĂNG thêm GetRate
        private double GetRate(int termMonths)
        {
            return termMonths switch
            {
                1 => 3.5,
                2 => 3.7,
                3 => 3.8,
                6 => 4.8,
                9 => 4.9,
                12 => 5.2,
                18 => 5.5,
                24 => 5.8,
                36 => 5.8,
                _ => 0
            };
        }
    }
}
