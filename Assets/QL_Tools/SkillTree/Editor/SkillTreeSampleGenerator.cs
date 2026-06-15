using System.IO;
using UnityEditor;
using UnityEngine;

namespace SkillTree.Editor
{
    public static class SkillTreeSampleGenerator
    {
        private const string Dir = "Assets/QL_Tools/SkillTree/Samples/Generated";

        [MenuItem("Tools/SkillTree/Samples/Create Rune Tree (Global)")]
        public static void CreateRuneTree()
        {
            var tree = CreateTree("RuneTree", "runetree.global", "gold");

            var start = AddNode(tree, "Awakening", "Center of the rune tree.", SkillNodeType.Passive, 1, new CostCurve(CostMode.Fixed, 0), new Vector2(400, 300), true);

            // South — Formation
            var cmd2 = AddNode(tree, "Rune of Command II", "Unlock the 2nd hero slot.", SkillNodeType.Passive, 1, new CostCurve(CostMode.Fixed, 50000), new Vector2(400, 470));
            AddFlag(tree, cmd2, "formation.slot2");
            Link(start, cmd2);

            var cmd3 = AddNode(tree, "Rune of Command III", "Unlock the 3rd hero slot.", SkillNodeType.Passive, 1, new CostCurve(CostMode.Fixed, 150000), new Vector2(400, 640));
            AddFlag(tree, cmd3, "formation.slot3");
            Link(cmd2, cmd3);

            var slot2 = AddNode(tree, "Second Active Skill Slot", "Unlock a 2nd active skill slot.", SkillNodeType.Passive, 1, new CostCurve(CostMode.Fixed, 50000), new Vector2(580, 470));
            AddFlag(tree, slot2, "skill.slot2");
            Link(start, slot2);

            // Southeast — Combat
            var war = AddNode(tree, "Rune of War", "+ATK per rank.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 100, 150), new Vector2(620, 360));
            AddStat(tree, war, "attack", 5);
            Link(start, war);

            var resilience = AddNode(tree, "Rune of Resilience", "+Armor per rank.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 100, 120), new Vector2(790, 360));
            AddStat(tree, resilience, "armor", 3);
            Link(war, resilience);

            var gale = AddNode(tree, "Rune of Gale", "+Move Speed per rank.", SkillNodeType.Passive, 3, new CostCurve(CostMode.Linear, 80, 80), new Vector2(720, 220));
            AddStat(tree, gale, "moveSpeed", 2);
            Link(war, gale);

            var frenzy = AddNode(tree, "Rune of Frenzy", "+Attack Speed per rank.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 200, 200), new Vector2(960, 360));
            AddStat(tree, frenzy, "attackSpeed", 1);
            Link(resilience, frenzy);

            var ascension = AddNode(tree, "Rune of Ascension", "End-zone: % all damage.", SkillNodeType.Passive, 3, CostCurve.PerRank(1000000, 3000000, 50000000), new Vector2(1130, 360));
            AddStat(tree, ascension, "damagePercent", 5);
            Link(frenzy, ascension);

            // Northwest — Gold
            var wealth = AddNode(tree, "Rune of Wealth", "+Gold per kill.", SkillNodeType.Passive, 10, new CostCurve(CostMode.Linear, 50, 50), new Vector2(240, 240));
            AddStat(tree, wealth, "goldPerKill", 1);
            Link(start, wealth);

            var plunder = AddNode(tree, "Rune of Plunder", "+Boss gold.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 500, 500), new Vector2(80, 240));
            AddStat(tree, plunder, "bossGold", 10);
            Link(wealth, plunder);

            // Southwest — Experience
            var growth = AddNode(tree, "Rune of Growth", "+XP per kill.", SkillNodeType.Passive, 10, new CostCurve(CostMode.Linear, 50, 50), new Vector2(240, 380));
            AddStat(tree, growth, "xpPerKill", 1);
            Link(start, growth);

            // North — Storage
            var expansion = AddNode(tree, "Rune of Expansion", "+Inventory slots.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 100, 100), new Vector2(400, 140));
            AddStat(tree, expansion, "inventory", 4);
            Link(start, expansion);

            var mainspring = AddNode(tree, "Rune of the Mainspring", "Enable auto-open chests.", SkillNodeType.Passive, 1, new CostCurve(CostMode.Fixed, 200000), new Vector2(400, 20));
            AddFlag(tree, mainspring, "chest.autoOpen");
            Link(expansion, mainspring);

            // Northeast — Chest
            var fortune = AddNode(tree, "Rune of Fortune", "+Chest drop rate.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 120, 120), new Vector2(620, 240));
            AddStat(tree, fortune, "chestDropRate", 2);
            Link(start, fortune);

            FinishTree(tree);
        }

        [MenuItem("Tools/SkillTree/Samples/Create Knight Tree (Hero)")]
        public static void CreateKnightTree()
        {
            var tree = CreateTree("KnightTree", "herotree.knight", "skillpoint:knight");

            var root = AddNode(tree, "Basic Training", "Knight fundamentals.", SkillNodeType.Passive, 1, new CostCurve(CostMode.Fixed, 0), new Vector2(400, 80), true);

            var maxhp = AddNode(tree, "Max HP", "+Max HP per rank.", SkillNodeType.Passive, 10, new CostCurve(CostMode.Linear, 1, 1), new Vector2(400, 220));
            AddStat(tree, maxhp, "maxHp", 50);
            Link(root, maxhp);

            var piercing = AddNode(tree, "Piercing Thrust", "Thrust forward, physical damage in range.", SkillNodeType.Active, 5, new CostCurve(CostMode.Linear, 1, 1), new Vector2(230, 220));
            AddAbility(tree, piercing, "knight.piercing_thrust");
            Link(root, piercing);

            var shield = AddNode(tree, "Shield Charge", "Charge and collide for physical damage.", SkillNodeType.Active, 5, new CostCurve(CostMode.Linear, 1, 1), new Vector2(570, 220));
            AddAbility(tree, shield, "knight.shield_charge");
            Link(root, shield);

            var armor = AddNode(tree, "Armor Enhancement", "+Armor per rank.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 1, 1), new Vector2(230, 360));
            AddStat(tree, armor, "armor", 4);
            Link(maxhp, armor);

            var regen = AddNode(tree, "HP Regen per sec", "+HP regen per rank.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 1, 1), new Vector2(400, 360));
            AddStat(tree, regen, "hpRegen", 2);
            Link(maxhp, regen);

            var retribution = AddNode(tree, "Retribution Strike", "Multi-hit; more strikes at low HP.", SkillNodeType.Active, 3, new CostCurve(CostMode.Linear, 2, 1), new Vector2(100, 360));
            AddAbility(tree, retribution, "knight.retribution");
            Link(piercing, retribution);

            var aegis = AddNode(tree, "Aegis Field", "Shield blocks damage for all allies in area.", SkillNodeType.Active, 3, new CostCurve(CostMode.Linear, 2, 2), new Vector2(570, 360));
            AddAbility(tree, aegis, "knight.aegis");
            AddStat(tree, aegis, "shieldBlock", 110);
            Link(shield, aegis);

            var hpkill = AddNode(tree, "Add HP per Kill", "+HP recovered on kill.", SkillNodeType.Passive, 5, new CostCurve(CostMode.Linear, 1, 1), new Vector2(400, 500));
            AddStat(tree, hpkill, "hpPerKill", 1);
            Link(regen, hpkill);

            var sacred = AddNode(tree, "Sacred Blade", "Lv30: buff ATK and recover 2 HP per kill.", SkillNodeType.Active, 1, new CostCurve(CostMode.Fixed, 3), new Vector2(400, 640));
            AddAbility(tree, sacred, "knight.sacred_blade");
            AddStat(tree, sacred, "attack", 20);
            AddStat(tree, sacred, "hpPerKill", 2);
            Link(hpkill, sacred);
            Link(aegis, sacred);

            FinishTree(tree);
        }

        private static SkillTreeSO CreateTree(string fileName, string treeId, string currencyId)
        {
            EnsureFolder(Dir);
            var tree = ScriptableObject.CreateInstance<SkillTreeSO>();
            tree.name = fileName;
            tree.Editor_SetMeta(treeId, currencyId);
            AssetDatabase.CreateAsset(tree, AssetDatabase.GenerateUniqueAssetPath($"{Dir}/{fileName}.asset"));
            return tree;
        }

        private static SkillNodeSO AddNode(SkillTreeSO tree, string display, string description, SkillNodeType type, int maxRank, CostCurve cost, Vector2 position, bool root = false)
        {
            var node = ScriptableObject.CreateInstance<SkillNodeSO>();
            node.name = display;
            node.Editor_EnsureId();
            node.Editor_Configure(display, description, type, maxRank, cost);
            node.Editor_SetGraphPosition(position);
            AssetDatabase.AddObjectToAsset(node, tree);
            tree.Editor_AddNode(node);
            if (root) tree.Editor_SetRoot(node, true);
            EditorUtility.SetDirty(node);
            return node;
        }

        private static void AddStat(SkillTreeSO tree, SkillNodeSO node, string statId, float perRank, ModifierType type = ModifierType.Additive)
        {
            var fx = ScriptableObject.CreateInstance<StatModifierEffectSO>();
            fx.name = $"{node.name}_{statId}";
            fx.Editor_Configure(statId, perRank, type);
            AssetDatabase.AddObjectToAsset(fx, tree);
            node.Editor_AddEffect(fx);
            EditorUtility.SetDirty(fx);
            EditorUtility.SetDirty(node);
        }

        private static void AddFlag(SkillTreeSO tree, SkillNodeSO node, string flagId)
        {
            var fx = ScriptableObject.CreateInstance<UnlockFlagEffectSO>();
            fx.name = $"{node.name}_flag";
            fx.Editor_Configure(flagId, true);
            AssetDatabase.AddObjectToAsset(fx, tree);
            node.Editor_AddEffect(fx);
            EditorUtility.SetDirty(fx);
            EditorUtility.SetDirty(node);
        }

        private static void AddAbility(SkillTreeSO tree, SkillNodeSO node, string abilityId)
        {
            var fx = ScriptableObject.CreateInstance<GrantAbilityEffectSO>();
            fx.name = $"{node.name}_ability";
            fx.Editor_Configure(abilityId);
            AssetDatabase.AddObjectToAsset(fx, tree);
            node.Editor_AddEffect(fx);
            EditorUtility.SetDirty(fx);
            EditorUtility.SetDirty(node);
        }

        private static void Link(SkillNodeSO prerequisite, SkillNodeSO node)
        {
            node.Editor_AddPrerequisite(prerequisite);
            EditorUtility.SetDirty(node);
        }

        private static void FinishTree(SkillTreeSO tree)
        {
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
            SkillTreeGraphWindow.OpenFor(tree);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
