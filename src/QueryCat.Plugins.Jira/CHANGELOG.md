# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.4.7]

- Add `url` and `username` plugin configuration options (`qcat plugin options set jira ...`).
- Support API key authorization with `qcat plugin auth login jira --api-key`: Jira Cloud API token (with `username` option) or personal access token (without it).
- Resolve Jira Cloud site for OAuth2 by `url` option, show authorized sites if it cannot be selected.
- Remove the default Jira URL, show an error if it is not set.
- Remove cached configuration so refreshed OAuth2 tokens are used.
- Remove `jira_set_basic_auth()` and `jira_set_token()` functions, use `qcat plugin auth login jira` instead.
- Remove `jira_set_url()` function, use `qcat plugin options set jira url ...` instead.

## [0.4.6]

- Add `jira_set_token(token)` overload for personal access token (bearer) authentication in Jira Data Center/Server.
- Reset cached configuration when authentication settings or instance URL change.

## [0.4.5]

- Add OAuth2 authentication for Jira Cloud.
- Update RestSharp package to `114.1.0`.

## [0.4.4]

- Fix jira_issue_search() input.
