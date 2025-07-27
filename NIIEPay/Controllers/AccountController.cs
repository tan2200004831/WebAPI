using Microsoft.AspNetCore.Mvc;
using NIIEPay.Data;
using NIIEPay.Model;

namespace NIIEPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly NiiepayContext _context;

        public AccountController(NiiepayContext context)
        {
            _context = context;
        }

        // 🧩 3.1 API Đăng ký tài khoản
        [HttpPost("register")]
        public IActionResult Register(Account account)
        {
            if (_context.Accounts.Any(a => a.AccountNumber == account.AccountNumber))
                return BadRequest("Số tài khoản đã tồn tại.");

            _context.Accounts.Add(account);
            _context.SaveChanges();
            return Ok(account);
        }

        // 🧩 3.2 API Truy vấn thông tin tài khoản
        [HttpGet("{accountNumber}")]
        public IActionResult GetByAccountNumber(string accountNumber)
        {
            var account = _context.Accounts
                .Where(a => a.AccountNumber == accountNumber)
                .Select(a => new AccountDto
                {
                    AccountNumber = a.AccountNumber,
                    AccountHolder = a.AccountHolder,
                    Phone = a.Phone,
                    Email = a.Email,
                    ExpiryDate = a.ExpiryDate,
                    AvailableBalance = a.AvailableBalance
                })
                .FirstOrDefault();

            if (account == null) return NotFound();

            return Ok(account);
        }
    }
}
