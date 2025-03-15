namespace Demo0.Models.Entity;

public class User
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public ICollection<UserRole>? UserRoles { get; set; }
}
