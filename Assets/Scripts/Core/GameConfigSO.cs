using UnityEngine;
using TopDownShooter.Networking;

namespace TopDownShooter.Core
{
    [CreateAssetMenu(menuName = "TopDownShooter/Config/Game Config")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Wave Settings")]
        [SerializeField] private WaveConfigSO waveConfig;

        [Header("Enemy Settings")]
        [SerializeField] private EnemyConfigSO enemyConfig;

        [Header("Projectile Settings")]
        [SerializeField] private ProjectileConfigSO projectileConfig;

        [Header("Effect Settings")]
        [SerializeField] private EffectConfigSO hitEffectConfig;

        public WaveConfigSO WaveConfig => waveConfig;
        public EnemyConfigSO EnemyConfig => enemyConfig;
        public ProjectileConfigSO ProjectileConfig => projectileConfig;
        public EffectConfigSO HitEffectConfig => hitEffectConfig;
    }
}
