using System.IO;
using UnityEngine;

namespace SkillTree
{
    public class JsonFileStorage : ISkillTreeStorage
    {
        public const int CurrentVersion = 1;

        private readonly string folder;

        public JsonFileStorage(string subFolder = "SkillTrees")
        {
            folder = Path.Combine(Application.persistentDataPath, subFolder);
        }

        public bool Exists(string treeId)
        {
            return !string.IsNullOrEmpty(treeId) && File.Exists(PathFor(treeId));
        }

        public void Save(SkillTreeState state)
        {
            if (state == null || string.IsNullOrEmpty(state.TreeId)) return;
            Directory.CreateDirectory(folder);
            state.Version = CurrentVersion;
            File.WriteAllText(PathFor(state.TreeId), JsonUtility.ToJson(state, true));
        }

        public SkillTreeState Load(string treeId)
        {
            if (!Exists(treeId)) return null;
            var state = JsonUtility.FromJson<SkillTreeState>(File.ReadAllText(PathFor(treeId)));
            if (state == null) return null;
            Migrate(state);
            return state;
        }

        public void Delete(string treeId)
        {
            if (string.IsNullOrEmpty(treeId)) return;
            var path = PathFor(treeId);
            if (File.Exists(path)) File.Delete(path);
        }

        protected virtual void Migrate(SkillTreeState state)
        {
            if (state.Version < CurrentVersion) state.Version = CurrentVersion;
        }

        private string PathFor(string treeId)
        {
            return Path.Combine(folder, Sanitize(treeId) + ".json");
        }

        private static string Sanitize(string treeId)
        {
            if (string.IsNullOrEmpty(treeId)) return "default";
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                treeId = treeId.Replace(invalid, '_');
            }
            return treeId;
        }
    }
}
