namespace VRAcademy.Api.Models;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
