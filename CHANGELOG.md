# Changelog

## 0.2.3

- Add Development section to README
- Add GenerateDocumentationFile and RepositoryType to .csproj

## 0.2.0 (2026-03-12)

### Added

- Request and response body capture with configurable size limit
- Header logging for request and response headers
- Slow request detection with configurable threshold and callback

## 0.1.1 (2026-03-10)

- Fix README path in csproj so README displays on nuget.org

## 0.1.0 (2026-03-10)

- Initial release
- `RequestLog` record capturing method, URI, status code, elapsed time, and timestamp
- `DebugHandler` delegating handler with configurable callback
- Default constructor writes formatted log lines to `Console`
