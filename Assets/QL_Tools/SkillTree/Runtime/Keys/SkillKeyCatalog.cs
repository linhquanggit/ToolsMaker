using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace SkillTree
{
    [CreateAssetMenu(fileName = "SkillKeyCatalog", menuName = "SkillTree/Key Catalog")]
    public class SkillKeyCatalog : ScriptableObject
    {
        [SerializeField] private List<string> stats = new List<string>();
        [SerializeField] private List<string> flags = new List<string>();
        [SerializeField] private List<string> abilities = new List<string>();

        public IReadOnlyList<string> Stats => stats;
        public IReadOnlyList<string> Flags => flags;
        public IReadOnlyList<string> Abilities => abilities;

#if UNITY_EDITOR
        private static SkillKeyCatalog cached;

        public static SkillKeyCatalog Find()
        {
            if (cached != null) return cached;
            var guids = AssetDatabase.FindAssets("t:SkillKeyCatalog");
            if (guids.Length == 0) return null;
            cached = AssetDatabase.LoadAssetAtPath<SkillKeyCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return cached;
        }

        public static IEnumerable<string> StatKeys() => Find()?.stats ?? Enumerable.Empty<string>();
        public static IEnumerable<string> FlagKeys() => Find()?.flags ?? Enumerable.Empty<string>();
        public static IEnumerable<string> AbilityKeys() => Find()?.abilities ?? Enumerable.Empty<string>();

        public void Editor_SetKeys(IEnumerable<string> statKeys, IEnumerable<string> flagKeys, IEnumerable<string> abilityKeys)
        {
            stats = Clean(statKeys);
            flags = Clean(flagKeys);
            abilities = Clean(abilityKeys);
        }

        private static List<string> Clean(IEnumerable<string> values)
        {
            return values.Where(v => !string.IsNullOrEmpty(v)).Distinct().OrderBy(v => v).ToList();
        }
#endif
    }
}
