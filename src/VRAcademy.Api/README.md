# VRAcademy.Api

ASP.NET Core backend za evidenciju VR obuka radnika.

## Tehnologije

- .NET 10
- ASP.NET Core Minimal API
- Microsoft hosting/runtime stack
- Entity Framework Core 10
- SQL Server provider
- PostgreSQL provider za opcionu Neon demo varijantu

Podrazumevana lokalna baza: SQL Server uz Entity Framework Core.
Demo hosting baza: Neon PostgreSQL preko `Database:Provider=PostgreSql`.

## Naming standard

Backend klase, C# namespace-i, persistence entiteti i SQL tabela imena koriste engleski jezik.

Primeri:

- `Company`, `Worker`, `Course`, `TrainingScenario`, `Enrollment`, `Certificate`
- `CompanyEntity`, `WorkerEntity`, `CourseEntity`
- `Companies`, `Workers`, `Courses`, `TrainingScenarios`, `Enrollments`, `Certificates`

Lokalizovan sadrzaj, kao sto su `NameSr`, `DescriptionSr` i korisnicke poruke, moze ostati na srpskom kada je namenjen prikazu korisniku.

## Multi-tenant pravilo

Korisnik uvek pripada jednoj kompaniji.

Podaci koji su vezani za kompaniju:

- korisnici kompanije
- radnici
- upisi na kurseve
- rezultati obuke
- sertifikati

Ove rute zahtevaju `Authorization: Bearer <accessToken>` i automatski koriste `companyId` iz tokena. Klijent ne salje `companyId` u zahtevima za radnike ili upise.

Globalni podaci:

- scenariji
- kursevi

## Baza i migracije

Podrazumevani connection string koristi SQL Server LocalDB:

```json
"TrainingDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=VRAcademyTraining;Trusted_Connection=True;MultipleActiveResultSets=true"
```

Ako LocalDB nije instaliran, instalirati SQL Server Express LocalDB ili promeniti `ConnectionStrings:TrainingDatabase` u `appsettings.Development.json`.

Za Neon demo koristi se:

```text
DATABASE_URL=postgresql://<user>:<password>@<host>/<database>?sslmode=require
Database__Provider=PostgreSql
Database__EnsureCreated=true
Database__FallbackToInMemory=false
```

Backend prihvata `DATABASE_URL`, `NEON_DATABASE_URL` ili `ConnectionStrings__TrainingDatabase`. `EnsureCreated` je namenjen samo za demo okruzenje bez rucnog migracionog koraka.

Kreiranje / azuriranje baze:

```powershell
dotnet ef database update --project src\VRAcademy.Api\VRAcademy.Api.csproj --startup-project src\VRAcademy.Api\VRAcademy.Api.csproj
```

Migracije se nalaze u:

```text
src\VRAcademy.Api\Persistence\Migrations
```

## Pokretanje

```powershell
dotnet run --project src\VRAcademy.Api\VRAcademy.Api.csproj
```

Zatim otvoriti:

```text
http://localhost:5000/api
```

Port moze biti drugaciji ako ga Visual Studio ili `launchSettings.json` dodele automatski.

## Prvi API tok

1. `POST /api/auth/login`
2. System admin: `GET /api/system/companies`, `POST /api/system/companies`, `PATCH /api/system/companies/{companyId}/subscription`
3. Company admin: `POST /api/invitations`, `POST /api/users/reset-password`, `GET /api/dashboard/summary`
4. Company admin: `GET /api/courses`, `POST /api/workers`, `POST /api/enrollments`
5. Worker portal: `GET /api/worker-portal/me`, `POST /api/worker-portal/enrollments/{enrollmentId}/start`
6. External exam program: `POST /api/exams/{examId}/result`
7. Certificates: `GET /api/certificates`, `GET /api/certificates/verify/{certificateNumber}`

Ako je rezultat kursa najmanje 80, backend automatski izdaje sertifikat koji vazi 12 meseci.

## Auth rute

### Registracija

`POST /api/auth/register`

```json
{
  "email": "admin@vracademy.test",
  "password": "TestPass123",
  "firstName": "Srdjan",
  "lastName": "Admin",
  "companyName": "VR Academy Demo"
}
```

Registracija kreira korisnika i kompaniju ako kompanija jos ne postoji. Prvi korisnik dobija ulogu `CompanyAdministrator`.

### Login

`POST /api/auth/login`

```json
{
  "email": "admin@vracademy.test",
  "password": "TestPass123"
}
```

Odgovor sadrzi privremeni `accessToken`. Za proveru profila koristi se:

```text
Authorization: Bearer <accessToken>
```

`GET /api/auth/me`

### Korisnici kompanije

`GET /api/users`

Header:

```text
Authorization: Bearer <accessToken>
```

Vraca samo korisnike kompanije kojoj pripada ulogovani korisnik.

`POST /api/users`

Header:

```text
Authorization: Bearer <accessToken>
```

Body:

```json
{
  "email": "worker@vracademy.test",
  "password": "TestPass123",
  "firstName": "Pera",
  "lastName": "Peric",
  "role": "User"
}
```

Samo `CompanyAdministrator` ili `SystemAdministrator` moze dodati korisnika za kompaniju. Kroz ovu rutu se dodaje obican `User`; administratorske uloge se kreiraju posebnim administrativnim tokom.

### System admin

`SystemAdministrator` koristi posebne rute za upravljanje tenant-ima:

- `GET /api/system/companies`
- `POST /api/system/companies`
- `PATCH /api/system/companies/{companyId}/subscription`

Podrzani nivoi preplate su `SmallBusiness`, `MediumBusiness` i `Enterprise`.

### Pozivnice i reset lozinke

`POST /api/invitations` kreira `User` nalog za zaposlenog i vraca `invitationUrl` i privremenu lozinku. Ako zahtev sadrzi `employeeNumber`, backend ce po potrebi kreirati i worker zapis sa istom email adresom.

`POST /api/users/reset-password` resetuje lozinku. `CompanyAdministrator` moze resetovati samo korisnike svoje kompanije, a `SystemAdministrator` moze resetovati globalno.

### Kreiranje radnika u tenant-u

`POST /api/workers`

Header:

```text
Authorization: Bearer <accessToken>
```

Body:

```json
{
  "firstName": "Pera",
  "lastName": "Peric",
  "employeeNumber": "A-001",
  "department": "Bezbednost"
}
```

Kompanija radnika se uzima iz tokena ulogovanog korisnika.

### Dodela kursa radniku

`POST /api/enrollments`

Header:

```text
Authorization: Bearer <accessToken>
```

Body:

```json
{
  "workerId": "00000000-0000-0000-0000-000000000000",
  "courseId": "00000000-0000-0000-0000-000000000000",
  "dueAt": "2026-09-05T21:59:59.000Z"
}
```

`dueAt` je opcioni rok do kada zaposleni treba da polozi kurs. Dodelu kurseva i pregled svih statusa vidi samo `CompanyAdministrator` ili `SystemAdministrator`. Obican `User` vidi samo svoje kurseve kroz `GET /api/worker-portal/me`.

Odgovor za dodeljenu obuku sadrzi `examId`. Poseban program za polaganje dobija taj identifikator, a rezultat vraca ovako:

```json
{
  "status": "passed",
  "score": 92,
  "durationMinutes": 34
}
```

Ruta:

```text
POST /api/exams/{examId}/result
```
