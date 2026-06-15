using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTree.Editor
{
    public class SkillNodeView : Node
    {
        public SkillNodeSO Data { get; }
        public Port Input { get; }
        public Port Output { get; }

        public SkillNodeView(SkillNodeSO data)
        {
            Data = data;
            title = string.IsNullOrEmpty(data.DisplayName) ? data.name : data.DisplayName;

            Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            Input.portName = "Requires";
            inputContainer.Add(Input);

            Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            Output.portName = "Unlocks";
            outputContainer.Add(Output);

            var info = new Label($"{data.NodeType} · max {data.MaxRank}");
            info.style.unityFontStyleAndWeight = FontStyle.Italic;
            info.style.marginLeft = 6;
            info.style.marginTop = 4;
            extensionContainer.Add(info);
            RefreshExpandedState();

            SetPosition(new Rect(data.GraphPosition, new Vector2(190f, 120f)));
        }
    }
}
