# QueryCat Plugin for JIRA

- [Functions](Functions.md)
- [Schema](Schema.md)
- [Changelog](CHANGELOG.md)

## Usage

Before querying, you should specify the JIRA instance URL and authentication method. You can do it using the following functions:

```sql
jira_set_url('https://your-domain.atlassian.net');
jira_set_basic_auth("ivanov@example.com", "tanchiki");
```

Or you can use API token by following this URL: https://id.atlassian.com/manage-profile/security/api-tokens

```sql
jira_set_token('text@example.com', 'qwertyuiop1234567890');
```

## Examples

```sql
jira_set_token('text@example.com', JIRA_TOKEN);
jira_set_url(JIRA_URL);

select top 10 * from jira_issue_search() ji where ji.jql =
'assignee = currentUser() AND Status NOT IN (Completed., Cancelled, Canceled, Accepted, Done, Completed)';

select "__data" as data from jira_issue_search() ji where ji.jql = 'key = TEST-1863';
```
