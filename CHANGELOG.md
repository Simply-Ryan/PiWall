# Changelog

All notable changes to PitWall will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project setup and documentation
- Folder structure for three-tier architecture (Telemetry Bridge, Frontend, Backend)
- GitHub Actions CI/CD workflow templates
- Comprehensive README.md with feature overview
- Detailed PROJECT_PLAN.md with 4-phase roadmap
- AGENT_CONTEXT.md with comprehensive development guidelines
- .gitignore templates for all three components

### Planned
- Phase 1: Telemetry Engine (Multi-sim UDP/API normalization, WebSocket server)
- Phase 2: Live Race Engineer (Dashboard HUD, Fuel Calculator, Voice Notifications)
- Phase 3: Post-Session Analytics (Telemetry Overlay, AI Debrief, Stint Tracking)
- Phase 4: Ecosystem & Hardware (Setup Library, Hardware Tracking, Team Collaboration)

## [0.0.1] - 2026-04-08 (Foundation Release)

### Added
- Initial GitHub repository initialization
- Project structure and documentation
- Development standards and guidelines

### Notes
- Foundation phase: Repository setup and planning
- CI/CD pipelines ready for development
- All three components scaffolded and ready for development
- Comprehensive documentation for developers and contributors

---

## Release Notes by Phase

### Phase 1: Telemetry Engine (Q2 2026)
**Target Release:** v0.1.0

#### Planned Features
- iRacing UDP listener with real-time telemetry parsing
- ACC (Assetto Corsa Competizione) API integration
- Assetto Corsa native UDP support
- F1 24/25 API connectors
- Unified JSON telemetry schema for all sims
- WebSocket server with sub-20ms latency
- Connection health monitoring and auto-reconnection
- Unit tests (80%+ coverage)
- Comprehensive documentation

### Phase 2: Live Race Engineer (Q3 2026)
**Target Release:** v1.0.0-alpha

#### Planned Features
- React Native client for iOS, Android, and Web
- Real-time Dashboard HUD with telemetry visualization
- Gear, Speed, and Throttle/Brake display
- Delta tracking (vs. best lap and session average)
- Tire temperature monitoring (inner/middle/outer zones)
- Tire wear visualization
- Fuel strategy calculator with consumption tracking
- "Fuel to End" predictions with safety margins
- Voice notifications with TTS integration
- Dynamic callout system ("Car left", "Leader pitted", etc.)
- Redux state management with WebSocket integration
- Unit and integration tests (70%+ coverage)
- Mobile and web responsive UI

### Phase 3: Post-Session Analytics (Q4 2026)
**Target Release:** v1.5.0

#### Planned Features
- PostgreSQL telemetry storage and indexing
- Telemetry overlay (lap-to-lap comparison graphs)
- Throttle/brake trace analysis visualization
- Speed and acceleration overlays
- AI-powered debrief system using LLM API
- Automatic telemetry analysis and insight generation
- Performance scoring system
- Stint tracker with historical tire wear tracking
- Fuel usage pattern analysis
- Pit stop optimization recommendations
- REST API for analytics and querying
- Integration tests with database
- Advanced documentation

### Phase 4: Ecosystem & Hardware (Q1 2027)
**Target Release:** v2.0.0

#### Planned Features
- User authentication and authorization system
- Multi-user profile management with role-based access control
- Cloud-synced setup library (.sto, .json format support)
- Setup tagging system (Qualifying, Race, High Downforce, etc.)
- Setup sharing with granular permissions
- Hardware health tracking module
- Wheelbase rotation hour logging
- Shifter/button click counter
- Maintenance reminder system with wear predictions
- Team collaboration features
- Shared telemetry storage for teams
- Team leaderboards and comparative analytics
- Admin dashboard for system management
- Comprehensive documentation and guides

---

## Version History Details

### v0.0.1 (2026-04-08)
- **Status:** Foundation Release
- **Components:** Project structure setup
- **Documentation:** Complete
- **Tests:** CI/CD pipelines configured
- **Issues:** None (new project)

---

## Known Limitations

### Current Limitations
- Telemetry latency varies by sim (UDP vs API)
- Voice spotter limited to English initially
- Fuel calculations assume consistent race pace
- Setup library available after Phase 4
- Hardware tracking requires manual calibration

### Tracked Issues
See [GitHub Issues](https://github.com/yourusername/PitWall/issues) for current bugs and feature requests.

---

## Deprecation Policy

Features will be marked as deprecated at least one minor version before removal. Users will be notified through:
- Release notes with clear migration path
- In-app notifications
- Documentation updates

### Example Deprecation Timeline
```
v1.0.0: Feature marked as @deprecated
v1.1.0: Feature still available but warnings shown
v2.0.0: Feature removed
```

---

## Migration Guides

### v0.1.0 → v1.0.0 (Phase 1 → Phase 2)
- WebSocket connection format updated
- Telemetry schema v1 to v2
- New fields added to `vehicle.tires` object
- See [MIGRATION_v0.1_to_v1.0.md](docs/MIGRATION_v0.1_to_v1.0.md)

---

## Contributing

We welcome contributions! When creating a pull request:

1. Reference the relevant issue
2. Follow [Conventional Commits](https://www.conventionalcommits.org/)
3. Include tests for new features
4. Update README.md and PROJECT_PLAN.md if scope changes

### Commit Message Types
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation
- `style:` - Code style (formatting, missing semicolons, etc.)
- `refactor:` - Code refactoring without behavior change
- `perf:` - Performance improvements
- `test:` - Test additions/modifications
- `chore:` - Build, CI/CD, dependencies
- `ci:` - CI/CD configuration changes

### Example
```
feat(fuel-calc): implement consumption-based pit strategy

Calculates optimal pit window based on average consumption
and remaining race distance. Includes safety margin warnings.

Closes #42
```

---

## Support

- **Documentation:** [README.md](README.md), [docs/](docs/)
- **Issues:** [GitHub Issues](https://github.com/yourusername/PitWall/issues)
- **Discussions:** [GitHub Discussions](https://github.com/yourusername/PitWall/discussions)
- **Email:** support@pitwall-racing.dev

---

## License

PitWall is licensed under the [Apache License 2.0](LICENSE).

© 2026 PitWall Contributors

---

*Changelog Last Updated: 2026-04-08*
*Follow [Keep a Changelog](https://keepachangelog.com/) format*
