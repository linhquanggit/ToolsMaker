using System.Collections.Generic;

namespace SkillTree
{
    public class DictionaryStatContext : ISkillEffectContext
    {
        private class Modifier
        {
            public string StatId;
            public float Value;
            public ModifierType Type;
            public object Source;
        }

        private readonly List<Modifier> modifiers = new List<Modifier>();
        private readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();
        private readonly Dictionary<string, int> abilities = new Dictionary<string, int>();

        public void AddStatModifier(string statId, float value, ModifierType type, object source)
        {
            if (string.IsNullOrEmpty(statId)) return;
            modifiers.Add(new Modifier { StatId = statId, Value = value, Type = type, Source = source });
        }

        public void RemoveModifiersFrom(object source)
        {
            modifiers.RemoveAll(m => Equals(m.Source, source));
        }

        public void SetFlag(string flagId, bool value)
        {
            if (!string.IsNullOrEmpty(flagId)) flags[flagId] = value;
        }

        public void GrantAbility(string abilityId, int rank)
        {
            if (!string.IsNullOrEmpty(abilityId)) abilities[abilityId] = rank;
        }

        public void RevokeAbility(string abilityId)
        {
            if (!string.IsNullOrEmpty(abilityId)) abilities.Remove(abilityId);
        }

        public bool GetFlag(string flagId) => flags.TryGetValue(flagId, out var value) && value;

        public bool HasAbility(string abilityId) => abilities.ContainsKey(abilityId);

        public int GetAbilityRank(string abilityId) => abilities.TryGetValue(abilityId, out var rank) ? rank : 0;

        public float GetStat(string statId, float baseValue = 0f)
        {
            float additive = 0f;
            float multiplier = 1f;
            bool hasOverride = false;
            float overrideValue = 0f;

            foreach (var modifier in modifiers)
            {
                if (modifier.StatId != statId) continue;
                switch (modifier.Type)
                {
                    case ModifierType.Additive:
                        additive += modifier.Value;
                        break;
                    case ModifierType.Multiplicative:
                        multiplier *= modifier.Value;
                        break;
                    case ModifierType.Override:
                        hasOverride = true;
                        overrideValue = modifier.Value;
                        break;
                }
            }

            return hasOverride ? overrideValue : (baseValue + additive) * multiplier;
        }
    }
}
