# VR Academy on one Hetzner server

This deployment runs the complete system on one Hetzner Cloud VPS:

- ASP.NET Core 10 API and static frontend in one application container
- Microsoft SQL Server 2022 Express in a private container network
- Caddy as the public reverse proxy with automatic HTTPS
- persistent Docker volumes for the database and TLS certificates

Only ports `22`, `80`, and `443` are public. SQL Server is never exposed to the internet.

## 1. Create the server

Create an **x86_64** Hetzner Cloud server with Ubuntu 24.04 and an SSH key. Use at least 4 GB RAM; 8 GB is recommended because SQL Server and the application share the same machine. Do not choose an ARM server because the Microsoft SQL Server container requires x86_64.

Connect to the server and clone the repository:

```bash
ssh root@SERVER_IP
apt-get update && apt-get install -y git
git clone https://github.com/srdjan-ethernal/VRAcademy.git /opt/vracademy
cd /opt/vracademy
sudo bash deploy/hetzner/setup-server.sh
```

Log out and back in after the setup script completes.

## 2. Configure DNS

At the DNS provider, set these records to the Hetzner server IP:

```text
A      @      SERVER_IP
A      www    SERVER_IP
```

Remove the old GitHub Pages records only when the Hetzner application is ready for the final switch. DNS changes may take time to propagate.

## 3. Configure secrets

```bash
cd /opt/vracademy
cp deploy/hetzner/.env.example deploy/hetzner/.env
nano deploy/hetzner/.env
```

At minimum, set:

```text
APP_DOMAIN=vracademy.io
MSSQL_SA_PASSWORD=a-strong-unique-password
```

The `.env` file is ignored by Git. Do not commit it.

When importing the existing Azure database, leave `BOOTSTRAP_ADMIN_ENABLED=false` because the existing administrator will be imported with the data. For a completely fresh database, set the bootstrap values and enable them only for the first deployment:

```text
BOOTSTRAP_ADMIN_ENABLED=true
BOOTSTRAP_ADMIN_COMPANY=Ethernal
BOOTSTRAP_ADMIN_EMAIL=your-admin@example.com
BOOTSTRAP_ADMIN_PASSWORD=a-different-strong-password
```

After the first successful administrator login, set `BOOTSTRAP_ADMIN_ENABLED=false` and run `deploy.sh` again. Leaving it enabled would reset that administrator password whenever the application starts.

If Google login is used, update the Google OAuth application with this redirect URI:

```text
https://vracademy.io/api/auth/google/callback
```

Then set `GOOGLE_CLIENT_ID` and `GOOGLE_CLIENT_SECRET` in `.env`.

## 4. First deployment

```bash
cd /opt/vracademy
bash deploy/hetzner/deploy.sh
```

The script pulls the Microsoft SQL Server and Caddy images, builds the .NET 10 application, starts all services, applies EF Core migrations, and checks `/api/health`.

Check status and logs:

```bash
docker compose --env-file deploy/hetzner/.env -f deploy/hetzner/docker-compose.yml ps
docker compose --env-file deploy/hetzner/.env -f deploy/hetzner/docker-compose.yml logs -f app
```

## 5. Updates

```bash
cd /opt/vracademy
git pull --ff-only
bash deploy/hetzner/deploy.sh
```

The database volume is preserved while the application container is rebuilt.

## Database backup

Create a verified SQL Server backup and copy it to `deploy/hetzner/backups`:

```bash
cd /opt/vracademy
bash deploy/hetzner/backup.sh
```

The script keeps local backups for 14 days. Copy these files to another machine or an external storage service because a backup stored only on the same VPS does not protect against total server loss.

To run a backup every night at 02:30:

```bash
crontab -e
```

Add:

```text
30 2 * * * cd /opt/vracademy && bash deploy/hetzner/backup.sh >> /var/log/vracademy-backup.log 2>&1
```

## Move existing Azure SQL data

For a clean first test, let the application create an empty local database. Before the final DNS switch, export the existing Azure SQL database as a `.bacpac` from Azure Portal and import it into the local `VRAcademyTraining` database with Microsoft SqlPackage.

Recommended cutover order:

1. Deploy and test Hetzner with an empty database.
2. Put the current application into a short maintenance window.
3. Export the Azure SQL database to a `.bacpac`.
4. Stop the Hetzner application container, import the `.bacpac`, and start the application again.
5. Run `deploy.sh` so any newer EF migrations are applied.
6. Verify login, companies, workers, enrollments, and certificates.
7. Point `vracademy.io` to the Hetzner server.

Do not delete the Azure resources until the imported data and Hetzner backups have been verified.

## Security notes

- SQL Server is isolated on the internal Docker network and has no host port.
- The application port is bound only to `127.0.0.1`; public traffic goes through Caddy.
- The default demo administrator is disabled in this deployment.
- Caddy automatically renews HTTPS certificates.
- Keep Ubuntu and Docker updated and store database backups outside the VPS.
