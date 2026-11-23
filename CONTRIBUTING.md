# Git 协作流程

| 分支 | 用途 |
| --- | --- |
| `main` | 稳定、可运行的主分支。通过 Pull Request 合并。 |
| `feature/<name>` | 开发新功能，例如 `feature/player-camera`。 |
| `fix/<name>` | 修复缺陷，例如 `fix/player-respawn`。 |

1. 从最新的 `main` 创建工作分支，例如 `git switch -c feature/player-camera origin/main`。
2. 提交改动并推送工作分支。GitHub Actions 每次推送都会运行 **Push Review**。
3. 在 GitHub 开一个目标为 `main` 的 Pull Request。创建 PR 或继续向该分支推送提交时，**PR Review** 和 **Unity Compile and Tests** 会验证合入 `main` 后的结果。两项检查通过且分支与最新 `main` 同步后，即可合并。

**PR Review** 检查分支命名、合并冲突标记、空白错误、Unity 包配置、编辑器版本，以及新增或重命名脚本的 `.meta`。**Unity Compile and Tests** 使用项目指定的 Windows Unity Editor 编译脚本，并运行 EditMode 和 PlayMode 测试。日志和 NUnit XML 可从 Actions 的 `unity-validation` 附件下载。

## Unity CI 环境

在专用的隔离 Windows runner 上，通过 Unity Hub 安装 `ProjectSettings/ProjectVersion.txt` 指定的版本，并在 runner 使用的 Windows 账号下激活 Unity。runner 还需要 PowerShell 7、Git 和项目依赖源的网络访问权限。将 runner 的专用标签写入 GitHub Actions 仓库变量 `UNITY_CI_RUNNER`。

首次在 GitHub Actions 验证成功后，将 GitHub Actions 的 **Unity Compile and Tests** 添加到 `main` 规则集的必需状态检查，保留 **PR Review**。缺少编辑器、没有生成测试报告或没有测试通过都会使 Unity 检查失败。

在装有同一 Unity 版本的 Windows 环境中，可以从项目根目录重跑各阶段。

```powershell
./.github/scripts/Test-Unity.ps1 -Phase Compile
./.github/scripts/Test-Unity.ps1 -Phase EditMode
./.github/scripts/Test-Unity.ps1 -Phase PlayMode
```
