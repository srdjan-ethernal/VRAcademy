using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;
using VRAcademy.Api.Domain;
using VRAcademy.Api.Models;
using VRAcademy.Api.Persistence;
using VRAcademy.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var databaseProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var autoCreateDatabase = builder.Configuration.GetValue<bool>("Database:EnsureCreated");
var connectionStringCandidates = new[]
{
    new ConnectionStringCandidate("DATABASE_URL", builder.Configuration["DATABASE_URL"]),
    new ConnectionStringCandidate("NEON_DATABASE_URL", builder.Configuration["NEON_DATABASE_URL"]),
    new ConnectionStringCandidate("AZURE_SQL_CONNECTION_STRING", builder.Configuration["AZURE_SQL_CONNECTION_STRING"]),
    new ConnectionStringCandidate("ConnectionStrings:TrainingDatabase", builder.Configuration.GetConnectionString("TrainingDatabase")),
    new ConnectionStringCandidate("ConnectionStrings__TrainingDatabase", builder.Configuration["ConnectionStrings__TrainingDatabase"])
};
var normalizedConnectionStringCandidates = connectionStringCandidates
    .Select(candidate => candidate with { Value = NormalizeConnectionString(candidate.Value, databaseProvider) })
    .ToArray();
var usableConnectionString = normalizedConnectionStringCandidates
    .FirstOrDefault(candidate => IsUsableConnectionString(candidate.Value, databaseProvider));
var fallbackConnectionString = normalizedConnectionStringCandidates
    .FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate.Value));
var configuredInMemoryServices = databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase)
    || databaseProvider.Equals("Memory", StringComparison.OrdinalIgnoreCase);
var fallbackToInMemory = builder.Configuration.GetValue("Database:FallbackToInMemory", true);
var useInMemoryServices = configuredInMemoryServices || (fallbackToInMemory && usableConnectionString is null);
var selectedConnectionString = useInMemoryServices
    ? new ConnectionStringCandidate(configuredInMemoryServices ? "InMemory" : "InMemoryFallback", null)
    : IsPostgreSqlProvider(databaseProvider) ? usableConnectionString : usableConnectionString ?? fallbackConnectionString;
var invalidConnectionStringSources = normalizedConnectionStringCandidates
    .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Value) && !IsUsableConnectionString(candidate.Value, databaseProvider))
    .Select(candidate => candidate.Source)
    .ToArray();
var rawConnectionString = useInMemoryServices
    ? fallbackConnectionString?.Value
    : selectedConnectionString?.Value;
var connectionStringSource = selectedConnectionString?.Source ?? "none";
var connectionString = selectedConnectionString?.Value;
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
var allowAnyOrigin = builder.Configuration.GetValue<bool>("Cors:AllowAnyOrigin");
const string frontendCorsPolicy = "Frontend";
const string GoogleOAuthStateCookieName = "vr_academy_google_oauth_state";

if (useInMemoryServices && invalidConnectionStringSources.Length > 0)
{
    Console.WriteLine($"Database startup: using in-memory services because these connection string sources are invalid: {string.Join(", ", invalidConnectionStringSources)}");
}

if (!useInMemoryServices)
{
    builder.Services.AddDbContext<TrainingDbContext>(options =>
    {
        if (databaseProvider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)
            || databaseProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:TrainingDatabase is required when Database:Provider is PostgreSql.");
            }

            options.UseNpgsql(connectionString);
            return;
        }

        var sqlServerConnectionString = connectionString
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=VRAcademyTraining;Trusted_Connection=True;MultipleActiveResultSets=true";
        if (!sqlServerConnectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("SQL Server connection string must include a 'Server=' segment. Check the AZURE_SQL_CONNECTION_STRING or ConnectionStrings__TrainingDatabase secret value.");
        }

        options.UseSqlServer(sqlServerConnectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorNumbersToAdd: null);
        });
    });
}
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        if (allowAnyOrigin)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
if (useInMemoryServices)
{
    builder.Services.AddSingleton<ITrainingRepository, InMemoryTrainingRepository>();
    builder.Services.AddSingleton<IAuthService, InMemoryAuthService>();
}
else
{
    builder.Services.AddScoped<ITrainingRepository, EfTrainingRepository>();
    builder.Services.AddScoped<IAuthService, EfAuthService>();
}
builder.Services.AddScoped<IEmailNotificationService, SmtpEmailNotificationService>();
builder.Services.AddHttpClient();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (!useInMemoryServices && autoCreateDatabase)
{
    using var scope = app.Services.CreateScope();
    Console.WriteLine($"Database startup: provider={databaseProvider}; source={connectionStringSource}; rawLength={rawConnectionString?.Length ?? 0}; normalizedLength={connectionString?.Length ?? 0}; startsWithServer={connectionString?.StartsWith("Server=", StringComparison.OrdinalIgnoreCase) ?? false}");
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TrainingDbContext>();
        dbContext.Database.EnsureCreated();
        EnsureCompatibilityColumns(dbContext, databaseProvider);
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Database startup failed: {exception.GetType().Name}: {exception.Message}");
        throw;
    }
}

