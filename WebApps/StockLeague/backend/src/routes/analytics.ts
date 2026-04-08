import { Router, Response } from 'express';
import { PrismaClient } from '@prisma/client';
import { authMiddleware, AuthRequest } from '../middleware/auth';
import { ValidationError, NotFoundError } from '../middleware/errorHandler';
import { logger } from '../utils/logger';

const router = Router();
const prisma = new PrismaClient();

// GET /api/sessions/:id/analytics - Detailed session analytics
router.get('/:id/analytics', authMiddleware, async (req: AuthRequest, res, next) => {
  try {
    const { id } = req.params;

    const session = await prisma.session.findUnique({
      where: { id },
      include: {
        laps: true,
        telemetry: true,
        statistics: true,
      },
    });

    if (!session) {
      throw new NotFoundError('Session not found');
    }

    if (session.userId !== req.userId) {
      return res.status(403).json({ error: 'Unauthorized' });
    }

    // Calculate analytics
    const laps = session.laps || [];
    const telemetry = session.telemetry || [];

    if (laps.length === 0) {
      throw new ValidationError('No lap data available for analytics');
    }

    const lapTimes = laps.map(l => l.lapTime);
    const bestLap = Math.min(...lapTimes);
    const worstLap = Math.max(...lapTimes);
    const avgLap = lapTimes.reduce((a, b) => a + b, 0) / lapTimes.length;
    const consistency = stddev(lapTimes);

    const speeds = telemetry.map(t => t.speed);
    const maxSpeed = speeds.length ? Math.max(...speeds) : 0;
    const avgSpeed = speeds.length ? speeds.reduce((a, b) => a + b, 0) / speeds.length : 0;

    const engineTemps = telemetry.map(t => t.engineTemp);
    const maxEngineTemp = engineTemps.length ? Math.max(...engineTemps) : 0;
    const avgEngineTemp = engineTemps.length ? engineTemps.reduce((a, b) => a + b, 0) / engineTemps.length : 0;

    // Sector analysis
    const sectorTimes = {
      sector1: laps.map(l => l.sector1).filter(s => s),
      sector2: laps.map(l => l.sector2).filter(s => s),
      sector3: laps.map(l => l.sector3).filter(s => s),
    };

    const sectorAnalysis = {
      sector1: {
        best: Math.min(...sectorTimes.sector1),
        avg: sectorTimes.sector1.reduce((a, b) => a + b, 0) / sectorTimes.sector1.length,
        worst: Math.max(...sectorTimes.sector1),
      },
      sector2: {
        best: Math.min(...sectorTimes.sector2),
        avg: sectorTimes.sector2.reduce((a, b) => a + b, 0) / sectorTimes.sector2.length,
        worst: Math.max(...sectorTimes.sector2),
      },
      sector3: {
        best: Math.min(...sectorTimes.sector3),
        avg: sectorTimes.sector3.reduce((a, b) => a + b, 0) / sectorTimes.sector3.length,
        worst: Math.max(...sectorTimes.sector3),
      },
    };

    res.json({
      session: {
        id: session.id,
        name: session.name,
        track: session.track,
        simulator: session.simulator,
        duration: session.endTime ? (session.endTime.getTime() - session.startTime.getTime()) / 1000 : 0,
      },
      laps: {
        totalLaps: laps.length,
        bestLap,
        worstLap,
        averageLap: avgLap,
        improvement: bestLap - avgLap,
        consistency,
        personalBests: laps.filter(l => l.personalBest).length,
      },
      speed: {
        maxSpeed,
        averageSpeed: avgSpeed,
        speedRange: maxSpeed - Math.min(...speeds),
      },
      temperature: {
        maxEngineTemp,
        averageEngineTemp: avgEngineTemp,
        engineTempRange: maxEngineTemp - Math.min(...engineTemps),
      },
      sectors: sectorAnalysis,
      fuel: {
        totalConsumed: laps.reduce((sum, l) => sum + (l.fuelConsumed || 0), 0),
        averagePerLap: laps.reduce((sum, l) => sum + (l.fuelConsumed || 0), 0) / laps.length,
      },
      telemetryDataPoints: telemetry.length,
    });
  } catch (error) {
    next(error);
  }
});

