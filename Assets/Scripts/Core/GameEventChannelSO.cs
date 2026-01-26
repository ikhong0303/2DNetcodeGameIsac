using System;
using UnityEngine;

namespace TopDownShooter.Core
{
    [CreateAssetMenu(menuName = "TopDownShooter/Events/Game Event Channel")]
    public class GameEventChannelSO : ScriptableObject
    {
        public event Action EventRaised;

        public void Raise()
        {
            EventRaised?.Invoke();
        }
    }
}
