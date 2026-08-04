# VRSimulator.Api

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
"TrainingDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=VRSimulatorTraining;Trusted_Connection=True;MultipleActiveResultSets=true"
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
dotnet ef database update --project src\VRSimulator.Api\VRSimulator.Api.csproj --startup-project src\VRSimulator.Api\VRSimulator.Api.csproj
```

Migracije se nalaze u:

```text
src\VRSimulator.Api\Persistence\Migrations
```

## Pokretanje

```powershell
dotnet run --project src\VRSimulator.Api\VRSimulator.Api.csproj
```

Zatim otvoriti:

```text
http://localhost:5000/api
```

Port moze biti drugaciji ako ga Visual Studio ili `launchSettings.json` dodele automatski.

## Prvi API tok

1. `POST /api/auth/register`
2. `POST /api/auth/login`
3. `GET /api/auth/me`
4. `GET /api/users`
5. `POST /api/users`
6. `GET /api/courses`
7. `POST /api/workers`
8. `POST /api/enrollments`
9. `POST /api/enrollments/{enrollmentId}/complete`
10. `GET /api/certificates`

Ako je rezultat kursa najmanje 80, backend automatski izdaje sertifikat koji vazi 12 meseci.

## Auth rute

### Registracija

`POST /api/auth/register`

```json
{
  "email": "admin@safetysim.test",
  "password": "TestPass123",
  "firstName": "Srdjan",
  "lastName": "Admin",
  "companyName": "Safety Sim Demo"
}
```

Registracija kreira korisnika i kompaniju ako kompanija jos ne postoji. Prvi korisnik dobija ulogu `CompanyAdmin`.

### Login

`POST /api/auth/login`

```json
{
  "email": "admin@safetysim.test",
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
  "email": "instructor@safetysim.test",
  "password": "TestPass123",
  "firstName": "Ivan",
  "lastName": "Instruktor",
  "role": "Instructor"
}
```

Samo `CompanyAdmin` moze dodati korisnika. Kroz ovu rutu mogu se dodati `Instructor` i `Employee` korisnici. `CompanyAdmin` se kreira registracijom kompanije.

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
