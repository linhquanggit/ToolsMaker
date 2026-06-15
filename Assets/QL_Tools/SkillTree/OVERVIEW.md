# SkillTree — Tổng quan & Hướng dẫn

Tài liệu giải thích: mô hình pack, cách **thêm node/skill**, các **loại effect**, và cách **apply vào game**.

---

## 1. Mô hình (đọc trước)

Pack tách làm 3 tầng — đây là điều quan trọng nhất:

| Tầng | Là gì | Sống ở đâu |
|---|---|---|
| **Definition** | Định nghĩa cây: node, cost, prereq, effect | Asset `SkillTreeSO` (bất biến lúc chạy) |
| **State** | Ai đã mở gì, còn bao nhiêu điểm | `SkillTreeState` (POCO, lưu JSON, **mỗi owner một bản**) |
| **Effect → Game** | Hiệu ứng thực sự lên stat/skill | Qua interface `ISkillEffectContext` (game tự định nghĩa) |

→ Một asset tree dùng cho **nhiều instance**: 1 global Rune Tree + N hero, mỗi hero một `State` riêng nhưng chung một `SkillTreeSO`.

**Luồng dữ liệu:**
```
[GraphView]  ──vẽ──>  SkillTreeSO (definition)
                          │
   new SkillTreeController(def, state, context)
                          │ TryUnlock(nodeId)
                          ▼
   trừ điểm → set rank → áp Effects của node
                          │ effect.Apply(context, rank, node)
                          ▼
   ISkillEffectContext  (vd DictionaryStatContext)
                          │
   game đọc: GetStat / GetFlag / HasAbility   ← áp vào gameplay
                          │
   storage.Save(state)  → JSON
```

---

## 2. Cấu trúc một node (`SkillNodeSO`)

| Field | Ý nghĩa |
|---|---|
| `id` | GUID tự sinh — khóa ổn định để lưu state (đừng sửa) |
| `displayName` / `description` / `icon` | Hiển thị |
| `nodeType` | `Active` (skill bấm) hoặc `Passive` (buff) — chỉ để phân loại/hiển thị |
| `maxRank` | Số cấp tối đa (1 = mở/khóa; >1 = nâng cấp nhiều lần) |
| `cost` | `CostCurve` — giá mỗi rank |
| `prerequisites` | Điều kiện mở (xem mục 4) |
| `effects` | Danh sách `SkillEffectSO` — node *làm gì* (xem mục 3) |

`cost` là `CostCurve` với 4 mode:
- **Fixed**: giá cố định mọi rank.
- **Linear**: `baseCost + perRankIncrement * (rank-1)`.
- **PerRank**: mảng giá từng rank.
- **Curve**: theo `AnimationCurve`.

---

## 3. Các loại Effect (node *làm gì*)

Effect chỉ **mô tả** (một string key + giá trị). Tác động thật do `ISkillEffectContext` của bạn quyết định.

### a. Stat Modifier — đổi chỉ số
`Create > SkillTree > Effects > Stat Modifier`

| Field | Ví dụ |
|---|---|
| `statId` | `attack`, `maxHp`, `goldPerKill` |
| `valuePerRank` | `5` |
| `modifierType` | `Additive` (cộng) · `Multiplicative` (nhân) · `Override` (đè) |

Rank `r` → `AddStatModifier(statId, valuePerRank * r, type, node)`. Vd `attack +5/rank`, rank 3 = `+15`.

### b. Unlock Flag — mở khóa tính năng (boolean)
`Create > SkillTree > Effects > Unlock Flag`

| Field | Ví dụ |
|---|---|
| `flagId` | `formation.slot2`, `chest.autoOpen` |
| `valueWhenUnlocked` | `true` |

Mở node → `SetFlag(flagId, value)`. Dùng cho mở slot, bật cơ chế... (không phải số).

### c. Grant Ability — cấp skill
`Create > SkillTree > Effects > Grant Ability`

| Field | Ví dụ |
|---|---|
| `abilityId` | `knight.piercing_thrust` |

Mở node → `GrantAbility(abilityId, rank)`; rank để scale cấp skill.

> Một node gắn **nhiều effect** được (vd Aegis Field = Grant Ability + Stat Modifier `shieldBlock`).
> Cần loại hiệu ứng mới? Tạo subclass `SkillEffectSO`, override `Apply/Remove/GetPreview` — **không sửa core**.

---

## 4. Thêm node/skill & nối điều kiện

### Cách A — GraphView (trực quan)
1. `Create > SkillTree > Skill Tree` → ra asset (hoặc nút trong `Tools > SkillTree > Tutorial`).
2. `Tools > SkillTree > Graph Editor`, kéo asset vào ô **Tree**.
3. **Add Node** (hoặc chuột phải > Add Skill Node) — node là sub-asset của tree.
4. Chọn node ở Project → set field ở Inspector → gắn effect vào list `effects`.
5. Nối port **Unlocks → Requires** để tạo prerequisite (kéo từ node cha sang node con).
6. Kéo node gốc vào list `rootNodes` của tree.
7. **Validate** → bắt cycle / Id trùng / node không tới được từ root.

