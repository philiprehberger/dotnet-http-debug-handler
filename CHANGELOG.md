# Changelog

## 0.2.7 (2026-03-31)

- Standardize README to 3-badge format with emoji Support section
- Update CI actions to v5 for Node.js 24 compatibility
- Add GitHub issue templates, dependabot config, and PR template

## 0.2.6 (2026-03-24)

- Add unit tests
- Add test step to CI workflow

## 0.2.5 (2026-03-22)

- Add dates to changelog entries

## 0.2.4 (2026-03-20)

- Align README and csproj descriptions

## 0.2.3 (2026-03-16)

- Add Development section to README
- Add GenerateDocumentationFile and RepositoryType to .csproj

## 0.2.0 (2026-03-12)

- Add request and response body capture with configurable size limit
- Add header logging for request and response headers
- Add slow request detection with configurable threshold and callback

## 0.1.1 (2026-03-10)

- Fix README path in csproj so README displays on nuget.org

## 0.1.0 (2026-03-10)

- Initial release
- `RequestLog` record capturing method, URI, status code, elapsed time, and timestamp
- `DebugHandler` delegating handler with configurable callback
- Default constructor writes formatted log lines to `Console`
