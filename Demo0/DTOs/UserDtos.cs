namespace Demo0.DTOs;

public record RegisterDto(
    string Username,
    string Email,
    string Password
);

public record UpdateDto(
    string Username,
    string Email,
    string Password
);

public record ProfileDto(
    int Id,
    string Username,
    string Email,
    List<string> Roles
);

public record LoginDto(
    string Email,
    string Password,
    string ClientId
);
