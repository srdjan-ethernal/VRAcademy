namespace VRAcademy.Api.Models;

public sealed record ResetPasswordRequest(
    string Email,
    string NewPassword);
