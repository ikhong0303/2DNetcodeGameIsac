using UnityEngine;
using TopDownShooter.Networking;

namespace TopDownShooter.Core
{
    [CreateAssetMenu(menuName = "TopDownShooter/Config/Effect Config")]
    public class EffectConfigSO : ScriptableObject
    {
        [SerializeField] private NetworkEffect effectPrefab;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private int poolSize = 16;

        public NetworkEffect EffectPrefab => effectPrefab;
        public float Lifetime => lifetime;
        public int PoolSize => poolSize;
    }
}
