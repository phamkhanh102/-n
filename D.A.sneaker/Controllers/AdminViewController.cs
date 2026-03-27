using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace D.A.sneaker.Controllers
{
    public class AdminViewController : Controller
    {
        private readonly IHttpClientFactory _http;

        public AdminViewController(IHttpClientFactory http)
        {
            _http = http;
        }

        public async Task<IActionResult> Dashboard()
        {
            var client = _http.CreateClient();

            // ⚠️ tạm thời hardcode để test
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "TOKEN_ADMIN_CỦA_BẠN");

            var res = await client.GetAsync("https://localhost:7244/api/admin/dashboard");

            var json = await res.Content.ReadAsStringAsync();

            ViewBag.Data = json;

            return View();
        }
    }
}