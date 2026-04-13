# Copilot Instructions

## 项目指南
- 遵循项目的整体设计和架构，确保代码的一致性和可维护性。 
- 遵循项目设计文件 [游戏机制.docx](Assets/SmallGames/VampireSurvival/游戏机制.docx) 的设计需要和要求。
- 遵循项目功能需求 [小游戏.docx](Assets/SmallGames/VampireSurvival/小游戏.docx) 的要求。

## 项目编码规范
- 尽可能使用中文进行代码注释和文档编写，确保团队成员能够理解和维护代码。
- 遵循C#编码规范，包括命名规范、注释规范、代码结构规范等。
- 使用清晰、简洁的命名，避免使用缩写和不明确的名称。
- 在代码中添加适当的注释，解释复杂的逻辑和重要的步骤。
- 保持代码结构清晰，避免过长的方法和类，适当使用设计模式和分层架构。
- 确保代码的可读性和可维护性，遵循单一职责原则和开放封闭原则。
- 在提交代码前进行充分的测试，确保代码的正确性和稳定性。
- 遵循 [PlayerData.cs](Assets/SmallGames/VampireSurvival/Setting/Scripts/Player/PlayerData.cs) 的代码风格。
- 遵循 [GameManager.cs](Assets/SmallGames/VampireSurvival/Setting/Scripts/Core/GameManager.cs) 的代码风格。
- 使用 `PlayerHealthController` 的受击无敌状态（`isHit` / `isInSprint`）替代敌人的独立攻击冷却。