# 摩斯电码解谜游戏

Unity 2022.3 LTS 项目

## 项目概述

基于摩斯电码的解谜游戏，包含基础密码模块和原创密码推理系统。

## 项目结构

```
Assets/
├── Scripts/
│   ├── Core/         # 核心系统（单例、事件总线等）
│   ├── Input/        # 摩斯码输入系统
│   ├── Audio/        # 音频管理
│   ├── Puzzle/       # 解谜逻辑
│   └── UI/           # 界面控制
├── Prefabs/          # 预制体
├── Scenes/           # 场景文件
├── Resources/        # 动态加载资源
├── Art/              # 美术资源
│   ├── Models/       # 3D模型
│   ├── Textures/     # 纹理
│   ├── Materials/    # 材质
│   ├── Animations/   # 动画
│   └── Audio/        # 音效
├── Plugins/          # 第三方插件
└── Editor/           # 编辑器扩展
```

## 开发环境

- **Unity**: 2022.3.43f1c1 LTS
- **脚本语言**: C#
- **版本控制**: Git + Git LFS

## 协作规范

### 分支策略
- `main`: 稳定版本
- `develop`: 日常开发
- `feature/*`: 功能分支

### 提交规范
```
feat(scope): 描述
fix(scope): 描述
docs(scope): 描述
```

## 快速开始

1. 克隆仓库
2. 在 Unity Hub 中添加项目
3. 打开 `Assets/Scenes/Main` 场景
4. 开始开发

