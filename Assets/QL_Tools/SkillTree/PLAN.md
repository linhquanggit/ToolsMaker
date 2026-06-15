# SkillTree Pack — Plan triển khai

Pack skill tree data-driven, tách rời, tái sử dụng ở nhiều nơi (tham khảo cơ chế Rune Tree / Hero Skill Tree của Taskbar Hero).

## Quyết định đã chốt
- **Namespace / asmdef**: `SkillTree` (phẳng, không tiền tố) — dễ tách dùng độc lập.
- **Save**: `JsonUtility` (zero dependency) — Dictionary lưu dạng `List<NodeRank>`, cache Dictionary lúc runtime.
- **Editor graph**: UIToolkit **GraphView** (kéo-nối node).
- **Đóng gói**: giữ asmdef Runtime + Editor để cô lập, drop-in sang project khác.
- Tuân convention: `DPDebug` cho log, Odin cho inspector, không comment.

## Nguyên tắc cốt lõi
1. **Definition ≠ State ≠ Effect**: định nghĩa tree (SO) bất biến, state (POCO) theo từng owner, effect agnostic với hệ stat.
2. Pack **không biết** hệ stat của game — mọi liên hệ đi qua 1 interface `ISkillEffectContext`.
3. Mở rộng = thêm subclass / impl interface, **không sửa core**.
4. Một định nghĩa tree → nhiều instance (1 global tree + N hero tree).

## Cấu trúc thư mục
```
Assets/QL_Tools/SkillTree/
├── PLAN.md
├── Runtime/
│   ├── SkillTree.asmdef
│   ├── Data/
│   │   ├── SkillTreeSO.cs
│   │   ├── SkillNodeSO.cs
│   │   ├── PrerequisiteGroup.cs
│   │   ├── CostCurve.cs
│   │   ├── SkillNodeType.cs
│   │   └── Effects/
│   │       ├── ModifierType.cs
│   │       ├── SkillEffectSO.cs          (abstract — điểm mở rộng chính)
│   │       ├── StatModifierEffectSO.cs
│   │       ├── UnlockFlagEffectSO.cs
│   │       └── GrantAbilityEffectSO.cs
│   ├── Logic/
│   │   ├── SkillTreeController.cs
│   │   ├── SkillTreeState.cs
│   │   ├── SkillTreeEvents.cs
│   │   └── UnlockResult.cs
│   ├── Context/
│   │   └── ISkillEffectContext.cs
│   └── Save/
│       ├── ISkillTreeStorage.cs
│       └── JsonFileStorage.cs
├── Editor/
│   ├── SkillTree.Editor.asmdef
│   ├── SkillTreeGraphWindow.cs           (GraphView)
│   ├── SkillNodeGraphView.cs
│   ├── SkillNodeView.cs
│   └── SkillTreeValidator.cs
└── Samples~/
    ├── RuneTree/                          ví dụ global tree (gold, đa nhánh)
    └── HeroTree/                          ví dụ per-class (active + passive)
```

## API chính
- `SkillNodeSO`: `Id`, `DisplayName/Description/Icon`, `NodeType`, `MaxRank`, `Cost(CostCurve)`, `Prerequisites`, `Effects`, `GridPosition`.
- `SkillTreeSO`: `TreeId`, `CurrencyId`, `Nodes`, `RootNodes`, `FindById()`.
- `ISkillEffectContext`: `AddStatModifier / RemoveModifiersFrom / SetFlag / GrantAbility`.
- `SkillEffectSO` (abstract): `Apply(ctx, rank)`, `Remove(ctx, rank)`, `GetPreview(rank)`.
- `SkillTreeController`: `IsUnlocked / GetRank / CanUnlock / TryUnlock / Respec / AddPoints / ReapplyAllEffects`, `Events`.
- `UnlockResult`: `Success | NotEnoughPoints | PrerequisiteNotMet | MaxRankReached | NodeNotFound`.
- `ISkillTreeStorage` + `JsonFileStorage` (persistentDataPath, có `Version` migrate).

## Dùng trong 3 bước
1. Author tree trong GraphView → ra asset `SkillTreeSO`.
2. Viết 1 class `: ISkillEffectContext` nối vào hệ stat.
3. `new SkillTreeController(tree, state, ctx)` → `TryUnlock(id)`, subscribe `Events`, `storage.Save(state)`.

## Điểm mở rộng
| Muốn thêm | Làm gì | Đụng core? |
|---|---|---|
| Hành vi node mới | subclass `SkillEffectSO` | Không |
| Kiểu chi phí mới | thêm case `CostCurve` | Không |
| Luật prereq mới | mở rộng `PrerequisiteGroup.Rule` | Không |
| Backend save mới | impl `ISkillTreeStorage` | Không |
| Gắn hệ stat khác | impl `ISkillEffectContext` | Không |
| Nhiều tree/instance | thêm asset + state | Không |

## Lộ trình
1. Core data (Data/) — không UI.
2. Logic + Events (Logic/, Context/).
3. Effects (Data/Effects/) — 3 effect mẫu.
4. Save (Save/) — JsonUtility + versioning.
5. Editor — GraphView window + Validator.
6. Samples — RuneTree + HeroTree.
7. asmdef + README tích hợp.
