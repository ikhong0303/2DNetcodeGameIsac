using Unity.Netcode;
using UnityEngine;

namespace IsaacLike.Net
{
    public class ServerSpawnManager : NetworkBehaviour
    {
        [SerializeField] private NetworkObject enemyPrefab;
        [SerializeField] private Transform[] enemySpawnPoints;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            {
                return;
            }

            for (int i = 0; i < enemySpawnPoints.Length; i++)
            {
                Transform sp = enemySpawnPoints[i];
                if (sp == null)
                {
                    continue;
                }

                NetworkObject obj = Instantiate(enemyPrefab, sp.position, Quaternion.identity);
                obj.Spawn(true);
            }
        }
    }
}
