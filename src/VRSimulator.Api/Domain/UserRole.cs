namespace VRSimulator.Api.Domain;

public enum UserRole
{
    SystemAdministrator,
    CompanyAdministrator,
    User,
    CompanyAdmin = CompanyAdministrator,
    Instructor = User,
    Employee = User
}

