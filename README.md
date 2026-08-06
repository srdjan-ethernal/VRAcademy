---
title: VR Academy
colorFrom: green
colorTo: blue
sdk: docker
app_port: 7860
---

# VR Academy

Profesionalni visejezicni sajt za VR simulacije industrijske bezbednosti.

Trenutni scenariji:

- VR simulacija za protivpozarnu zastitu
- VR simulacija za upravljanje radioaktivnim materijalima
- VR simulacija za upravljanje hemijskim otpadom
- VR simulacija za upravljanje gradjevinskim otpadom
- VR simulacija za upravljanje elektronskim otpadom
- VR simulacija za upravljanje biomedicinskim otpadom

Sajt je staticki i moze se objaviti preko GitHub Pages, Netlify, Cloudflare Pages ili bilo kog statickog hostinga.

Stranice:

- Pocetna: `index.html`
- Cene: `pricing.html`
- Sertifikati: `certificates.html`
- System admin: `system-admin.html`
- Platforma za administratore kompanije: `platform.html`
- Portal zaposlenog: `worker.html`

## Backend

Backend je u folderu `src/VRAcademy.Api`.

Tehnologije:

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core 10
- SQL Server provider
- PostgreSQL provider za opcionu Neon demo varijantu
- Microsoft runtime stack

Pokretanje:

```powershell
dotnet run --project src\VRAcademy.Api\VRAcademy.Api.csproj
```

Pocetne rute:

- `GET /api/health`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/google/start`
- `GET /api/auth/google/callback`
- `GET /api/auth/me`
- `GET /api/system/companies`
- `POST /api/system/companies`
- `PATCH /api/system/companies/{companyId}/subscription`
- `GET /api/users`
- `POST /api/users`
- `POST /api/users/reset-password`
- `POST /api/invitations`
- `GET /api/companies`
- `GET /api/dashboard/summary`
- `GET /api/scenarios`
- `GET /api/courses`
- `GET /api/workers`
- `POST /api/workers`
- `POST /api/enrollments`
- `POST /api/enrollments/{enrollmentId}/complete`
- `POST /api/exams/{examId}/result`
- `GET /api/certificates`

Korisnik se registruje uz kompaniju. Radnici, upisi i sertifikati se citaju i menjaju samo u okviru kompanije ulogovanog korisnika preko Bearer tokena. Backend podrazumevano koristi SQL Server kroz Entity Framework Core, a za demo hosting podrzan je Azure SQL preko konfiguracije. Sledeci korak je ASP.NET Core Identity ili Microsoft Entra ID prema tipu korisnika.

Uloge su `SystemAdministrator`, `CompanyAdministrator` i `User`. Neulogovani posetioci ne vide interne stranice `Admin`, `Platforma` i `Moj portal`; `SystemAdministrator` upravlja kompanijama i nivoima preplate, `CompanyAdministrator` upravlja zaposlenima i dodeljenim kursevima, a `User` vidi samo svoje zakazane i prethodne obuke kroz Moj portal. Kada administrator pozove zaposlenog, moze uneti privremenu lozinku i broj zaposlenog; sistem kreira korisnicki nalog i po potrebi worker zapis sa istom email adresom.

Svaka dodeljena obuka dobija `ExamId`. Poseban program za polaganje dobija taj `ExamId`, a rezultat vraca kroz `POST /api/exams/{examId}/result` sa statusom, rezultatom i trajanjem. Backend na osnovu rezultata oznacava polaganje kao polozeno ili nepolozeno i kreira sertifikat kada je kurs polozen.

Google login/registracija koristi OAuth 2.0 redirect tok. Na hostingu treba podesiti:

```text
Authentication__Google__ClientId=<google-client-id>
Authentication__Google__ClientSecret=<google-client-secret>
```

Google OAuth authorized redirect URI treba da bude:

```text
https://<vas-domen>/api/auth/google/callback
```

Za lokalnu bazu instalirati SQL Server Express LocalDB ili podesiti `ConnectionStrings:TrainingDatabase` na postojeci SQL Server. Migracije su u `src/VRAcademy.Api/Persistence/Migrations`.

## Demo deployment: Hugging Face + Neon

Repo sadrzi `Dockerfile` za Hugging Face Docker Space. Docker build objavljuje ASP.NET Core API i staticki frontend zajedno, na portu `7860`. Ovo je najjednostavniji demo deployment: Hugging Face hostuje aplikaciju, a Neon PostgreSQL cuva podatke.

Za Neon demo bazu podesiti Hugging Face Space secrets/variables:

```text
DATABASE_URL=postgresql://<user>:<password>@<host>/<database>?sslmode=require
Database__Provider=PostgreSql
Database__EnsureCreated=true
Database__FallbackToInMemory=false
Cors__AllowAnyOrigin=true
```

Backend podrzava i `NEON_DATABASE_URL` i `ConnectionStrings__TrainingDatabase`, ali `DATABASE_URL` je najjednostavniji jer ga Neon cesto prikazuje direktno. API pri startu kreira tabele za demo bazu kada je `Database__EnsureCreated=true`. `Database__FallbackToInMemory=false` je vazno za ovaj setup: ako baza nije dobro povezana, aplikacija treba jasno da prijavi gresku umesto da podatke cuva samo privremeno u memoriji.

Frontend automatski koristi isti domen kao API kada je otvoren preko javnog HTTPS domena, a lokalno ostaje na `http://localhost:5222`.

