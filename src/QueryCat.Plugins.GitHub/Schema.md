# Schema

- [github_branches](#github_branches)
- [github_commits](#github_commits)
- [github_commits_ref](#github_commits_ref)
- [github_issue_comments](#github_issue_comments)
- [github_issue_timeline](#github_issue_timeline)
- [github_pull_comments](#github_pull_comments)
- [github_pull_commits](#github_pull_commits)
- [github_pull_files](#github_pull_files)
- [github_pull_reviews](#github_pull_reviews)
- [github_pull_reviews_requests](#github_pull_reviews_requests)
- [github_pulls](#github_pulls)
- [github_rate_limits](#github_rate_limits)
- [github_search_issues](#github_search_issues)

## **github_branches**

```
github_branches(): Object<IRowsInput>
```

Return GitHub branches of specific repository.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `name`| `String` |  |  | Branch name. |
| `commit_sha`| `String` |  |  | Commit SHA the branch refers to. |
| `commit_url`| `String` |  |  | Commit URL the branch refers to. |
| `protected`| `Boolean` |  |  | True if branch is protected. |

## **github_commits**

```
github_commits(): Object<IRowsInput>
```

Return GitHub commits of specific repository.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `sha`| `String` | yes |  | SHA of the commit. |
| `author_name`| `String` | yes |  | The login name of the author of the commit. |
| `author_login`| `String` |  |  | The email name of the author of the commit. |
| `author_date`| `Timestamp` | yes |  | Timestamp when the author made this commit. |
| `committer_login`| `String` |  |  | The login name of committer of the commit. |
| `verified`| `Boolean` |  |  | True if the commit was verified with a signature. |
| `message`| `String` |  |  | Commit message. |

## **github_commits_ref**

```
github_commits_ref(): Object<IRowsInput>
```

Return GitHub commit info of specific repository.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `sha`| `Void` | yes |  | SHA of the commit. |
| `author_name`| `String` | yes |  | The login name of the author of the commit. |
| `author_login`| `String` |  |  | The email name of the author of the commit. |
| `author_date`| `Timestamp` | yes |  | Timestamp when the author made this commit. |
| `committer_login`| `String` |  |  | The login name of committer of the commit. |
| `verified`| `Boolean` |  |  | True if the commit was verified with a signature. |
| `message`| `String` |  |  | Commit message. |
| `additions`| `Integer` |  |  | The number of additions in the commit. |
| `deletions`| `Integer` |  |  | The number of deletions in the commit. |

## **github_issue_comments**

```
github_issue_comments(): Object<IRowsInput>
```

Return GitHub comments for the specific issue.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `id`| `Integer` |  |  | Comment id. |
| `issue_number`| `Void` | yes | yes | Issue number. |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `body`| `String` |  |  | Comment body. |
| `url`| `String` |  |  | Comment URL. |
| `created_by_id`| `Integer` |  |  | User id who created the comment. |
| `created_by_login`| `String` |  |  | User login who created the comment. |
| `created_at`| `Timestamp` |  |  | Date of comment creation. |
| `updated_at`| `Timestamp` |  |  | Date of comment update. |

## **github_issue_timeline**

```
github_issue_timeline(): Object<IRowsInput>
```

Return GitHub timeline for the specific issue.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `id`| `Integer` |  |  | Timeline id. |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `event`| `String` |  |  | Event. |
| `label_name`| `String` |  |  | Label name. |
| `commit_id`| `String` |  |  | The related commit identifier. |
| `actor_login`| `String` |  |  | Actor login. |
| `actor_email`| `String` |  |  | Actor email. |
| `assignee_login`| `String` |  |  | Assignee login. |
| `assignee_email`| `String` |  |  | Assignee email. |
| `number`| `Integer` | yes | yes | Issue or pull request number. |
| `created_at`| `Timestamp` |  |  | Creation date and time. |

## **github_pull_comments**

```
github_pull_comments(): Object<IRowsInput>
```

Return GitHub comments for the specific pull request.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `id`| `Integer` |  |  | Comment id. |
| `pull_review_id`| `Integer` |  |  | Pull request review id. |
| `pull_number`| `Integer` | yes | yes | Pull request number. |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `body`| `String` |  |  | Comment body. |
| `url`| `String` |  |  | Comment URL. |
| `path`| `String` |  |  | Relative path of the file the comment is about. |
| `commit_id`| `String` |  |  | Commit id. |
| `original_commit_id`| `String` |  |  | Original commit it. |
| `diff_hunk`| `String` |  |  | Diff hunk the comment is about. |
| `in_reply_to_id`| `Integer` |  |  | Id of the comment this comment replies to. |
| `position`| `Integer` |  |  | Comment position. |
| `created_by_id`| `Integer` |  |  | User id who created the comment. |
| `created_by_login`| `String` |  |  | User login who created the comment. |
| `created_at`| `Timestamp` |  |  | Date of comment creation. |
| `updated_at`| `Timestamp` |  |  | Date of comment update. |

## **github_pull_commits**

```
github_pull_commits(): Object<IRowsInput>
```

Return GitHub commits for the specific pull request.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `pull_number`| `Integer` | yes | yes | Pull request number. |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `node_id`| `String` |  |  | Node id. |
| `message`| `String` |  |  | Commit message. |
| `is_verified`| `Boolean` |  |  | Is verified. |
| `sha`| `String` |  |  | Commit SHA. |
| `url`| `String` |  |  | Commit URL. |
| `committer_login`| `String` |  |  | Committer login. |
| `committer_email`| `String` |  |  | Committer email. |
| `author_login`| `String` |  |  | Author login. |
| `author_email`| `String` |  |  | Author email. |
| `author_date`| `Timestamp` |  |  | Author contribution date. |
| `committer_date`| `Timestamp` |  |  | Committer contribution date. |

## **github_pull_files**

```
github_pull_files(): Object<IRowsInput>
```

Return GitHub files for the specific pull request.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `pull_number`| `Integer` | yes | yes | Pull request number. |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `file_name`| `String` |  |  | File name. |
| `status`| `String` |  |  | Status, one of: added, removed, modified, renamed, copied, changed, unchanged. |
| `additions`| `Integer` |  |  | Number of additions. |
| `deletions`| `Integer` |  |  | Number of deletions. |
| `changes`| `Integer` |  |  | Number of changes. |
| `patch`| `String` |  |  | File patch. |
| `blob_url`| `String` |  |  | BLOB URL. |
| `contents_url`| `String` |  |  | Contents URL. |
| `raw_url`| `String` |  |  | Raw URL. |
| `sha`| `String` |  |  | File SHA. |
| `previous_file_name`| `String` |  |  | Previous file name. |

## **github_pull_reviews**

```
github_pull_reviews(): Object<IRowsInput>
```

Return GitHub reviews for the specific pull request.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `id`| `Integer` |  |  | The identifier of the review. |
| `body`| `String` |  |  | The body of review. |
| `state`| `String` |  |  | The state (approve, comment, etc). |
| `url`| `String` |  |  | The review URL. |
| `submitted_at`| `Timestamp` |  |  | Identifies when the Pull Request Review was submitted. |
| `author_login`| `String` |  |  | Author login. |
| `author_email`| `String` |  |  | Author email. |
| `author_association`| `String` |  |  | Author's association with the subject of the PR the review was raised on. |
| `commit_id`| `String` |  |  | Commit identifier. |
| `pull_number`| `Integer` | yes | yes | Pull request number. |

## **github_pull_reviews_requests**

```
github_pull_reviews_requests(): Object<IRowsInput>
```

Return GitHub review requests for the specific pull request.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `type`| `String` |  |  | Type (team or user). |
| `id`| `Integer` |  |  | The identifier of the review request. |
| `name`| `String` |  |  | Name of user or team. |
| `ldap_name`| `String` |  |  | LDAP distinguished name. |
| `user_login`| `String` |  |  | Login for user type items. |
| `html_url`| `String` |  |  | HTML URL. |
| `pull_number`| `Integer` | yes | yes | Pull request number. |

## **github_pulls**

```
github_pulls(): Object<IRowsInput>
```

Return GitHub pull requests of specific repository.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `id`| `Integer` |  |  | Pull request id. |
| `repository_full_name`| `String` | yes | yes | The full name of the repository. |
| `number`| `Integer` | yes |  | The pull request issue number. |
| `title`| `String` |  |  | Pull request title. |
| `author_login`| `String` |  |  | The login name of the user that submitted the PR. |
| `state`| `String` | yes |  | The state or the PR (open, closed). |
| `body`| `String` |  |  | Pull request title. |
| `additions`| `Integer` |  |  | The number of additions in this PR. |
| `deletions`| `Integer` |  |  | The number of deletions in this PR. |
| `changed_files`| `Integer` |  |  | The number of changed files. |
| `closed_at`| `Timestamp` |  |  | The timestamp when the PR was closed. |
| `comments`| `Integer` |  |  | The number of comments on the PR. |
| `commits`| `Integer` |  |  | The number of commits in this PR. |
| `created_at`| `Timestamp` |  |  | The timestamp when the PR was created. |
| `draft`| `Boolean` |  |  | If true, the PR is in draft. |
| `base_ref`| `String` |  |  | The base branch of the PR in GitHub. |
| `head_ref`| `String` |  |  | The head branch of the PR in GitHub. |
| `locked`| `Boolean` |  |  | If true, the PR is locked. |
| `maintainer_can_modify`| `Boolean` |  |  | If true, people with push access to the upstream repository of a fork owned by a user account can commit to the forked branches. |
| `mergeable`| `Boolean` |  |  | If true, the PR can be merged. |
| `mergeable_state`| `String` |  |  | The mergeability state of the PR. |
| `merged`| `Boolean` |  |  | If true, the PR has been merged. |
| `merged_at`| `Timestamp` |  |  | The timestamp when the PR was merged. |

## **github_rate_limits**

```
github_rate_limits(): Object<IRowsInput>
```

Return GitHub current account rate limit information.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `limit`| `Integer` |  |  | The maximum number of requests that the consumer is permitted to make per hour. |
| `remaining`| `Integer` |  |  | The number of requests remaining in the current rate limit window. |
| `reset`| `Timestamp` |  |  | The date and time at which the current rate limit window resets. |
| `reset_at_utc_epoch`| `Integer` |  |  | The date and time at which the current rate limit window resets - in UTC epoch seconds. |

## **github_search_issues**

```
github_search_issues(): Object<IRowsInput>
```

Search GitHub issues and pull requests.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `id`| `Integer` |  |  | Issue id. |
| `title`| `String` |  |  | Issue title. |
| `body`| `String` |  |  | Issue body. |
| `state`| `String` |  |  | Issue state. |
| `author`| `String` |  |  | Issue author. |
| `comments`| `Integer` |  |  | Number of comments. |
| `number`| `Integer` |  |  | Issue number. |
| `url`| `String` |  |  | URL to HTML. |
| `closed_by_user_id`| `Integer` |  |  | The user who closed the issue. |
| `closed_at`| `Timestamp` | yes |  | The date when issue was closed. |
| `merged_at`| `Timestamp` |  |  | The date when pull request was merged. |
| `repository_full_name`| `String` |  |  |  |
| `created_at`| `Timestamp` | yes |  | Issue creation date. |
| `term`| `String` | yes |  |  |
