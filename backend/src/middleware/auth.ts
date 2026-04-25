import { Request, Response, NextFunction } from 'express';
import jwt, { JwtPayload } from 'jsonwebtoken';
import { logger } from '../utils/logger';

export interface AuthRequest extends Request {
  userId?: string;
  user?: { id: string; username: string; email: string };
}

export const authMiddleware = (req: AuthRequest, res: Response, next: NextFunction): void => {
  try {
    const token = req.headers.authorization?.split(' ')[1];

    if (!token) {
      res.status(401).json({
        error: 'Unauthorized',
        message: 'No authentication token provided',
      });
      return;
    }

    const secret = process.env.JWT_SECRET || 'your-secret-key';
    const decoded = jwt.verify(token, secret) as JwtPayload & { userId: string; username: string; email: string };

    req.userId = decoded.userId;
    req.user = { id: decoded.userId, username: decoded.username, email: decoded.email };
    next();
  } catch (error) {
    logger.warn(`Authentication failed: ${error instanceof Error ? error.message : String(error)}`);
    res.status(401).json({
      error: 'Unauthorized',
      message: 'Invalid or expired token',
    });
  }
};

export const generateToken = (userId: string, username: string, email: string): string => {
  const secret = process.env.JWT_SECRET || 'your-secret-key';
  const expiresIn = process.env.JWT_EXPIRY || '7d';

  const token = jwt.sign(
    { userId, username, email },
    secret as string,
    { expiresIn } as any
  );

  return token;
};
