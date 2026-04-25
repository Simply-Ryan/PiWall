# Getting Backend Running

Your frontend is now displaying correctly! The next step is to start the backend API and database.

## Quick Start Backend

**Open a new terminal and run:**

```bash
cd /workspaces/PitWall/backend
docker compose up --build
```

This will:
1. Download PostgreSQL image (14-alpine)
2. Download Node.js build environment
3. Build the backend API container
4. Start both database and API services
5. Run database migrations automatically

## Expected Output

Watch for these messages:

```
✓ Creating network "pitwall-network"
✓ Creating pitwall-db ... done
✓ Creating pitwall-api ... done

pitwall-api   | ✓ API running on http://0.0.0.0:3000
pitwall-db    | ✓ PostgreSQL healthy
```

## Verify Backend is Running

**In a separate terminal, test the health endpoint:**

```bash
curl http://localhost:3000/health
```

Should return:
```json
{"status":"ok","timestamp":"2026-04-25T..."}
```

## Frontend will Connect Automatically

Once the backend is running:
1. Frontend will detect the connection
2. Status changes from "Waiting..." to "Connected"
3. Dashboard loads with available controls
4. You can interact with the full PitWall application

## Troubleshooting

**If Docker is not installed:**
```bash
# Install Docker (Ubuntu/Debian)
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
```

**If containers won't start:**
```bash
# Check Docker daemon status
docker ps

# View logs
docker compose logs api
docker compose logs db

# Clean up and restart
docker compose down --volumes
docker compose up --build
```

**If port 3000 is already in use:**
```bash
# Find what's using port 3000
lsof -i :3000

# Or change PORT in .env
PORT=3001 docker compose up
```

## Application URLs

Once everything is running:

| Component | URL | Status |
|-----------|-----|--------|
| Frontend | http://localhost:5173 | ✓ Running |
| Backend API | http://localhost:3000 | Waiting... |
| Database | localhost:5432 | Internal only |
| Health Check | http://localhost:3000/health | Waiting... |

---

**Next Step:** Start the backend with Docker Compose, then refresh your frontend!
