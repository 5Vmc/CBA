# CBA_Card

## 项目说明

本仓库的主要 Unity 工程位于 `bigBangClient` 目录。

从现有配置判断，该项目对应的产品名称为：

- `中职篮:全力以赴`

仓库根目录下还包含若干导出产物、构建资源和文档目录，例如：

- `androidExport`：Android 导出目录
- `iosExport`：iOS 导出目录
- `BuildAssets`：构建相关资源
- `docs`：接入文档、热更新说明、支付和平台资料

## Unity 版本

项目所需 Unity 版本记录在：

- `bigBangClient/ProjectSettings/ProjectVersion.txt`

当前版本为：

- `2021.3.16f1c1`

完整修订号为：

- `2021.3.16f1c1 (56dbfdd6697f)`

建议直接使用 **Unity Hub 安装并打开 Unity 2021.3.16f1c1**，避免因为小版本差异导致包导入、脚本编译或构建流程异常。

## 如何打开工程

1. 使用 Unity Hub。
2. 选择 `Add` 或 `Open`。
3. 指向目录 `bigBangClient`。
4. 使用 `Unity 2021.3.16f1c1` 打开工程。

## 默认启动信息

已确认的默认构建场景为：

- `Assets/Scenes/MainScene.unity`

构建场景配置文件：

- `bigBangClient/ProjectSettings/EditorBuildSettings.asset`

代码入口脚本：

- `bigBangClient/Assets/Scripts/Entry.cs`

该入口会在启动时初始化多个状态机状态，并首先切换到 `StateLoading`。

## 主要目录

以下目录对阅读和维护工程最重要：

- `bigBangClient/Assets/Scripts`：业务脚本、逻辑、UI、工具代码
- `bigBangClient/Assets/Babu`：构建配置、构建脚本、SDK、通用框架代码
- `bigBangClient/Assets/Scenes`：场景资源
- `bigBangClient/Assets/LocalAsset`：本地配置和资源
- `bigBangClient/Packages`：Unity 包依赖
- `bigBangClient/ProjectSettings`：Unity 工程配置
- `docs`：外部接入资料、热更新说明、支付文档、平台证书和说明

## 关键依赖

`bigBangClient/Packages/manifest.json` 中可见的主要依赖包括：

- `com.tuyoogame.yooasset` `1.2.4`
- `com.unity.textmeshpro` `3.0.8`
- `com.unity.timeline` `1.7.6`
- `com.unity.test-framework` `1.1.33`
- `com.unity.ugui` `1.0.0`

此外，工程启用了 `openupm.cn` 的 scoped registry，用于拉取 `YooAsset`。

## 构建相关

与构建直接相关的文件和目录：

- `bigBangClient/Assets/Babu/BuildConfig/BabuBuildConfig.json`
- `bigBangClient/Assets/Babu/BuildScript`

从配置可见，项目至少包含以下构建目标或渠道配置：

- `default`
- `app_store_oversea`
- `google_play`

相关 Python 构建脚本包括：

- `AndroidBuilder.py`
- `IOSBuilder.py`
- `GooglePlayBuilder.py`
- `BuildHotFix.py`

## 文档与注意事项

- `docs/热更新说明`：热更新相关资料
- `docs/咪咕`：咪咕平台接入资料
- `docs/小程序微信支付`：微信支付接入资料
- `docs/在Unity启动前使用安卓原生隐私协议`：Android 原生启动前隐私协议处理说明

仓库内存在证书、密钥、导出工程和日志等内容。正式整理仓库前，建议额外检查以下内容是否应继续保留在版本库中：

- keystore / p12 / mobileprovision / API key
- 导出目录和构建日志
- `obj`、`Logs`、`MemoryCaptures`、`.vs` 等本地生成内容

## 编码说明

本 README 建议使用 **UTF-8 with BOM** 保存，以兼容 Windows 环境下常见编辑器对中文内容的显示。
