using UnityEngine;
using UnityEngine.Events;

namespace TopDownShooter.Core
{
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEventChannelSO channel;
        [SerializeField] private UnityEvent response;

        private void OnEnable()
        {
            if (channel != null)
            {
                channel.EventRaised += OnEventRaised;
            }
        }

        private void OnDisable()
        {
            if (channel != null)
            {
                channel.EventRaised -= OnEventRaised;
            }
        }

        private void OnEventRaised()
        {
            response?.Invoke();
        }
    }
}
