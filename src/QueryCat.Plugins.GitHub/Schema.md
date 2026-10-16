# Schema

- [github_branches](#github_branches)
- [github_commits](#github_commits)
- [github_commits_ref](#github_commits_ref)
- [github_gists](#github_gists)
- [github_issue_comments](#github_issue_comments)
- [github_issue_timeline](#github_issue_timeline)
- [github_organization_members](#github_organization_members)
- [github_organizations](#github_organizations)
- [github_package_versions](#github_package_versions)
- [github_packages](#github_packages)
- [github_pull_comments](#github_pull_comments)
- [github_pull_commits](#github_pull_commits)
- [github_pull_files](#github_pull_files)
- [github_pull_reviews](#github_pull_reviews)
- [github_pull_reviews_requests](#github_pull_reviews_requests)
- [github_pulls](#github_pulls)
- [github_rate_limits](#github_rate_limits)
- [github_repositories](#github_repositories)
- [github_repository_collaborators](#github_repository_collaborators)
- [github_repository_contents](#github_repository_contents)
- [github_repository_deployments](#github_repository_deployments)
- [github_repository_environments](#github_repository_environments)
- [github_search_issues](#github_search_issues)
- [github_tags](#github_tags)
- [github_team_members](#github_team_members)
- [github_team_repositories](#github_team_repositories)
- [github_teams](#github_teams)
- [github_tree](#github_tree)
- [github_user](#github_user)
- [github_workflows](#github_workflows)

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
| `author_login`| `String` |  |  | The login name of the author of the commit. |
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
| `author_login`| `String` |  |  | The login name of the author of the commit. |
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

## **github_organization_members**

```
github_organization_members(): Object<IRowsInput>
```

Return GitHub organization members.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `organization`| `String` | yes | yes | The organization login name. |
| `login`| `String` |  |  | The login name of the member. |
| `id`| `Integer` |  |  | The unique ID of the member. |
| `node_id`| `String` |  |  | The node ID of the member. |
| `name`| `String` |  |  | The name of the member. |
| `company`| `String` |  |  | The company of the member. |
| `blog`| `String` |  |  | The blog URL of the member. |
| `location`| `String` |  |  | The location of the member. |
| `email`| `String` |  |  | The email address of the member. |
| `bio`| `String` |  |  | The bio of the member. |
| `avatar_url`| `String` |  |  | The avatar URL of the member. |
| `html_url`| `String` |  |  | The HTML URL of the member's profile. |
| `type`| `String` |  |  | The type of account. |
| `site_admin`| `Boolean` |  |  | Whether the member is a site administrator. |
| `created_at`| `Timestamp` |  |  | The timestamp when the member was created. |
| `updated_at`| `Timestamp` |  |  | The timestamp when the member was last updated. |
| `followers`| `Integer` |  |  | The number of followers. |
| `following`| `Integer` |  |  | The number of following. |
| `public_repos`| `Integer` |  |  | The number of public repositories. |
| `public_gists`| `Integer` |  |  | The number of public gists. |

## **github_organizations**

```
github_organizations(): Object<IRowsInput>
```

Return GitHub organizations.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `login`| `String` | yes |  | The login name of the organization. |
| `id`| `Integer` |  |  | The unique ID of the organization. |
| `node_id`| `String` |  |  | The node ID of the organization. |
| `name`| `String` |  |  | The name of the organization. |
| `company`| `String` |  |  | The company name of the organization. |
| `description`| `String` |  |  | The description of the organization. |
| `is_verified`| `Boolean` |  |  | Whether the organization is verified. |
| `avatar_url`| `String` |  |  | The avatar URL of the organization. |
| `html_url`| `String` |  |  | The HTML URL of the organization. |
| `blog`| `String` |  |  | The blog URL of the organization. |
| `email`| `String` |  |  | The email address of the organization. |
| `location`| `String` |  |  | The location of the organization. |
| `bio`| `String` |  |  | The bio of the organization. |
| `created_at`| `Timestamp` |  |  | The timestamp when the organization was created. |
| `updated_at`| `Timestamp` |  |  | The timestamp when the organization was last updated. |
| `total_private_repos`| `Integer` |  |  | The total number of private repositories. |
| `owned_private_repos`| `Integer` |  |  | The number of owned private repositories. |
| `public_repos`| `Integer` |  |  | The number of public repositories. |
| `public_gists`| `Integer` |  |  | The number of public gists. |
| `private_gists`| `Integer` |  |  | The number of private gists. |
| `followers`| `Integer` |  |  | The number of followers. |
| `following`| `Integer` |  |  | The number of following. |
| `collaborators`| `Integer` |  |  | The number of collaborators. |
| `disk_usage`| `Integer` |  |  | The disk usage in kilobytes. |
| `type`| `String` |  |  | The type of the account. |
| `url`| `String` |  |  | The API URL of the organization. |

## **github_package_versions**

```
github_package_versions(): Object<IRowsInput>
```

Return GitHub package versions for a specific package in an organization.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `organization`| `String` | yes | yes | The organization login name. |
| `package_name`| `String` | yes | yes | The name of the package. |
| `package_type`| `String` | yes | yes | The type of the package (npm, maven, rubygems, docker, nuget, or container). |
| `id`| `Integer` |  |  | The unique ID of the package version. |
| `name`| `String` |  |  | The name of the package version. |
| `url`| `String` |  |  | The API URL of the package version. |
| `html_url`| `String` |  |  | The HTML URL of the package version. |
| `package_html_url`| `String` |  |  | The HTML URL of the package. |
| `created_at`| `Timestamp` |  |  | The timestamp when the package version was created. |
| `updated_at`| `Timestamp` |  |  | The timestamp when the package version was last updated. |

## **github_packages**

```
github_packages(): Object<IRowsInput>
```

Return GitHub packages for an organization.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `organization`| `String` | yes | yes | The organization login name. |
| `id`| `Integer` |  |  | The unique ID of the package. |
| `name`| `String` |  |  | The name of the package. |
| `package_type`| `String` | yes |  | The type of the package (npm, maven, rubygems, docker, nuget, or container). |
| `visibility`| `String` | yes |  | The visibility of the package (public, private, or internal). |
| `version_count`| `Integer` |  |  | The number of versions of the package. |
| `url`| `String` |  |  | The API URL of the package. |
| `html_url`| `String` |  |  | The HTML URL of the package. |
| `created_at`| `Timestamp` |  |  | The timestamp when the package was created. |
| `updated_at`| `Timestamp` |  |  | The timestamp when the package was last updated. |
| `owner_login`| `String` |  |  | The login name of the package owner. |
| `owner_id`| `Integer` |  |  | The unique ID of the package owner. |
| `owner_type`| `String` |  |  | The type of the package owner. |

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
| `original_commit_id`| `String` |  |  | Original commit id. |
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
| `author_name`| `String` |  |  | The name of the user that submitted the PR. |
| `author_login`| `String` |  |  | The login name of the user that submitted the PR. |
| `state`| `String` | yes |  | The state or the PR (open, closed). |
| `body`| `String` |  |  | Pull request body. |
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

## **github_repository_deployments**

```
github_repository_deployments(): Object<IRowsInput>
```

Return GitHub deployments of a specific repository.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository (owner/repo). |
| `id`| `Integer` |  |  | The unique ID of the deployment. |
| `node_id`| `String` |  |  | The node ID of the deployment. |
| `sha`| `String` |  |  | The SHA of the deployment. |
| `ref`| `String` |  |  | The ref of the deployment. |
| `task`| `String` |  |  | The deployment task. |
| `environment`| `String` | yes |  | The environment of the deployment. |
| `original_environment`| `String` |  |  | The original environment of the deployment. |
| `description`| `String` |  |  | The description of the deployment. |
| `creator_login`| `String` |  |  | The login of the user who created the deployment. |
| `created_at`| `Timestamp` |  |  | The timestamp when the deployment was created. |
| `updated_at`| `Timestamp` |  |  | The timestamp when the deployment was last updated. |
| `statuses_url`| `String` |  |  | The URL for the deployment statuses. |
| `repository_url`| `String` |  |  | The URL of the repository. |
| `payload`| `Object` |  |  | The payload of the deployment. |

## **github_repository_environments**

```
github_repository_environments(): Object<IRowsInput>
```

Return GitHub environments of a specific repository.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository (owner/repo). |
| `id`| `Integer` |  |  | The unique ID of the environment. |
| `node_id`| `String` |  |  | The node ID of the environment. |
| `name`| `String` |  |  | The name of the environment. |
| `url`| `String` |  |  | The API URL of the environment. |
| `html_url`| `String` |  |  | The HTML URL of the environment. |
| `created_at`| `Timestamp` |  |  | The timestamp when the environment was created. |
| `updated_at`| `Timestamp` |  |  | The timestamp when the environment was last updated. |

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

## **github_tags**

```
github_tags(): Object<IRowsInput>
```

Return GitHub tags of a specific repository.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository (owner/repo). |
| `name`| `String` |  |  | The name of the tag. |
| `commit_sha`| `String` |  |  | The SHA of the commit the tag points to. |
| `commit_url`| `String` |  |  | The URL of the commit the tag points to. |
| `zipball_url`| `String` |  |  | The URL of the zipball archive for the tag. |
| `tarball_url`| `String` |  |  | The URL of the tarball archive for the tag. |

## **github_team_members**

```
github_team_members(): Object<IRowsInput>
```

Return GitHub members of a specific team.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `organization`| `String` | yes | yes | The organization login name. |
| `team_slug`| `String` | yes | yes | The team slug name. |
| `login`| `String` |  |  | The login name of the team member. |
| `id`| `Integer` |  |  | The unique ID of the team member. |
| `node_id`| `String` |  |  | The node ID of the team member. |
| `avatar_url`| `String` |  |  | The avatar URL of the team member. |
| `html_url`| `String` |  |  | The GitHub profile URL of the team member. |
| `type`| `String` |  |  | The type of the team member (User or Bot). |
| `site_admin`| `Boolean` |  |  | True if the team member is a site admin. |

## **github_team_repositories**

```
github_team_repositories(): Object<IRowsInput>
```

Return GitHub repositories of a specific team.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `organization`| `String` | yes | yes | The organization login name. |
| `team_slug`| `String` | yes | yes | The team slug name. |
| `id`| `Integer` |  |  | The unique ID of the repository. |
| `name`| `String` |  |  | The name of the repository. |
| `full_name`| `String` |  |  | The full name of the repository (owner/repo). |
| `description`| `String` |  |  | The description of the repository. |
| `private`| `Boolean` |  |  | True if the repository is private. |
| `fork`| `Boolean` |  |  | True if the repository is a fork. |
| `archived`| `Boolean` |  |  | True if the repository is archived. |
| `language`| `String` |  |  | The primary language of the repository. |
| `stargazers_count`| `Integer` |  |  | The number of stars on the repository. |
| `forks_count`| `Integer` |  |  | The number of forks of the repository. |
| `open_issues_count`| `Integer` |  |  | The number of open issues. |
| `html_url`| `String` |  |  | The HTML URL of the repository. |
| `clone_url`| `String` |  |  | The clone URL of the repository. |
| `default_branch`| `String` |  |  | The default branch of the repository. |
| `permissions_admin`| `Boolean` |  |  | True if the team has admin access to the repository. |
| `permissions_push`| `Boolean` |  |  | True if the team has push access to the repository. |
| `permissions_pull`| `Boolean` |  |  | True if the team has pull access to the repository. |

## **github_teams**

```
github_teams(): Object<IRowsInput>
```

Return GitHub teams of specific organization.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `organization`| `String` | yes | yes | The organization login name. |
| `id`| `Integer` |  |  | Team id. |
| `node_id`| `String` |  |  | Team node id. |
| `name`| `String` |  |  | Team name. |
| `slug`| `String` |  |  | Team slug. |
| `description`| `String` |  |  | Team description. |
| `privacy`| `String` |  |  | Team privacy (secret or closed). |
| `permission`| `String` |  |  | Team permission. |
| `members_count`| `Integer` |  |  | Number of team members. |
| `repos_count`| `Integer` |  |  | Number of repositories. |
| `url`| `String` |  |  | Team URL. |

## **github_tree**

```
github_tree(): Object<IRowsInput>
```

Return GitHub tree entries of a specific repository and SHA.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository (owner/repo). |
| `tree_sha`| `String` | yes | yes | The SHA of the tree. |
| `recursive`| `Boolean` | yes |  | If true, return objects or subtrees referenced by the tree. |
| `path`| `String` |  |  | The file path referenced in the tree. |
| `mode`| `String` |  |  | The file mode (100644 for blob, 100755 for executable, 040000 for tree, 160000 for commit, 120000 for symlink). |
| `type`| `Void` |  |  | The type of object (blob, tree, or commit). |
| `sha`| `String` |  |  | The SHA1 checksum ID of the object in the tree. |
| `size`| `Integer` |  |  | The size of the blob in bytes. |
| `url`| `String` |  |  | The URL to the object referenced in the tree. |

## **github_user**

```
github_user(): Object<IRowsInput>
```

Return GitHub user information by login.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `login`| `String` | yes | yes | The login name of the user. |
| `id`| `Integer` |  |  | The unique ID of the user. |
| `node_id`| `String` |  |  | The node ID of the user. |
| `avatar_url`| `String` |  |  | The avatar URL of the user. |
| `html_url`| `String` |  |  | The GitHub profile URL of the user. |
| `type`| `String` |  |  | The type of account (User or Organization). |
| `site_admin`| `Boolean` |  |  | True if the user is a site admin. |
| `name`| `String` |  |  | The name of the user. |
| `company`| `String` |  |  | The company of the user. |
| `blog`| `String` |  |  | The blog URL of the user. |
| `location`| `String` |  |  | The location of the user. |
| `email`| `String` |  |  | The email of the user. |
| `hireable`| `Boolean` |  |  | True if the user is hireable. |
| `bio`| `String` |  |  | The biography of the user. |
| `public_repos`| `Integer` |  |  | The number of public repositories. |
| `public_gists`| `Integer` |  |  | The number of public gists. |
| `followers`| `Integer` |  |  | The number of followers. |
| `following`| `Integer` |  |  | The number of users following. |
| `created_at`| `Timestamp` |  |  | The timestamp when the user was created. |
| `updated_at`| `Timestamp` |  |  | The timestamp when the user was last updated. |

## **github_workflows**

```
github_workflows(): Object<IRowsInput>
```

Return GitHub Actions workflows of a specific repository.

| Name | Type | Key | Required | Description |
| --- | --- | --- | --- | --- |
| `repository_full_name`| `String` | yes | yes | The full name of the repository (owner/repo). |
| `id`| `Integer` |  |  | The unique ID of the workflow. |
| `node_id`| `String` |  |  | The node ID of the workflow. |
| `name`| `String` |  |  | The name of the workflow. |
| `path`| `String` |  |  | The path to the workflow file. |
| `state`| `Void` |  |  | The state of the workflow (active, deleted, disabled_fork, disabled_inactivity, disabled_manually). |
| `created_at`| `Timestamp` |  |  | The timestamp when the workflow was created. |
| `updated_at`| `Timestamp` |  |  | The timestamp when the workflow was last updated. |
| `url`| `String` |  |  | The API URL of the workflow. |
| `html_url`| `String` |  |  | The HTML URL of the workflow. |
| `badge_url`| `String` |  |  | The badge URL of the workflow. |
