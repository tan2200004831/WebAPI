using Microsoft.AspNetCore.Mvc;
using NIIEPay.Data;
using NIIEPay.Dto;

namespace NIIEPay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepositController : Controller
    {
        private readonly NiiepayContext _context;

        public DepositController(NiiepayContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit([FromBody] DepositDto dto)
        {
            var account = await _context.Accounts.FindAsync(dto.AccountId);
            if (account == null)
                return NotFound("Account not found");

            account.AvailableBalance += dto.Amount;

            // Nếu dùng bảng Deposit riêng
            var deposit = new Deposit
            {
                AccountId = dto.AccountId,
                Amount = dto.Amount,
                DepositDate = DateTime.Now
            };

            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();

            return Ok("Deposit successful");
        }
    }
}
