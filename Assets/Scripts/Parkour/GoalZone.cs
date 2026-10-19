using UnityEngine;
using UnityEngine.Events;

namespace Parkour
{
    /// <summary>
    /// Portal de chegada: ao tocar, o GameManager mostra a Vitória.
    /// Precisa de um Collider "Is Trigger".
    /// </summary>
    public class GoalZone : MonoBehaviour
    {
        public UnityEvent onGoal;

        void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerRespawn _)) return;
            onGoal?.Invoke();
            GameManager.Instance.Win();   // Win() só vale uma vez (só funciona em Playing)
        }
    }
}
