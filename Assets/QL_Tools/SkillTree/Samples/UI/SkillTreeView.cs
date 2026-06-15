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
        [SerializeField] private Vector2 contentOrigin = new Vector2(60f, -60f);

        private SkillTreeController controller;
        private RectTransform content;
        private Text header;
        private Font font;
        private string lastAction = string.Empty;
        private readonly List<SkillNodeButton> buttons = new List<SkillNodeButton>();

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
            nodeButton.Bind(node, controller, background, title, sub, button, OnNodeClicked);
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

        private static Vector2 UiPos(SkillNodeSO node)
        {
            return new Vector2(node.GraphPosition.x, -node.GraphPosition.y);
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
