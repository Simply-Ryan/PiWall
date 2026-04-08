# 🏁 PitWall: Quick Start Guide for Developers

## Architecture at a Glance

```
Simracing Hardware (iRacing, ACC, F1, etc.)
        ↓
Telemetry Bridge (C# .NET Service - Windows only)
        ↓
Backend API (Node.js/Express + PostgreSQL)
        ↓
        ├─→ REST API (port 3000)
        ├─→ WebSocket (port 9999)
        └─→ Database (PostgreSQL, port 5432)
        ↓
Frontend (React Native - Web/Mobile)
        ↓
User Interface (http://localhost:5173 or http://localhost:3001)
```

**Key Facts:**
- ✅ **All runs on your computer** (except optional cloud features)
- ✅ **Separate processes** - Backend and Frontend are independent
- ✅ **They communicate** - Frontend connects to Backend via HTTP/WebSocket
- ✅ **Optional Telemetry Bridge** - Only needed if using real racing sim data

---

## 🎯 Complete Setup (First Time)

### Option 1: Automated Setup (Recommended)

**Windows:**
```bash
setup.bat
```

**macOS/Linux:**
```bash
chmod +x setup.sh
./setup.sh
```

This handles everything:
- ✅ Installs Node.js dependencies
- ✅ Sets up PostgreSQL (via Docker or local)
- ✅ Runs database migrations
- ✅ Seeds sample data

### Option 2: Manual Setup (Advanced)

#### Step 1: Prerequisites

Make sure you have:
- **Node.js 18+** - Download from https://nodejs.org/
- **Docker Desktop** (recommended) - https://www.docker.com/
- **Git** - https://git-scm.com/

Verify installation:
```bash
node --version    # Should be v18.0.0 or higher
npm --version     # Should be v9.0.0 or higher
docker --version  # Optional but recommended
```

#### Step 2: Clone Repository

```bash
git clone https://github.com/Simply-Ryan/PitWall.git
cd PitWall
```

#### Step 3: Install Dependencies

**Backend:**
```bash
cd backend
npm install
cd ..
```

**Frontend:**
```bash
cd frontend
npm install
cd ..
```

#### Step 4: Setup Database

**Using Docker (Easiest):**
```bash
cd backend
docker-compose up -d
# Wait ~5 seconds for PostgreSQL to start

# Initialize database schema
npx prisma migrate dev --name init

cd ..
```

**Using Local PostgreSQL:**
1. Install PostgreSQL from https://www.postgresql.org/
2. Create a database called `pitwall`
3. Update `backend/.env` with connection details
4. Run the same migration command above

#### Step 5: Create Environment Files

**`backend/.env`:**
```
DATABASE_URL=postgresql://postgres:password@localhost:5432/pitwall
JWT_SECRET=your-super-secret-key-min-32-chars-long-12345
NODE_ENV=development
PORT=3000
CORS_ORIGIN=http://localhost:5173
```

**`frontend/.env`** (optional):
```
REACT_APP_API_URL=http://localhost:3000
REACT_APP_WS_URL=ws://localhost:9999
```

---

## 🚀 Running the App (Daily Use)

### Using Quick Start Scripts (Easiest - One Click!)

**Windows:**
1. Double-click `start-all.bat` in the project root
2. This will:
   - Start PostgreSQL (Docker)
   - Install dependencies (if needed)
   - Open Backend in new terminal (port 3000)
   - Open Frontend in new terminal (port 5173)

**macOS/Linux:**
```bash
chmod +x start-all.sh
./start-all.sh
```

### Using VS Code Tasks (If scripts don't work)

**NOTE:** VS Code needs to be restarted after first clone to pick up tasks.json

1. **Restart VS Code completely** (close and reopen)
2. Press `Ctrl+Shift+P` → type "Tasks: Run Task"
3. Select a task:
   - `▶️ Start All Services` - Starts everything
   - `🐘 Start PostgreSQL (Docker)` - Database only
   - `🚀 Backend: Start Development Server` - Backend only
   - `🌐 Frontend: Start Web Development Server` - Frontend only

### Using Terminal (Manual)

**Terminal 1 - Start Database:**
```bash
cd backend
docker-compose up -d
```

**Terminal 2 - Start Backend:**
```bash
cd backend
npm run dev
# Outputs: Server listening on port 3000
```

