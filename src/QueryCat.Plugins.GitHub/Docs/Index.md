# GitHub Plugin

## Sources

### **github_branches**

```
github_branches(): Object<IRowsInput>
```

Return Github branches of specific repository.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `name` | `String` |  | Branch name. |
| `commit_sha` | `String` |  | Commit SHA the branch refers to. |
| `commit_url` | `String` |  | Commit URL the branch refers to. |
| `protected` | `Boolean` |  | True if branch is protected. |

### **github_commits**

```
github_commits(): Object<IRowsInput>
```

Return Github commits of specific repository.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `sha` | `String` |  | SHA of the commit. |
| `author_name` | `String` |  | The login name of the author of the commit. |
| `author_date` | `Timestamp` |  | Timestamp when the author made this commit. |
| `committer_login` | `String` |  | The login name of committer of the commit. |
| `verified` | `Boolean` |  | True if the commit was verified with a signature. |
| `message` | `String` |  | Commit message. |

### **github_commits_ref**

```
github_commits_ref(): Object<IRowsInput>
```

Return Github commit info of specific repository.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `sha` | `String` | yes | SHA of the commit. |
| `author_name` | `String` |  | The login name of the author of the commit. |
| `author_date` | `Timestamp` |  | Timestamp when the author made this commit. |
| `committer_login` | `String` |  | The login name of committer of the commit. |
| `verified` | `Boolean` |  | True if the commit was verified with a signature. |
| `message` | `String` |  | Commit message. |
| `additions` | `Integer` |  | The number of additions in the commit. |
| `deletions` | `Integer` |  | The number of deletions in the commit. |

### **github_pulls**

```
github_pulls(): Object<IRowsInput>
```

Return Github pull requests of specific repository.

| Name | Type | Required | Description |
| --- | --- | --- | --- |
| `repository_full_name` | `String` | yes | The full name of the repository. |
| `number` | `Integer` | yes | The pull request issue number. |
| `title` | `String` |  | Pull request title. |
| `author_login` | `String` |  | The login name of the user that submitted the PR. |
| `state` | `Void` |  | The state or the PR (open, closed). |
| `body` | `String` |  | Pull request title. |
| `additions` | `Integer` |  | The number of additions in this PR. |
| `deletions` | `Integer` |  | The number of deletions in this PR. |
| `changed_files` | `Integer` |  | The number of changed files. |
| `closed_at` | `Void` |  | The timestamp when the PR was closed. |
| `comments` | `Integer` |  | The number of comments on the PR. |
| `commits` | `Integer` |  | The number of commits in this PR. |
| `created_at` | `Timestamp` |  | The timestamp when the PR was created. |
| `draft` | `Boolean` |  | If true, the PR is in draft. |
| `base_ref` | `String` |  | The base branch of the PR in GitHub. |
| `head_ref` | `String` |  | The head branch of the PR in GitHub. |
| `locked` | `Boolean` |  | If true, the PR is locked. |
| `maintainer_can_modify` | `Boolean` |  | If true, people with push access to the upstream repository of a fork owned by a user account can commit to the forked branches. |
| `mergeable` | `Boolean` |  | If true, the PR can be merged. |
| `mergeable_state` | `Void` |  | The mergeability state of the PR. |
| `merged` | `Boolean` |  | If true, the PR has been merged. |
| `merged_at` | `Void` |  | The timestamp when the PR was merged. |
