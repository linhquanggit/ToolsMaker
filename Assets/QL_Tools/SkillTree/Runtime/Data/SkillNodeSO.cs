using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SkillTree
{
    [CreateAssetMenu(fileName = "SkillNode", menuName = "SkillTree/Skill Node")]
    public class SkillNodeSO : ScriptableObject
    {
        [SerializeField, ReadOnly] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField, PreviewField(48)] private Sprite icon;
        [SerializeField] private SkillNodeType nodeType = SkillNodeType.Passive;
        [SerializeField, MinValue(1)] private int maxRank = 1;
        [SerializeField] private CostCurve cost = new CostCurve();
        [SerializeField] private List<PrerequisiteGroup> prerequisites = new List<PrerequisiteGroup>();
        [SerializeField] private List<SkillEffectSO> effects = new List<SkillEffectSO>();
        [SerializeField, HideInInspector] private Vector2 graphPosition;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public SkillNodeType NodeType => nodeType;
        public int MaxRank => maxRank;
        public CostCurve Cost => cost;
        public IReadOnlyList<PrerequisiteGroup> Prerequisites => prerequisites;
        public IReadOnlyList<SkillEffectSO> Effects => effects;
        public Vector2 GraphPosition => graphPosition;

        public bool ArePrerequisitesMet(Func<string, int> rankOf)
        {
            if (prerequisites == null || prerequisites.Count == 0) return true;
            foreach (var group in prerequisites)
            {
                if (group == null) continue;
                if (!group.IsSatisfied(rankOf)) return false;
            }
            return true;
        }

#if UNITY_EDITOR
        public void Editor_EnsureId()
        {
            if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString("N");
        }

        public void Editor_SetGraphPosition(Vector2 value) => graphPosition = value;

        public void Editor_Configure(string displayName, string description, SkillNodeType nodeType, int maxRank, CostCurve cost)
        {
            this.displayName = displayName;
            this.description = description;
            this.nodeType = nodeType;
            this.maxRank = Mathf.Max(1, maxRank);
            if (cost != null) this.cost = cost;
        }

        public void Editor_AddEffect(SkillEffectSO effect)
        {
            if (effect != null && !effects.Contains(effect)) effects.Add(effect);
        }

        public void Editor_AddPrerequisite(SkillNodeSO other)
        {
            if (other == null || other == this) return;
            foreach (var group in prerequisites)
            {
                if (group != null && group.Editor_Contains(other)) return;
            }
            var target = prerequisites.Count > 0 ? prerequisites[0] : null;
            if (target == null)
            {
                target = new PrerequisiteGroup();
                prerequisites.Add(target);
            }
            target.Editor_AddNode(other);
        }

        public void Editor_RemovePrerequisite(SkillNodeSO other)
        {
            foreach (var group in prerequisites) group?.Editor_RemoveNode(other);
        }

        private void OnValidate()
        {
            Editor_EnsureId();
            if (maxRank < 1) maxRank = 1;
        }
#endif
    }
}
