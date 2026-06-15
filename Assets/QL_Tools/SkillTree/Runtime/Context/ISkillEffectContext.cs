namespace SkillTree
{
    public enum ModifierType
    {
        Additive,
        Multiplicative,
        Override
    }

    public interface ISkillEffectContext
    {
        void AddStatModifier(string statId, float value, ModifierType type, object source);
        void RemoveModifiersFrom(object source);
        void SetFlag(string flagId, bool value);
        void GrantAbility(string abilityId, int rank);
        void RevokeAbility(string abilityId);
    }
}
