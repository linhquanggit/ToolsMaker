using UnityEngine;

namespace SkillTree
{
    [CreateAssetMenu(fileName = "GrantAbilityEffect", menuName = "SkillTree/Effects/Grant Ability")]
    public class GrantAbilityEffectSO : SkillEffectSO
    {
        [SerializeField] private string abilityId;

        public override void Apply(ISkillEffectContext context, int rank, object source)
        {
            if (context == null || string.IsNullOrEmpty(abilityId)) return;
            context.GrantAbility(abilityId, rank);
        }

        public override void Remove(ISkillEffectContext context, object source)
        {
            if (context == null || string.IsNullOrEmpty(abilityId)) return;
            context.RevokeAbility(abilityId);
        }

        public override string GetPreview(int rank)
        {
            return $"Grant {abilityId} (rank {rank})";
        }
    }
}
