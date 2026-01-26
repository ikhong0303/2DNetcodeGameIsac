using UnityEngine;
using TopDownShooter.Core;
using TopDownShooter.Data;
using TopDownShooter.Pooling;

namespace TopDownShooter.Combat
{
    /// <summary>
    /// Weapon component that handles firing projectiles.
    /// Uses object pooling for efficient projectile management.
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        private WeaponData _weaponData;
        private int _ownerIndex;
        private Transform _firePoint;
        private float _fireCooldown;
        private ObjectPool<Projectile> _projectilePool;
        private AudioSource _audioSource;

        private static Projectile _projectilePrefab;

        /// <summary>
        /// Initializes the weapon with data and owner information.
        /// </summary>
        public void Initialize(WeaponData data, int ownerIndex, Transform firePoint)
        {
            _weaponData = data;
            _ownerIndex = ownerIndex;
            _firePoint = firePoint != null ? firePoint : transform;
            _fireCooldown = 0;

            // Setup audio
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;

            // Get or create projectile pool
            SetupProjectilePool();
        }

        private void SetupProjectilePool()
        {
            // Create projectile prefab if it doesn't exist
            if (_projectilePrefab == null)
            {
                _projectilePrefab = CreateProjectilePrefab();
            }

            // Get pool from PoolManager
            if (PoolManager.Instance != null)
            {
                _projectilePool = PoolManager.Instance.GetOrCreatePool(
                    "Projectiles",
                    _projectilePrefab,
                    50,
                    200
                );
            }
            else
            {
                // Fallback: create local pool
                _projectilePool = new ObjectPool<Projectile>(_projectilePrefab, 20, 100);
            }
        }

        private Projectile CreateProjectilePrefab()
        {
            // Create a simple projectile prefab at runtime
            var obj = new GameObject("Projectile_Prefab");
            obj.SetActive(false);

            // Add components
            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.15f;
            collider.isTrigger = true;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.sortingOrder = 5;

            var projectile = obj.AddComponent<Projectile>();

            // Don't destroy on load so it persists
            DontDestroyOnLoad(obj);

            return projectile;
        }

        private Sprite CreateCircleSprite()
        {
            // Create a simple circle texture
            int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        float alpha = Mathf.Clamp01((radius - dist) / 2f);
                        texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );
        }

        private void Update()
        {
            if (_fireCooldown > 0)
            {
                _fireCooldown -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Attempts to fire a projectile in the given direction.
        /// </summary>
        /// <returns>True if a projectile was fired</returns>
        public bool TryFire(Vector2 direction)
        {
            if (_fireCooldown > 0 || _weaponData == null || _projectilePool == null)
            {
                return false;
            }

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.right;
            }

            Fire(direction.normalized);
            return true;
        }

        private void Fire(Vector2 direction)
        {
            // Get projectile from pool
            Projectile projectile = _projectilePool.Get(_firePoint.position);
            if (projectile == null) return;

            // Initialize projectile
            projectile.Initialize(
                direction,
                _weaponData.ProjectileSpeed,
                _weaponData.Damage,
                _ownerIndex,
                _weaponData.ProjectileLifetime,
                _weaponData.ProjectileColor,
                _projectilePool
            );

            // Set scale
            projectile.transform.localScale = Vector3.one * _weaponData.ProjectileScale;

            // Reset cooldown
            _fireCooldown = _weaponData.FireRate;

            // Play sound
            if (_weaponData.FireSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_weaponData.FireSound, _weaponData.FireSoundVolume);
            }

            // Trigger event
            GameEvents.TriggerProjectileFired(_firePoint.position, direction, _ownerIndex);
        }
    }
}
