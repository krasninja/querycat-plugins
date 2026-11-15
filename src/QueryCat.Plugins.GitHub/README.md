# QueryCat Plugin for GitHub

- [Functions](Functions.md)
- [Schema](Schema.md)
- [Changelog](CHANGELOG.md)

GitHub API plugin.

## Authentication

To use OAuth2, create a [GitHub OAuth app](https://github.com/settings/developers), enable **Device Flow**, and log in with its client ID:

```shell
qcat plugin auth login github --client-id <client-id>
```

The host requests the `repo`, `read:org`, and `read:packages` scopes and manages the access token. To use a personal access token instead, run:

```shell
qcat plugin auth login github --api-key
```

The legacy `github_set_token('XXX')` function remains available as a fallback.

## Example Queries

**Get commit details by hash**

```sql
select * from github_commits_ref()
where repository_full_name = 'krasninja/querycat' and sha = '208590f512789b0ce2d1b7d6f98e6a1c9e2e4a1d';
```

**Search assigned PRs**

```sql
select * from github_search_issues('is:pr archived:false sort:updated-desc is:closed ') limit 20;
```

## Links

- https://github.com/turbot/steampipe-plugin-github
- https://docs.github.com/en/rest?apiVersion=2022-11-28
