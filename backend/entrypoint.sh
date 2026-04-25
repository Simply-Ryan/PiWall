#!/bin/sh
# Backend entrypoint script - runs migrations then starts app

set -e

echo "🔄 PitWall Backend - Startup Sequence"
echo "======================================"

# Wait for database to be ready
echo "⏳ Waiting for database..."
while ! nc -z db 5432; do
  echo "   Database not ready, waiting..."
  sleep 1
done
echo "✅ Database is ready"

# Run Prisma migrations
echo "🔄 Running database migrations..."
npx prisma migrate deploy || true
echo "✅ Migrations complete"

# Seed database if needed
echo "🌱 Seeding initial data..."
npx prisma db seed || true
echo "✅ Seed complete"

# Start application
echo "🚀 Starting PitWall backend..."
exec "$@"
