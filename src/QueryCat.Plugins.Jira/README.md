# QueryCat Plugin for JIRA

- [Functions](Functions.md)
- [Schema](Schema.md)
- [Changelog](CHANGELOG.md)

## Usage

The plugin has the following configuration options. Set them once, QueryCat stores them in its config:

| Option     | Description                                                                                       |
|------------|---------------------------------------------------------------------------------------------------|
| `url`      | Jira instance URL. Required for API key authorization. For OAuth2 it selects the Jira Cloud site.  |
| `username` | Jira Cloud username (email). Set it to use the API key as a Jira Cloud API token.                 |

```shell
qcat plugin options set jira url https://your-domain.atlassian.net
qcat plugin options list jira
```

### OAuth2 (Jira Cloud)

Log in to Atlassian using the built-in OAuth2 integration:

```shell
qcat plugin auth login jira
```

Select the Jira sites that the app can access on the Atlassian consent screen. QueryCat stores the tokens and refreshes them when needed. If more than one site is authorized, set the `url` option to select the site.

To log out, run `qcat plugin auth logout jira`.

### API key

QueryCat can store an API key instead of the OAuth2 token:

```shell
qcat plugin auth login jira --api-key
```

- **Jira Cloud API token.** Create the token here: https://id.atlassian.com/manage-profile/security/api-tokens. Set the `url` and `username` (your email) options. The token is sent using basic authentication.

    ```shell
    qcat plugin options set jira url https://your-domain.atlassian.net
    qcat plugin options set jira username ivanov@example.com
    ```

- **Personal access token (Jira Data Center/Server 8.14+).** Create it in Profile → Personal Access Tokens. Set the `url` option and leave `username` unset. The token is sent as a bearer token.

    ```shell
    qcat plugin options set jira url https://jira.example.com
    qcat plugin options unset jira username
    ```

## Examples

```sql
select top 10 * from jira_issue_search() ji where ji.jql =
'assignee = currentUser() AND Status NOT IN (Completed., Cancelled, Canceled, Accepted, Done, Completed)';

select "__data" as data from jira_issue_search() ji where ji.jql = 'key = TEST-1863';
```
