using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using D.A.sneaker.Data;
using D.A.sneaker.Services;
using D.A.sneaker.Models;
using System.Security.Claims;

namespace D.A.sneaker.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly OllamaService _ai;

        public ChatController(AppDbContext context, OllamaService ai)
        {
            _context = context;
            _ai = ai;
        }

        // ══════════════════════════════════════════════════════
        //  POST /api/chat  –  Main chat endpoint
        // ══════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest(new { error = "Tin nhắn trống." });

            // ── Lấy userId từ JWT (nếu đã đăng nhập) hoặc dùng 0 (guest) ──
            int userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var parsedId))
                userId = parsedId;

            var query = message.ToLower().Trim();

            //──────────────────────────────────────────────────
            // 1. INTENT DETECTION – phân loại câu hỏi
            //──────────────────────────────────────────────────
            var intent = _ai.DetectIntent(message);

            //──────────────────────────────────────────────────
            // 2. INSTANT REPLY (không cần AI) – trả lời ngay
            //──────────────────────────────────────────────────
            var instant = _ai.GetInstantReply(intent, message);
            if (instant != null)
            {
                // Lưu lịch sử ngắn gọn
                await SaveHistory(userId, message, instant);
                return Ok(new { type = "text", message = instant, intent = intent.ToString() });
            }

            //──────────────────────────────────────────────────
            // 3. LOAD DATA từ DB
            //──────────────────────────────────────────────────
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Variants)
                .Take(30)
                .ToListAsync();

            var histories = await _context.ChatHistories
                .Where(h => h.UserId == userId || h.UserId == null)
                .OrderByDescending(x => x.CreatedAt)
                .Take(6)
                .ToListAsync();

            //──────────────────────────────────────────────────
            // 4. SMART PRODUCT MATCHING
            //──────────────────────────────────────────────────
            var matchedProducts = FindMatchingProducts(products, query);

            // ── Trả về danh sách sản phẩm nếu tìm thấy nhiều ──
            if (matchedProducts.Count > 1)
            {
                var replyText = $"Mình tìm thấy {matchedProducts.Count} mẫu phù hợp! 👟";
                await SaveHistory(userId, message, replyText);
                return Ok(new
                {
                    type = "products",
                    data = matchedProducts.Select(p => new {
                        p.Id, p.Name, p.Brand, p.Price, p.MainImage, p.Description,
                        stock = p.Variants?.Sum(v => v.Stock) ?? 0
                    }),
                    message = replyText,
                    intent = intent.ToString()
                });
            }

            //──────────────────────────────────────────────────
            // 5. CONTEXT CHO AI – cung cấp thêm thông tin
            //──────────────────────────────────────────────────
            string? extraContext = null;

            if (matchedProducts.Count == 1)
            {
                var p = matchedProducts.First();
                var totalStock = p.Variants?.Sum(v => v.Stock) ?? 0;
                extraContext = $"Khách đang quan tâm: {p.Name} ({p.Brand}) | Giá: {p.Price:n0}đ | " +
                               $"Tồn kho: {totalStock} đôi | Mô tả: {p.Description}";

                // Nhớ sản phẩm đang xem (state)
                await UpdateUserState(userId, p.Id);
            }
            else
            {
                // Kiểm tra xem user có đang trong ngữ cảnh sản phẩm không
                var state = await _context.UserChatStates
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (state?.CurrentProductId != null)
                {
                    var contextProduct = await _context.Products
                        .Include(p => p.Variants)
                        .FirstOrDefaultAsync(p => p.Id == state.CurrentProductId);

                    if (contextProduct != null)
                    {
                        var totalStock = contextProduct.Variants?.Sum(v => v.Stock) ?? 0;
                        extraContext = $"Khách đang hỏi tiếp về sản phẩm: {contextProduct.Name} ({contextProduct.Brand}) " +
                                      $"| Giá: {contextProduct.Price:n0}đ | Tồn kho: {totalStock} đôi";
                    }
                }
            }

            //──────────────────────────────────────────────────
            // 6. GỌI AI (Ollama)
            //──────────────────────────────────────────────────
            var aiReply = await _ai.AskAI(message, products, histories, extraContext);

            await SaveHistory(userId, message, aiReply);

            return Ok(new
            {
                type = "text",
                message = aiReply,
                intent = intent.ToString()
            });
        }

        // ══════════════════════════════════════════════════════
        //  GET /api/chat/history  –  Lấy lịch sử chat
        // ══════════════════════════════════════════════════════
        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int limit = 20)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var history = await _context.ChatHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedAt)
                .Take(Math.Min(limit, 50))
                .Select(h => new {
                    h.Id, h.Message, h.Response, h.CreatedAt
                })
                .ToListAsync();

            return Ok(history);
        }

        // ══════════════════════════════════════════════════════
        //  DELETE /api/chat/history  –  Xóa lịch sử
        // ══════════════════════════════════════════════════════
        [Authorize]
        [HttpDelete("history")]
        public async Task<IActionResult> ClearHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var old = _context.ChatHistories.Where(h => h.UserId == userId);
            _context.ChatHistories.RemoveRange(old);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa lịch sử chat." });
        }

        //──────────────────────────────────────────────────────
        // HELPER: Smart product matching
        //──────────────────────────────────────────────────────
        private static List<Product> FindMatchingProducts(List<Product> products, string query)
        {
            // Các keyword đặc biệt
            var styleMap = new Dictionary<string, string[]> {
                { "chạy bộ",    new[]{ "run","chạy","training","marathon" } },
                { "tập gym",    new[]{ "gym","training","sport","tape" } },
                { "đi học",     new[]{ "học","school","casual","đường phố" } },
                { "bóng rổ",    new[]{ "basketball","jordan","court" } },
                { "êm",         new[]{ "êm","cushion","foam","boost" } },
                { "nhẹ",        new[]{ "nhẹ","light","flyknit","mesh" } },
                { "bền",        new[]{ "bền","durable","leather","da" } },
            };

            return products.Where(p =>
            {
                var name = p.Name.ToLower();
                var brand = p.Brand.ToLower();
                var desc = (p.Description ?? "").ToLower();

                // Match tên hoặc brand trực tiếp
                if (query.Contains(name) || name.Contains(query.Split(' ').FirstOrDefault() ?? ""))
                    return true;
                if (query.Contains(brand))
                    return true;

                // Match theo style keywords
                foreach (var kv in styleMap)
                    if (query.Contains(kv.Key) && kv.Value.Any(k => desc.Contains(k) || name.Contains(k)))
                        return true;

                return false;
            }).Take(8).ToList();
        }

        //──────────────────────────────────────────────────────
        // HELPER: Lưu ChatHistory
        //──────────────────────────────────────────────────────
        private async Task SaveHistory(int userId, string message, string response)
        {
            try
            {
                _context.ChatHistories.Add(new ChatHistory
                {
                    UserId    = userId > 0 ? userId : null,
                    Message   = message[..Math.Min(message.Length, 1000)],
                    Response  = response[..Math.Min(response.Length, 2000)],
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Chat] SaveHistory error: {ex.Message}");
            }
        }

        //──────────────────────────────────────────────────────
        // HELPER: Cập nhật UserChatState
        //──────────────────────────────────────────────────────
        private async Task UpdateUserState(int userId, int productId)
        {
            try
            {
                var state = await _context.UserChatStates
                    .FirstOrDefaultAsync(s => s.UserId == userId);
                if (state == null)
                {
                    state = new UserChatState { UserId = userId };
                    _context.UserChatStates.Add(state);
                }
                state.CurrentProductId = productId;
                state.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            catch { /* Bỏ qua lỗi state – không ảnh hưởng chat */ }
        }
    }
}