**Terminal 3 - Start Frontend:**
```bash
cd frontend
npm run web
# Outputs: To open the app in a browser, press w
```

**Terminal 4 (Optional) - Start Telemetry Bridge:**
```bash
cd telemetry-bridge
dotnet run
# Only if using real racing sim data
```

### Troubleshooting Task Discovery

If tasks don't appear in VS Code:

1. **Restart VS Code completely:**
   - Close VS Code
   - Reopen the PitWall folder
   - Try `Ctrl+Shift+P` → "Tasks: Run Task" again

2. **Use the Quick Start Scripts Instead:**
   - Windows: Double-click `start-all.bat`
   - macOS/Linux: Run `chmod +x start-all.sh && ./start-all.sh`

3. **Clear VS Code Cache:**
   - Delete `.vscode/tasks.json` and `.vscode/launch.json`
   - Restart VS Code
   - They will be re-created on next fetch from git

### Accessing the App

Once everything is running:

1. **Frontend:**
   - Web: http://localhost:5173
   - Press `w` in the terminal to open automatically

2. **Backend API Documentation:**
   - http://localhost:3000/api/docs

3. **Database Viewer (Prisma Studio):**
   - Open Task: `📊 Prisma: Open Database Studio`
   - Or run: `cd backend && npm run prisma:studio`
   - Opens at http://localhost:5555

---

## 📊 How Data Flows

### Demo Mode (No Backend Required)

1. Open Frontend: http://localhost:5173
2. Click "📊 View Dashboard (with Sample Data)"
3. See mock telemetry from Redux store
4. Frontend-only, no backend calls

### Live Mode (Backend Required)

1. Backend running on port 3000
2. Frontend connects automatically
3. Frontend fetches user sessions
4. Can load real telemetry from database

### Real Racing Sim (All Components)

1. **Telemetry Bridge** reads iRacing/ACC/F1 UDP streams
2. **Telemetry Bridge** sends unified stream to Frontend
3. **Backend** stores historical data in PostgreSQL
4. **Frontend** displays live + historical data

---

## 🔧 Common Tasks (VS Code)

| Task | Command (Ctrl+Shift+P → Tasks) | Alternative |
|------|--------------------------------|-------------|
| **Setup for first time** | `🏗️ Setup Complete Application` | `npm install` in each folder |
| **Start everything** | `▶️ Start All Services` | `./start-all.bat` or `./start-all.sh` |
| **Start database** | `🐘 Start PostgreSQL (Docker)` | `cd backend && docker-compose up -d` |
| **Start backend** | `🚀 Backend: Start Development Server` | `cd backend && npm run dev` |
| **Start frontend** | `🌐 Frontend: Start Web Development Server` | `cd frontend && npm run web` |
| **Run tests** | `🧪 Backend/Frontend: Run Tests` | `npm run test` |
| **Check code quality** | `🧹 Backend/Frontend: Lint Code` | `npm run lint` |
| **Type checking** | `🔧 Type Check: Frontend` | `npm run type-check` |
| **Build production** | `🔧 Build: Backend` / `Build: Frontend` | `npm run build` |
| **View database** | `📊 Prisma: Open Database Studio` | `npm run prisma:studio` |
| **Stop Docker** | `🐳 Docker: Stop All Containers` | `docker-compose down` |
| **Reset database** | `♻️ Database: Reset` | `npm run db:reset` |

---

## 🐛 Troubleshooting

### Problem: "Cannot connect to PostgreSQL"

**Solution:**
```bash
# Check if Docker is running
docker ps

# If not, start PostgreSQL
cd backend
docker-compose up -d

# Wait 5-10 seconds for it to start
docker-compose ps
```

### Problem: "Port 3000 already in use"

**Solution:**
```bash
# Find what's using port 3000
# Windows:
netstat -ano | findstr :3000

# Kill the process
taskkill /PID <PID> /F

# Or change the port in backend/.env
PORT=3001
```

### Problem: "Port 5173 already in use"

**Solution:**
Expo will automatically use 5174, 5175, etc. Check terminal output for the actual port.

### Problem: "npm install fails"

**Solution:**
```bash
# Clear npm cache
npm cache clean --force

# Delete lock files
rm package-lock.json

# Try again
npm install
```

### Problem: "Prisma migration fails"

**Solution:**
```bash
# Reset database (WARNING: Deletes all data)
cd backend
npm run db:reset

# Or manually:
docker-compose down -v  # Remove volume
docker-compose up -d    # Restart fresh
npx prisma migrate dev --name init
```

