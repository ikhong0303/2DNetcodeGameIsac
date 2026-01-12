using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace IsaacLike.Net
{
    public class NetworkHealth : NetworkBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHp = 6;

        [Header("UI (optional)")]
        [SerializeField] private TMP_Text hpText;

        public NetworkVariable<int> CurrentHp { get; private set; }

        private void Awake()
        {
            CurrentHp = new NetworkVariable<int>(
                maxHp,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                CurrentHp.Value = maxHp;
            }

            CurrentHp.OnValueChanged += OnHpChanged;
            RefreshText(CurrentHp.Value);
        }

        public override void OnNetworkDespawn()
        {
            CurrentHp.OnValueChanged -= OnHpChanged;
        }

        private void OnHpChanged(int previous, int next)
        {
            RefreshText(next);
        }

        private void RefreshText(int hp)
        {
            if (hpText != null)
            {
                hpText.text = hp.ToString();
            }
        }

        public void SetHpText(TMP_Text text)
        {
            hpText = text;
            RefreshText(CurrentHp.Value);
        }

        public void ApplyDamage(int damage)
        {
            if (!IsServer)
            {
                return;
            }

            int next = Mathf.Clamp(CurrentHp.Value - damage, 0, maxHp);
            CurrentHp.Value = next;

            if (CurrentHp.Value <= 0)
            {
                NetworkObject.Despawn();
            }
        }

        public void Heal(int amount)
        {
            if (!IsServer)
            {
                return;
            }

            int next = Mathf.Clamp(CurrentHp.Value + amount, 0, maxHp);
            CurrentHp.Value = next;
        }
    }
}
