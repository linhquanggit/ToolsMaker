using UnityEngine;

namespace SkillTree
{
    public abstract class SkillEffectSO : ScriptableObject
    {
        public abstract void Apply(ISkillEffectContext context, int rank, object source);
        public abstract void Remove(ISkillEffectContext context, object source);
        public abstract string GetPreview(int rank);
    }
}
