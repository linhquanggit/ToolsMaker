using Sirenix.OdinInspector;
using UnityEngine;

namespace SkillTree.Samples
{
    public class SkillTreeDemo : MonoBehaviour
    {
        [SerializeField] private SkillTreeSO tree;
        [SerializeField] private int startingPoints = 10;
        [SerializeField] private string sampleStatId = "attack";

        private SkillTreeController controller;
        private DictionaryStatContext context;
        private ISkillTreeStorage storage;

        [ShowInInspector, ReadOnly]
        public int AvailablePoints => controller != null ? controller.AvailablePoints : 0;

        [ShowInInspector, ReadOnly]
        public float SampleStat => context != null ? context.GetStat(sampleStatId) : 0f;

        private void Awake()
        {
            if (tree == null) return;

            storage = new JsonFileStorage();
            context = new DictionaryStatContext();

            var state = storage.Exists(tree.TreeId)
                ? storage.Load(tree.TreeId)
                : SkillTreeState.CreateFor(tree, startingPoints);

            controller = new SkillTreeController(tree, state, context);
            controller.ReapplyAllEffects();
        }

        [Button, EnableIf("@controller != null")]
        private UnlockResult Unlock(string nodeId)
        {
            return controller != null ? controller.TryUnlock(nodeId) : UnlockResult.NodeNotFound;
        }

        [Button, EnableIf("@controller != null")]
        private void Respec() => controller?.Respec();

        [Button, EnableIf("@controller != null")]
        private void AddPoints(int amount) => controller?.AddPoints(amount);

        [Button, EnableIf("@controller != null")]
        private void Save()
        {
            if (controller != null) storage.Save(controller.State);
        }
    }
}
