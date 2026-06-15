using System;
using UnityEngine;

namespace SkillTree
{
    public enum CostMode
    {
        Fixed,
        Linear,
        PerRank,
        Curve
    }

    [Serializable]
    public class CostCurve
    {
        [SerializeField] private CostMode mode = CostMode.Fixed;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private int perRankIncrement = 1;
        [SerializeField] private int[] perRankCosts = Array.Empty<int>();
        [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(1f, 1f, 10f, 10f);

        public CostMode Mode => mode;
        public int BaseCost => baseCost;

        public int GetCost(int targetRank)
        {
            if (targetRank < 1) return 0;
            switch (mode)
            {
                case CostMode.Fixed:
                    return baseCost;
                case CostMode.Linear:
                    return baseCost + perRankIncrement * (targetRank - 1);
                case CostMode.PerRank:
                    if (perRankCosts == null || perRankCosts.Length == 0) return baseCost;
                    int index = Mathf.Clamp(targetRank - 1, 0, perRankCosts.Length - 1);
                    return perRankCosts[index];
                case CostMode.Curve:
                    return Mathf.RoundToInt(curve.Evaluate(targetRank));
                default:
                    return baseCost;
            }
        }
    }
}