if (!useInMemoryServices)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TrainingDbContext>();
        DemoAccountSeeder.EnsureDemoAccount(dbContext, app.Configuration);
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Demo account seed skipped: {exception.GetType().Name}: {exception.Message}");
    }
}

app.UseCors(frontendCorsPolicy);
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/", (IWebHostEnvironment environment) =>
{
    var indexPath = Path.Combine(environment.WebRootPath ?? environment.ContentRootPath, "index.html");
    return Results.File(indexPath, "text/html");
});

app.MapGet("/api", () => Results.Ok(new
{
    name = "VR Academy Training API",
    tenancy = "Company-scoped data is resolved from the Bearer token.",
    endpoints = new[]
    {
        "GET /api/health",
        "POST /api/auth/register",
        "POST /api/auth/login",
        "GET /api/auth/google/start",
        "GET /api/auth/google/callback",
        "GET /api/auth/me",
        "POST /api/auth/change-password",
        "GET /api/system/companies",
        "POST /api/system/companies",
        "PATCH /api/system/companies/{companyId}/subscription",
        "DELETE /api/system/companies/{companyId}",
        "GET /api/users",
        "POST /api/users",
        "POST /api/users/reset-password",
        "POST /api/invitations",
        "GET /api/companies",
        "GET /api/dashboard/summary",
        "GET /api/scenarios",
        "GET /api/courses",
        "GET /api/workers",
        "POST /api/workers",
        "POST /api/enrollments",
        "GET /api/enrollments",
        "POST /api/enrollments/{enrollmentId}/complete",
        "GET /api/certificates",
        "GET /api/certificates/verify/{certificateNumber}",
        "GET /api/workers/{workerId}/certificates",
        "GET /api/certificates/{certificateId}",
        "POST /api/notifications/reminders",
        "GET /api/worker-portal/me",
        "POST /api/worker-portal/enrollments/{enrollmentId}/start",
        "POST /api/worker-portal/enrollments/{enrollmentId}/complete",
        "POST /api/exams/{examId}/result"
    }
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    timestampUtc = DateTimeOffset.UtcNow
}));

