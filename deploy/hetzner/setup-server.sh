#!/usr/bin/env bash
set -euo pipefail

if [[ "$(uname -m)" != "x86_64" ]]; then
  echo "Microsoft SQL Server container requires an x86_64 Hetzner server."
  exit 1
fi

if [[ "${EUID}" -ne 0 ]]; then
  echo "Run this script with sudo: sudo bash deploy/hetzner/setup-server.sh"
  exit 1
fi

apt-get update
apt-get install -y ca-certificates curl git gnupg ufw

install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
chmod a+r /etc/apt/keyrings/docker.asc

. /etc/os-release
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $VERSION_CODENAME stable" \
  > /etc/apt/sources.list.d/docker.list

apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 443/udp
ufw --force enable

systemctl enable --now docker

if [[ -n "${SUDO_USER:-}" && "$SUDO_USER" != "root" ]]; then
  usermod -aG docker "$SUDO_USER"
fi

echo "Server preparation completed. Log out and back in before running deploy.sh."
