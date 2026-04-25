import dotenv from 'dotenv';

// Load environment variables
dotenv.config();

import { createApp } from './app';
import { logger } from './utils/logger';

const PORT = parseInt(process.env.PORT || '3001', 10);
const NODE_ENV = process.env.NODE_ENV || 'development';

async function main(): Promise<void> {
  try {
    logger.info(`Starting PitWall Backend (${NODE_ENV})`);

    const app = createApp();

    app.listen(PORT, '0.0.0.0', () => {
      logger.info(`Server listening on http://0.0.0.0:${PORT} (environment: ${NODE_ENV})`);
    });

    // Graceful shutdown
    process.on('SIGTERM', () => {
      logger.info('SIGTERM received, shutting down gracefully');
      process.exit(0);
    });

    process.on('SIGINT', () => {
      logger.info('SIGINT received, shutting down gracefully');
      process.exit(0);
    });
  } catch (error) {
    logger.error(`Failed to start server: ${error instanceof Error ? error.message : String(error)}`);
    process.exit(1);
  }
}

main();
