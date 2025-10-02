# QueryCat Plugin for GitHub

- [Functions](Functions.md)
- [Schema](Schema.md)
- [Changelog](CHANGELOG.md)

GitHub API plugin. To begin using it:

1. generate GitHub token: https://github.com/settings/tokens;
2. call `github_set_token` function before using plugin functions: `github_set_token('XXX')`;

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
