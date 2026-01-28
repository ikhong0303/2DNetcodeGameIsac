using Unity.Netcode;
using UnityEngine;
using TopDownShooter.Core;
using TopDownShooter.Pooling;

namespace TopDownShooter.Networking
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class NetworkEnemy : NetworkBehaviour
    {
        [SerializeField] private EnemyConfigSO config;

        private Rigidbody2D body;
        private NetworkHealth health;
        private SpriteRenderer spriteRenderer;
        private float nextAttackTime;
        private ulong lastAttackerId;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<NetworkHealth>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void OnNetworkSpawn()
        {
            // Ensure health reference is valid
            if (health == null)
            {
                health = GetComponent<NetworkHealth>();
            }

            if (IsServer)
            {
                if (health != null && config != null)
                {
                    Debug.Log($"[NetworkEnemy] OnNetworkSpawn: {gameObject.name} - Initializing HP to {config.MaxHealth}, IsDowned was: {health.IsDowned.Value}");
                    health.CurrentHealth.Value = config.MaxHealth;
                    health.IsDowned.Value = false;
                }
                else
                {
                    Debug.LogError($"[NetworkEnemy] OnNetworkSpawn: {gameObject.name} - health or config is null! health={health}, config={config}");
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer || config == null)
            {
                return;
            }

            var target = FindClosestPlayer();
            if (target == null)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = (target.position - transform.position).normalized;
            body.linearVelocity = direction * config.MoveSpeed;
        }

        private Transform FindClosestPlayer()
        {
            float closest = float.MaxValue;
            Transform closestTransform = null;

            foreach (var player in FindObjectsOfType<NetworkPlayerController>())
            {
                if (player == null || player.TryGetComponent<NetworkHealth>(out var playerHealth) && playerHealth.IsDowned.Value)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closest)
                {
                    closest = distance;
                    closestTransform = player.transform;
                }
            }

            return closestTransform;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!IsServer || config == null)
            {
                return;
            }

            if (Time.time < nextAttackTime)
            {
                return;
            }

            if (collision.collider.TryGetComponent<NetworkHealth>(out var targetHealth))
            {
                targetHealth.ApplyDamage(config.ContactDamage);
                nextAttackTime = Time.time + config.AttackCooldown;
            }
        }

        public void ReceiveDamage(int amount, ulong attackerId)
    {
        if (!IsServer)
        {
            return;
        }

        // Fallback: try to get health component if not cached
        if (health == null)
        {
            health = GetComponent<NetworkHealth>();
            if (health == null)
            {
                Debug.LogError($"[NetworkEnemy] {gameObject.name} is missing NetworkHealth component! Cannot take damage.");
                return;
            }
        }

        // Check if already dead (use CurrentHealth instead of IsDowned for enemies)
        if (health.CurrentHealth.Value <= 0)
        {
            Debug.Log($"[NetworkEnemy] {gameObject.name} is already dead (HP={health.CurrentHealth.Value}), ignoring damage.");
            return;
        }

        lastAttackerId = attackerId;
        int previousHealth = health.CurrentHealth.Value;
        
        // Apply damage directly to CurrentHealth
        int newHealth = Mathf.Max(0, previousHealth - amount);
        health.CurrentHealth.Value = newHealth;
        
        Debug.Log($"[NetworkEnemy] {gameObject.name} took {amount} damage. HP: {previousHealth} -> {newHealth}/{config?.MaxHealth ?? 0}");

        // Server spawns hit effect (NetworkObject spawn must be on server)
        SpawnHitEffect(transform.position);

        // Visual feedback on all clients (flash red)
        TriggerHitFeedbackClientRpc();

        if (newHealth <= 0)
        {
            Debug.Log($"[NetworkEnemy] {gameObject.name} HP reached 0, calling HandleDeath.");
            HandleDeath();
        }
    }


    private void HandleDeath()
    {
        if (IsServer)
        {
            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(lastAttackerId, out var attackerObject) &&
                attackerObject.TryGetComponent<NetworkPlayerController>(out var player))
            {
                player.AddScore(config.ScoreValue);
            }

            NetworkGameManager.Instance?.RegisterEnemyDeath(this);
            NetworkObject.Despawn(true);
        }
    }

    // Visual hit feedback: flash red on all clients
    [ClientRpc]
    private void TriggerHitFeedbackClientRpc()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        var originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new UnityEngine.WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void SpawnHitEffect(Vector3 position)
    {
        // NetworkObject spawn can only happen on the server
        if (!IsServer) return;

        var hitConfig = NetworkGameManager.Instance?.HitEffectConfig;
        if (hitConfig == null || hitConfig.EffectPrefab == null)
        {
            Debug.LogWarning("[NetworkEnemy] HitEffectConfig or EffectPrefab is null. Skipping hit effect.");
            return;
        }

        var prefabNetworkObject = hitConfig.EffectPrefab.GetComponent<NetworkObject>();
        if (prefabNetworkObject == null)
        {
            Debug.LogWarning("[NetworkEnemy] EffectPrefab is missing NetworkObject component.");
            return;
        }

        var effectObject = NetworkObjectPool.Instance.Spawn(prefabNetworkObject, position, Quaternion.identity);
        if (effectObject != null && effectObject.TryGetComponent<TopDownShooter.Networking.NetworkEffect>(out var effect))
        {
            effect.Play(hitConfig.Lifetime);
        }
    }
}
}
