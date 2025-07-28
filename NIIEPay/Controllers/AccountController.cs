using Microsoft.AspNetCore.Mvc;
using NIIEPay.Data;
using NIIEPay.Dto;

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

        // API Đăng ký tài khoản
        [HttpPost("register")]
        public IActionResult Register(RegisterAccountDto dto)
        {
            if (_context.Accounts.Any(a => a.AccountNumber == dto.AccountNumber))
                return BadRequest("Số tài khoản đã tồn tại.");

            var account = new Account
            {
                AccountNumber = dto.AccountNumber,
                Password = dto.Password,
                AccountHolder = dto.AccountHolder,
                Phone = dto.Phone,
                Email = dto.Email,
                CitizenId = dto.CitizenId,
                ExpiryDate = dto.ExpiryDate,
                AvailableBalance = 0
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();
            return Ok(account);
        }

    }
}