**Prerequisite (`PrerequisiteGroup`):**
- `rule = All` → cần **mọi** node trong group.
- `rule = Any` → cần **ít nhất 1**.
- `rule = NofM` → cần `requiredCount` trong số đó.
- `minRank` → node cha phải đạt rank tối thiểu.
- Nhiều group = **AND** giữa các group (linh hoạt mọi topology).

### Cách B — Sinh bằng code
`Tools > SkillTree > Samples > Create Rune Tree / Knight Tree` dựng sẵn cây hoàn chỉnh (mô phỏng TaskBar Hero). Xem [SkillTreeSampleGenerator.cs](Editor/SkillTreeSampleGenerator.cs) làm mẫu để viết generator riêng.

---

## 5. Apply vào game (3 mức, chọn 1)

### Mức 1 — `SkillTreeRuntime` (dễ nhất, không cần code nối)
Drop component lên hero/account, gán `tree`. Game đọc thẳng:
```csharp
var skills = hero.GetComponent<SkillTreeRuntime>();

float attack  = skills.GetStat("attack", baseValue: 10f); // base + buff từ tree
bool autoOpen = skills.GetFlag("chest.autoOpen");
if (skills.HasAbility("knight.piercing_thrust")) { /* cho dùng skill */ }

skills.TryUnlock(nodeId);                  // unlock + autoSave + bắn OnChanged
skills.OnChanged.AddListener(RecalcStats); // hoặc hook trong Inspector
```
Inspector: `startingPoints`, `loadOnAwake`, `autoSave`. `SnapshotStats()` trả `Dictionary<string,float>` để apply hàng loạt.

### Mức 2 — tự dựng controller + `DictionaryStatContext`
```csharp
var context = new DictionaryStatContext();
var storage = new JsonFileStorage();
var state = storage.Exists(tree.TreeId) ? storage.Load(tree.TreeId) : SkillTreeState.CreateFor(tree, 10);

var controller = new SkillTreeController(tree, state, context);
controller.ReapplyAllEffects();
controller.Events.OnNodeUnlocked += (node, rank) => { /* update UI */ };

if (controller.TryUnlock(nodeId) == UnlockResult.Success) storage.Save(controller.State);

float atk = context.GetStat("attack", 10f);
```
`GetStat`: nếu có `Override` thì lấy giá trị đè, ngược lại `(base + tổng Additive) * tích Multiplicative`.

### Mức 3 — ghép thẳng vào hệ stat riêng (`ISkillEffectContext`)
Pack không biết hệ stat của bạn → implement 5 method để đẩy thẳng:
```csharp
public class HeroSkillContext : ISkillEffectContext
{
    private readonly Hero hero;
    public HeroSkillContext(Hero hero) => this.hero = hero;

    public void AddStatModifier(string statId, float value, ModifierType type, object source)
    {
        switch (statId)
        {
            case "attack": hero.BonusAttack += value; break;
            case "maxHp":  hero.BonusMaxHp  += value; break;
        }
    }
    public void RemoveModifiersFrom(object source) { /* gỡ theo source khi respec/đổi rank */ }
    public void SetFlag(string flagId, bool value)     { hero.Flags[flagId] = value; }
    public void GrantAbility(string abilityId, int r)  { hero.LearnSkill(abilityId, r); }
    public void RevokeAbility(string abilityId)        { hero.ForgetSkill(abilityId); }
}
```
> `source` = chính `SkillNodeSO`. Khi đổi rank/respec, controller gọi `RemoveModifiersFrom(node)` rồi áp lại — hãy lưu modifier kèm `source` để gỡ đúng (xem [DictionaryStatContext.cs](Runtime/Context/DictionaryStatContext.cs)).

---

## 6. Save / Load
- Mặc định `JsonFileStorage` → `Application.persistentDataPath/SkillTrees/<treeId>.json`.
- `state.Version` có sẵn để migrate; override `JsonFileStorage.Migrate` khi đổi cấu trúc.
- Backend khác (PlayerPrefs/cloud) → implement `ISkillTreeStorage`.

## 7. Bảng tra nhanh "muốn thêm X"
| Thêm gì | Làm gì | Sửa core? |
|---|---|---|
| Hành vi node mới | subclass `SkillEffectSO` | Không |
| Kiểu chi phí mới | thêm case `CostCurve.GetCost` | Không |
| Luật prereq mới | mở rộng `PrerequisiteRule` | Không |
| Backend save | implement `ISkillTreeStorage` | Không |
| Ghép hệ stat riêng | implement `ISkillEffectContext` | Không |
| Nhiều cây/instance | thêm asset + state | Không |
