using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using D.A.sneaker.Data;
using D.A.sneaker.Services;
using D.A.sneaker.Models;
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

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] string message)
        {
            var query = message.ToLower();
            // load state (demo: 1 user duy nhất)
            var userId = 1; // demo

            var state = await _context.UserChatStates
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (state == null)
            {
                state = new UserChatState();
                _context.UserChatStates.Add(state);
                await _context.SaveChangesAsync();
            }
            //--------------------------------
            // LOAD DATA
            //--------------------------------

            var products = await _context.Products
.Take(20)
.ToListAsync();

            var histories = await _context.ChatHistories
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync();

            //--------------------------------
            // SEARCH
            //--------------------------------

            var matchedProducts = products
                .Where(p =>
                    query.Contains(p.Name.ToLower()) ||
                    query.Contains(p.Brand.ToLower()) ||
                    (p.Description != null && query.Contains("chạy") && p.Description.ToLower().Contains("chạy")) ||
                    (p.Description != null && query.Contains("êm") && p.Description.ToLower().Contains("êm")) ||
                    (p.Description != null && query.Contains("nhẹ") && p.Description.ToLower().Contains("nhẹ"))
                )
                .ToList();

            //--------------------------------
            // SAVE HISTORY FIRST
            //--------------------------------

            var chat = new ChatHistory
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.Now
            };
            //--------------------------------
            // RETURN PRODUCTS
            //--------------------------------

            if (matchedProducts.Count > 1)
            {
                chat.Response = $"Returned {matchedProducts.Count} products";

                _context.ChatHistories.Add(chat);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    type = "products",
                    data = matchedProducts
                });
            }

            if (matchedProducts.Count == 1)
            {

                var product = matchedProducts.First();
                state.CurrentProductId = product.Id;
                state.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                var detailContext = $@"
KHÁCH ĐANG QUAN TÂM SẢN PHẨM:
Tên: {product.Name}
Hãng: {product.Brand}
Giá: {product.Price:n0} VND
Mô tả: {product.Description}
";

                var AiReply = await _ai.AskAI(
                    message + "\n" + detailContext,
                    products,
                    histories);

                return Ok(new { type = "text", message = AiReply });
            }
            // nếu không tìm thấy nhưng user đang xem sản phẩm trước đó
            if (matchedProducts.Count == 0 && state.CurrentProductId != null)
            {
                var currentProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == state.CurrentProductId);

                if (currentProduct != null)
                {
                    var detailContext = $@"
KHÁCH ĐANG HỎI TIẾP VỀ SẢN PHẨM:
Tên: {currentProduct.Name}
Hãng: {currentProduct.Brand}
Giá: {currentProduct.Price:n0} VND
Mô tả: {currentProduct.Description}
";

                    var aiReplyFollow = await _ai.AskAI(
                        message + "\n" + detailContext,
                        products,
                        histories);

                    return Ok(new { type = "text", message = aiReplyFollow });
                }
            }
            //--------------------------------
            // CALL AI
            //--------------------------------

            var aiReply = await _ai.AskAI(message, products, histories);

            chat.Response = aiReply;

            _context.ChatHistories.Add(chat);
            await _context.SaveChangesAsync();

            //--------------------------------

            return Ok(new
            {
                type = "text",
                message = aiReply
            });
        }

    }
}

