using VRAcademy.Api.Models;

namespace VRAcademy.Api.Services;

public interface IAuthService
{
    Result<AuthResponse> Register(RegisterUserRequest request);

    Result<AuthResponse> Login(LoginRequest request);

    Result<UserProfileResponse> GetCurrentUser(string accessToken);

    IReadOnlyCollection<UserProfileResponse> GetUsersForCompany(Guid companyId);

    Result<UserProfileResponse> CreateCompanyUser(Guid companyId, CreateCompanyUserRequest request);

    IReadOnlyCollection<CompanyResponse> GetCompanies();

    Result<CompanyResponse> GetCompany(Guid companyId);
}
