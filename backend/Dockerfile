# Build stage
FROM node:18-alpine AS builder

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN rm -f package-lock.json && \
    npm install && \
    npm install -g typescript

# Copy source code
COPY . .

# Generate Prisma client
RUN npx prisma generate

# Build TypeScript
RUN npm run build

# Runtime stage - Use Debian bullseye for OpenSSL 1.1 support
FROM node:18-bullseye

WORKDIR /app

# Install dumb-init and OpenSSL for Prisma
RUN apt-get update && \
    apt-get install -y --no-install-recommends dumb-init libssl1.1 && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy package files
COPY package*.json ./

# Install production dependencies only
RUN rm -f package-lock.json && \
    npm install --omit=dev

# Copy built application from builder
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/node_modules/.prisma ./node_modules/.prisma

# Copy prisma directory for schema
COPY --from=builder /app/prisma ./prisma

# Regenerate Prisma client for Debian target
RUN npx prisma generate

# Create logs directory
RUN mkdir -p logs

# Install netcat-openbsd for database connectivity check
RUN apt-get update && \
    apt-get install -y --no-install-recommends netcat-openbsd && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy entrypoint script
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD node -e "require('http').get('http://localhost:' + (process.env.PORT || 3000) + '/health', (r) => {if (r.statusCode !== 200) throw new Error(r.statusCode)})"

# Expose port
EXPOSE 3000

# Run application with dumb-init and entrypoint script
ENTRYPOINT ["dumb-init", "--"]
CMD ["/app/entrypoint.sh", "node", "dist/index.js"]