app.MapGet("/api/diagnostics/database", (IServiceProvider serviceProvider) =>
{
    try
    {
        if (useInMemoryServices)
        {
            return Results.Ok(new
            {
                status = "ok",
                provider = databaseProvider,
                source = "InMemory",
                canConnect = true,
                pendingMigrations = 0
            });
        }

        using var diagnosticScope = serviceProvider.CreateScope();
        var dbContext = diagnosticScope.ServiceProvider.GetRequiredService<TrainingDbContext>();
        var canConnect = dbContext.Database.CanConnect();
        return Results.Ok(new
        {
            status = canConnect ? "ok" : "unavailable",
            provider = databaseProvider,
            source = connectionStringSource,
            canConnect,
            pendingMigrations = dbContext.Database.GetPendingMigrations().Count()
        });
    }
    catch (Exception exception)
    {
        return Results.Json(new
        {
            status = "error",
            provider = databaseProvider,
            source = connectionStringSource,
            errorType = exception.GetType().Name,
            message = exception.Message
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapPost("/api/auth/register", (RegisterUserRequest request, IAuthService authService) =>
{
    var result = authService.Register(request);
    return result.Match(
        auth => Results.Created($"/api/users/{auth.User.Id}", auth),
        error => Results.BadRequest(new ProblemResponse(error)));
});

app.MapPost("/api/auth/login", (LoginRequest request, IAuthService authService) =>
{
    var result = authService.Login(request);
    return result.Match(
        auth => Results.Ok(auth),
        error => Results.BadRequest(new ProblemResponse(error)));
});

app.MapGet("/api/auth/google/start", (
    HttpRequest request,
    HttpResponse response,
    IConfiguration configuration,
    string? mode,
    string? companyName,
    string? returnUrl) =>
{
    var googleClientId = GetGoogleClientId(configuration);
    if (string.IsNullOrWhiteSpace(googleClientId))
    {
        return RedirectToLoginWithError("Google login nije konfigurisan.");
    }

    var normalizedMode = string.Equals(mode, "register", StringComparison.OrdinalIgnoreCase)
        ? "register"
        : "login";
    if (normalizedMode == "register" && string.IsNullOrWhiteSpace(companyName))
    {
        return RedirectToLoginWithError("Naziv kompanije je obavezan za Google registraciju.");
    }

    var nonce = CreateBase64Url(RandomNumberGenerator.GetBytes(32));
    var state = EncodeGoogleOAuthState(new GoogleOAuthState(
        nonce,
        normalizedMode,
        normalizedMode == "register" ? companyName?.Trim() : null,
        NormalizeLocalReturnUrl(returnUrl)));
    response.Cookies.Append(GoogleOAuthStateCookieName, nonce, new CookieOptions
    {
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = request.IsHttps,
        Expires = DateTimeOffset.UtcNow.AddMinutes(10)
    });

    var authorizationUrl = "https://accounts.google.com/o/oauth2/v2/auth" + BuildQueryString(new Dictionary<string, string?>
    {
        ["client_id"] = googleClientId,
        ["redirect_uri"] = GetGoogleCallbackUrl(request),
        ["response_type"] = "code",
        ["scope"] = "openid email profile",
        ["state"] = state,
        ["prompt"] = "select_account"
    });

    return Results.Redirect(authorizationUrl);
});

app.MapGet("/api/auth/google/callback", async (
    HttpRequest request,
    HttpResponse response,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IAuthService authService,
    string? code,
    string? state,
    string? error) =>
{
    if (!string.IsNullOrWhiteSpace(error))
    {
        return RedirectToLoginWithError($"Google prijava nije uspela: {error}");
    }

    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
    {
        return RedirectToLoginWithError("Google nije vratio kompletan odgovor.");
    }

    if (!TryDecodeGoogleOAuthState(state, out var oauthState) ||
        !request.Cookies.TryGetValue(GoogleOAuthStateCookieName, out var expectedNonce) ||
        !string.Equals(oauthState.Nonce, expectedNonce, StringComparison.Ordinal))
    {
        return RedirectToLoginWithError("Google prijava je istekla. Pokusajte ponovo.");
    }

    response.Cookies.Delete(GoogleOAuthStateCookieName);

    var googleClientId = GetGoogleClientId(configuration);
    var googleClientSecret = GetGoogleClientSecret(configuration);
    if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
    {
        return RedirectToLoginWithError("Google login nije konfigurisan.");
    }

    var httpClient = httpClientFactory.CreateClient();
    var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["client_id"] = googleClientId,
        ["client_secret"] = googleClientSecret,
        ["code"] = code,
        ["grant_type"] = "authorization_code",
        ["redirect_uri"] = GetGoogleCallbackUrl(request)
    }));

    if (!tokenResponse.IsSuccessStatusCode)
    {
        return RedirectToLoginWithError("Google token nije dobijen.");
    }

    await using var tokenStream = await tokenResponse.Content.ReadAsStreamAsync();
    using var tokenJson = await JsonDocument.ParseAsync(tokenStream);
    var accessToken = GetJsonString(tokenJson.RootElement, "access_token");
    if (string.IsNullOrWhiteSpace(accessToken))
    {
        return RedirectToLoginWithError("Google nije vratio access token.");
    }

    using var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://openidconnect.googleapis.com/v1/userinfo");
    userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var userInfoResponse = await httpClient.SendAsync(userInfoRequest);
    if (!userInfoResponse.IsSuccessStatusCode)
    {
        return RedirectToLoginWithError("Google profil nije procitan.");
    }

    await using var userInfoStream = await userInfoResponse.Content.ReadAsStreamAsync();
    using var userInfoJson = await JsonDocument.ParseAsync(userInfoStream);
    var googleProfile = ReadGoogleProfile(userInfoJson.RootElement);
    if (googleProfile is null)
    {
        return RedirectToLoginWithError("Google nalog nije vratio validan email.");
    }

    var authResult = authService.LoginWithExternalProvider(new ExternalLoginRequest(
        googleProfile.Email,
        googleProfile.FirstName,
        googleProfile.LastName,
        oauthState.Mode == "register" ? oauthState.CompanyName : null));

    return authResult.Match(
        auth => Results.Content(BuildGoogleAuthSuccessHtml(auth), "text/html", Encoding.UTF8),
        authError => RedirectToLoginWithError(authError));
});

app.MapGet("/api/auth/me", (HttpRequest request, IAuthService authService) =>
{
    var token = GetBearerToken(request);
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.Unauthorized();
    }

    var result = authService.GetCurrentUser(token);
    return result.Match(
        user => Results.Ok(user),
        _ => Results.Unauthorized());
});

app.MapPost("/api/auth/change-password", (ChangePasswordRequest request, HttpRequest httpRequest, IAuthService authService) =>
{
    var token = GetBearerToken(httpRequest);
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.Unauthorized();
    }

    var result = authService.ChangePassword(token, request);
    return result.Match(
        auth => Results.Ok(auth),
        error => Results.BadRequest(new ProblemResponse(error)));
});

