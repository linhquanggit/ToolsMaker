using System.Collections.Generic;

namespace SkillTree
{
    public class SkillTreeController
    {
        private readonly SkillTreeSO tree;
        private readonly SkillTreeState state;
        private readonly ISkillEffectContext context;
        private readonly SkillTreeEvents events = new SkillTreeEvents();

        public SkillTreeSO Tree => tree;
        public SkillTreeState State => state;
        public SkillTreeEvents Events => events;
        public int AvailablePoints => state.AvailablePoints;

        public SkillTreeController(SkillTreeSO tree, SkillTreeState state, ISkillEffectContext context)
        {
            this.tree = tree;
            this.state = state;
            this.context = context;
        }

        public bool IsUnlocked(string nodeId) => state.GetRank(nodeId) > 0;

        public int GetRank(string nodeId) => state.GetRank(nodeId);

        public int GetNextCost(string nodeId)
        {
            var node = tree != null ? tree.FindById(nodeId) : null;
            if (node == null) return 0;
            int current = state.GetRank(nodeId);
            return current >= node.MaxRank ? 0 : node.Cost.GetCost(current + 1);
        }

        public UnlockResult CanUnlock(string nodeId)
        {
            var node = tree != null ? tree.FindById(nodeId) : null;
            if (node == null) return UnlockResult.NodeNotFound;

            int current = state.GetRank(nodeId);
            if (current >= node.MaxRank) return UnlockResult.MaxRankReached;
            if (!node.ArePrerequisitesMet(state.GetRank)) return UnlockResult.PrerequisiteNotMet;
            if (state.AvailablePoints < node.Cost.GetCost(current + 1)) return UnlockResult.NotEnoughPoints;
            return UnlockResult.Success;
        }

        public UnlockResult TryUnlock(string nodeId)
        {
            var result = CanUnlock(nodeId);
            if (result != UnlockResult.Success) return result;

            var node = tree.FindById(nodeId);
            int current = state.GetRank(nodeId);
            int newRank = current + 1;

            state.TrySpendPoints(node.Cost.GetCost(newRank));
            state.SetRank(nodeId, newRank);
            ApplyNode(node, newRank);

            events.RaisePointsChanged(state.AvailablePoints);
            events.RaiseRankChanged(node, newRank);
            if (current == 0) events.RaiseNodeUnlocked(node, newRank);
            return UnlockResult.Success;
        }

        public void Respec()
        {
            if (tree == null) return;

            int refund = 0;
            foreach (var node in tree.Nodes)
            {
                if (node == null) continue;
                int rank = state.GetRank(node.Id);
                if (rank <= 0) continue;
                for (int k = 1; k <= rank; k++) refund += node.Cost.GetCost(k);
                RemoveNode(node);
            }

            state.ClearRanks();
            state.RefundAllPoints(refund);
            events.RaisePointsChanged(state.AvailablePoints);
            events.RaiseRespec();
        }

        public void AddPoints(int amount)
        {
            state.AddPoints(amount);
            events.RaisePointsChanged(state.AvailablePoints);
        }

        public void ReapplyAllEffects()
        {
            if (tree == null || context == null) return;
            foreach (var node in tree.Nodes)
            {
                if (node == null) continue;
                int rank = state.GetRank(node.Id);
                if (rank > 0) ApplyNode(node, rank);
            }
        }

        private void ApplyNode(SkillNodeSO node, int rank)
        {
            if (context == null) return;
            RemoveNode(node);
            IReadOnlyList<SkillEffectSO> effects = node.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i]?.Apply(context, rank, node);
            }
        }

        private void RemoveNode(SkillNodeSO node)
        {
            if (context == null) return;
            IReadOnlyList<SkillEffectSO> effects = node.Effects;
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i]?.Remove(context, node);
            }
        }
    }
}
