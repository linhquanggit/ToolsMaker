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

## Output cho game (dễ áp dụng nhất)
Drop component **`SkillTreeRuntime`** lên hero/account, gán `tree` → game đọc kết quả trực tiếp:
```csharp
var skills = hero.GetComponent<SkillTreeRuntime>();

float attack = skills.GetStat("attack", baseValue: 10f); // base + tổng buff từ tree
bool autoOpen = skills.GetFlag("chest.autoOpen");
if (skills.HasAbility("knight.piercing_thrust")) { /* cho phép dùng skill */ }

skills.TryUnlock(nodeId);          // unlock + tự lưu (autoSave) + bắn OnChanged
skills.OnChanged.AddListener(RecalcStats);   // hoặc hook ngay trong Inspector
```
- `GetStat / GetFlag / HasAbility / GetAbilityRank` — đọc output đã tổng hợp.
- `SnapshotStats()` — lấy toàn bộ stat dạng `Dictionary<string,float>` để apply hàng loạt.
- `OnChanged` (UnityEvent) — game tự tính lại khi unlock/respec; `autoSave` ghi JSON.
- Cần ghép thẳng vào hệ stat riêng? Tự implement `ISkillEffectContext` thay cho `DictionaryStatContext`.

## Editor
`Tools > SkillTree > Graph Editor`:
- Kéo asset `SkillTreeSO` vào ô **Tree**.
- Chuột phải / nút **Add Node** để tạo node (lưu thành sub-asset của tree).
- Nối port **Unlocks → Requires** để tạo prerequisite.
- **Validate**: bắt cycle, Id trùng/rỗng, node không tới được từ root.

## Data mẫu (1 click)
`Tools > SkillTree > Samples`:
- **Create Rune Tree (Global)** — tree global theo nhánh la bàn (Formation/Combat/Gold/XP/Storage/Chest), currency `gold`, node nhiều cấp, node cuối `Rune of Ascension` (1M→3M→50M).
- **Create Knight Tree (Hero)** — tree per-class (active + passive: Piercing Thrust, Shield Charge, Aegis Field, Sacred Blade...), currency `skillpoint:knight`.

Mô phỏng cấu trúc dữ liệu TaskBar Hero; sinh ra asset thật tại `Samples/Generated/` để test ngay.

## UI runtime (test trực quan)
Component `SkillTreeViewDemo` (Samples) tự dựng UI từ data — không cần prefab:
1. Tạo scene rỗng → GameObject → add **SkillTreeViewDemo**.
2. Gán `tree` (vd `RuneTree`), `startingPoints`. **Play**.
3. UI hiện cây node (vị trí theo GraphView), đường nối prerequisite, màu theo trạng thái:
   - xanh lá = MAX, xanh teal = đã có rank, vàng = mở được, cam = thiếu điểm, xám = khóa.
   - Click node để unlock / rank up; header hiện currency + điểm còn lại. Kéo để pan.
4. Nút Odin `Respec` / `Save` trên component.

`SkillTreeView.Initialize(controller)` dùng được độc lập nếu bạn tự dựng Canvas/controller.

**Demo scene đầy đủ (1 click):** `Tools > SkillTree > Samples > Create Demo Scene` → sinh scene có sẵn `Player` (DemoPlayerStats) + cây skill bên trái + **bảng PLAYER STATS bên phải**. Play → click node, xem chỉ số đổi `base (+bonus) = total` theo thời gian thực.

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
