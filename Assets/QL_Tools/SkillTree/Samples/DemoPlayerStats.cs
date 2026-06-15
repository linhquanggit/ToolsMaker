using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillTree.Samples
{
    public class DemoPlayerStats : MonoBehaviour
    {
        [Serializable]
        public struct BaseStat
        {
            public string statId;
            public float baseValue;
        }

        [SerializeField]
        private List<BaseStat> baseStats = new List<BaseStat>
        {
            new BaseStat { statId = "attack", baseValue = 10f },
            new BaseStat { statId = "maxHp", baseValue = 100f },
            new BaseStat { statId = "armor", baseValue = 5f },
            new BaseStat { statId = "attackSpeed", baseValue = 1f },
            new BaseStat { statId = "moveSpeed", baseValue = 5f },
            new BaseStat { statId = "goldPerKill", baseValue = 1f },
            new BaseStat { statId = "xpPerKill", baseValue = 1f },
            new BaseStat { statId = "bossGold", baseValue = 0f },
            new BaseStat { statId = "chestDropRate", baseValue = 0f },
            new BaseStat { statId = "inventory", baseValue = 20f },
            new BaseStat { statId = "damagePercent", baseValue = 0f },
        };

        private DictionaryStatContext context;

        public IReadOnlyList<BaseStat> BaseStats => baseStats;

        public void SetContext(DictionaryStatContext context) => this.context = context;

        public float GetBase(string statId)
        {
            for (int i = 0; i < baseStats.Count; i++)
            {
                if (baseStats[i].statId == statId) return baseStats[i].baseValue;
            }
            return 0f;
        }

        public float GetTotal(string statId)
        {
            float baseValue = GetBase(statId);
            return context != null ? context.GetStat(statId, baseValue) : baseValue;
        }

        public float GetBonus(string statId) => GetTotal(statId) - GetBase(statId);
    }
}
