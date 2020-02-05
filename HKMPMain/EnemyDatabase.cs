using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModCommon;

namespace HKMPMain
{
    public static class EnemyDatabase
    {
        public static Dictionary<string, GameObject> enemies = new Dictionary<string, GameObject>();

        public static void AddEnemy(GameObject enemy)
        {
            string name = TrimFromName(enemy);

            if(enemies.ContainsKey(name))
            {
                return;
            }

            GameObject clone = GameObject.Instantiate(enemy, null);

            clone.SetActive(false);
            GameObject.DontDestroyOnLoad(clone);

            clone.name = name;
            enemies.Add(name, clone);
            MPLogger.Log($"ADDED {name} TO DATABASE");
        }

        public static string TrimFromName(GameObject obj)
        {
            MPLogger.Log($"Checking name of : {obj.name}");

            string name = obj.name;
            name.TrimGameObjectName();
            name.Trim();

            while (name.EndsWith("(Clone)"))
            {
                name.TrimGameObjectName();
                name.Trim();
            }

            return name;
        }

        public static GameObject SpawnEnemy(string name, Vector3 position)
        {
            if(!enemies.ContainsKey(name))
            {
                return null;
            }

            GameObject enemy = GameObject.Instantiate(enemies[name], position, Quaternion.identity);

            enemy.SetActive(true);

            return enemy;
        }
    }
}
