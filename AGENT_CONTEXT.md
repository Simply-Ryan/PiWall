# 🤖 AGENT_CONTEXT.md - AI Development Instructions

This document provides comprehensive context and instructions for AI coding agents (GitHub Copilot, etc.) working on the PitWall project. Follow these guidelines to ensure consistency, quality, and architectural alignment.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Core Principles](#core-principles)
3. [Code Style & Conventions](#code-style--conventions)
4. [Architecture Patterns](#architecture-patterns)
5. [Common Tasks & Solutions](#common-tasks--solutions)
6. [Error Handling & Validation](#error-handling--validation)
7. [Testing Requirements](#testing-requirements)
8. [Documentation Requirements](#documentation-requirements)
9. [Performance & Optimization](#performance--optimization)
10. [Security Best Practices](#security-best-practices)

---

## Project Overview

**PitWall** is a three-tier simracing telemetry ecosystem:

### Components
1. **Telemetry Bridge** - Windows C# Service (UDP/API listeners, WebSocket server)
2. **Frontend** - React Native (iOS/Android/Web mobile/HUD)
3. **Backend** - Node.js + PostgreSQL (REST API, analytics, storage)

### Key Constraints
- **Zero-Bug Policy:** Strict typing, comprehensive testing, error handling
- **Unified Telemetry Schema:** All sims normalize to single JSON format
- **Sub-20ms Latency:** WebSocket streaming for real-time performance
- **Clean Architecture:** 100% separation of concerns
- **Type Safety:** No `any` types allowed

---

## Core Principles

### 1. Strict TypeScript/C# Typing
**You should:**
- ✅ Always use explicit types for variables, parameters, and returns
- ✅ Enable strict mode in tsconfig.json
- ✅ Use nullable reference types in C#
- ✅ Create interfaces/types for complex objects
- ✅ Use unions and discriminated unions instead of `any`

**Never:**
- ❌ Use `any` type
- ❌ Omit type annotations
- ❌ Use `Object` or `Function` as types
- ❌ Mix typed and untyped code

```typescript
// ❌ Bad
function validateData(data: any) {
  return data.speed > 0;
}

// ✅ Good
interface TelemetryData {
  speed: number;
  timestamp: number;
}

function validateData(data: TelemetryData): boolean {
  return data.speed > 0;
}
```

### 2. Error Handling First
**You should:**
- ✅ Create custom error classes for domain errors
- ✅ Always handle async errors with try-catch
- ✅ Provide context in error messages
- ✅ Use error boundaries in React components
- ✅ Log errors with context (not just console.error)

**Never:**
- ❌ Swallow exceptions silently
- ❌ Use generic `Error` class
- ❌ Leave error handlers empty
- ❌ Log sensitive data

```typescript
// ❌ Bad
async function processData(data: unknown) {
  const result = JSON.parse(data);
  return result;
}

// ✅ Good
export class DataProcessingError extends Error {
  constructor(message: string, public context?: Record<string, unknown>) {
    super(message);
    this.name = 'DataProcessingError';
  }
}

async function processData(data: unknown): Promise<TelemetryData> {
  try {
    if (!data) {
      throw new DataProcessingError('Data is null or undefined');
    }
    const result = JSON.parse(String(data));
    return result as TelemetryData;
  } catch (error) {
    logger.error('Failed to process data', { error, context: data });
    throw new DataProcessingError('Data processing failed', { raw: data });
  }
}
```

### 3. Test-Driven Development (TDD)
**You should:**
- ✅ Write tests BEFORE implementation for core logic
- ✅ Follow AAA pattern (Arrange-Act-Assert)
- ✅ Test unhappy paths and edge cases
- ✅ Mock external dependencies
- ✅ Aim for 80%+ coverage on core modules

**Never:**
- ❌ Write code without tests
- ❌ Skip testing error scenarios
- ❌ Leave TODOs in production code
- ❌ Test only the happy path

```typescript
// ✅ Example: TDD Pattern
describe('calculateFuelToEnd', () => {
  describe('happy path', () => {
    it('should calculate fuel needed for remaining distance', () => {
      // Arrange
      const avgConsumption = 1.5;
      const remainingDistance = 300;
      const trackLength = 10;
      const expected = 45;

      // Act
      const result = calculateFuelToEnd(avgConsumption, remainingDistance, trackLength);

      // Assert
      expect(result).toBe(expected);
    });
  });

  describe('edge cases', () => {
    it('should throw error if consumption is negative', () => {
      // Arrange
      const avgConsumption = -1.5;

      // Act & Assert
      expect(() => calculateFuelToEnd(avgConsumption, 300, 10)).toThrow(FuelCalculationError);
    });

    it('should handle zero remaining distance', () => {
      // Arrange
      const result = calculateFuelToEnd(1.5, 0, 10);

      // Assert
      expect(result).toBe(0);
    });
  });
});
```

### 4. Clean Architecture
**You should:**
- ✅ Separate domain logic from framework code
- ✅ Use dependency injection
- ✅ Create service/repository layers
- ✅ Put business logic in domain/ folder
- ✅ Keep UI dumb (presentational)

```
src/
├── domain/              # Pure business logic (tools/frameworks independent)
│   ├── models/          # Data structures (interfaces)
│   ├── services/        # Business logic (e.g., FuelCalculationService)
│   └── errors/          # Domain-specific errors
├── adapters/            # Framework integrations
│   ├── controllers/     # Express route handlers
│   ├── repositories/    # Database access
│   └── api-client/      # External API integrations
├── ui/                  # React components (frontend only)
└── tests/               # Test files
```

### 5. Documentation is Code
**You should:**
- ✅ Write JSDoc/DocFX for every function
- ✅ Include @param, @returns, @throws
- ✅ Provide @example for complex functions
- ✅ Document why, not just what
- ✅ Update docs when code changes

**Never:**
- ❌ Leave functions without documentation
- ❌ Write obvious comments
- ❌ Have outdated documentation

---

## Code Style & Conventions

### TypeScript Conventions

#### File Naming
```
src/
├── services/
│   ├── telemetry.service.ts        # Services (lowercase.service.ts)
│   └── telemetry.service.test.ts
├── components/
│   ├── DashboardHUD.tsx            # React components (PascalCase.tsx)
│   └── DashboardHUD.test.tsx
├── types/
│   └── telemetry.types.ts         # Type definitions
├── utils/
│   └── calculateFuel.ts            # Utilities (use verbs for functions)
```

#### Variable Naming
```typescript
// ✅ Good
const telemetryData: TelemetryData = {};
const avgConsumption: number = 1.5;
const isConnected: boolean = true;
const FUEL_TANK_CAPACITY = 100;  // Constants: UPPER_SNAKE_CASE

// ❌ Bad
const data: any = {};
const consumption = 1.5;           // Missing type
const connected = true;
const fuelCapacity = 100;         // Not clearly a constant
```

#### Function Naming
```typescript
// ✅ Good
function calculateFuelToEnd(consumption: number): number {}
function isValidTelemetry(data: Unknown): boolean {}
async function fetchTelemetryData(id: string): Promise<TelemetryData> {}

// ❌ Bad
function calc() {}
function check(x: any) {}
async function fetch() {}
```

### C# Conventions

#### Class & Method Naming
```csharp
// ✅ Good
public class TelemetryService
{
    public async Task ProcessDataAsync(TelemetryData data)
    {
        // Implementation
    }
}

// ❌ Bad
public class telemetryService
{
    public Task processData(TelemetryData data)  // Missing Async suffix
    {
        // Implementation
    }
}
```

#### Property Naming
```csharp
// ✅ Good
public class Vehicle
{
    public int RPM { get; set; }
    public decimal Speed { get; set; }
    public string? Model { get; set; }  // Nullable
}

// ❌ Bad
public class Vehicle
{
    public int rpm;                    // Not PascalCase
    public decimal speed;
    public object model;               // Not typed
}
```

---

## Architecture Patterns

### 1. Domain-Driven Design (DDD)

Structure your code around domain concepts:

```typescript
// ✅ Good Structure
src/domain/
├── fuel/
│   ├── FuelCalculator.ts           # Core business logic
│   ├── FuelRepository.ts           # Interface (abstraction)
│   └── errors/
│       └── InsufficientFuelError.ts
├── telemetry/
│   ├── TelemetryNormalizer.ts      # Core business logic
│   └── TelemetryParser.ts
└── races/
    ├── RaceService.ts
    └── PitStopOptimizer.ts
```

### 2. Dependency Injection

Use constructor injection for loose coupling:

```typescript
// ✅ Good
interface IFuelRepository {
  saveFuelData(data: FuelData): Promise<void>;
  getFuelHistory(vehicleId: string): Promise<FuelData[]>;
}

class FuelService {
  constructor(private fuelRepo: IFuelRepository) {}

  async analyzeFuel(vehicleId: string): Promise<FuelAnalysis> {
    const history = await this.fuelRepo.getFuelHistory(vehicleId);
    // Analysis logic
  }
}

// Usage
const repository = new PostgresFuelRepository();
const service = new FuelService(repository);
```

### 3. Service Layer Pattern

```typescript
// ✅ Domain Service (business logic)
class TelemetryNormalizationService {
  normalize(iracingData: IracingTelemetry): UnifiedTelemetry {
    return {
      timestamp: iracingData.SessionTime,
      vehicle: {
        speed: iracingData.Speed,
        rpm: iracingData.EngineRPM,
        gear: iracingData.Gear,
      },
      // ... map other fields
    };
  }
}

// ✅ API Controller (adapter)
export class TelemetryController {
  constructor(private normalizationService: TelemetryNormalizationService) {}

  @Get('/normalize')
  async normalize(@Body() data: IracingTelemetry): Promise<UnifiedTelemetry> {
    try {
      return this.normalizationService.normalize(data);
    } catch (error) {
      throw new HttpException('Normalization failed', HttpStatus.BAD_REQUEST);
    }
  }
}
```

### 4. Repository Pattern

```typescript
// ✅ Repository interface (domain)
interface ITelemetryRepository {
  save(telemetry: TelemetryData): Promise<void>;
  findById(id: string): Promise<TelemetryData | null>;
  findBySessionId(sessionId: string): Promise<TelemetryData[]>;
}

// ✅ Implementation (adapter)
export class PostgresTelemetryRepository implements ITelemetryRepository {
  constructor(private db: PrismaClient) {}

  async save(telemetry: TelemetryData): Promise<void> {
    await this.db.telemetry.create({
      data: telemetry,
    });
  }

  async findById(id: string): Promise<TelemetryData | null> {
    return this.db.telemetry.findUnique({ where: { id } });
  }

  async findBySessionId(sessionId: string): Promise<TelemetryData[]> {
    return this.db.telemetry.findMany({ where: { sessionId } });
  }
}
```

---

## Common Tasks & Solutions

### Task 1: Add a New Sim Support (e.g., F1 24)

```typescript
// Step 1: Define parser interface
interface ISimParser {
  parse(data: Buffer): TelemetryData;
  validate(data: TelemetryData): boolean;
}

// Step 2: Create F1 24 parser
export class F124Parser implements ISimParser {
  parse(data: Buffer): TelemetryData {
    // Parse F1 24 UDP packet
    const raw = this.decodeF124Packet(data);
    return {
      timestamp: Date.now(),
      session: {
        game: 'f1-24',
        track: raw.trackName,
        // ...
      },
      vehicle: {
        speed: raw.speed,
        rpm: raw.rpm,
        // ...
      },
    };
  }

  validate(data: TelemetryData): boolean {
    return data.session?.game === 'f1-24' && data.vehicle?.speed >= 0;
  }

  private decodeF124Packet(data: Buffer): Raw F124Data {
    // UDP packet decoding logic
  }
}

// Step 3: Register in telemetry bridge
class TelemetryBridge {
  private parsers: Map<string, ISimParser> = new Map();

  constructor() {
    this.parsers.set('iRacing', new IracingParser());
    this.parsers.set('ACC', new ACCParser());
    this.parsers.set('AC', new ACParser());
    this.parsers.set('F1-24', new F124Parser());  // New!
  }

  handleData(sim: string, data: Buffer): void {
    const parser = this.parsers.get(sim);
    if (!parser) throw new UnknownSimError(`Unknown sim: ${sim}`);

    const telemetry = parser.parse(data);
    if (!parser.validate(telemetry)) {
      throw new InvalidTelemetryError('Validation failed', { sim, data });
    }

    this.broadcast(telemetry);
  }
}

// Step 4: Test it
describe('F124Parser', () => {
  it('should parse F1 24 UDP packet', () => {
    const parser = new F124Parser();
    const packet = createMockF124Packet();
    const result = parser.parse(packet);

    expect(result.session?.game).toBe('f1-24');
    expect(result.vehicle?.speed).toBeGreaterThan(0);
  });

  it('should validate correctly formatted telemetry', () => {
    const parser = new F124Parser();
    const validTelemetry = createValidF124Telemetry();
    expect(parser.validate(validTelemetry)).toBe(true);
  });
});
```

### Task 2: Add a New Calculation (e.g., Tire Wear Prediction)

```typescript
// Step 1: Define domain model
interface TireWearData {
  currentWear: number;  // 0-100%
  temperature: number;  // Celsius
  lapAge: number;       // Laps on current compound
  trackGrip: number;    // 0-1
}

interface TireWearPrediction {
  wearAtLap: number;
  degradationRate: number;  // % per lap
  estWearOut: number;  // Estimated lap when tire wears out
}

// Step 2: Implement calculation service
export class TireWearCalculator {
  /**
   * Predicts tire wear progression based on current data.
   * Uses exponential degradation model.
   *
   * @param data - Current tire wear telemetry
   * @param lapsAhead - Number of future laps to predict
   * @returns Tire wear prediction at specified lap
   * @throws {TireDataError} If input data is invalid
   *
   * @example
   * ```typescript
   * const pred = calculator.predict({
   *   currentWear: 25,
   *   temperature: 95,
   *   lapAge: 5,
   *   trackGrip: 0.95
   * }, 10);
   * // { wearAtLap: 34, degradationRate: 0.9, estWearOut: 112 }
   * ```
   */
  predict(data: TireWearData, lapsAhead: number): TireWearPrediction {
    this.validate(data);

    const degradationRate = this.calculateDegradationRate(data);
    const wearAtLap = data.currentWear + (degradationRate * lapsAhead);
    const estWearOut = data.currentWear + (degradationRate * (100 - data.currentWear));

    return {
      wearAtLap: Math.min(wearAtLap, 100),
      degradationRate,
      estWearOut: Math.ceil(estWearOut),
    };
  }

  private calculateDegradationRate(data: TireWearData): number {
    // Formula: base rate * temp factor * grip factor
    const baseRate = 0.85;  // % per lap
    const tempFactor = Math.min(data.temperature / 100, 1.5);  // Hotter = more wear
    const gripFactor = data.trackGrip > 0 ? 1 / data.trackGrip : 1;

    return baseRate * tempFactor * gripFactor;
  }

  private validate(data: TireWearData): void {
    if (data.currentWear < 0 || data.currentWear > 100) {
      throw new TireDataError('Wear must be between 0 and 100', { data });
    }
    if (data.temperature < -40 || data.temperature > 150) {
      throw new TireDataError('Invalid tire temperature', { data });
    }
  }
}

// Step 3: Test it thoroughly
describe('TireWearCalculator', () => {
  let calculator: TireWearCalculator;

  beforeEach(() => {
    calculator = new TireWearCalculator();
  });

  describe('predict', () => {
    it('should predict tire wear progression', () => {
      const data: TireWearData = {
        currentWear: 25,
        temperature: 95,
        lapAge: 5,
        trackGrip: 0.95,
      };

      const result = calculator.predict(data, 10);

      expect(result.wearAtLap).toBeGreaterThan(25);
      expect(result.degradationRate).toBeGreaterThan(0);
      expect(result.estWearOut).toBeGreaterThan(result.wearAtLap);
    });

    it('should cap wear at 100%', () => {
      const data: TireWearData = {
        currentWear: 95,
        temperature: 105,
        lapAge: 50,
        trackGrip: 0.8,
      };

      const result = calculator.predict(data, 10);

      expect(result.wearAtLap).toBeLessThanOrEqual(100);
    });

    it('should throw error on invalid wear percentage', () => {
      const data: TireWearData = {
        currentWear: 150,  // Invalid!
        temperature: 95,
        lapAge: 5,
        trackGrip: 0.95,
      };

      expect(() => calculator.predict(data, 10)).toThrow(TireDataError);
    });
  });
});

// Step 4: Use in service
export class RaceService {
  constructor(private tireCalculator: TireWearCalculator) {}

  async generatePitRecommendations(sessionId: string): Promise<PitRecommendation[]> {
    const tires = await this.getTireData(sessionId);
    const recommendations: PitRecommendation[] = [];

    for (const tire of tires) {
      const prediction = this.tireCalculator.predict(tire.current, 15);

      if (prediction.wearAtLap > 80) {
        recommendations.push({
          type: 'tire-change',
          urgency: 'high',
          reason: `Tire wear will reach ${prediction.wearAtLap}%`,
          estimatedLap: prediction.estWearOut,
        });
      }
    }

    return recommendations;
  }
}
```

### Task 3: Add React Component

```typescript
// Step 1: Define types
interface TireTemperature {
  inner: number;
  middle: number;
  outer: number;
}

interface TireDisplayProps {
  position: 'FL' | 'FR' | 'RL' | 'RR';
  temperature: TireTemperature;
  wear: number;  // 0-100
  isOverheating?: boolean;
  onWarning?: (warning: string) => void;
}

// Step 2: Create component
export const TireDisplay: React.FC<TireDisplayProps> = ({
  position,
  temperature,
  wear,
  isOverheating = false,
  onWarning,
}) => {
  const [maxTemp, setMaxTemp] = React.useState(0);

  React.useEffect(() => {
    const max = Math.max(temperature.inner, temperature.middle, temperature.outer);
    setMaxTemp(max);

    if (max > 110) {
      onWarning?.(`${position} tire overheating: ${max}°C`);
    }
  }, [temperature, position, onWarning]);

  // ✅ Component without business logic - pure presentation
  return (
    <div className={`tire-display ${isOverheating ? 'danger' : ''}`}>
      <div className="tire-position">{position}</div>
      <div className="tire-temps">
        <div className="temp-inner">{Math.round(temperature.inner)}°</div>
        <div className="temp-middle">{Math.round(temperature.middle)}°</div>
        <div className="temp-outer">{Math.round(temperature.outer)}°</div>
      </div>
      <div className="tire-wear">
        <div className="wear-bar" style={{ width: `${100 - wear}%` }} />
        <span>{wear}%</span>
      </div>
    </div>
  );
};

// Step 3: Test component
import { render, screen } from '@testing-library/react';

describe('TireDisplay', () => {
  it('should render tire position and temperatures', () => {
    render(
      <TireDisplay
        position="FL"
        temperature={{ inner: 80, middle: 85, outer: 90 }}
        wear={25}
      />
    );

    expect(screen.getByText('FL')).toBeInTheDocument();
    expect(screen.getByText('80°')).toBeInTheDocument();
  });

  it('should apply danger class when overheating', () => {
    const { container } = render(
      <TireDisplay
        position="FL"
        temperature={{ inner: 120, middle: 125, outer: 130 }}
        wear={25}
        isOverheating={true}
      />
    );

    expect(container.querySelector('.tire-display')).toHaveClass('danger');
  });

  it('should call onWarning callback when temperature exceeds threshold', () => {
    const onWarning = jest.fn();

    render(
      <TireDisplay
        position="FL"
        temperature={{ inner: 110, middle: 115, outer: 120 }}
        wear={25}
        onWarning={onWarning}
      />
    );

    expect(onWarning).toHaveBeenCalledWith(expect.stringContaining('overheating'));
  });
});

// Step 4: Use in parent component
export const TireMonitr: React.FC<TireMonitorProps> = ({ telemetry }) => {
  const handleTireWarning = (warning: string) => {
    console.warn('TIRE WARNING:', warning);
    // Trigger notification sound, UI alert, etc.
  };

  return (
    <div className="tire-monitor">
      <TireDisplay
        position="FL"
        temperature={telemetry.tires.fl.temp}
        wear={telemetry.tires.fl.wear}
        isOverheating={telemetry.tires.fl.temp.middle > 110}
        onWarning={handleTireWarning}
      />
      {/* Repeat for FR, RL, RR */}
    </div>
  );
};
```

---

## Error Handling & Validation

### Custom Error Classes

```typescript
// ✅ Domain error hierarchy
export abstract class DomainError extends Error {
  abstract readonly code: string;

  constructor(message: string) {
    super(message);
    this.name = this.constructor.name;
    Error.captureStackTrace(this, this.constructor);
  }
}

export class TelemetryError extends DomainError {
  readonly code = 'TELEMETRY_ERROR';

  constructor(
    message: string,
    public readonly failedSim?: string,
    public readonly rawData?: Buffer
  ) {
    super(message);
  }
}

export class ValidationError extends DomainError {
  readonly code = 'VALIDATION_ERROR';

  constructor(
    message: string,
    public readonly field: string,
    public readonly value: unknown
  ) {
    super(message);
  }
}

// Usage in try-catch
try {
  const telemetry = parseTelemetry(data);
} catch (error) {
  if (error instanceof TelemetryError) {
    logger.error('Telemetry parsing failed', {
      sim: error.failedSim,
      code: error.code,
    });
    // Handle specific case
  } else if (error instanceof ValidationError) {
    logger.warn('Validation failed', { field: error.field });
  } else {
    logger.error('Unknown error', { error });
  }
}
```

### Input Validation

```typescript
import { z } from 'zod';

// ✅ Use Zod for runtime validation
const TelemetrySchema = z.object({
  timestamp: z.number().int().positive(),
  session: z.object({
    id: z.string().min(1),
    game: z.enum(['iRacing', 'ACC', 'AC', 'F1-24']),
  }),
  vehicle: z.object({
    speed: z.number().min(0).max(400),
    rpm: z.number().min(0).max(20000),
  }),
});

type TelemetryData = z.infer<typeof TelemetrySchema>;

// Use in service
function processTelemetry(data: unknown): TelemetryData {
  try {
    return TelemetrySchema.parse(data);
  } catch (error) {
    if (error instanceof z.ZodError) {
      throw new ValidationError('Invalid telemetry', error.issues[0].path.join('.'), data);
    }
    throw error;
  }
}
```

---

## Testing Requirements

### Unit Test Template

```typescript
describe('FunctionName', () => {
  // Setup
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('happy path', () => {
    it('should do what it promises', () => {
      // Arrange
      const input = createTestData();

      // Act
      const result = functionUnderTest(input);

      // Assert
      expect(result).toEqual(expectedValue);
    });
  });

  describe('edge cases', () => {
    it('should handle boundary values', () => {
      const result = functionUnderTest(0);
      expect(result).toBeDefined();
    });

    it('should handle null/undefined', () => {
      expect(() => functionUnderTest(null)).toThrow(Error);
    });
  });

  describe('error scenarios', () => {
    it('should throw appropriate error on invalid input', () => {
      expect(() => functionUnderTest(invalidInput)).toThrow(CustomError);
    });
  });
});
```

### Test Coverage Checklist
- ✅ Happy path (normal operation)
- ✅ Edge cases (boundaries, limits)
- ✅ Error scenarios (exceptions, failures)
- ✅ Integration with dependencies
- ✅ Performance characteristics

---

## Documentation Requirements

### Function Documentation Template

```typescript
/**
 * [Brief one-line description]
 *
 * [Longer explanation of behavior, algorithms, important notes]
 *
 * @param paramName - [Description of parameter and its constraints]
 * @param anotherParam - [Description, including valid range if applicable]
 * @returns [Description of return type and meaning]
 * @throws {ErrorType} [When this error is thrown and why]
 *
 * @example
 * ```typescript
 * const result = myFunction(param1, param2);
 * console.log(result); // Output: expectedValue
 * ```
 *
 * @see [Related functions or documentation]
 * @since 1.0.0
 */
export function myFunction(paramName: Type, anotherParam: Type): ReturnType {
  // Implementation
}
```

### README for Each Component

```markdown
# Component Name

## Overview
What does this do?

## Usage
How to use it?

## Configuration
What settings are available?

## Examples
Real-world usage examples

## Testing
How to test this component?

## Performance
Performance characteristics and benchmarks

## Troubleshooting
Common issues and solutions
```

---

## Performance & Optimization

### Performance Targets

| Metric | Target | Component |
|--------|--------|-----------|
| Telemetry latency | < 20ms | Bridge |
| API response | < 500ms | Backend |
| Dashboard FPS | 60 (desktop), 30+ (mobile) | Frontend |
| Memory usage | < 200MB | Bridge |
| CPU usage | < 40% | All |

### Optimization Checklist

- ✅ Use memoization for expensive computations
- ✅ Lazy load components and data
- ✅ Implement proper database indexes
- ✅ Cache frequently accessed data
- ✅ Use efficient algorithms (O(n) vs O(n²))
- ✅ Profile with performance tools
- ✅ Monitor memory leaks

```typescript
// ✅ Memoization example
const memoizedExpensiveCalculation = useMemo(
  () => calculateFuelToEnd(consumption, distance, trackLength),
  [consumption, distance, trackLength]
);

// ✅ Database index for frequently queried fields
// In Prisma schema:
model Telemetry {
  id       String    @id @default(cuid())
  sessionId String   // @@index([sessionId]) - Add index
  timestamp DateTime
  vehicleSpeed Float

  @@index([sessionId])
  @@index([timestamp])  // For time-range queries
}
```

---

## Security Best Practices

### Authentication & Authorization
- ✅ Use JWT tokens with short expiration
- ✅ Implement role-based access control (RBAC)
- ✅ Validate all user inputs
- ✅ Use HTTPS/WSS for all connections
- ✅ Never store passwords in plaintext

### Data Protection
- ✅ Encrypt sensitive data at rest
- ✅ Use parameterized queries to prevent SQL injection
- ✅ Validate and sanitize user input
- ✅ Never log sensitive information
- ✅ Implementation rate limiting

### Example: Secure API Endpoint

```typescript
// Protected route with auth middleware
@Get('/telemetry/:sessionId')
@UseGuards(JwtAuthGuard)
@UseGuards(RoleGuard('admin', 'user'))
async getTelemetry(
  @Param('sessionId', ParseUUIDPipe) sessionId: string,
  @Request() req: any
): Promise<TelemetryData[]> {
  // Authorization: Check if user owns this session
  const session = await this.sessionService.getSession(sessionId);
  if (session.userId !== req.user.id && req.user.role !== 'admin') {
    throw new ForbiddenException('Unauthorized');
  }

  // Return sanitized data
  return this.telemetryService.getTelemetry(sessionId);
}
```

---

## Conclusion

Follow these guidelines to ensure PitWall maintains its standards for quality, performance, and reliability. When in doubt:

1. **Ask the agents:** Check existing code for patterns
2. **Refer to PROJECT_PLAN.md:** For architectural decisions
3. **Run tests:** Make sure nothing breaks
4. **Document your changes:** Future developers (and you!) will thank you
5. **Leave code better than you found it:** Refactor as you go

---

*Document Version: 1.0*
*Last Updated: 2026-04-08*
*For Updates: Contact the PitWall Development Team*
