# Git 协作流程

| 分支 | 用途 |
| --- | --- |
| `main` | 稳定、可运行的主分支。通过 Pull Request 合并。 |
| `feature/<name>` | 开发新功能，例如 `feature/player-camera`。 |
| `fix/<name>` | 修复缺陷，例如 `fix/player-respawn`。 |

1. 从最新的 `main` 创建工作分支，例如 `git switch -c feature/player-camera origin/main`。
2. 提交改动并推送工作分支。GitHub Actions 每次推送都会运行 **Push Review**。
3. 在 GitHub 开一个目标为 `main` 的 Pull Request。创建 PR 或继续向该分支推送提交时，**PR Review** 会检查合入 `main` 后的结果。检查通过且分支与最新 `main` 同步后，即可合并。

自动审查会检查分支命名、差异中的合并冲突标记和空白错误、Unity 包配置、编辑器版本记录，以及新增或重命名的 C# 脚本是否包含对应的 `.meta`。Unity 运行效果、编译和测试由项目验证覆盖。
