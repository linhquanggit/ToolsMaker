using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree
{
    public enum PrerequisiteRule
    {
        All,
        Any,
        NofM
    }

    [Serializable]
    public class PrerequisiteGroup
    {
        [SerializeField] private PrerequisiteRule rule = PrerequisiteRule.All;
        [SerializeField] private int requiredCount = 1;
        [SerializeField] private int minRank = 1;
        [SerializeField] private List<SkillNodeSO> nodes = new List<SkillNodeSO>();

        public PrerequisiteRule Rule => rule;
        public int RequiredCount => requiredCount;
        public int MinRank => minRank;
        public IReadOnlyList<SkillNodeSO> Nodes => nodes;

        public bool IsSatisfied(Func<string, int> rankOf)
        {
            if (nodes == null || nodes.Count == 0) return true;

            int validCount = 0;
            int satisfiedCount = 0;
            foreach (var node in nodes)
            {
                if (node == null) continue;
                validCount++;
                if (rankOf(node.Id) >= minRank) satisfiedCount++;
            }

            if (validCount == 0) return true;

            switch (rule)
            {
                case PrerequisiteRule.All:
                    return satisfiedCount >= validCount;
                case PrerequisiteRule.Any:
                    return satisfiedCount >= 1;
                case PrerequisiteRule.NofM:
                    return satisfiedCount >= requiredCount;
                default:
                    return false;
            }
        }

#if UNITY_EDITOR
        public bool Editor_Contains(SkillNodeSO node) => nodes != null && nodes.Contains(node);
        public int Editor_Count => nodes != null ? nodes.Count : 0;

        public void Editor_AddNode(SkillNodeSO node)
        {
            if (node != null && !nodes.Contains(node)) nodes.Add(node);
        }

        public void Editor_RemoveNode(SkillNodeSO node) => nodes.Remove(node);
#endif
    }
}
