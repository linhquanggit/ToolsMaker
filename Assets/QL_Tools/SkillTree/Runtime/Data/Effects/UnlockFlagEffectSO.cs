using Sirenix.OdinInspector;
using UnityEngine;

namespace SkillTree
{
    [CreateAssetMenu(fileName = "UnlockFlagEffect", menuName = "SkillTree/Effects/Unlock Flag")]
    public class UnlockFlagEffectSO : SkillEffectSO
    {
        [SerializeField, ValueDropdown("@SkillTree.SkillKeyCatalog.FlagKeys()")] private string flagId;
        [SerializeField] private bool valueWhenUnlocked = true;

#if UNITY_EDITOR
        public void Editor_Configure(string flagId, bool valueWhenUnlocked)
        {
            this.flagId = flagId;
            this.valueWhenUnlocked = valueWhenUnlocked;
        }
#endif

        public override void Apply(ISkillEffectContext context, int rank, object source)
        {
            if (context == null || string.IsNullOrEmpty(flagId)) return;
            context.SetFlag(flagId, valueWhenUnlocked);
        }

        public override void Remove(ISkillEffectContext context, object source)
        {
            if (context == null || string.IsNullOrEmpty(flagId)) return;
            context.SetFlag(flagId, !valueWhenUnlocked);
        }

        public override string GetPreview(int rank)
        {
            return $"Set {flagId} = {valueWhenUnlocked}";
        }
    }
}
