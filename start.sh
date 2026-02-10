#!/bin/bash
# MyPhotoBooth - Quick start script for all services

set -e

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$PROJECT_ROOT"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}   MyPhotoBooth - Starting Services${NC}"
echo -e "${BLUE}========================================${NC}"

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    echo -e "${RED}Error: Docker is not running. Please start Docker Desktop.${NC}"
    exit 1
fi

# Start PostgreSQL if not running
if ! docker ps | grep -q "myphotobooth-postgres"; then
    echo -e "${YELLOW}Starting PostgreSQL...${NC}"
    docker-compose up -d postgres
else
    echo -e "${GREEN}PostgreSQL already running${NC}"
fi

# Function to cleanup background processes on exit
cleanup() {
    echo -e "\n${YELLOW}Stopping services...${NC}"
    jobs -p | xargs -r kill
    exit 0
}
trap cleanup SIGINT SIGTERM

# Start Backend
echo -e "${YELLOW}Starting Backend API (port 5149)...${NC}"
cd "$PROJECT_ROOT/src/MyPhotoBooth.API"
dotnet run > /tmp/photobooth-backend.log 2>&1 &
BACKEND_PID=$!
echo -e "${GREEN}Backend started (PID: $BACKEND_PID)${NC}"

# Wait for backend to be ready
echo -e "${YELLOW}Waiting for backend to start...${NC}"
timeout=30
while [ $timeout -gt 0 ]; do
    if curl -s http://localhost:5149/health >/dev/null 2>&1 || curl -s http://localhost:5149/scalar/v1 >/dev/null 2>&1; then
        echo -e "${GREEN}Backend is ready!${NC}"
        break
    fi
    sleep 1
    ((timeout--))
done

if [ $timeout -eq 0 ]; then
    echo -e "${RED}Backend failed to start. Check logs: tail -f /tmp/photobooth-backend.log${NC}"
    kill $BACKEND_PID 2>/dev/null
    exit 1
fi

# Start Frontend
echo -e "${YELLOW}Starting Frontend (port 3000)...${NC}"
cd "$PROJECT_ROOT/src/client"
npm run dev > /tmp/photobooth-frontend.log 2>&1 &
FRONTEND_PID=$!
echo -e "${GREEN}Frontend started (PID: $FRONTEND_PID)${NC}"

echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}   All services started successfully!   ${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "${BLUE}Frontend:${NC}  http://localhost:3000"
echo -e "${BLUE}Backend:${NC}   http://localhost:5149"
echo -e "${BLUE}API Docs:${NC}  http://localhost:5149/scalar/v1"
echo -e "\n${YELLOW}Press Ctrl+C to stop all services${NC}"

# Wait for any process to exit
wait
