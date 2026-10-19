using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Trampolim: ao entrar no gatilho, lança o jogador para cima.
    /// </summary>
    public class JumpPad : MonoBehaviour
    {
        [Tooltip("Altura do lançamento em unidades.")]
        public float launchHeight = 4.3f;

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ThirdPersonController player)) player.Launch(launchHeight);
        }
    }
}
