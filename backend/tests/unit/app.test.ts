import { createApp } from '@/app';
import request from 'supertest';

describe('Express App', () => {
  it('should start and respond to health check', async () => {
    const app = createApp();

    const response = await request(app).get('/health');

    expect(response.status).toBe(200);
    expect(response.body).toHaveProperty('status', 'ok');
    expect(response.body).toHaveProperty('timestamp');
  });

  it('should return API status', async () => {
    const app = createApp();

    const response = await request(app).get('/api/v1/status');

    expect(response.status).toBe(200);
    expect(response.body).toHaveProperty('service', 'PitWall Backend API');
    expect(response.body).toHaveProperty('version', '0.1.0');
    expect(response.body).toHaveProperty('status', 'running');
  });

  it('should return 404 for unknown routes', async () => {
    const app = createApp();

    const response = await request(app).get('/unknown-route');

    expect(response.status).toBe(404);
    expect(response.body).toHaveProperty('error', 'Not Found');
  });
});
