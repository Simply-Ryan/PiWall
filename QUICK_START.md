# 🚀 PitWall Quick Start Guide

Get PitWall running in 5 minutes!

---

## Prerequisites (One-time Setup)

### 1. Install Node.js
- **Windows:** Download from https://nodejs.org/ (LTS version)
- **macOS:** `brew install node`
- **Linux:** `sudo apt install nodejs npm`

Verify: Open terminal and run `node --version` (should be v18+)

### 2. Install PostgreSQL
**Option A: Docker (Easiest - 1 minute)**
- Install Docker Desktop from https://www.docker.com/products/docker-desktop
- We'll start the database automatically

**Option B: Local PostgreSQL (Recommended for production)**
- **Windows:** Download from https://www.postgresql.org/download/windows/
- **macOS:** `brew install postgresql@14 && brew services start postgresql@14`
- **Linux:** `sudo apt install postgresql`
- Default: runs on `localhost:5432`

---

## Run PitWall

### Windows Users

**First time setup:**
```bash
setup.bat
```

**Every time you want to run the app:**
```bash
start-all.bat
```

### macOS/Linux Users

**First time setup:**
```bash
chmod +x setup.sh
./setup.sh
```

**Every time you want to run the app:**
```bash
chmod +x start-all.sh
./start-all.sh
```

---

## Access the App

Once started, open these in your browser:

- **Frontend:** http://localhost:5173
- **Backend API:** http://localhost:3000
- **API Documentation:** http://localhost:3000 (once backend loads)

---

## Manual Step-by-Step (If Scripts Don't Work)

### Terminal 1: Setup Database

```bash
# Only needed if NOT using Docker
# Verify PostgreSQL is running, then create the database:
npx prisma db push
npx prisma db seed
```

### Terminal 2: Start Backend

```bash
cd backend
npm install
npm run dev
```

Wait for: `🚀 Server running at http://localhost:3000`

### Terminal 3: Start Frontend

```bash
cd frontend
npm install
npm run web
```

Wait for browser to open automatically at http://localhost:5173

---

## Troubleshooting

### "Node.js not found"
→ Install Node.js from https://nodejs.org/ and restart terminal

### "Can't connect to database"
→ Option A: Start Docker Desktop
→ Option B: Start PostgreSQL manually and ensure it's running on port 5432

### "Port 3000/5173 already in use"
→ Kill the process or set different PORT: `PORT=3001 npm run dev`

### "npm install fails"
→ Run: `npm install --legacy-peer-deps`

### "TypeScript errors in build"
→ Run: `npm run build` in the failing directory

---

## Development Tips

- **Hot reload:** Changes auto-refresh when you save
- **Debug Backend:** Check logs in terminal 2
- **Debug Frontend:** Open browser DevTools (F12)
- **API Testing:** Use http://localhost:3000/api/docs (Postman works too)
- **Database UI:** Run `cd backend && npx prisma studio`

---

## What's Running?

| Service | Port | Technology |
|---------|------|-----------|
| Frontend | 5173 | React Native / Expo Web |
| Backend API | 3000 | Node.js / Express |
| PostgreSQL | 5432 | Docker or Local |
| Prisma Studio | 5555 | Database UI (optional) |

---

## Next Steps

1. ✅ App is running!
2. 📖 Read [Full Documentation](./README.md)
3. 🛠️ Check [Developer Guide](./PROJECT_PLAN.md)
4. 🧪 Run tests: `npm test`
