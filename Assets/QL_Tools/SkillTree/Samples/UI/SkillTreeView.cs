using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SkillTree.Samples
{
    [RequireComponent(typeof(RectTransform))]
    public class SkillTreeView : MonoBehaviour
    {
        [SerializeField] private float nodeWidth = 176f;
        [SerializeField] private float nodeHeight = 66f;
        [SerializeField] private float layoutSpacing = 1.8f;
        [SerializeField] private Vector2 contentOrigin = new Vector2(80f, -80f);

        private SkillTreeController controller;
        private RectTransform content;
        private Text header;
        private Font font;
        private string lastAction = string.Empty;
        private readonly List<SkillNodeButton> buttons = new List<SkillNodeButton>();

        private const float TooltipWidth = 340f;
        private const float TooltipTextWidth = 316f;

        private RectTransform tooltip;
        private Text tooltipText;

        public void Initialize(SkillTreeController controller)
        {
            this.controller = controller;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            Build();

            controller.Events.OnPointsChanged += _ => RefreshAll();
            controller.Events.OnRankChanged += (_, __) => RefreshAll();
            controller.Events.OnRespec += RefreshAll;
            RefreshAll();
        }

        public void RefreshAll()
        {
            header.text = string.IsNullOrEmpty(lastAction)
                ? $"{controller.Tree.CurrencyId}:  {controller.AvailablePoints}      —  click node = unlock · drag = pan · scroll wheel = zoom"
                : $"{controller.Tree.CurrencyId}:  {controller.AvailablePoints}      —  {lastAction}";
            for (int i = 0; i < buttons.Count; i++) buttons[i].Refresh();
        }

        private void Build()
        {
            var selfRect = (RectTransform)transform;

            var panel = NewChild("Panel", selfRect);
            var panelRect = (RectTransform)panel.transform;
            Stretch(panelRect);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.10f, 0.10f, 0.13f, 1f);
            var panner = panel.AddComponent<SkillTreeDragPanner>();

            content = (RectTransform)NewChild("Content", panelRect).transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = contentOrigin;
            content.sizeDelta = new Vector2(2000f, 1200f);
            panner.Target = content;

            header = CreateText(selfRect, string.Empty, 26, TextAnchor.MiddleLeft);
            var hr = header.rectTransform;
            hr.anchorMin = new Vector2(0f, 1f);
            hr.anchorMax = new Vector2(1f, 1f);
            hr.pivot = new Vector2(0f, 1f);
            hr.anchoredPosition = new Vector2(12f, -6f);
            hr.sizeDelta = new Vector2(-24f, 32f);

            foreach (var node in controller.Tree.Nodes)
            {
                if (node == null) continue;
                foreach (var group in node.Prerequisites)
                {
                    if (group == null) continue;
                    foreach (var prereq in group.Nodes)
                    {
                        if (prereq != null) CreateLine(UiPos(prereq), UiPos(node));
                    }
                }
            }

            foreach (var node in controller.Tree.Nodes)
            {
                if (node != null) CreateNode(node);
            }

            CreateTooltip(selfRect);
        }

        private void CreateTooltip(RectTransform parent)
        {
            var go = NewChild("Tooltip", parent);
            tooltip = (RectTransform)go.transform;
            tooltip.anchorMin = new Vector2(0.5f, 0.5f);
            tooltip.anchorMax = new Vector2(0.5f, 0.5f);
            tooltip.pivot = new Vector2(0f, 1f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.07f, 0.96f);
            bg.raycastTarget = false;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.7f, 0.7f, 0.8f, 0.5f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            tooltipText = CreateText(tooltip, string.Empty, 16, TextAnchor.UpperLeft);
            tooltipText.supportRichText = true;
            tooltipText.horizontalOverflow = HorizontalWrapMode.Wrap;
            tooltipText.verticalOverflow = VerticalWrapMode.Overflow;
            var tr = tooltipText.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(0f, 1f);
            tr.pivot = new Vector2(0f, 1f);
            tr.anchoredPosition = new Vector2(12f, -10f);
            tr.sizeDelta = new Vector2(TooltipTextWidth, 0f);

            tooltip.gameObject.SetActive(false);
        }

        private void OnNodeHover(SkillNodeButton nodeButton, bool entered)
        {
            if (tooltip == null) return;
            if (!entered)
            {
                tooltip.gameObject.SetActive(false);
                return;
            }

            tooltipText.text = BuildInfo(nodeButton.Node);
            float textHeight = tooltipText.preferredHeight;
            tooltipText.rectTransform.sizeDelta = new Vector2(TooltipTextWidth, textHeight);
            tooltip.sizeDelta = new Vector2(TooltipWidth, textHeight + 20f);

            Vector3 worldCenter = nodeButton.Rect.TransformPoint(nodeButton.Rect.rect.center);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, null, out Vector2 local);
            tooltip.anchoredPosition = local + new Vector2(nodeWidth * 0.5f + 12f, nodeHeight * 0.5f);

            tooltip.SetAsLastSibling();
            tooltip.gameObject.SetActive(true);
        }

        private string BuildInfo(SkillNodeSO node)
        {
            int rank = controller.GetRank(node.Id);
            int max = node.MaxRank;
            int previewRank = Mathf.Clamp(rank > 0 ? rank : 1, 1, max);

            var sb = new System.Text.StringBuilder();
            sb.Append("<b><size=20>").Append(node.DisplayName).Append("</size></b>\n");
            sb.Append("<color=#9fb4c8>").Append(node.NodeType).Append("  ·  Rank ").Append(rank).Append('/').Append(max).Append("</color>\n");

            if (!string.IsNullOrEmpty(node.Description))
                sb.Append('\n').Append(node.Description).Append('\n');

            if (node.Effects != null && node.Effects.Count > 0)
            {
                sb.Append("\n<b>Effects</b>\n");
                for (int i = 0; i < node.Effects.Count; i++)
                {
                    var effect = node.Effects[i];
                    if (effect == null) continue;
                    sb.Append("<color=#7fd6a0>• ").Append(effect.GetPreview(previewRank)).Append("</color>\n");
                }
            }

            sb.Append('\n');
            if (rank >= max) sb.Append("<color=#82d36a>MAX RANK</color>");
            else sb.Append("<color=#e6c84a>Next cost: ").Append(controller.GetNextCost(node.Id)).Append("  (").Append(controller.Tree.CurrencyId).Append(")</color>");

            var can = controller.CanUnlock(node.Id);
            if (rank < max && can != UnlockResult.Success)
                sb.Append("\n<color=#d98a8a>").Append(can).Append("</color>");

            return sb.ToString();
        }

        private void CreateNode(SkillNodeSO node)
        {
            var go = NewChild(node.DisplayName, content);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = UiPos(node);
            rt.sizeDelta = new Vector2(nodeWidth, nodeHeight);

            var background = go.AddComponent<Image>();
            background.raycastTarget = true;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
            outline.effectDistance = new Vector2(2f, -2f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = background;

            var title = CreateText(rt, node.DisplayName, 20, TextAnchor.UpperCenter);
            StretchPadded(title.rectTransform, 4f, 6f);

            var sub = CreateText(rt, string.Empty, 17, TextAnchor.LowerCenter);
            StretchPadded(sub.rectTransform, 4f, 6f);
            sub.color = Color.white;

            var nodeButton = go.AddComponent<SkillNodeButton>();
            nodeButton.Bind(node, controller, background, title, sub, button, OnNodeClicked, OnNodeHover);
            buttons.Add(nodeButton);
        }

        private void CreateLine(Vector2 a, Vector2 b)
        {
            var go = NewChild("Line", content);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            Vector2 delta = b - a;
            rt.anchoredPosition = a + delta * 0.5f;
            rt.sizeDelta = new Vector2(delta.magnitude, 3f);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.18f);
            image.raycastTarget = false;
        }

        private void OnNodeClicked(SkillNodeSO node)
        {
            UnlockResult result = controller.TryUnlock(node.Id);
            lastAction = result == UnlockResult.Success
                ? $"Unlocked: {node.DisplayName} (rank {controller.GetRank(node.Id)})"
                : $"{node.DisplayName} → {result}";
            RefreshAll();
        }

        private Text CreateText(RectTransform parent, string content, int fontSize, TextAnchor anchor)
        {
            var go = NewChild("Text", parent);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void StretchPadded(RectTransform rt, float horizontal, float vertical)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(horizontal, vertical);
            rt.offsetMax = new Vector2(-horizontal, -vertical);
        }

        private static GameObject NewChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private Vector2 UiPos(SkillNodeSO node)
        {
            return new Vector2(node.GraphPosition.x, -node.GraphPosition.y) * layoutSpacing;
        }
    }

    public class SkillTreeDragPanner : MonoBehaviour, IDragHandler, IScrollHandler
    {
        public RectTransform Target;
        public float MinZoom = 0.35f;
        public float MaxZoom = 2.5f;
        public float ZoomStep = 0.12f;

        public void OnDrag(PointerEventData eventData)
        {
            if (Target != null) Target.anchoredPosition += eventData.delta;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Target == null || Mathf.Approximately(eventData.scrollDelta.y, 0f)) return;

            float current = Target.localScale.x;
            float factor = eventData.scrollDelta.y > 0f ? 1f + ZoomStep : 1f - ZoomStep;
            float target = Mathf.Clamp(current * factor, MinZoom, MaxZoom);
            if (Mathf.Approximately(target, current)) return;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(Target, eventData.position, eventData.enterEventCamera, out Vector3 before);
            Target.localScale = new Vector3(target, target, 1f);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(Target, eventData.position, eventData.enterEventCamera, out Vector3 after);
            Target.position += before - after;
        }
    }
}
