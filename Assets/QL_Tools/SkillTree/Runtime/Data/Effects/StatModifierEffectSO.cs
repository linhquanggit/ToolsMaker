using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SkillTree
{
    public enum StatValueMode
    {
        Linear,
        PerRank,
        Curve
    }

    [CreateAssetMenu(fileName = "StatModifierEffect", menuName = "SkillTree/Effects/Stat Modifier")]
    public class StatModifierEffectSO : SkillEffectSO
    {
        [SerializeField, ValueDropdown("@SkillTree.SkillKeyCatalog.StatKeys()")] private string statId;
        [SerializeField] private ModifierType modifierType = ModifierType.Additive;
        [SerializeField] private StatValueMode valueMode = StatValueMode.Linear;

        [SerializeField, ShowIf(nameof(valueMode), StatValueMode.Linear)]
        private float valuePerRank = 1f;

        [SerializeField, ShowIf(nameof(valueMode), StatValueMode.PerRank), Tooltip("Tổng bonus tại mỗi rank (index 0 = rank 1).")]
        private float[] perRankValues = Array.Empty<float>();

        [SerializeField, ShowIf(nameof(valueMode), StatValueMode.Curve), Tooltip("Evaluate(rank) = tổng bonus tại rank đó.")]
        private AnimationCurve valueCurve = AnimationCurve.Linear(1f, 1f, 5f, 5f);

        public float GetValue(int rank)
        {
            if (rank < 1) return 0f;
            switch (valueMode)
            {
                case StatValueMode.PerRank:
                    if (perRankValues == null || perRankValues.Length == 0) return valuePerRank * rank;
                    return perRankValues[Mathf.Clamp(rank - 1, 0, perRankValues.Length - 1)];
                case StatValueMode.Curve:
                    return valueCurve != null ? valueCurve.Evaluate(rank) : valuePerRank * rank;
                default:
                    return valuePerRank * rank;
            }
        }

#if UNITY_EDITOR
        public void Editor_Configure(string statId, float valuePerRank, ModifierType modifierType)
        {
            this.statId = statId;
            this.valuePerRank = valuePerRank;
            this.modifierType = modifierType;
            this.valueMode = StatValueMode.Linear;
        }

        public void Editor_ConfigurePerRank(string statId, float[] perRankValues, ModifierType modifierType)
        {
            this.statId = statId;
            this.perRankValues = perRankValues ?? Array.Empty<float>();
            this.modifierType = modifierType;
            this.valueMode = StatValueMode.PerRank;
        }
#endif

        public override void Apply(ISkillEffectContext context, int rank, object source)
        {
            if (context == null || string.IsNullOrEmpty(statId)) return;
            context.AddStatModifier(statId, GetValue(rank), modifierType, source);
        }

        public override void Remove(ISkillEffectContext context, object source)
        {
            context?.RemoveModifiersFrom(source);
        }

        public override string GetPreview(int rank)
        {
            float total = GetValue(rank);
            string sign = total >= 0f ? "+" : string.Empty;
            return modifierType == ModifierType.Multiplicative
                ? $"{statId} x{total}"
                : $"{statId} {sign}{total}";
        }
    }
}
