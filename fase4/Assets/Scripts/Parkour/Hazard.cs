using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Marca um obstáculo que empurra o jogador ao encostar (o ThirdPersonController lê estes valores).
    /// Fica no mesmo objeto do Collider (ou num pai dele).
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        public float knockback = 11f;       // força horizontal do empurrão
        public float upwardForce = 6f;      // força para cima
        public float controlLock = 0.35f;   // tempo sem controle do jogador depois do empurrão
    }
}
