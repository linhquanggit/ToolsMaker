using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SkillTree.Editor
{
    public static class SkillKeyCatalogBuilder
    {
        private const string DefaultPath = "Assets/QL_Tools/SkillTree/SkillKeyCatalog.asset";

        [MenuItem("Tools/SkillTree/Keys/Create or Refresh Catalog")]
        public static void CreateOrRefresh()
        {
            var stats = new List<string>();
            var flags = new List<string>();
            var abilities = new List<string>();

            CollectStandalone<StatModifierEffectSO>(stats, flags, abilities);
            CollectStandalone<UnlockFlagEffectSO>(stats, flags, abilities);
            CollectStandalone<GrantAbilityEffectSO>(stats, flags, abilities);

            foreach (var guid in AssetDatabase.FindAssets("t:SkillTreeSO"))
            {
                var tree = AssetDatabase.LoadAssetAtPath<SkillTreeSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (tree == null) continue;
                foreach (var node in tree.Nodes)
                {
                    if (node == null) continue;
                    foreach (var effect in node.Effects) AddFromEffect(effect, stats, flags, abilities);
                }
            }

            var catalog = SkillKeyCatalog.Find();
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<SkillKeyCatalog>();
                AssetDatabase.CreateAsset(catalog, DefaultPath);
            }

            catalog.Editor_SetKeys(stats, flags, abilities);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
            EditorUtility.DisplayDialog("SkillKeyCatalog",
                $"Catalog cập nhật:\n• {Distinct(stats)} stat\n• {Distinct(flags)} flag\n• {Distinct(abilities)} ability\n\nThêm key mới: sửa trực tiếp trong asset này.", "OK");
        }

        private static void CollectStandalone<T>(List<string> stats, List<string> flags, List<string> abilities) where T : SkillEffectSO
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                AddFromEffect(asset, stats, flags, abilities);
            }
        }

        private static void AddFromEffect(SkillEffectSO effect, List<string> stats, List<string> flags, List<string> abilities)
        {
            if (effect == null) return;
            var so = new SerializedObject(effect);
            switch (effect)
            {
                case StatModifierEffectSO _: AddValue(so, "statId", stats); break;
                case UnlockFlagEffectSO _: AddValue(so, "flagId", flags); break;
                case GrantAbilityEffectSO _: AddValue(so, "abilityId", abilities); break;
            }
        }

        private static void AddValue(SerializedObject so, string propertyName, List<string> output)
        {
            var value = so.FindProperty(propertyName)?.stringValue;
            if (!string.IsNullOrEmpty(value)) output.Add(value);
        }

        private static int Distinct(List<string> values)
        {
            return new HashSet<string>(values).Count;
        }
    }
}
