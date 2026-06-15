using System;
using UnityEngine;
using UnityEngine.UI;

namespace SkillTree.Samples
{
    public class SkillNodeButton : MonoBehaviour
    {
        private static readonly Color ColorMaxed = new Color(0.20f, 0.58f, 0.27f);
        private static readonly Color ColorRanked = new Color(0.18f, 0.45f, 0.55f);
        private static readonly Color ColorAvailable = new Color(0.72f, 0.62f, 0.20f);
        private static readonly Color ColorNoPoints = new Color(0.52f, 0.34f, 0.16f);
        private static readonly Color ColorLocked = new Color(0.24f, 0.24f, 0.28f);

        private SkillNodeSO node;
        private SkillTreeController controller;
        private Image background;
        private Text title;
        private Text sub;
        private Action<SkillNodeSO> onClick;

        public SkillNodeSO Node => node;

        public void Bind(SkillNodeSO node, SkillTreeController controller, Image background, Text title, Text sub, Button button, Action<SkillNodeSO> onClick)
        {
            this.node = node;
            this.controller = controller;
            this.background = background;
            this.title = title;
            this.sub = sub;
            this.onClick = onClick;
            button.onClick.AddListener(() => this.onClick?.Invoke(this.node));
        }

        public void Refresh()
        {
            int rank = controller.GetRank(node.Id);
            int max = node.MaxRank;
            UnlockResult can = controller.CanUnlock(node.Id);

            if (rank >= max) background.color = ColorMaxed;
            else if (rank > 0) background.color = ColorRanked;
            else if (can == UnlockResult.Success) background.color = ColorAvailable;
            else if (can == UnlockResult.NotEnoughPoints) background.color = ColorNoPoints;
            else background.color = ColorLocked;

            title.text = node.DisplayName;

            if (rank >= max) sub.text = $"MAX {rank}/{max}";
            else if (rank > 0) sub.text = $"{rank}/{max}  ▸ {controller.GetNextCost(node.Id)}";
            else sub.text = $"Cost {controller.GetNextCost(node.Id)}";
        }
    }
}
