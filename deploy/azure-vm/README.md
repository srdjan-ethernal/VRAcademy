# Azure VM deployment

This deployment runs the ASP.NET Core API and the static frontend in one Docker container, behind Caddy on the Azure VM. Data is stored in Azure SQL Database Free tier.

## Azure resources

1. Create an Azure SQL Database using the free offer.
2. Create an Ubuntu Azure VM.
3. Open inbound ports `22`, `80`, and `443` on the VM network security group.
4. Add the VM public IP address to the Azure SQL server firewall.

## VM setup

Install Docker on the VM:

```bash
sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg git
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo usermod -aG docker "$USER"
```

Log out and back in, then clone and configure:

```bash
git clone https://github.com/srdjan-ethernal/VRAcademy.git
cd VRAcademy
cp deploy/azure-vm/.env.example deploy/azure-vm/.env
nano deploy/azure-vm/.env
```

For first deployment without a domain, keep:

```text
APP_DOMAIN=:80
```

When a domain or subdomain is pointed to the VM public IP, set:

```text
APP_DOMAIN=training.example.com
```

Caddy will automatically request HTTPS certificates for a real domain.

## Deploy

```bash
bash deploy/azure-vm/deploy.sh
```

## Update

```bash
cd VRAcademy
git pull
bash deploy/azure-vm/deploy.sh
```

## Check

```bash
docker compose --env-file deploy/azure-vm/.env -f deploy/azure-vm/docker-compose.yml ps
docker compose --env-file deploy/azure-vm/.env -f deploy/azure-vm/docker-compose.yml logs -f vrsimulator
curl http://localhost/api/health
```

## Notes

- `Database__FallbackToInMemory=false` is intentional for VM deployment. If Azure SQL is misconfigured, the app should fail visibly instead of silently using demo memory storage.
- EF migrations run during startup through the demo account/database setup code, so the schema is created in Azure SQL when the connection string and firewall are correct.
- Keep `deploy/azure-vm/.env` out of Git because it contains the database password.
