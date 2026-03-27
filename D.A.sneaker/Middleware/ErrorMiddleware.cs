using System.Net;

namespace D.A.sneaker.Middleware
{
    public class ErrorMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;

                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}