using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SkillTree
{
    public class SkillTreeRuntime : MonoBehaviour
    {
        [SerializeField] private SkillTreeSO tree;
        [SerializeField] private int startingPoints = 0;
        [SerializeField] private bool loadOnAwake = true;
        [SerializeField] private bool autoSave = true;

        [SerializeField] private UnityEvent onChanged = new UnityEvent();

        private DictionaryStatContext context;
        private ISkillTreeStorage storage;
        private SkillTreeController controller;

        public SkillTreeController Controller => controller;
        public DictionaryStatContext Output => context;
        public UnityEvent OnChanged => onChanged;
        public bool IsReady => controller != null;
        public int AvailablePoints => controller != null ? controller.AvailablePoints : 0;

        private void Awake()
        {
            if (tree == null)
            {
                enabled = false;
                return;
            }
            Build();
        }

        public void Build()
        {
            context = new DictionaryStatContext();
            storage = new JsonFileStorage();

            var state = loadOnAwake && storage.Exists(tree.TreeId)
                ? storage.Load(tree.TreeId)
                : SkillTreeState.CreateFor(tree, startingPoints);

            controller = new SkillTreeController(tree, state, context);
            controller.ReapplyAllEffects();
            controller.Events.OnRankChanged += (_, __) => Changed();
            controller.Events.OnPointsChanged += _ => Changed();
            controller.Events.OnRespec += Changed;
            Changed();
        }

        public UnlockResult TryUnlock(string nodeId)
        {
            return controller != null ? controller.TryUnlock(nodeId) : UnlockResult.NodeNotFound;
        }

        public bool IsUnlocked(string nodeId) => controller != null && controller.IsUnlocked(nodeId);

        public int GetRank(string nodeId) => controller != null ? controller.GetRank(nodeId) : 0;

        public void AddPoints(int amount) => controller?.AddPoints(amount);

        public void Respec() => controller?.Respec();

        public void Save()
        {
            if (controller != null) storage.Save(controller.State);
        }

        // ---- Output the game reads ----
        public float GetStat(string statId, float baseValue = 0f)
        {
            return context != null ? context.GetStat(statId, baseValue) : baseValue;
        }

        public bool GetFlag(string flagId) => context != null && context.GetFlag(flagId);

        public bool HasAbility(string abilityId) => context != null && context.HasAbility(abilityId);

        public int GetAbilityRank(string abilityId) => context != null ? context.GetAbilityRank(abilityId) : 0;

        public Dictionary<string, float> SnapshotStats(IReadOnlyDictionary<string, float> baseValues = null)
        {
            return context != null ? context.SnapshotStats(baseValues) : new Dictionary<string, float>();
        }

        private void Changed()
        {
            if (autoSave && controller != null) storage.Save(controller.State);
            onChanged.Invoke();
        }
    }
}
