#!/bin/bash
cd /workspaces/PitWall
git add -A
git commit -m "Fix: Resolve all backend TypeScript compilation and Prisma deployment issues

- Fixed 25+ TypeScript compilation errors across backend services
  * Fixed StrategyMonitor, RaceSimulator, MultiDriverStrategyCalculator
  * Corrected TelemetryData type definitions with proper object structures
  * Fixed JWT signing type assertions
  * Fixed timestamp type conversions from Date | number

- Fixed stray syntax error in RaceSimulator.ts (line 95)

- Fixed Prisma configuration for Docker deployment
  * Updated schema.prisma with debian-openssl-1.1.x binary target
  * Modified Dockerfile to regenerate Prisma in runtime stage

- Fixed Docker base image and dependencies
  * Switched runtime from Alpine to Debian Bullseye for OpenSSL 1.1 support
  * Added libssl1.1 package installation in runtime stage
  * Build stage remains Alpine for efficiency

- Fixed frontend component syntax errors
  * Removed orphaned duplicate StyleSheet.create() code in DeltaDisplay.tsx
  * Removed orphaned duplicate StyleSheet.create() code in FuelDisplay.tsx

Backend API now starts cleanly on port 3000 with PostgreSQL 5432
Frontend compiles successfully and ready to run on port 5173"
git push origin main
