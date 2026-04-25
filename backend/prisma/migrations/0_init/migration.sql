-- CreateTable User
CREATE TABLE "User" (
    "id" TEXT NOT NULL PRIMARY KEY,
    "username" TEXT NOT NULL UNIQUE,
    "email" TEXT NOT NULL UNIQUE,
    "password" TEXT NOT NULL,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- CreateTable Session
CREATE TABLE "Session" (
    "id" TEXT NOT NULL PRIMARY KEY,
    "userId" TEXT NOT NULL,
    "track" TEXT NOT NULL,
    "car" TEXT NOT NULL,
    "weather" TEXT NOT NULL DEFAULT 'Clear',
    "sessionType" TEXT NOT NULL DEFAULT 'Practice',
    "startedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "endedAt" TIMESTAMP(3),
    "duration" INTEGER,
    "lapsTaken" INTEGER NOT NULL DEFAULT 0,
    "bestLapTime" DOUBLE PRECISION,
    "averageLapTime" DOUBLE PRECISION,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "Session_userId_fkey" FOREIGN KEY ("userId") REFERENCES "User" ("id") ON DELETE RESTRICT ON UPDATE CASCADE
);

-- CreateTable Lap
CREATE TABLE "Lap" (
    "id" TEXT NOT NULL PRIMARY KEY,
    "sessionId" TEXT NOT NULL,
    "lapNumber" INTEGER NOT NULL,
    "time" DOUBLE PRECISION NOT NULL,
    "speed" DOUBLE PRECISION,
    "fuelLevel" DOUBLE PRECISION,
    "tireTemp" DOUBLE PRECISION,
    "notes" TEXT,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "Lap_sessionId_fkey" FOREIGN KEY ("sessionId") REFERENCES "Session" ("id") ON DELETE CASCADE ON UPDATE CASCADE
);

-- CreateTable Telemetry
CREATE TABLE "Telemetry" (
    "id" TEXT NOT NULL PRIMARY KEY,
    "sessionId" TEXT NOT NULL,
    "lapNumber" INTEGER NOT NULL,
    "timestamp" TIMESTAMP(3) NOT NULL,
    "speed" DOUBLE PRECISION NOT NULL,
    "throttle" DOUBLE PRECISION,
    "braking" DOUBLE PRECISION,
    "steering" DOUBLE PRECISION,
    "lateralG" DOUBLE PRECISION,
    "longitudinalG" DOUBLE PRECISION,
    "fuelLevel" DOUBLE PRECISION,
    "tirePressure" DOUBLE PRECISION,
    "tireTemperature" DOUBLE PRECISION,
    "engineRPM" DOUBLE PRECISION,
    "gear" INTEGER,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "Telemetry_sessionId_fkey" FOREIGN KEY ("sessionId") REFERENCES "Session" ("id") ON DELETE CASCADE ON UPDATE CASCADE
);

-- CreateTable Analytics
CREATE TABLE "Analytics" (
    "id" TEXT NOT NULL PRIMARY KEY,
    "sessionId" TEXT NOT NULL,
    "totalDistance" DOUBLE PRECISION NOT NULL,
    "totalTime" DOUBLE PRECISION NOT NULL,
    "averageSpeed" DOUBLE PRECISION,
    "maxSpeed" DOUBLE PRECISION,
    "averageFuel" DOUBLE PRECISION,
    "averageTireTemp" DOUBLE PRECISION,
    "drivingStyle" TEXT,
    "recommendations" TEXT,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "Analytics_sessionId_fkey" FOREIGN KEY ("sessionId") REFERENCES "Session" ("id") ON DELETE CASCADE ON UPDATE CASCADE
);

-- CreateTable Strategy
CREATE TABLE "Strategy" (
    "id" TEXT NOT NULL PRIMARY KEY,
    "sessionId" TEXT NOT NULL,
    "raceLength" INTEGER NOT NULL,
    "estimatedLaps" INTEGER NOT NULL,
    "fuelPerLap" DOUBLE PRECISION NOT NULL,
    "pitStops" INTEGER NOT NULL DEFAULT 1,
    "tireStrategy" TEXT NOT NULL DEFAULT 'All Medium',
    "notes" TEXT,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "Strategy_sessionId_fkey" FOREIGN KEY ("sessionId") REFERENCES "Session" ("id") ON DELETE CASCADE ON UPDATE CASCADE
);

-- CreateTable Leaderboard
CREATE TABLE "Leaderboard" (
    "id" TEXT NOT NULL PRIMARY KEY,
    "track" TEXT NOT NULL,
    "car" TEXT NOT NULL,
    "userId" TEXT NOT NULL,
    "username" TEXT NOT NULL,
    "bestLapTime" DOUBLE PRECISION NOT NULL,
    "sessionDate" TIMESTAMP(3) NOT NULL,
    "rank" INTEGER,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "Leaderboard_userId_fkey" FOREIGN KEY ("userId") REFERENCES "User" ("id") ON DELETE CASCADE ON UPDATE CASCADE
);

-- CreateIndex
CREATE UNIQUE INDEX "User_username_key" ON "User"("username");

-- CreateIndex
CREATE UNIQUE INDEX "User_email_key" ON "User"("email");

-- CreateIndex
CREATE INDEX "Session_userId_idx" ON "Session"("userId");

-- CreateIndex
CREATE INDEX "Session_track_idx" ON "Session"("track");

-- CreateIndex
CREATE INDEX "Lap_sessionId_idx" ON "Lap"("sessionId");

-- CreateIndex
CREATE INDEX "Telemetry_sessionId_lapNumber_idx" ON "Telemetry"("sessionId", "lapNumber");

-- CreateIndex
CREATE INDEX "Analytics_sessionId_idx" ON "Analytics"("sessionId");

-- CreateIndex
CREATE INDEX "Strategy_sessionId_idx" ON "Strategy"("sessionId");

-- CreateIndex
CREATE INDEX "Leaderboard_track_car_idx" ON "Leaderboard"("track", "car");

-- CreateIndex
CREATE INDEX "Leaderboard_userId_idx" ON "Leaderboard"("userId");