app.MapGet("/api/system/companies", (HttpRequest request, IAuthService authService) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user => IsSystemAdministrator(user)
            ? Results.Ok(authService.GetCompanies())
            : Results.Forbid(),
        _ => Results.Unauthorized());
});

app.MapPost("/api/system/companies", (CreateCompanyRequest request, HttpRequest httpRequest, IAuthService authService) =>
{
    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsSystemAdministrator(user))
            {
                return Results.Forbid();
            }

            var result = authService.CreateCompany(request);
            return result.Match(
                company => Results.Created($"/api/system/companies/{company.Id}", company),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapPatch("/api/system/companies/{companyId:guid}/subscription", (
    Guid companyId,
    UpdateCompanySubscriptionRequest request,
    HttpRequest httpRequest,
    IAuthService authService) =>
{
    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsSystemAdministrator(user))
            {
                return Results.Forbid();
            }

            var result = authService.UpdateCompanySubscription(companyId, request);
            return result.Match(
                company => Results.Ok(company),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapDelete("/api/system/companies/{companyId:guid}", (
    Guid companyId,
    HttpRequest httpRequest,
    IAuthService authService) =>
{
    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsSystemAdministrator(user))
            {
                return Results.Forbid();
            }

            if (user.CompanyId == companyId)
            {
                return Results.BadRequest(new ProblemResponse("Ne mozete obrisati kompaniju u kojoj je vas administratorski nalog."));
            }

            var result = authService.DeleteCompany(companyId);
            return result.Match(
                _ => Results.NoContent(),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapGet("/api/users", (HttpRequest request, IAuthService authService) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user => IsCompanyAdministrator(user)
            ? Results.Ok(authService.GetUsersForCompany(user.CompanyId))
            : Results.Forbid(),
        _ => Results.Unauthorized());
});

app.MapPost("/api/users", (CreateCompanyUserRequest request, HttpRequest httpRequest, IAuthService authService) =>
{
    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsCompanyAdministrator(user))
            {
                return Results.Forbid();
            }

            var result = authService.CreateCompanyUser(user.CompanyId, request);
            return result.Match(
                createdUser => Results.Created($"/api/users/{createdUser.Id}", createdUser),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapPost("/api/users/reset-password", (ResetPasswordRequest request, HttpRequest httpRequest, IAuthService authService) =>
{
    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsCompanyAdministrator(user))
            {
                return Results.Forbid();
            }

            Guid? companyScope = IsSystemAdministrator(user) ? null : user.CompanyId;
            var result = authService.ResetPassword(companyScope, request);
            return result.Match(
                updatedUser => Results.Ok(updatedUser),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapPost("/api/invitations", (
    InviteUserRequest request,
    HttpRequest httpRequest,
    IAuthService authService,
    ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsCompanyAdministrator(user))
            {
                return Results.Forbid();
            }

            if (repository.GetWorkerByEmail(user.CompanyId, request.Email) is null &&
                !string.IsNullOrWhiteSpace(request.EmployeeNumber))
            {
                var workerResult = repository.CreateWorker(user.CompanyId, new CreateWorkerRequest(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.EmployeeNumber,
                    string.IsNullOrWhiteSpace(request.Department) ? "-" : request.Department));

                if (!workerResult.IsSuccess)
                {
                    return Results.BadRequest(new ProblemResponse(workerResult.Error ?? "Radnik nije kreiran."));
                }
            }

            var result = authService.InviteCompanyUser(user.CompanyId, request, GetApplicationBaseUrl(httpRequest));
            return result.Match(
                invitation => Results.Created($"/api/users/{invitation.User.Id}", invitation),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapGet("/api/companies", (HttpRequest request, IAuthService authService) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user => IsCompanyAdministrator(user)
            ? authService.GetCompany(user.CompanyId).Match(
                company => Results.Ok(new[] { company }),
                error => Results.BadRequest(new ProblemResponse(error)))
            : Results.Forbid(),
        _ => Results.Unauthorized());
});

app.MapGet("/api/dashboard/summary", (HttpRequest request, IAuthService authService, ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user => IsCompanyAdministrator(user)
            ? Results.Ok(repository.GetDashboardSummary(user.CompanyId))
            : Results.Forbid(),
        _ => Results.Unauthorized());
});

app.MapGet("/api/scenarios", (ITrainingRepository repository) =>
{
    return Results.Ok(repository.GetScenarios());
});

app.MapGet("/api/courses", (ITrainingRepository repository) =>
{
    return Results.Ok(repository.GetCourses());
});

app.MapGet("/api/workers", (HttpRequest request, IAuthService authService, ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user => IsCompanyAdministrator(user)
            ? Results.Ok(repository.GetWorkers(user.CompanyId))
            : Results.Forbid(),
        _ => Results.Unauthorized());
});

app.MapPost("/api/workers", (CreateWorkerRequest request, HttpRequest httpRequest, IAuthService authService, ITrainingRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
    {
        return Results.BadRequest(new ProblemResponse("Ime i prezime radnika su obavezni."));
    }

    if (string.IsNullOrWhiteSpace(request.EmployeeNumber))
    {
        return Results.BadRequest(new ProblemResponse("Broj zaposlenog je obavezan."));
    }

    if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains('@'))
    {
        return Results.BadRequest(new ProblemResponse("Email radnika nije ispravan."));
    }

    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsCompanyAdministrator(user))
            {
                return Results.Forbid();
            }

            var result = repository.CreateWorker(user.CompanyId, request);
            return result.Match(
                worker => Results.Created($"/api/workers/{worker.Id}", worker),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapPost("/api/enrollments", (CreateEnrollmentRequest request, HttpRequest httpRequest, IAuthService authService, ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsCompanyAdministrator(user))
            {
                return Results.Forbid();
            }

            var result = repository.CreateEnrollment(user.CompanyId, request);
            return result.Match(
                enrollment => Results.Created($"/api/enrollments/{enrollment.Id}", enrollment),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapGet("/api/enrollments", (HttpRequest request, IAuthService authService, ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user => IsCompanyAdministrator(user)
            ? Results.Ok(repository.GetEnrollments(user.CompanyId))
            : Results.Forbid(),
        _ => Results.Unauthorized());
});

app.MapPost("/api/enrollments/{enrollmentId:guid}/complete", (
    Guid enrollmentId,
    CompleteTrainingRequest request,
    HttpRequest httpRequest,
    IAuthService authService,
    ITrainingRepository repository) =>
{
    if (request.Score < 0 || request.Score > 100)
    {
        return Results.BadRequest(new ProblemResponse("Rezultat mora biti izmedju 0 i 100."));
    }

    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsCompanyAdministrator(user))
            {
                return Results.Forbid();
            }

            var result = repository.CompleteEnrollment(user.CompanyId, enrollmentId, request);
            return result.Match(
                completion => Results.Ok(completion),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapGet("/api/certificates", (HttpRequest request, IAuthService authService, ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user => IsCompanyAdministrator(user)
            ? Results.Ok(repository.GetCertificates(user.CompanyId))
            : Results.Forbid(),
        _ => Results.Unauthorized());
});

app.MapGet("/api/certificates/verify/{certificateNumber}", (string certificateNumber, ITrainingRepository repository) =>
{
    var certificate = repository.VerifyCertificate(certificateNumber);
    return certificate is null ? Results.NotFound() : Results.Ok(certificate);
});

app.MapGet("/api/workers/{workerId:guid}/certificates", (Guid workerId, HttpRequest request, IAuthService authService, ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user => IsCompanyAdministrator(user)
            ? Results.Ok(repository.GetCertificatesForWorker(user.CompanyId, workerId))
            : Results.Forbid(),
        _ => Results.Unauthorized());
});

app.MapGet("/api/certificates/{certificateId:guid}", (Guid certificateId, HttpRequest request, IAuthService authService, ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsCompanyAdministrator(user))
            {
                return Results.Forbid();
            }

            var certificate = repository.GetCertificate(user.CompanyId, certificateId);
            return certificate is null ? Results.NotFound() : Results.Ok(certificate);
        },
        _ => Results.Unauthorized());
});

app.MapPost("/api/notifications/reminders", (
    SendReminderRequest request,
    HttpRequest httpRequest,
    IAuthService authService,
    ITrainingRepository repository,
    IEmailNotificationService emailService) =>
{
    if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new ProblemResponse("Naslov i poruka su obavezni."));
    }

    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsCompanyAdministrator(user))
            {
                return Results.Forbid();
            }

            var worker = repository.GetWorker(user.CompanyId, request.WorkerId);
            if (worker is null)
            {
                return Results.NotFound();
            }

            var result = emailService.SendReminder(worker, request.Subject, request.Message);
            return result.Match(
                reminder => Results.Ok(reminder),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapGet("/api/worker-portal/me", (HttpRequest request, IAuthService authService, ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsWorkerUser(user))
            {
                return Results.Forbid();
            }

            var worker = repository.GetWorkerByEmail(user.CompanyId, user.Email);
            if (worker is null)
            {
                return Results.NotFound(new ProblemResponse("Radnik sa email adresom prijavljenog korisnika nije pronadjen."));
            }

            var courses = repository.GetCourses();
            var enrollments = repository.GetEnrollments(user.CompanyId)
                .Where(enrollment => enrollment.WorkerId == worker.Id)
                .ToList();
            var certificates = repository.GetCertificatesForWorker(user.CompanyId, worker.Id);

            return Results.Ok(new WorkerPortalResponse(worker, courses, enrollments, certificates));
        },
        _ => Results.Unauthorized());
});

app.MapPost("/api/worker-portal/enrollments/{enrollmentId:guid}/start", (
    Guid enrollmentId,
    HttpRequest request,
    IAuthService authService,
    ITrainingRepository repository) =>
{
    var currentUser = ResolveCurrentUser(request, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsWorkerUser(user))
            {
                return Results.Forbid();
            }

            var worker = repository.GetWorkerByEmail(user.CompanyId, user.Email);
            if (worker is null)
            {
                return Results.NotFound(new ProblemResponse("Radnik sa email adresom prijavljenog korisnika nije pronadjen."));
            }

            var result = repository.StartEnrollment(user.CompanyId, worker.Id, enrollmentId);
            return result.Match(
                enrollment => Results.Ok(enrollment),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapPost("/api/worker-portal/enrollments/{enrollmentId:guid}/complete", (
    Guid enrollmentId,
    CompleteTrainingRequest request,
    HttpRequest httpRequest,
    IAuthService authService,
    ITrainingRepository repository) =>
{
    if (request.Score < 0 || request.Score > 100)
    {
        return Results.BadRequest(new ProblemResponse("Rezultat mora biti izmedju 0 i 100."));
    }

    if (request.DurationMinutes <= 0)
    {
        return Results.BadRequest(new ProblemResponse("Trajanje obuke mora biti vece od 0."));
    }

    var currentUser = ResolveCurrentUser(httpRequest, authService);
    return currentUser.Match(
        user =>
        {
            if (!IsWorkerUser(user))
            {
                return Results.Forbid();
            }

            var worker = repository.GetWorkerByEmail(user.CompanyId, user.Email);
            if (worker is null)
            {
                return Results.NotFound(new ProblemResponse("Radnik sa email adresom prijavljenog korisnika nije pronadjen."));
            }

            var ownsEnrollment = repository.GetEnrollments(user.CompanyId)
                .Any(enrollment => enrollment.Id == enrollmentId && enrollment.WorkerId == worker.Id);
            if (!ownsEnrollment)
            {
                return Results.NotFound(new ProblemResponse("Upis na kurs nije pronadjen za prijavljenog radnika."));
            }

            var result = repository.CompleteEnrollment(user.CompanyId, enrollmentId, request);
            return result.Match(
                completion => Results.Ok(completion),
                error => Results.BadRequest(new ProblemResponse(error)));
        },
        _ => Results.Unauthorized());
});

app.MapPost("/api/exams/{examId}/result", (
    string examId,
    ExternalExamResultRequest request,
    ITrainingRepository repository) =>
{
    var result = repository.CompleteExternalExam(examId, request);
    return result.Match(
        completion => Results.Ok(completion),
        error => Results.BadRequest(new ProblemResponse(error)));
});

app.Run();

static string? GetBearerToken(HttpRequest request)
{
    var authorization = request.Headers.Authorization.ToString();
    const string bearerPrefix = "Bearer ";

    if (authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
    {
        return authorization[bearerPrefix.Length..].Trim();
    }

    return null;
}

static Result<UserProfileResponse> ResolveCurrentUser(HttpRequest request, IAuthService authService)
{
    var token = GetBearerToken(request);
    return string.IsNullOrWhiteSpace(token)
        ? Result<UserProfileResponse>.Failure("Token nije poslat.")
        : authService.GetCurrentUser(token);
}

static bool IsCompanyAdministrator(UserProfileResponse user)
{
    return user.Role is UserRole.SystemAdministrator or UserRole.CompanyAdministrator;
}

static bool IsSystemAdministrator(UserProfileResponse user)
{
    return user.Role == UserRole.SystemAdministrator;
}

static bool IsWorkerUser(UserProfileResponse user)
{
    return user.Role == UserRole.User;
}

static void EnsureCompatibilityColumns(TrainingDbContext dbContext, string provider)
{
    if (IsPostgreSqlProvider(provider))
    {
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Enrollments\" ADD COLUMN IF NOT EXISTS \"DueAt\" timestamp with time zone NULL;");
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Enrollments\" ADD COLUMN IF NOT EXISTS \"ExamId\" character varying(80) NULL;");
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Companies\" ADD COLUMN IF NOT EXISTS \"SubscriptionLevel\" character varying(40) NOT NULL DEFAULT 'SmallBusiness';");
        return;
    }

    dbContext.Database.ExecuteSqlRaw("IF COL_LENGTH('Enrollments', 'DueAt') IS NULL ALTER TABLE [Enrollments] ADD [DueAt] datetimeoffset NULL;");
    dbContext.Database.ExecuteSqlRaw("IF COL_LENGTH('Enrollments', 'ExamId') IS NULL ALTER TABLE [Enrollments] ADD [ExamId] nvarchar(80) NULL;");
    dbContext.Database.ExecuteSqlRaw("IF COL_LENGTH('Companies', 'SubscriptionLevel') IS NULL ALTER TABLE [Companies] ADD [SubscriptionLevel] nvarchar(40) NOT NULL CONSTRAINT DF_Companies_SubscriptionLevel DEFAULT 'SmallBusiness';");
}

static string GetApplicationBaseUrl(HttpRequest request)
{
    var forwardedProto = request.Headers["X-Forwarded-Proto"].FirstOrDefault();
    var scheme = string.IsNullOrWhiteSpace(forwardedProto) ? request.Scheme : forwardedProto;
    var forwardedHost = request.Headers["X-Forwarded-Host"].FirstOrDefault();
    var host = string.IsNullOrWhiteSpace(forwardedHost) ? request.Host.Value : forwardedHost;
    return $"{scheme}://{host}";
}

static string GetGoogleCallbackUrl(HttpRequest request)
{
    return $"{GetApplicationBaseUrl(request)}/api/auth/google/callback";
}

static string? GetGoogleClientId(IConfiguration configuration)
{
    return configuration["Authentication:Google:ClientId"] ?? configuration["GoogleAuth:ClientId"];
}

static string? GetGoogleClientSecret(IConfiguration configuration)
{
    return configuration["Authentication:Google:ClientSecret"] ?? configuration["GoogleAuth:ClientSecret"];
}

static string BuildQueryString(IReadOnlyDictionary<string, string?> values)
{
    var query = values
        .Where(value => !string.IsNullOrWhiteSpace(value.Value))
        .Select(value => $"{Uri.EscapeDataString(value.Key)}={Uri.EscapeDataString(value.Value!)}");
    return $"?{string.Join("&", query)}";
}

static string NormalizeLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
    {
        return "login.html";
    }

    var trimmed = returnUrl.Trim();
    return Uri.TryCreate(trimmed, UriKind.Absolute, out _) || trimmed.StartsWith("//", StringComparison.Ordinal)
        ? "login.html"
        : trimmed;
}

static string EncodeGoogleOAuthState(GoogleOAuthState state)
{
    return CreateBase64Url(JsonSerializer.SerializeToUtf8Bytes(state));
}

static bool TryDecodeGoogleOAuthState(string value, out GoogleOAuthState state)
{
    state = new GoogleOAuthState(string.Empty, "login", null, "login.html");
    try
    {
        var bytes = DecodeBase64Url(value);
        var parsedState = JsonSerializer.Deserialize<GoogleOAuthState>(bytes);
        if (parsedState is null || string.IsNullOrWhiteSpace(parsedState.Nonce))
        {
            return false;
        }

        state = parsedState;
        return true;
    }
    catch
    {
        return false;
    }
}

static GoogleProfile? ReadGoogleProfile(JsonElement userInfo)
{
    var email = GetJsonString(userInfo, "email");
    var verifiedEmail = userInfo.TryGetProperty("email_verified", out var verifiedProperty) &&
        verifiedProperty.ValueKind == JsonValueKind.True;
    if (string.IsNullOrWhiteSpace(email) || !verifiedEmail)
    {
        return null;
    }

    var firstName = GetJsonString(userInfo, "given_name");
    var lastName = GetJsonString(userInfo, "family_name");
    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
    {
        var displayName = GetJsonString(userInfo, "name");
        var nameParts = (displayName ?? email.Split('@')[0])
            .Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        firstName = string.IsNullOrWhiteSpace(firstName) ? nameParts.FirstOrDefault() ?? "Google" : firstName;
        lastName = string.IsNullOrWhiteSpace(lastName)
            ? nameParts.Skip(1).FirstOrDefault() ?? "User"
            : lastName;
    }

    return new GoogleProfile(email, firstName, lastName);
}

static string? GetJsonString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;
}

static IResult RedirectToLoginWithError(string message)
{
    return Results.Redirect($"login.html?authError={Uri.EscapeDataString(message)}");
}

static string BuildGoogleAuthSuccessHtml(AuthResponse auth)
{
    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    options.Converters.Add(new JsonStringEnumConverter());
    var authJson = JsonSerializer.Serialize(auth, options);
    var role = auth.User.Role;
    var redirectUrl = role == UserRole.SystemAdministrator
        ? "system-admin.html"
        : role == UserRole.User
            ? "worker.html"
            : "platform.html";

    return $$"""
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>VR Academy Google sign in</title>
  </head>
  <body>
    <script>
      localStorage.setItem("safetySimAuth", JSON.stringify({{authJson}}));
      window.location.replace("{{redirectUrl}}");
    </script>
  </body>
</html>
""";
}

static string CreateBase64Url(byte[] bytes)
{
    return Convert.ToBase64String(bytes)
        .TrimEnd('=')
        .Replace("+", "-", StringComparison.Ordinal)
        .Replace("/", "_", StringComparison.Ordinal);
}

static byte[] DecodeBase64Url(string value)
{
    var base64 = value
        .Replace("-", "+", StringComparison.Ordinal)
        .Replace("_", "/", StringComparison.Ordinal);
    var padding = base64.Length % 4;
    if (padding > 0)
    {
        base64 = base64.PadRight(base64.Length + 4 - padding, '=');
    }

    return Convert.FromBase64String(base64);
}

static string? NormalizeConnectionString(string? value, string provider)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    var normalized = value.Trim().Trim('`').Trim();
    if (IsPostgreSqlProvider(provider) && TryConvertPostgreSqlUrl(normalized, out var postgresConnectionString))
    {
        return postgresConnectionString;
    }

    const string secretNamePrefix = "ConnectionStrings__TrainingDatabase=";
    if (normalized.StartsWith(secretNamePrefix, StringComparison.OrdinalIgnoreCase))
    {
        normalized = normalized[secretNamePrefix.Length..].Trim();
    }

    var serverIndex = normalized.IndexOf("Server=", StringComparison.OrdinalIgnoreCase);
    if (serverIndex > 0)
    {
        normalized = normalized[serverIndex..].Trim();
    }

    var serverLine = normalized
        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.Trim().Trim('`').Trim())
        .FirstOrDefault(line => line.Contains("Server=", StringComparison.OrdinalIgnoreCase));

    if (serverLine is not null)
    {
        var lineServerIndex = serverLine.IndexOf("Server=", StringComparison.OrdinalIgnoreCase);
        return lineServerIndex > 0 ? serverLine[lineServerIndex..].Trim() : serverLine;
    }

    return normalized;
}

