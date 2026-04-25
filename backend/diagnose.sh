#!/bin/bash

# Quick diagnostic script for PitWall backend issues

echo "🔍 PitWall Backend Diagnostics"
echo "════════════════════════════════════"
echo ""

# 1. Check Docker
echo "1. Checking Docker..."
if ! docker info > /dev/null 2>&1; then
    echo "   ❌ Docker daemon not running"
    exit 1
fi
echo "   ✓ Docker is running"

# 2. Check containers
echo ""
echo "2. Checking containers..."
echo ""
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# 3. Check if backend container exists
echo ""
echo "3. Backend container details..."
if docker inspect pitwall-api > /dev/null 2>&1; then
    echo "   ✓ Backend container exists"
    docker inspect pitwall-api | grep -E '"State":|"Running":|"Status":' | head -5
else
    echo "   ❌ Backend container not found"
fi

# 4. Check logs
echo ""
echo "4. Recent backend logs:"
echo "   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
docker logs --tail 20 pitwall-api 2>/dev/null || echo "   No logs available yet"

# 5. Check if port is listening
echo ""
echo "5. Checking port 3000..."
if netstat -tuln 2>/dev/null | grep -q 3000; then
    echo "   ✓ Port 3000 is listening"
elif ss -tuln 2>/dev/null | grep -q 3000; then
    echo "   ✓ Port 3000 is listening"
else
    echo "   ⚠ Port 3000 appears to not be listening yet"
fi

# 6. Health check
echo ""
echo "6. Testing health endpoint..."
if curl -s http://localhost:3000/health > /dev/null 2>&1; then
    echo "   ✓ Health endpoint is responding"
    curl -s http://localhost:3000/health | jq '.' 2>/dev/null || curl -s http://localhost:3000/health
else
    echo "   ❌ Health endpoint is not responding"
fi

echo ""
echo "════════════════════════════════════"
echo "✅ Diagnostics complete"
echo ""
