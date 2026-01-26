using System;
using UnityEngine;

namespace TopDownShooter.Core
{
    [CreateAssetMenu(menuName = "TopDownShooter/Config/Wave Config")]
    public class WaveConfigSO : ScriptableObject
    {
        [SerializeField] private float timeBetweenWaves = 3f;
        [SerializeField] private WaveDefinition[] waves;

        public float TimeBetweenWaves => timeBetweenWaves;
        public WaveDefinition[] Waves => waves;
    }

    [Serializable]
    public struct WaveDefinition
    {
        [Min(1)] public int enemyCount;
        [Min(0.1f)] public float spawnInterval;
    }
}
