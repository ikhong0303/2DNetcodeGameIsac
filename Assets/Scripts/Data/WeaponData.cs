using UnityEngine;

namespace TopDownShooter.Data
{
    /// <summary>
    /// ScriptableObject containing weapon configuration data.
    /// Supports various weapon types with different behaviors.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "TopDownShooter/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Stats")]
        [Tooltip("Display name of the weapon")]
        [SerializeField] private string weaponName = "Default Weapon";

        [Tooltip("Damage dealt per projectile")]
        [SerializeField, Range(1, 100)] private int damage = 10;

        [Tooltip("Time between shots in seconds")]
        [SerializeField, Range(0.01f, 2f)] private float fireRate = 0.2f;

        [Header("Projectile")]
        [Tooltip("Speed of the projectile")]
        [SerializeField, Range(1f, 50f)] private float projectileSpeed = 15f;

        [Tooltip("Lifetime of the projectile in seconds")]
        [SerializeField, Range(0.1f, 10f)] private float projectileLifetime = 3f;

        [Tooltip("Size scale of the projectile")]
        [SerializeField, Range(0.1f, 3f)] private float projectileScale = 1f;

        [Header("Visual")]
        [Tooltip("Color of the projectile")]
        [SerializeField] private Color projectileColor = Color.yellow;

        [Tooltip("Sprite for the projectile")]
        [SerializeField] private Sprite projectileSprite;

        [Header("Audio")]
        [Tooltip("Sound to play when firing")]
        [SerializeField] private AudioClip fireSound;

        [Tooltip("Volume of the fire sound")]
        [SerializeField, Range(0f, 1f)] private float fireSoundVolume = 0.5f;

        // Public accessors (read-only)
        public string WeaponName => weaponName;
        public int Damage => damage;
        public float FireRate => fireRate;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public float ProjectileScale => projectileScale;
        public Color ProjectileColor => projectileColor;
        public Sprite ProjectileSprite => projectileSprite;
        public AudioClip FireSound => fireSound;
        public float FireSoundVolume => fireSoundVolume;
    }
}
