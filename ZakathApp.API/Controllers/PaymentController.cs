using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZakathApp.API.Data;
using ZakathApp.API.DTOs;
using ZakathApp.API.Helpers;
using ZakathApp.API.Models;

namespace ZakathApp.API.Controllers
{
    [Route("api/Payment")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly ZakathDbContext _context;
        private readonly HijriDateHelper _hijriHelper;

        public PaymentController(ZakathDbContext context, HijriDateHelper hijriHelper)
        {
            _context     = context;
            _hijriHelper = hijriHelper;
        }

        // POST /api/Payment
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            // override UserID with the authenticated user (security)
            dto.UserID = userId;

            var payment = new ZakathPayment
            {
                UserID               = dto.UserID,
                CalculationID        = dto.CalculationID,
                PaymentAmount        = dto.PaymentAmount,
                PaymentDate          = dto.PaymentDate,
                HijriPaymentDate     = _hijriHelper.ConvertToHijri(dto.PaymentDate),
                RecipientName        = dto.RecipientName,
                RecipientCategory    = dto.RecipientCategory,
                RecipientContact     = dto.RecipientContact,
                PaymentMethod        = dto.PaymentMethod,
                TransactionReference = dto.TransactionReference,
                Notes                = dto.Notes,
                IsVerified           = false,
                CreatedDate          = DateTime.Now
            };

            _context.ZakathPayments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Payment recorded successfully", Data = new { PaymentID = payment.PaymentID } });
        }

        // GET /api/Payment
        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var payments = await _context.ZakathPayments
                .Where(p => p.UserID == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var dtos = payments.Select(p => new PaymentDto
            {
                PaymentID            = p.PaymentID,
                UserID               = p.UserID,
                CalculationID        = p.CalculationID,
                PaymentAmount        = p.PaymentAmount,
                PaymentDate          = p.PaymentDate,
                HijriPaymentDate     = p.HijriPaymentDate,
                RecipientName        = p.RecipientName,
                RecipientCategory    = p.RecipientCategory,
                PaymentMethod        = p.PaymentMethod,
                TransactionReference = p.TransactionReference,
                IsVerified           = p.IsVerified
            }).ToList();

            return Ok(new { Success = true, Message = "Payments retrieved", Data = dtos });
        }

        // GET /api/Payment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var p = await _context.ZakathPayments
                .FirstOrDefaultAsync(x => x.PaymentID == id && x.UserID == userId);

            if (p == null)
                return NotFound(new { Success = false, Message = "Payment not found" });

            return Ok(new
            {
                Success = true,
                Message = "Payment retrieved",
                Data    = new PaymentDto
                {
                    PaymentID            = p.PaymentID,
                    UserID               = p.UserID,
                    CalculationID        = p.CalculationID,
                    PaymentAmount        = p.PaymentAmount,
                    PaymentDate          = p.PaymentDate,
                    HijriPaymentDate     = p.HijriPaymentDate,
                    RecipientName        = p.RecipientName,
                    RecipientCategory    = p.RecipientCategory,
                    PaymentMethod        = p.PaymentMethod,
                    TransactionReference = p.TransactionReference,
                    IsVerified           = p.IsVerified
                }
            });
        }

        // GET /api/Payment/total?year=2025
        [HttpGet("total")]
        public async Task<IActionResult> GetTotalPaid([FromQuery] int year = 0)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            if (year == 0) year = DateTime.Now.Year;

            var total = await _context.ZakathPayments
                .Where(p => p.UserID == userId && p.PaymentDate.Year == year)
                .SumAsync(p => p.PaymentAmount);

            var count = await _context.ZakathPayments
                .CountAsync(p => p.UserID == userId && p.PaymentDate.Year == year);

            return Ok(new { Success = true, Message = "Total paid retrieved", Data = new { Year = year, TotalAmount = total, PaymentCount = count } });
        }

        // DELETE /api/Payment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            int userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var p = await _context.ZakathPayments
                .FirstOrDefaultAsync(x => x.PaymentID == id && x.UserID == userId);

            if (p == null)
                return NotFound(new { Success = false, Message = "Payment not found" });

            _context.ZakathPayments.Remove(p);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Payment deleted" });
        }

        private int GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
