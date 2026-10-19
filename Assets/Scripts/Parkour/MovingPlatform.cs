using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Plataforma que vai e volta entre dois pontos. O jogador (CharacterController) é carregado
    /// porque o controle lê "Delta" (quanto a plataforma andou neste frame).
    /// DefaultExecutionOrder negativo: roda ANTES do jogador, para o Delta estar atualizado.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class MovingPlatform : MonoBehaviour
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public float speed = 3f;
        public float pauseTime = 0.4f;
        [Range(0f, 1f)] public float startT = 0f;   // posição inicial no trajeto (para desencontrar plataformas)

        /// <summary>Deslocamento feito neste frame.</summary>
        public Vector3 Delta { get; private set; }

        float t;
        int direction = 1;
        float pause;
        float length;

        void Start()
        {
            length = Vector3.Distance(pointA, pointB);
            t = startT;
            transform.position = Vector3.Lerp(pointA, pointB, t);
        }

        void Update()
        {
            Delta = Vector3.zero;
            if (Time.timeScale == 0f || length < 0.01f) return;

            if (pause > 0f)
            {
                pause -= Time.deltaTime;
                return;
            }

            t += direction * speed * Time.deltaTime / length;
            if (t >= 1f) { t = 1f; direction = -1; pause = pauseTime; }
            else if (t <= 0f) { t = 0f; direction = 1; pause = pauseTime; }

            Vector3 newPos = Vector3.Lerp(pointA, pointB, t);
            Delta = newPos - transform.position;
            transform.position = newPos;
        }
    }
}
