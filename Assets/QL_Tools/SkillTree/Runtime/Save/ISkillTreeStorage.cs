namespace SkillTree
{
    public interface ISkillTreeStorage
    {
        bool Exists(string treeId);
        void Save(SkillTreeState state);
        SkillTreeState Load(string treeId);
        void Delete(string treeId);
    }
}
