# SkillTree Pack

Skill tree data-driven, tách rời, tái sử dụng nhiều nơi (mô hình Rune Tree / Hero Skill Tree kiểu Taskbar Hero).
Một định nghĩa tree → nhiều instance (1 global tree + N hero tree).

## Cài đặt
- Copy thư mục `SkillTree/` vào `Assets/`. Pack tự cô lập qua asmdef `SkillTree` (Runtime) và `SkillTree.Editor`.
- Yêu cầu: Unity 2022.3+, Odin Inspector (DLL auto-referenced).

## Kiến trúc
| Tầng | Vai trò |
|---|---|
| `SkillTreeSO` / `SkillNodeSO` | Định nghĩa tree (bất biến). |
| `SkillTreeState` | Trạng thái unlock theo từng owner (POCO, JsonUtility). |
| `SkillTreeController` | Logic: query + unlock/rankup/respec + events. |
| `ISkillEffectContext` | Biên giới tới hệ stat của game (không phụ thuộc ngược). |
| `SkillEffectSO` | Hành vi node (StatModifier / UnlockFlag / GrantAbility...). |
| `ISkillTreeStorage` | Save/Load (mặc định `JsonFileStorage`). |

## Dùng trong 3 bước
```csharp
// 1. Có asset SkillTreeSO (author bằng Tools > SkillTree > Graph Editor)
// 2. Có context — dùng sẵn DictionaryStatContext hoặc tự impl ISkillEffectContext
var context = new DictionaryStatContext();
var storage = new JsonFileStorage();

var state = storage.Exists(tree.TreeId)
    ? storage.Load(tree.TreeId)
    : SkillTreeState.CreateFor(tree, startingPoints: 10);

// 3. Điều khiển
var controller = new SkillTreeController(tree, state, context);
controller.ReapplyAllEffects();                 // sau khi load
controller.Events.OnNodeUnlocked += (node, rank) => { /* UI */ };

if (controller.TryUnlock(nodeId) == UnlockResult.Success)
    storage.Save(controller.State);
```

## Editor
`Tools > SkillTree > Graph Editor`:
- Kéo asset `SkillTreeSO` vào ô **Tree**.
- Chuột phải / nút **Add Node** để tạo node (lưu thành sub-asset của tree).
- Nối port **Unlocks → Requires** để tạo prerequisite.
- **Validate**: bắt cycle, Id trùng/rỗng, node không tới được từ root.

## Mở rộng
| Muốn thêm | Làm gì | Đụng core? |
|---|---|---|
| Hành vi node mới | subclass `SkillEffectSO` | Không |
| Kiểu chi phí mới | thêm case `CostCurve.GetCost` | Không |
| Luật prereq mới | mở rộng `PrerequisiteRule` | Không |
| Backend save mới | impl `ISkillTreeStorage` | Không |
| Gắn hệ stat khác | impl `ISkillEffectContext` | Không |
| Nhiều tree/instance | thêm asset + state | Không |

## Map sang Taskbar Hero
- **Rune Tree** (global, gold, đa nhánh, node nhiều cấp): 1 `SkillTreeSO`, `CurrencyId = "gold"`, `CostMode.Linear/PerRank`.
- **Hero Skill Tree** (per-class, active + passive): mỗi class 1 `SkillTreeSO`, mỗi hero 1 `SkillTreeState` riêng — cùng definition, nhiều instance.