---

## 📁 Project Structure

```
PitWall/
├── backend/                    # Node.js API Server
│   ├── src/
│   │   ├── index.ts           # Entry point
│   │   ├── routes/            # API endpoints
│   │   ├── models/            # Database models
│   │   └── services/          # Business logic
│   ├── prisma/                # Database schema
│   ├── docker-compose.yml     # PostgreSQL container
│   ├── package.json
│   └── .env                   # Environment variables
│
├── frontend/                  # React Native App
│   ├── src/
│   │   ├── App.tsx           # Entry point
│   │   ├── screens/          # App screens
│   │   ├── components/       # Reusable components
│   │   ├── types/            # TypeScript types
│   │   ├── hooks/            # Custom hooks
│   │   ├── services/         # Services (API calls)
│   │   └── redux/            # State management
│   ├── package.json
│   └── app.json              # Expo config
│
├── telemetry-bridge/         # C# Telemetry Service (Optional)
│   ├── src/
│   ├── PitWall.Telemetry.csproj
│   └── appsettings.json
│
└── .vscode/                  # VS Code configuration
    ├── tasks.json            # Quick start tasks
    └── launch.json           # Debugging config
```

---

## 🎓 Component Details

### Backend (Node.js + Express + PostgreSQL)

**What it does:**
- HTTP REST API (port 3000)
- WebSocket server (port 9999)
- Database operations (user, sessions, telemetry)
- JWT authentication
- Real-time race data broadcasting

**Key files:**
- `src/index.ts` - Express server setup
- `src/routes/` - API endpoints
- `prisma/schema.prisma` - Database schema

**Running:**
```bash
cd backend && npm run dev
```

### Frontend (React Native + Expo)

**What it does:**
- Mobile-first responsive UI
- Real-time telemetry visualization
- Smart Dashboard HUD
- Voice notifications
- Redux state management
- HTTP/WebSocket communication with backend

**Key files:**
- `src/App.tsx` - React Navigation setup
- `src/screens/` - App pages (Home, Dashboard, Settings)
- `src/components/` - UI components
- `src/redux/` - Redux store and slices

**Running:**
```bash
cd frontend && npm run web
```

### Telemetry Bridge (C# .NET)

**What it does:**
- Windows-only service
- Reads UDP streams from racing sims (iRacing, ACC, F1)
- Normalizes data to unified format
- Sends via WebSocket to frontend

**Key files:**
- `src/Program.cs` - Service entry point
- `appsettings.json` - Configuration

**Running:**
```bash
cd telemetry-bridge && dotnet run
```

*(Only needed if using real racing sim data)*

---

## ✅ Verification Checklist

After setup, verify everything works:

- [ ] Backend running on http://localhost:3000
- [ ] Frontend running on http://localhost:5173
- [ ] Can view API docs: http://localhost:3000/api/docs
- [ ] Can view database: http://localhost:5555 (Prisma Studio)
- [ ] Frontend loads without errors
- [ ] Can load sample telemetry data
- [ ] Dashboard displays mock data

**If all checkmarks pass,** you're ready to develop! ✨

---

## 🚀 Next Steps

1. **Explore the codebase:**
   - Frontend: `src/screens/` → React Navigation setup
   - Backend: `src/routes/` → API endpoints
   - Database: `prisma/schema.prisma` → Data models

2. **Make a change:**
   - Edit `frontend/src/screens/HomeScreen.tsx`
   - Frontend auto-reloads
   - See changes instantly

3. **Run tests:**
   ```bash
   npm run test          # Both frontend and backend
   npm run test:watch   # Watch mode
   ```

4. **Build for production:**
   ```bash
   npm run build        # Both frontend and backend
   ```

---

## 📚 Resources

- **React Navigation:** https://reactnavigation.org/
- **Expo Documentation:** https://docs.expo.dev/
- **Express.js:** https://expressjs.com/
- **Prisma ORM:** https://www.prisma.io/
- **Redux Toolkit:** https://redux-toolkit.js.org/
- **TypeScript:** https://www.typescriptlang.org/

---

## ❓ Questions?

- Check `INSTALL_AND_SETUP.md` for detailed setup
- Check `PROJECT_PLAN.md` for architecture details
- Check individual component READMEs in each folder

Happy coding! 🎉
