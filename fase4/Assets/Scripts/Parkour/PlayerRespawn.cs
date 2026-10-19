using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Guarda o último checkpoint e cuida de "cair no vazio":
    /// cair tira 1 vida (GameManager); se ainda tiver vida, volta pro checkpoint.
    /// A tecla R volta ao checkpoint sem perder vida.
    /// </summary>
    [RequireComponent(typeof(ThirdPersonController))]
    public class PlayerRespawn : MonoBehaviour
    {
        [Tooltip("Se cair mais que isso abaixo do checkpoint, perde uma vida.")]
        public float killDepth = 15f;

        ThirdPersonController controller;
        Vector3 point;
        Quaternion rot;

        void Awake()
        {
            controller = GetComponent<ThirdPersonController>();
            point = transform.position;     // posição inicial = primeiro checkpoint
            rot = transform.rotation;
        }

        /// <summary>Chamado pelos Checkpoints.</summary>
        public void SetCheckpoint(Transform t)
        {
            point = t.position;
            rot = t.rotation;
        }

        /// <summary>Leva o jogador para um ponto e o torna o checkpoint atual (reinícios do jogo).</summary>
        public void ResetTo(Transform t)
        {
            SetCheckpoint(t);
            Respawn();
        }

        void Update()
        {
            if (!GameManager.Instance.Playing) return;   // menu, pausa, game over, vitória

            if (GameInput.RespawnPressed) Respawn();
            else if (transform.position.y < point.y - killDepth) OnFell();
        }

        void OnFell()
        {
            // LoseLife devolve false quando as vidas acabaram (aí o GameManager abre o Game Over)
            if (GameManager.Instance.LoseLife()) Respawn();
        }

        public void Respawn() => controller.Teleport(point, rot);
    }
}
