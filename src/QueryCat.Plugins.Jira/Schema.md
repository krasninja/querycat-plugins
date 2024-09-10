# Schema

- [jira_issue](#jira_issue)
- [jira_issue_comments](#jira_issue_comments)
- [jira_issue_search](#jira_issue_search)

## **jira_issue**

```
jira_issue(): Object<IRowsInput>
```

Issues are the building blocks of any Jira project.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `String` |  | The issue identifier. |
| `key` | `String` | yes | The issue key. |
| `project_key` | `String` |  | The issue project key. |
| `project_name` | `String` |  | The issue project name. |
| `status` | `String` |  | The issue status. |
| `created` | `Timestamp` |  | The issue creation date and time. |
| `creator_account_id` | `String` |  | The issue creator account identifier. |
| `creator_display_name` | `String` |  | The issue creator name. |
| `summary` | `String` |  | The issue summary. |
| `priority` | `String` |  | The issue priority. |
| `description` | `String` |  | The issue description. |

## **jira_issue_comments**

```
jira_issue_comments(): Object<IRowsInput>
```

Get issue comments.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `String` |  | The comment identifier. |
| `issue_id` | `Integer` | yes | The comment issue identifier. |
| `author_account_id` | `String` |  | Author account id. |
| `author_display_name` | `String` |  | Author name. |
| `author_email_address` | `String` |  | Author email. |
| `update_author_account_id` | `String` |  | Update author account id. |
| `update_author_display_name` | `String` |  | Update author name. |
| `update_email_address` | `String` |  | Update author email. |
| `body` | `String` |  | Comment body. |
| `created` | `Timestamp` |  | Creation date and time. |
| `updated` | `Timestamp` |  | Update date and time. |

## **jira_issue_search**

```
jira_issue_search(): Object<IRowsInput>
```

Search issues using JQL.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `String` |  | The issue identifier. |
| `key` | `String` |  | The issue key. |
| `project_key` | `String` |  | The issue project key. |
| `project_name` | `String` |  | The issue project name. |
| `status` | `String` |  | The issue status. |
| `created` | `Timestamp` |  | The issue creation date and time. |
| `creator_account_id` | `String` |  | The issue creator account identifier. |
| `creator_display_name` | `String` |  | The issue creator name. |
| `summary` | `String` |  | The issue summary. |
| `priority` | `String` |  | The issue priority. |
| `jql` | `Void` | yes | JQL. |
