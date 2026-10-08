# Gesture Manager 简体中文汉化补丁

> 非官方（第三方）汉化 · 基于 Gesture Manager **v3.9.9**
> Unofficial Simplified Chinese translation patch for [Gesture Manager](https://github.com/BlackStartx/VRC-Gesture-Manager) v3.9.9

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## 这是什么

[Gesture Manager](https://github.com/BlackStartx/VRC-Gesture-Manager) 是 BlackStartx 开发的 VRChat Unity 编辑器工具，用来在 Unity 里预览 / 调试虚拟形象（Avatar）的手势、表情、动画等。

本仓库把它**编辑器界面里的英文文案**（菜单、按钮、设置面板、径向菜单描述、提示与报错等）替换成了简体中文。**只改文字，不改任何逻辑代码**，也不包含任何运行时功能，因此：

- ✅ **不需要加载虚拟形象（本体）** 就能看到汉化效果——只要工具在 Unity 里打开，界面就是中文的；
- ✅ 不影响工具原本的功能，与原版 100% 行为一致；
- ✅ 所有 `.meta` 文件保持原样，导入后 GUID 不变，引用不会丢失。

> 共修改 23 个 C# 文件、316 处文案。

## 适用版本

- 目标版本：**Gesture Manager 3.9.9**
- 安装方式：通过 **VRChat Creator Companion (VCC) / ALCOM** 安装的原版（包名 `vrchat.blackstartx.gesture-manager`，位于 `Packages/vrchat.blackstartx.gesture-manager/`）。

> ⚠️ 它是对 3.9.9 源码的汉化。如果你装的是其它版本，文案行号会对不上，请勿混用。

## 安装方法

### 方式一：unitypackage（最简单）

1. 到本仓库 **Releases** 下载 `GestureManager-CHS-3.9.9.unitypackage`；
2. 在 Unity 顶部菜单选择 **Assets → Import Package → Custom Package...**，选中该文件；
3. 在弹出的清单里全选 → **Import**，覆盖到 `Packages/vrchat.blackstartx.gesture-manager/` 即可。

> 如果直接把包**拖进** Project 窗口没反应，请用上面的菜单方式（这是 Unity 的已知行为，跟汉化无关）。

### 方式二：手动覆盖 Scripts（VPM 用户推荐）

1. 下载 Releases 里的 `GestureManager-CHS-3.9.9.zip` 并解压；
2. 把解压出的 `Scripts/` 文件夹整体覆盖到工程的
   `Packages/vrchat.blackstartx.gesture-manager/Scripts/`；
3. 回到 Unity 等待编译完成。

### 方式三：一键安装脚本（前两种都失效时）

1. 解压 zip，把 `_一键安装脚本/GmChsInstaller.cs` 复制到工程的 `Assets/Editor/` 下（没有就新建）；
2. 等待 Unity 编译，顶部菜单会出现
   **Tools → Gesture Manager 汉化 → 安装汉化（选择 Scripts 文件夹）**；
3. 选择解压出的 `Scripts` 文件夹，脚本会自动定位插件位置、先把原文件备份到工程外，再完成覆盖。
   同一菜单下还有「还原英文」和「当前插件位置」。

### ⚠️ 关于 VPM / VCC / ALCOM 的「还原」

VPM 在下次 **Resolve / 更新** 时，可能会用官方原版把 `Packages/` 里的文件覆盖回英文。
遇到这种情况，重新走一次「方式二」或「方式三」即可恢复中文。

## 还原英文

- 重新导入官方 Gesture Manager 3.9.9 安装包覆盖；或
- 用本仓库 `_英文原版备份/Scripts/` 的内容覆盖回去。

## 汉化覆盖范围

- 设置面板（常规 / 用户数据 / 编辑器 / 模拟设置）
- 主窗口（手势、权重、克隆、OSC、模型调试）
- 径向菜单全部按钮与描述（选项 / 表情 / 外观 / 感谢 / 克隆 / 工具）
- 工具箱（场景摄像机、可点击接触器、摆姿势、背景、动画预览、性能分析、径向菜单配色）
- 调试窗口（参数 / 追踪控制 / 动画器状态）
- OSC 模块与 OSC 配置文件面板
- 各类警告、错误提示与对话框

## 有意保留英文的内容

为避免破坏功能，以下内容**不改**：

- 动画资源名：`[GESTURE] Idle`、`[EMOTE 1] Wave` 等（Resources 加载路径）
- Animator 状态名，如 `[EXTRA] CustomAnimation`
- 内部键名：`GM3 Main Color`、`GM3 SceneCamera`、`GM3 ClickableContacts`（EditorPrefs 键）
- VRChat 参数名与 URL、支持者昵称、底部版权签名中的作者名
- 版本号常量 `Gesture Manager 3.9`

## 仓库内容

```
GestureManager-Chinese/
├── README.md                 本说明
├── LICENSE                  MIT（与上游一致）
├── Scripts/                 ★ 汉化后的源码（镜像上游 Scripts 结构，可直接覆盖）
├── tools/
│   └── translations.py      316 条「英文 → 中文」翻译记录，透明可查
├── _一键安装脚本/
│   └── GmChsInstaller.cs    Unity 一键安装 / 还原脚本
└── _英文原版备份/
    └── Scripts/             上游 3.9.9 英文原版，用于还原
```

## 免责声明

- 本仓库是**非官方第三方汉化**，与原作者 **BlackStartx** 无关。
- 上游项目使用 **MIT 协议**，本汉化同样以 MIT 协议分发；请保留原作者版权与许可声明。
- 本汉化仅修改界面文案，不对原工具的可用性、兼容性或任何问题负责；使用风险由使用者自行承担。
- 如原作者不希望存在此类汉化，可联系下架。

## 致谢

- 原项目：[BlackStartx/VRC-Gesture-Manager](https://github.com/BlackStartx/VRC-Gesture-Manager)（MIT）

---

### English

This is an **unofficial** Simplified-Chinese UI translation for **Gesture Manager v3.9.9** (a VRChat Unity editor tool by BlackStartx). It only replaces editor UI strings with Chinese — **no logic is changed, and no avatar is required** to see the translation: as soon as the tool opens in Unity, its interface is in Chinese.

Install via the `GestureManager-CHS-3.9.9.unitypackage` (Unity menu **Assets → Import Package → Custom Package**), or by overwriting the `Scripts/` folder into `Packages/vrchat.blackstartx.gesture-manager/Scripts/`. A one-click installer script and the original English sources are also included for restore.

Distributed under the **MIT license**, same as upstream.
