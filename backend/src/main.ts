import dotenv from 'dotenv';

// Load environment variables
dotenv.config();

import { createApp } from './app.js';
import { logger } from '@utils/logger';

const PORT = parseInt(process.env.PORT || '3001', 10);
const NODE_ENV = process.env.NODE_ENV || 'development';

async function main(): Promise<void> {
  try {
    logger.info(`Starting PitWall Backend (${NODE_ENV})`);

    const app = createApp();

    app.listen(PORT, '0.0.0.0', () => {
      logger.info(
        {
          port: PORT,
          environment: NODE_ENV,
          url: `http://localhost:${PORT}`,
        },
        'Server listening'
      );
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
    logger.error({ error }, 'Failed to start server');
    process.exit(1);
  }
}

main();
