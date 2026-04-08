// Backend test setup file
beforeAll(() => {
  // Setup test database connection, mocks, etc.
  process.env.DATABASE_URL = 'postgresql://test:test@localhost:5432/pitwall_test';
  process.env.NODE_ENV = 'test';
});

afterAll(async () => {
  // Cleanup test resources
});
