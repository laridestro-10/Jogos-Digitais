using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Câmera orbital em terceira pessoa, estilo Roblox:
    /// segure o BOTÃO DIREITO do mouse para girar e use o SCROLL para zoom.
    /// Tem colisão (não atravessa paredes) e usa a sensibilidade das Configurações (vale na hora).
    /// Fora do jogo (menu, game over, vitória) ela gira devagar em volta do jogador.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);

        [Header("Zoom (scroll)")]
        public float distance = 7f;
        public float minDistance = 2f;
        public float maxDistance = 25f;
        public float zoomStep = 1f;

        [Header("Rotação")]
        [Tooltip("Sensibilidade do mouse (se 'useSavedSensitivity' estiver ligado, vale o valor das Configurações).")]
        public float sensitivity = 2f;
        public bool useSavedSensitivity = true;
        public float minPitch = -30f;
        public float maxPitch = 70f;
        [Tooltip("ON = estilo Roblox (gira segurando o botão direito). OFF = gira sempre, com o mouse preso.")]
        public bool rotateOnlyWithRightMouse = true;
        [Tooltip("Graus por segundo da rotação automática no menu / game over / vitória.")]
        public float idleSpinSpeed = 10f;

        [Header("Colisão")]
        public float collisionRadius = 0.3f;
        public LayerMask collisionMask = ~0;

        // buffer reaproveitado: SphereCastNonAlloc não gera lixo de memória a cada frame
        static readonly RaycastHit[] hitBuffer = new RaycastHit[8];

        float yaw, pitch = 20f;
        float targetDistance, zoomCurrent, currentDistance;
        bool wasRotating;
        bool cursorInit;

        void Start()
        {
            if (target) yaw = target.eulerAngles.y;
            targetDistance = zoomCurrent = currentDistance = distance;
        }

        /// <summary>Volta a câmera para trás do jogador (novo jogo).</summary>
        public void ResetView(float newYaw)
        {
            yaw = newYaw;
            pitch = 20f;
        }

        void LateUpdate()
        {
            if (!target) return;

            GameManager gm = GameManager.Instance;
            if (gm.Playing)
            {
                HandleInput();
            }
            else
            {
                cursorInit = false;   // ao voltar a jogar, o cursor é reaplicado
                // girando de leve atrás do menu; na pausa fica parada
                if (gm.State != GameState.Paused) yaw += idleSpinSpeed * Time.unscaledDeltaTime;
            }

            Place();
        }

        void HandleInput()
        {
            if (useSavedSensitivity) sensitivity = GameSettings.Sensitivity;

            // ---- cursor / modo de rotação ----
            bool rotating = !rotateOnlyWithRightMouse || GameInput.RightMouseHeld;
            bool changed = !cursorInit || rotating != wasRotating;
            if (changed)
            {
                Cursor.lockState = rotating ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !rotating;
                wasRotating = rotating;
                cursorInit = true;
            }

            if (rotating && !changed)
            {
                Vector2 look = GameInput.Look;
                yaw += look.x * sensitivity;
                pitch = Mathf.Clamp(pitch - look.y * sensitivity, minPitch, maxPitch);
            }

            targetDistance = Mathf.Clamp(targetDistance - GameInput.Zoom * zoomStep, minDistance, maxDistance);
        }

        void Place()
        {
            // ---- zoom suave ----
            zoomCurrent = Mathf.Lerp(zoomCurrent, targetDistance, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + pivotOffset;
            Vector3 dir = rot * Vector3.back;

            // ---- não deixa a câmera atravessar paredes ----
            float wanted = zoomCurrent;
            int n = Physics.SphereCastNonAlloc(pivot, collisionRadius, dir, hitBuffer, zoomCurrent,
                                               collisionMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                RaycastHit h = hitBuffer[i];
                if (h.distance <= 0f) continue;                    // já começou dentro de algo
                if (h.transform.IsChildOf(target)) continue;       // ignora o próprio jogador
                if (h.distance < wanted) wanted = h.distance;
            }
            wanted = Mathf.Max(wanted, 0.6f);

            // aproxima na hora (parede), afasta suave
            if (wanted < currentDistance) currentDistance = wanted;
            else currentDistance = Mathf.Lerp(currentDistance, wanted, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));

            transform.SetPositionAndRotation(pivot + dir * currentDistance, rot);
        }
    }
}
