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
        // ĐĂNG fix đăng ký tài khoản (ràng buộc số dư khởi tạo tối thiểu 100,000 đ)
        [HttpPost("register")]
        public IActionResult Register(RegisterAccountDto dto)
        {
            if (_context.Accounts.Any(a => a.AccountNumber == dto.AccountNumber))
                return BadRequest(new { status = "FAIL", message = "Số tài khoản đã tồn tại." });

            if (dto.InitialBalance < 100000)
                return BadRequest(new { status = "FAIL", message = "Số dư ban đầu phải từ 100.000đ." });

            if (dto.ExpiryDate <= DateOnly.FromDateTime(DateTime.Now))
                return BadRequest(new { status = "FAIL", message = "CCCD đã hết hạn." });


            var account = new Account
            {
                AccountNumber = dto.AccountNumber,
                Password = dto.Password,
                AccountHolder = dto.AccountHolder,
                Phone = dto.Phone,
                Email = dto.Email,
                CitizenId = dto.CitizenId,
                ExpiryDate = dto.ExpiryDate,
                AvailableBalance = dto.InitialBalance
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();

            return Ok(new
            {
                status = "SUCCESS",
                accountId = account.AccountId,
                message = "Tạo tài khoản thành công"
            });
        }


        // ĐĂNG thêm truy vấn thông tin
        [HttpGet("{accountNumber}")]
        public ActionResult<AccountInfoDto> GetAccount(string accountNumber)
        {
            var account = _context.Accounts
                .Where(a => a.AccountNumber == accountNumber)
                .Select(a => new AccountInfoDto
                {
                    AccountNumber = a.AccountNumber,
                    AccountHolder = a.AccountHolder,
                    Phone = a.Phone,
                    CitizenId = a.CitizenId,
                    ExpiryDate = a.ExpiryDate.ToDateTime(TimeOnly.MinValue),
                    AvailableBalance = a.AvailableBalance
                })
                .FirstOrDefault();

            if (account == null)
                return NotFound("Không tìm thấy tài khoản.");

            return Ok(account);
        }
    }
}
