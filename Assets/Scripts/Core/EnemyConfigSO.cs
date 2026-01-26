using UnityEngine;
using TopDownShooter.Networking;

namespace TopDownShooter.Core
{
    [CreateAssetMenu(menuName = "TopDownShooter/Config/Enemy Config")]
    public class EnemyConfigSO : ScriptableObject
    {
        [SerializeField] private NetworkEnemy enemyPrefab;
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private int scoreValue = 10;

        public NetworkEnemy EnemyPrefab => enemyPrefab;
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int ContactDamage => contactDamage;
        public float AttackCooldown => attackCooldown;
        public int ScoreValue => scoreValue;
    }
}
