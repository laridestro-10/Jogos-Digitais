using UnityEngine;
using UnityEngine.Events;

namespace Parkour
{
    /// <summary>
    /// Coloque num objeto com um Collider "Is Trigger". Ao tocar, o jogador passa
    /// a renascer no "respawnPoint" quando cair.
    /// "index" é o número do checkpoint no percurso (1, 2, 3...). 0 = não conta (a largada).
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        public Transform respawnPoint;
        [Tooltip("Número do checkpoint no percurso (0 = não conta, como a largada).")]
        public int index;
        public UnityEvent onActivated;

        bool active;

        void OnTriggerEnter(Collider other)
        {
            if (!respawnPoint || !other.TryGetComponent(out PlayerRespawn player)) return;
            player.SetCheckpoint(respawnPoint);
            if (index > 0) GameManager.Instance.ReachCheckpoint(index);
            if (!active)
            {
                active = true;
                onActivated?.Invoke();
            }
        }
    }
}
