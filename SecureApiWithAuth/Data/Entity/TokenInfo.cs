namespace SecureApiWithAuth.Data.Entity
{
    public class TokenInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
