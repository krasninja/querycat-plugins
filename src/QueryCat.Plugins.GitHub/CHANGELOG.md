# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5.15]

- Add `github_gists` function.
- Fix `author_login` property mapping in `github_commits` (was incorrectly mapped to email).
- Fix `body` property description in `github_pulls`.
- Fix typo in `original_commit_id` description in `github_pull_comments`.
- Fix nullable reference handling in `github_pull_reviews_requests`.

## [0.5.14]

- Add `github_teams` function.

## [0.5.13]

- Update to .NET 10.

## [0.5.11]

- Add `github_rate_limits` source.
- Small updates.

## [0.5.10]

- Fix `committer_login` resolve.

## [0.5.9]

- Add `github_pull_files` function.
- Add `committer_email`, `author_date`, `committer_date` to `github_pull_commits` source.

## [0.4.7]

- Add `github_pull_commits` function.