// GET /api/sessions/:id/lap-comparison - Compare specific laps
router.get('/:id/lap-comparison', authMiddleware, async (req: AuthRequest, res, next) => {
  try {
    const { id } = req.params;
    const { lap1, lap2 } = req.query;

    if (!lap1 || !lap2) {
      throw new ValidationError('lap1 and lap2 query parameters required');
    }

    const session = await prisma.session.findUnique({
      where: { id },
      include: { laps: true },
    });

    if (!session) {
      throw new NotFoundError('Session not found');
    }

    if (session.userId !== req.userId) {
      return res.status(403).json({ error: 'Unauthorized' });
    }

    const lapN1 = session.laps?.find(l => l.lapNumber === parseInt(lap1 as string));
    const lapN2 = session.laps?.find(l => l.lapNumber === parseInt(lap2 as string));

    if (!lapN1 || !lapN2) {
      throw new NotFoundError('One or both laps not found');
    }

    res.json({
      comparison: {
        lap1: {
          lapNumber: lapN1.lapNumber,
          lapTime: lapN1.lapTime,
          sector1: lapN1.sector1,
          sector2: lapN1.sector2,
          sector3: lapN1.sector3,
          maxSpeed: lapN1.maxSpeed,
          personalBest: lapN1.personalBest,
        },
        lap2: {
          lapNumber: lapN2.lapNumber,
          lapTime: lapN2.lapTime,
          sector1: lapN2.sector1,
          sector2: lapN2.sector2,
          sector3: lapN2.sector3,
          maxSpeed: lapN2.maxSpeed,
          personalBest: lapN2.personalBest,
        },
        differences: {
          timeDiff: lapN2.lapTime - lapN1.lapTime,
          sector1Diff: (lapN2.sector1 || 0) - (lapN1.sector1 || 0),
          sector2Diff: (lapN2.sector2 || 0) - (lapN1.sector2 || 0),
          sector3Diff: (lapN2.sector3 || 0) - (lapN1.sector3 || 0),
          speedDiff: (lapN2.maxSpeed || 0) - (lapN1.maxSpeed || 0),
        },
        winner: lapN1.lapTime < lapN2.lapTime ? 'lap1' : 'lap2',
      },
    });
  } catch (error) {
    next(error);
  }
});

// GET /api/sessions/:id/telemetry-export - Export telemetry as CSV
router.get('/:id/telemetry-export', authMiddleware, async (req: AuthRequest, res, next) => {
  try {
    const { id } = req.params;

    const session = await prisma.session.findUnique({
      where: { id },
      include: { telemetry: { orderBy: { time: 'asc' } } },
    });

    if (!session) {
      throw new NotFoundError('Session not found');
    }

    if (session.userId !== req.userId) {
      return res.status(403).json({ error: 'Unauthorized' });
    }

    // Generate CSV
    const telemetry = session.telemetry || [];
    if (telemetry.length === 0) {
      throw new ValidationError('No telemetry data to export');
    }

    // CSV Headers
    const headers = [
      'Time',
      'Speed',
      'RPM',
      'Throttle',
      'Brake',
      'Clutch',
      'Engine Temp',
      'Fuel Level',
      'Tire Wear FL',
      'Tire Wear FR',
      'Tire Wear RL',
      'Tire Wear RR',
      'Tire Temp FL',
      'Tire Temp FR',
      'Tire Temp RL',
      'Tire Temp RR',
      'Brake Temp FL',
      'Brake Temp FR',
      'Brake Temp RL',
      'Brake Temp RR',
      'Weather',
      'Track Temp',
      'Lap Number',
    ];

    const rows = telemetry.map(t => [
      t.time.toISOString(),
      t.speed.toFixed(2),
      t.rpm.toFixed(0),
      t.throttle.toFixed(1),
      t.brake.toFixed(1),
      t.clutch.toFixed(1),
      t.engineTemp.toFixed(1),
      t.fuelLevel.toFixed(1),
      t.tireWearFL.toFixed(1),
      t.tireWearFR.toFixed(1),
      t.tireWearRL.toFixed(1),
      t.tireWearRR.toFixed(1),
      t.tireTempFL.toFixed(1),
      t.tireTempFR.toFixed(1),
      t.tireTempRL.toFixed(1),
      t.tireTempRR.toFixed(1),
      t.brakeTempFL.toFixed(1),
      t.brakeTempFR.toFixed(1),
      t.brakeTempRL.toFixed(1),
      t.brakeTempRR.toFixed(1),
      t.weather,
      t.trackTemp.toFixed(1),
      t.lapNumber,
    ]);

    const csv = [
      headers.join(','),
      ...rows.map(row => row.join(',')),
    ].join('\n');

    res.setHeader('Content-Type', 'text/csv');
    res.setHeader(
      'Content-Disposition',
      `attachment; filename="telemetry-${session.id}-${Date.now()}.csv"`
    );
    res.send(csv);

    logger.info(`Telemetry exported for session ${id}`);
  } catch (error) {
    next(error);
  }
});

