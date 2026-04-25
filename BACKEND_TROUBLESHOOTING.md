# PitWall Backend Startup Troubleshooting

## Current Situation

✅ **Frontend:** Running and displaying status page  
⏳ **Backend:** Starting (Docker containers initializing)

## Expected Timeline

- **First 10 seconds:** Docker images building/starting
- **10-30 seconds:** Database initialization  
- **30-60 seconds:** Backend becomes fully ready
- **After 60 seconds:** If still not connected, see troubleshooting below

## What the Frontend Shows

The frontend automatically checks backend health every 3 seconds and displays:
- ⟳ **Connecting** - Checking health endpoint
- ✓ **Connected** - Backend is responding  
- ⚠ **Error** - Backend returned an error status
- ✗ **Cannot reach backend** - No response from backend
- Shows attempt count so you can see how long it's been checking

## Verify Backend is Actually Running

### Option 1: Check Docker Containers

In a terminal, run:
```bash
docker ps
```

You should see two running containers:
```
CONTAINER ID   IMAGE                    STATUS
abc123def456   pitwall-db               Up 2 minutes (healthy)
xyz789abc123   pitwall-api              Up 2 minutes
```

If containers aren't showing or are exited, the startup failed.

### Option 2: Check Backend Logs

```bash
docker logs pitwall-api
```

Look for these success indicators:
```
✅ Database connected
🚀 Server running at http://0.0.0.0:3000
💪 Health check: http://0.0.0.0:3000/health
```

If you see errors, that's the problem - share them and I can help fix.

### Option 3: Run Diagnostic Script

```bash
chmod +x /workspaces/PitWall/backend/diagnose.sh
/workspaces/PitWall/backend/diagnose.sh
```

This will show:
- Container status
- Recent logs
- Port listening status
- Health endpoint test

## Common Issues and Fixes

### Issue 1: Port Already in Use

**Error message in logs:**
```
Error: listen EADDRINUSE :::3000
```

**Fix:**
```bash
# Kill process on port 3000
kill `lsof -ti :3000`

# Or restart Docker
docker compose down
docker compose up --build
```

### Issue 2: Database Connection Failed

**Error message in logs:**
```
Error: connect ECONNREFUSED 127.0.0.1:5432
```

**Fix:** Database container needs more time. Wait 30 seconds and check logs again.

### Issue 3: Out of Disk Space

**Error message in logs:**
```
no space left on device
```

**Fix:**
```bash
# Clean up Docker
docker system prune -a --volumes

# Retry
docker compose up --build
```

### Issue 4: Docker Compose Not Installed

**Error message:**
```
docker: 'compose' is not a command
```

**Fix:** Use `docker-compose` instead of `docker compose`:
```bash
cd /workspaces/PitWall/backend
docker-compose up --build
```

## Step-by-Step Recovery

If the backend isn't connecting after 2 minutes:

1. **Stop everything:**
   ```bash
   docker compose down
   ```

2. **Check the logs:**
   ```bash
   docker logs pitwall-api
   ```

3. **Clean and rebuild:**
   ```bash
   docker system prune -a --volumes
   docker compose up --build
   ```

4. **Wait 60 seconds** and refresh your frontend

5. **If still failing, check:**
   ```bash
   docker ps -a
   docker logs pitwall-db
   docker logs pitwall-api
   ```

## Success Criteria

✅ Both containers are running:
```bash
docker ps | grep pitwall
```

✅ Health endpoint responds:
```bash
curl https://urban-cod-pjr7xjxwxrx3r5jw-3000.app.github.dev/health
```

✅ Frontend shows "✓ Backend API is connected!"

## Once Backend is Connected

- Full app features unlock automatically
- Frontend will load the actual dashboard
- You'll have access to telemetry, sessions, strategy, fuel, etc.

## Need More Help?

Check these files for configuration:
- Backend config: `/workspaces/PitWall/backend/docker-compose.yml`
- API setup: `/workspaces/PitWall/backend/src/index.ts`
- Frontend API URL detection: `/workspaces/PitWall/frontend/src/MinimalApp.tsx`

The frontend automatically detects:
- Codespaces environment (uses https://{prefix}-3000.app.github.dev)
- Local environment (uses http://localhost:3000)
- Has retry logic with exponential backoff
- Shows attempt count so you know it's actively trying

---

**Keep the docker terminal open and visible** - you'll see real-time startup logs and any errors that occur.
