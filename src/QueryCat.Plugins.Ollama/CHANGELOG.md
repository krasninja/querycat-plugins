# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.3]

- Update OllamaSharp to 5.5.2.
- Fix null reference exception when a chat stream chunk has no message.
- Validate model and URL arguments and report connection errors with `QueryCatException`.
- Support Ollama servers hosted under a reverse proxy sub-path.
- Pack all third-party dependencies into the package.

## [0.1.2]

- Update OllamaSharp.

## [0.1.1]

- Update to .NET 10.

## [0.1.0]

- The initial release.
