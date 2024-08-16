# Schema

## **github_branches**

```
github_branches(): Object<IRowsInput>
```

Return GitHub branches of specific repository.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `name` | `String` |  | Branch name. |
| `commit_sha` | `String` |  | Commit SHA the branch refers to. |
| `commit_url` | `String` |  | Commit URL the branch refers to. |
| `protected` | `Boolean` |  | True if branch is protected. |

## **github_issue_comments**

```
github_issue_comments(): Object<IRowsInput>
```

Return GitHub comments for the specific issue.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `Integer` |  | Comment id. |
| `issue_number` | `Void` | yes | Issue number. |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `body` | `String` |  | Comment body. |
| `url` | `String` |  | Comment URL. |
| `created_by_id` | `Integer` |  | User id who created the comment. |
| `created_by_login` | `String` |  | User login who created the comment. |
| `created_at` | `Timestamp` |  | Date of comment creation. |
| `updated_at` | `Timestamp` |  | Date of comment update. |

## **github_pull_comments**

```
github_pull_comments(): Object<IRowsInput>
```

Return GitHub comments for the specific pull request.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `Integer` |  | Comment id. |
| `pull_id` | `Integer` |  | Pull request id. |
| `pull_number` | `Integer` | yes | Pull request number. |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `body` | `String` |  | Comment body. |
| `url` | `String` |  | Comment URL. |
| `path` | `String` |  | Relative path of the file the comment is about. |
| `commit_id` | `String` |  | Commit id. |
| `original_commit_id` | `String` |  | Original commit it. |
| `diff_hunk` | `String` |  | Diff hunk the comment is about. |
| `in_reply_to_id` | `Integer` |  | Id of the comment this comment replies to. |
| `position` | `Integer` |  | Comment position. |
| `created_by_id` | `Integer` |  | User id who created the comment. |
| `created_by_login` | `String` |  | User login who created the comment. |
| `created_at` | `Timestamp` |  | Date of comment creation. |
| `updated_at` | `Timestamp` |  | Date of comment update. |

## **github_pull_reviews**

```
github_pull_reviews(): Object<IRowsInput>
```

Return GitHub reviews for the specific pull request.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `id` | `Integer` |  | The identifier of the review. |
| `body` | `String` |  | The body of review. |
| `state` | `String` |  | The state (approve, comment, etc). |
| `url` | `String` |  | The review URL. |
| `submitted_at` | `Timestamp` |  | Identifies when the Pull Request Review was submitted. |
| `author_login` | `String` |  | Author login. |
| `author_email` | `String` |  | Author email. |
| `author_association` | `String` |  | Author's association with the subject of the PR the review was raised on. |
| `commit_id` | `String` |  | Commit identifier. |
| `pull_number` | `Integer` | yes | Pull request number. |

## **github_pull_reviews_requests**

```
github_pull_reviews_requests(): Object<IRowsInput>
```

Return GitHub review requests for the specific pull request.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `type` | `String` |  | Type (team or user). |
| `id` | `Integer` |  | The identifier of the review request. |
| `name` | `String` |  | Name of user or team. |
| `ldap_name` | `String` |  | LDAP distinguished name. |
| `user_login` | `String` |  | Login for user type items. |
| `html_url` | `String` |  | HTML URL. |
| `pull_number` | `Integer` | yes | Pull request number. |

## **github_pulls**

```
github_pulls(): Object<IRowsInput>
```

Return GitHub pull requests of specific repository.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `Integer` |  | Pull request id. |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `number` | `Integer` |  | The pull request issue number. |
| `title` | `String` |  | Pull request title. |
| `author_login` | `String` |  | The login name of the user that submitted the PR. |
| `state` | `String` |  | The state or the PR (open, closed). |
| `body` | `String` |  | Pull request title. |
| `additions` | `Integer` |  | The number of additions in this PR. |
| `deletions` | `Integer` |  | The number of deletions in this PR. |
| `changed_files` | `Integer` |  | The number of changed files. |
| `closed_at` | `Timestamp` |  | The timestamp when the PR was closed. |
| `comments` | `Integer` |  | The number of comments on the PR. |
| `commits` | `Integer` |  | The number of commits in this PR. |
| `created_at` | `Timestamp` |  | The timestamp when the PR was created. |
| `draft` | `Boolean` |  | If true, the PR is in draft. |
| `base_ref` | `String` |  | The base branch of the PR in GitHub. |
| `head_ref` | `String` |  | The head branch of the PR in GitHub. |
| `locked` | `Boolean` |  | If true, the PR is locked. |
| `maintainer_can_modify` | `Boolean` |  | If true, people with push access to the upstream repository of a fork owned by a user account can commit to the forked branches. |
| `mergeable` | `Boolean` |  | If true, the PR can be merged. |
| `mergeable_state` | `String` |  | The mergeability state of the PR. |
| `merged` | `Boolean` |  | If true, the PR has been merged. |
| `merged_at` | `Timestamp` |  | The timestamp when the PR was merged. |

## **github_search_issues**

```
github_search_issues(TERM: String := 'is:open archived:false assignee:@me'): Object<IRowsInput>
```

Search GitHub issues and pull requests.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `id` | `Integer` |  | Issue id. |
| `title` | `String` |  | Issue title. |
| `body` | `String` |  | Issue body. |
| `state` | `String` |  | Issue state. |
| `author` | `String` |  | Issue author. |
| `comments` | `Integer` |  | Number of comments. |
| `number` | `Integer` |  | Issue number. |
| `url` | `String` |  | URL to HTML. |
| `closed_by_user_id` | `Integer` |  | The user who closed the issue. |
| `closed_at` | `Timestamp` |  | The date when issue was closed. |
| `repository_full_name` | `String` |  |  |
| `created_at` | `Timestamp` |  | Issue creation date. |
