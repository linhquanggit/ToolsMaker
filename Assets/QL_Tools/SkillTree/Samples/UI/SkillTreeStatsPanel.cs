using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkillTree.Samples
{
    public class SkillTreeStatsPanel : MonoBehaviour
    {
        private DemoPlayerStats player;
        private Font font;
        private readonly List<Text> rows = new List<Text>();
        private readonly List<string> statIds = new List<string>();

        public void Initialize(DemoPlayerStats player)
        {
            this.player = player;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            Build();
            Refresh();
        }

        public void Refresh()
        {
            if (player == null) return;
            for (int i = 0; i < statIds.Count; i++)
            {
                string id = statIds[i];
                float baseValue = player.GetBase(id);
                float total = player.GetTotal(id);
                float bonus = total - baseValue;

                string bonusText = bonus > 0.0001f
                    ? $"  <color=#7fd6a0>(+{Trim(bonus)})</color>  = <b>{Trim(total)}</b>"
                    : $"  = {Trim(total)}";
                rows[i].text = $"<color=#c8d2dc>{id}</color>:  {Trim(baseValue)}{bonusText}";
            }
        }

        private void Build()
        {
            var selfRect = (RectTransform)transform;

            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);

            var title = CreateText("PLAYER STATS", 22, FontStyle.Bold);
            var tr = title.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0f, 1f);
            tr.anchoredPosition = new Vector2(14f, -12f);
            tr.sizeDelta = new Vector2(-28f, 30f);

            var stats = player != null ? player.BaseStats : null;
            float y = -54f;
            if (stats != null)
            {
                for (int i = 0; i < stats.Count; i++)
                {
                    statIds.Add(stats[i].statId);
                    var row = CreateText(string.Empty, 17, FontStyle.Normal);
                    var rr = row.rectTransform;
                    rr.anchorMin = new Vector2(0f, 1f);
                    rr.anchorMax = new Vector2(1f, 1f);
                    rr.pivot = new Vector2(0f, 1f);
                    rr.anchoredPosition = new Vector2(14f, y);
                    rr.sizeDelta = new Vector2(-28f, 26f);
                    rows.Add(row);
                    y -= 28f;
                }
            }
        }

        private Text CreateText(string content, int fontSize, FontStyle style)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.supportRichText = true;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static string Trim(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value)) ? Mathf.RoundToInt(value).ToString() : value.ToString("0.##");
        }
    }
}
