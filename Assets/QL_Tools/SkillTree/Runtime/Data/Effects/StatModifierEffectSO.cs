using UnityEngine;

namespace SkillTree
{
    [CreateAssetMenu(fileName = "StatModifierEffect", menuName = "SkillTree/Effects/Stat Modifier")]
    public class StatModifierEffectSO : SkillEffectSO
    {
        [SerializeField] private string statId;
        [SerializeField] private float valuePerRank = 1f;
        [SerializeField] private ModifierType modifierType = ModifierType.Additive;

#if UNITY_EDITOR
        public void Editor_Configure(string statId, float valuePerRank, ModifierType modifierType)
        {
            this.statId = statId;
            this.valuePerRank = valuePerRank;
            this.modifierType = modifierType;
        }
#endif

        public override void Apply(ISkillEffectContext context, int rank, object source)
        {
            if (context == null || string.IsNullOrEmpty(statId)) return;
            context.AddStatModifier(statId, valuePerRank * rank, modifierType, source);
        }

        public override void Remove(ISkillEffectContext context, object source)
        {
            context?.RemoveModifiersFrom(source);
        }

        public override string GetPreview(int rank)
        {
            float total = valuePerRank * rank;
            string sign = total >= 0f ? "+" : string.Empty;
            return modifierType == ModifierType.Multiplicative
                ? $"{statId} x{total}"
                : $"{statId} {sign}{total}";
        }
    }
}
