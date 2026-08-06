using VRAcademy.Api.Models;

namespace VRAcademy.Api.Services;

public interface IAuthService
{
    Result<AuthResponse> Register(RegisterUserRequest request);

    Result<AuthResponse> Login(LoginRequest request);

    Result<AuthResponse> LoginWithExternalProvider(ExternalLoginRequest request);

    Result<UserProfileResponse> GetCurrentUser(string accessToken);

    IReadOnlyCollection<UserProfileResponse> GetUsersForCompany(Guid companyId);

    Result<UserProfileResponse> CreateCompanyUser(Guid companyId, CreateCompanyUserRequest request);

    Result<InvitationResponse> InviteCompanyUser(Guid companyId, InviteUserRequest request, string baseUrl);

    Result<UserProfileResponse> ResetPassword(Guid? companyId, ResetPasswordRequest request);

    IReadOnlyCollection<CompanyResponse> GetCompanies();

    Result<CompanyResponse> GetCompany(Guid companyId);

    Result<CompanyResponse> CreateCompany(CreateCompanyRequest request);

    Result<CompanyResponse> UpdateCompanySubscription(Guid companyId, UpdateCompanySubscriptionRequest request);
}
