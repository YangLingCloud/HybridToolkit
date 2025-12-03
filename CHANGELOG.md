# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.2] - 2023-11-15

### Added
- 为所有核心模块脚本添加了完整的中文注释
  - 状态机系统 (StateMachine, State, Stage, Sequence等)
  - 相机控制系统 (CameraController, CameraSettings等)
  - 事件总线系统 (EventBus, EventBusUtil等)
  - 单例模式实现 (SingletonMono, PersistentSingletonMono等)

### Updated
- 更新了package.json，添加Unity InputSystem依赖 (v1.7.0)
- 完善了主README.md的文档结构
  - 重新组织功能特性介绍
  - 添加各模块详细说明
  - 删除了主README中的代码示例，改为指向各模块README
- 更新了依赖信息，明确了UniTask (v2.5.10) 和Unity InputSystem (v1.7.0) 的版本要求

## [0.0.1] - Initial Release

### Added
- 基础状态机系统实现
- 相机控制组件
- 事件总线系统
- 单例模式实现
- 项目基本结构和配置文件
