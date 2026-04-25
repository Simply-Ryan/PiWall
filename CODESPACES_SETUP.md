# PitWall GitHub Codespaces Setup Guide

## Current Status

✅ **Frontend:** Running at https://urban-cod-pjr7xjxwxrx3r5jw-5173.app.github.dev/
⏳ **Backend:** Waiting to be started

## What The App Does

The PitWall app automatically detects if you're using GitHub Codespaces and routes API calls to the correct domain:

- **Local development:** `http://localhost:3000`
- **GitHub Codespaces:** `https://{prefix}-3000.app.github.dev`

## Start the Backend

### Option 1: Terminal (Recommended)

Open a new terminal in Codespaces and run:

```bash
cd /workspaces/PitWall/backend
docker compose up --build
```

Watch for these messages:
```
✓ pitwall-db is healthy
✓ pitwall-api is running
```

### Option 2: Ports Tab in Codespaces

1. Click the **Ports** tab in your Codespaces editor
2. Look for port **3000** in the list
3. If not running, you'll need to use Option 1 (Terminal)

## Verify Backend is Running

Once the backend is running:

1. **Refresh your frontend** at https://urban-cod-pjr7xjxwxrx3r5jw-5173.app.github.dev/
2. Look at the **Backend Connection** status
3. Should change from "⟳ Checking..." to "✓ Connected!"

## If Backend Connection Still Fails

### Check 1: Verify Docker Containers

```bash
docker ps
```

You should see:
- `pitwall-db` (PostgreSQL)
- `pitwall-api` (Node.js API)

### Check 2: Check Logs

```bash
# View API logs
docker compose -f /workspaces/PitWall/backend/docker-compose.yml logs api

# View database logs
docker compose -f /workspaces/PitWall/backend/docker-compose.yml logs db
```

### Check 3: Test Backend Directly

```bash
# Get your Codespaces domain from the URL bar, e.g.:
# urban-cod-pjr7xjxwxrx3r5jw-3000.app.github.dev

curl https://urban-cod-pjr7xjxwxrx3r5jw-3000.app.github.dev/health
```

Should return:
```json
{"status":"ok","timestamp":"2026-04-25T..."}
```

### Check 4: Check CORS Settings

The backend has CORS enabled for all origins (`*`), so cross-domain requests should work.

## Port Forwarding Issues

GitHub Codespaces may show a 401 Unauthorized error the first time you access a port. This is normal and means you need to authenticate:

1. Click the lock/auth button in the browser
2. Authorize for your Codespaces
3. The port should then be accessible

## Full URL References

Replace `urban-cod-pjr7xjxwxrx3r5jw` with your actual Codespaces prefix:

| Service | URL |
|---------|-----|
| Frontend | https://urban-cod-pjr7xjxwxrx3r5jw-5173.app.github.dev/ |
| Backend API | https://urban-cod-pjr7xjxwxrx3r5jw-3000.app.github.dev |
| Health Check | https://urban-cod-pjr7xjxwxrx3r5jw-3000.app.github.dev/health |
| Database | Internal (not exposed) |

## Database Access

The PostgreSQL database is only accessible internally from within Docker containers:

```
Host: db
Port: 5432
Database: pitwall
User: postgres
Password: password
```

## Next Steps

1. Start backend with terminal command above
2. Refresh frontend page
3. Wait for status to show "✓ Connected!"
4. Full app features will be available

## Troubleshooting Commands

```bash
# Stop all containers
docker compose -f /workspaces/PitWall/backend/docker-compose.yml down

# Stop and remove volumes
docker compose -f /workspaces/PitWall/backend/docker-compose.yml down -v

# View all containers
docker ps -a

# View images
docker images

# Clean everything
docker system prune -a
```

---

**Need Help?** Check the logs with the commands above or verify your Codespaces domain is correct in the browser URL bar.
