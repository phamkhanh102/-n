namespace D.A.sneaker.DTOs
{
    public class LoginDTO
    {
        /// <summary>
        /// Chấp nhận email HOẶC username để đăng nhập
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
