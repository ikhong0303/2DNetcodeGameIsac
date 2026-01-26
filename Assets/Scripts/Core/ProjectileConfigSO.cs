using UnityEngine;
using TopDownShooter.Networking;

namespace TopDownShooter.Core
{
    [CreateAssetMenu(menuName = "TopDownShooter/Config/Projectile Config")]
    public class ProjectileConfigSO : ScriptableObject
    {
        [SerializeField] private NetworkProjectile projectilePrefab;
        [SerializeField] private float speed = 10f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 2f;
        [SerializeField] private int poolSize = 32;

        public NetworkProjectile ProjectilePrefab => projectilePrefab;
        public float Speed => speed;
        public int Damage => damage;
        public float Lifetime => lifetime;
        public int PoolSize => poolSize;
    }
}
