# GitHub Plugin for QueryCat

## Example Queries

**Get commit details by hash**

```sql
select * from github_commits_ref('krasninja/querycat', '208590f512789b0ce2d1b7d6f98e6a1c9e2e4a1d')
```

**Search assigned PRs**

```sql
select * from github_search_issues('is:pr archived:false sort:updated-desc is:closed ') limit 20;
```

## Links

- https://github.com/turbot/steampipe-plugin-github
- https://docs.github.com/en/rest?apiVersion=2022-11-28
