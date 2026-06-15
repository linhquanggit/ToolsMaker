using System;

namespace SkillTree
{
    public class SkillTreeEvents
    {
        public event Action<SkillNodeSO, int> OnNodeUnlocked;
        public event Action<SkillNodeSO, int> OnRankChanged;
        public event Action<int> OnPointsChanged;
        public event Action OnRespec;

        public void RaiseNodeUnlocked(SkillNodeSO node, int rank) => OnNodeUnlocked?.Invoke(node, rank);
        public void RaiseRankChanged(SkillNodeSO node, int rank) => OnRankChanged?.Invoke(node, rank);
        public void RaisePointsChanged(int points) => OnPointsChanged?.Invoke(points);
        public void RaiseRespec() => OnRespec?.Invoke();
    }
}
