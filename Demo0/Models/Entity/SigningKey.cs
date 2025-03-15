namespace Demo0.Models.Entity;

public class SigningKey
{
    public int Id { get; set; }
    public string? KeyId { get; set; }
    public string? PrivateKey { get; set; }
    public string? PublicKey { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
