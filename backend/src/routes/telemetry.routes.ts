import { Router, Request, Response, NextFunction } from 'express';
import logger from '@utils/logger';

const router = Router();

/**
 * Telemetry routes - Will be expanded during Phase 2-3
 * - POST /api/telemetry/ingest - Receive telemetry from backend
 * - GET /api/telemetry/sessions - List recorded sessions
 * - GET /api/telemetry/sessions/:id - Get session telemetry
 * - GET /api/telemetry/analysis/:id - Get AI analysis
 */

/**
 * Health check for telemetry service
 */
router.get('/health', (_req: Request, res: Response) => {
  res.json({
    service: 'Telemetry Service',
    status: 'healthy',
    timestamp: new Date().toISOString(),
  });
});

/**
 * Get telemetry service status
 */
router.get('/status', (_req: Request, res: Response) => {
  res.json({
    service: 'Telemetry Service',
    version: '0.1.0',
    status: 'operational',
    connectedClients: 0,
    recordedSessions: 0,
    timestamp: new Date().toISOString(),
  });
});

// Error handling middleware
router.use((err: any, _req: Request, res: Response, _next: NextFunction) => {
  logger.error({ error: err }, 'Telemetry route error');
  res.status(500).json({
    error: 'Telemetry Service Error',
    message: err.message,
  });
});

export default router;
