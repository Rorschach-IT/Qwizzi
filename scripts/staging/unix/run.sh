set -euo pipefail

echo "=== Qwizzi: starting staging environment ==="

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../../.." >/dev/null 2>&1 && pwd)"

start_step() {
  echo
  echo "==> $1"
}

assert_command_exists() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Required command '$1' was not found in PATH." >&2
    exit 1
  fi
}

assert_command_exists docker

start_step "Starting Docker containers"
cd "${REPO_ROOT}"
docker compose --env-file .env.staging -f docker-compose.staging.yml up -d --build

echo "Docker containers started successfully."

start_step "Showing container logs in real time"
echo "Press Ctrl + C to stop viewing logs."
echo "Containers will keep running in background."
echo ""
echo "Useful commands:"
echo "  docker compose ps"
echo "  docker compose logs -f"
echo "  docker compose --env-file .env.staging -f docker-compose.staging.yml down"
echo ""

docker compose logs -f