static bool IsUsableConnectionString(string? value, string provider)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    if (IsPostgreSqlProvider(provider))
    {
        return value.Contains("Host=", StringComparison.OrdinalIgnoreCase);
    }

    return value.Contains("Server=", StringComparison.OrdinalIgnoreCase);
}

static bool IsPostgreSqlProvider(string provider)
{
    return provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)
        || provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase);
}

static bool TryConvertPostgreSqlUrl(string value, out string connectionString)
{
    connectionString = string.Empty;
    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
        || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        return false;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');
    var query = ParseQuery(uri.Query);
    var sslMode = query.TryGetValue("sslmode", out var configuredSslMode) ? configuredSslMode : "Require";

    var port = uri.Port > 0 ? uri.Port : 5432;
    connectionString = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};Ssl Mode={sslMode};Trust Server Certificate=true";
    return true;
}

static Dictionary<string, string> ParseQuery(string query)
{
    return query
        .TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(part => part.Split('=', 2))
        .Where(parts => parts.Length == 2)
        .ToDictionary(
            parts => Uri.UnescapeDataString(parts[0]),
            parts => Uri.UnescapeDataString(parts[1]),
            StringComparer.OrdinalIgnoreCase);
}

public sealed record ConnectionStringCandidate(string Source, string? Value);

public sealed record GoogleOAuthState(string Nonce, string Mode, string? CompanyName, string ReturnUrl);

public sealed record GoogleProfile(string Email, string FirstName, string LastName);
