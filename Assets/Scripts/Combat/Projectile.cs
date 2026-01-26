using UnityEngine;
using TopDownShooter.Core;
using TopDownShooter.Player;
using TopDownShooter.Pooling;

namespace TopDownShooter.Combat
{
    /// <summary>
    /// Projectile behavior component.
    /// Handles movement, collision, and damage dealing.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TrailRenderer trailRenderer;

        private Rigidbody2D _rigidbody;
        private int _damage;
        private int _ownerIndex;
        private float _lifetime;
        private float _timer;
        private ObjectPool<Projectile> _pool;

        public int OwnerIndex => _ownerIndex;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;

            var collider = GetComponent<Collider2D>();
            collider.isTrigger = true;
        }

        /// <summary>
        /// Initializes the projectile with movement and damage data.
        /// </summary>
        public void Initialize(Vector2 direction, float speed, int damage, int ownerIndex, float lifetime, Color color, ObjectPool<Projectile> pool)
        {
            _damage = damage;
            _ownerIndex = ownerIndex;
            _lifetime = lifetime;
            _timer = 0;
            _pool = pool;

            // Set velocity
            _rigidbody.linearVelocity = direction.normalized * speed;

            // Set rotation to face movement direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Set visual
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.startColor = color;
                trailRenderer.endColor = new Color(color.r, color.g, color.b, 0);
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if we hit a player
            var health = other.GetComponent<Health>();
            if (health != null)
            {
                bool damaged = health.TakeDamage(_damage, _ownerIndex);

                if (damaged)
                {
                    GameEvents.TriggerProjectileHit(transform.position, true);
                }

                ReturnToPool();
                return;
            }

            // Check for wall/obstacle
            if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
            {
                GameEvents.TriggerProjectileHit(transform.position, false);
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            _rigidbody.linearVelocity = Vector2.zero;

            if (_pool != null)
            {
                _pool.Return(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            // Clear trail when disabled
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }
    }
}