Deployment tok:

1. Napraviti Neon PostgreSQL projekat i bazu.
2. Kopirati Neon pooled connection string u Hugging Face secret `DATABASE_URL`.
3. Dodati Hugging Face variables `Database__Provider=PostgreSql`, `Database__EnsureCreated=true`, `Database__FallbackToInMemory=false`, `Cors__AllowAnyOrigin=true`.
4. Napraviti Hugging Face Space sa SDK tipom `Docker`.
5. Pushovati repo u Space.
6. Otvoriti Space URL i testirati registraciju kompanije, dodavanje radnika, dodelu obuke i izdavanje sertifikata.

## Azure VM deployment

Za produkcioni demo na Microsoft stack-u koristi se Azure SQL Database Free tier i Azure VM sa Docker Compose deploymentom.

Fajlovi su u `deploy/azure-vm`:

- `docker-compose.yml` pokrece ASP.NET Core API i frontend zajedno
- `Caddyfile` objavljuje aplikaciju na portovima `80` i `443`
- `.env.example` pokazuje vrednosti koje treba popuniti na VM-u
- `deploy.sh` radi build i restart containera

Detaljni koraci su u `deploy/azure-vm/README.md`.

## Azure App Service continuous deployment

Repo sadrzi GitHub Actions workflow:

```text
.github/workflows/azure-app-service.yml
```

Workflow se pokrece automatski na svaki push u `main` i moze se pokrenuti rucno iz GitHub Actions taba. Build objavljuje ASP.NET Core backend i kopira staticki frontend u `wwwroot`, zatim deployuje paket na Azure App Service.

Azure App Service treba podesiti na .NET 10 runtime stack. Lokalni build zahteva instaliran .NET 10 SDK.

Potrebno u GitHub repository settings:

```text
Variable:
AZURE_WEBAPP_NAME=<ime Azure App Service aplikacije>

Secret:
AZURE_WEBAPP_PUBLISH_PROFILE=<sadrzaj publish profile fajla iz Azure App Service-a>
```

Potrebno u Azure App Service Configuration / Application settings:

```text
ConnectionStrings__TrainingDatabase=Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
Database__Provider=SqlServer
Database__EnsureCreated=true
Database__FallbackToInMemory=false
Cors__AllowAnyOrigin=true
```

Za Azure SQL free bazu koristi se ADO.NET connection string iz Azure SQL Database stranice. U connection string-u obavezno zameniti `<password>` stvarnom lozinkom baze. Ako App Service ne moze da pristupi bazi, u Azure SQL Server Networking ukljuciti pristup za Azure services ili dodati odgovarajuce firewall pravilo.

Ako se koristi publish profile deployment, u Azure App Service mora biti omogucen basic authentication za publishing profile. Za Linux App Service, ako Azure ne dozvoli download publish profile-a, dodati app setting `WEBSITE_WEBDEPLOY_USE_SCM=true`, sacuvati i zatim ponovo preuzeti publish profile.