// GET /api/sessions/:id/weather-history - Weather conditions during session
router.get('/:id/weather-history', authMiddleware, async (req: AuthRequest, res, next) => {
  try {
    const { id } = req.params;

    const session = await prisma.session.findUnique({
      where: { id },
      include: { telemetry: { orderBy: { time: 'asc' } } },
    });

    if (!session) {
      throw new NotFoundError('Session not found');
    }

    if (session.userId !== req.userId) {
      return res.status(403).json({ error: 'Unauthorized' });
    }

    const telemetry = session.telemetry || [];

    // Group by weather condition
    const weatherGroups: { [key: string]: any[] } = {};
    telemetry.forEach(t => {
      if (!weatherGroups[t.weather]) {
        weatherGroups[t.weather] = [];
      }
      weatherGroups[t.weather].push(t);
    });

    const weatherHistory = Object.entries(weatherGroups).map(([weather, data]) => {
      const avgTemp = data.reduce((sum, t) => sum + t.trackTemp, 0) / data.length;
      const avgSpeed = data.reduce((sum, t) => sum + t.speed, 0) / data.length;

      return {
        weather,
        duration: data.length, // Approximate time in data points
        averageTrackTemp: avgTemp.toFixed(1),
        averageSpeed: avgSpeed.toFixed(1),
        startTime: data[0].time,
        endTime: data[data.length - 1].time,
      };
    });

    res.json({
      session: {
        id: session.id,
        name: session.name,
        track: session.track,
      },
      weatherHistory: weatherHistory.sort((a, b) => a.startTime.getTime() - b.startTime.getTime()),
    });
  } catch (error) {
    next(error);
  }
});

// GET /api/sessions/driver-trends - Historical performance trends
router.get('/trends/driver-performance', authMiddleware, async (req: AuthRequest, res, next) => {
  try {
    const sessions = await prisma.session.findMany({
      where: { userId: req.userId },
      include: {
        laps: { orderBy: { createdAt: 'asc' } },
      },
      orderBy: { startTime: 'asc' },
    });

    const trends = sessions.map(session => {
      const laps = session.laps || [];
      if (laps.length === 0) {
        return null;
      }

      const lapTimes = laps.map(l => l.lapTime);
      return {
        date: session.startTime,
        track: session.track,
        simulator: session.simulator,
        bestLap: Math.min(...lapTimes),
        avgLap: lapTimes.reduce((a, b) => a + b, 0) / lapTimes.length,
        totalLaps: laps.length,
        improvement: Math.min(...lapTimes) - lapTimes[lapTimes.length - 1],
        personalBests: laps.filter(l => l.personalBest).length,
      };
    });

    res.json({
      userId: req.userId,
      trends: trends.filter(t => t !== null),
    });
  } catch (error) {
    next(error);
  }
});

// Helper function: calculate standard deviation
function stddev(arr: number[]): number {
  if (arr.length < 2) return 0;
  const mean = arr.reduce((a, b) => a + b, 0) / arr.length;
  const variance = arr.reduce((sum, val) => sum + Math.pow(val - mean, 2), 0) / arr.length;
  return Math.sqrt(variance);
}

export default router;
