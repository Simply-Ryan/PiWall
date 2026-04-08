import 'express-async-errors';
import express, { Express, Request, Response, NextFunction } from 'express';
import cors from 'cors';
import helmet from 'helmet';
import { logger } from '@utils/logger';

export class AppBuilder {
  private app: Express;

  constructor() {
    this.app = express();
    this.setupMiddleware();
    this.setupRoutes();
    this.setupErrorHandling();
  }

  private setupMiddleware(): void {
    // Security middleware
    this.app.use(helmet());

    // CORS
    this.app.use(
      cors({
        origin: process.env.CORS_ORIGIN?.split(',') || 'http://localhost:3000',
        credentials: true,
      })
    );

    // Body parsing
    this.app.use(express.json({ limit: '10mb' }));
    this.app.use(express.urlencoded({ limit: '10mb', extended: true }));

    // Request logging
    this.app.use((req: Request, res: Response, next: NextFunction) => {
      logger.info(
        {
          method: req.method,
          path: req.path,
          ip: req.ip,
        },
        'Incoming request'
      );
      next();
    });

    // Health check
    this.app.get('/health', (_req: Request, res: Response) => {
      res.json({ status: 'ok', timestamp: new Date().toISOString() });
    });
  }

  private setupRoutes(): void {
    // TODO: Add route modules as features are implemented
    // this.app.use('/api/telemetry', telemetryRoutes);
    // this.app.use('/api/sessions', sessionRoutes);
    // this.app.use('/api/users', userRoutes);

    this.app.get('/api/v1/status', (_req: Request, res: Response) => {
      res.json({
        service: 'PitWall Backend API',
        version: '0.1.0',
        status: 'running',
        timestamp: new Date().toISOString(),
      });
    });
  }

  private setupErrorHandling(): void {
    // 404 handler
    this.app.use((_req: Request, res: Response) => {
      res.status(404).json({
        error: 'Not Found',
        message: 'The requested resource does not exist',
        timestamp: new Date().toISOString(),
      });
    });

    // Global error handler
    this.app.use((err: any, _req: Request, res: Response, _next: NextFunction) => {
      logger.error(
        {
          error: err.message,
          stack: err.stack,
          code: err.code,
        },
        'Unhandled error'
      );

      const statusCode = err.statusCode || 500;
      const isDevelopment = process.env.NODE_ENV === 'development';

      res.status(statusCode).json({
        error: err.name || 'Internal Server Error',
        message: err.message || 'An unexpected error occurred',
        ...(isDevelopment && { stack: err.stack }),
        timestamp: new Date().toISOString(),
      });
    });
  }

  public getApp(): Express {
    return this.app;
  }
}

export const createApp = (): Express => {
  return new AppBuilder().getApp();
};
