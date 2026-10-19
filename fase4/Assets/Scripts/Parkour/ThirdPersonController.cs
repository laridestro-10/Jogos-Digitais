using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Controle do jogador em terceira pessoa, estilo Roblox, usando CharacterController.
    /// - Anda relativo à câmera, com aceleração alta (liga/desliga na hora).
    /// - Pulo com "coyote time" e "jump buffer"; segurar Espaço pula de novo ao pisar no chão.
    /// - Acompanha plataformas móveis (MovingPlatform).
    /// - Recebe empurrão de obstáculos (Hazard) e impulso de trampolins (JumpPad).
    /// - Empurra caixas com física (Rigidbody).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Se vazio, usa a Main Camera.")]
        public Transform cameraTransform;

        [Header("Movimento")]
        public float walkSpeed = 7f;
        public float sprintSpeed = 7f;            // Roblox não tem corrida; aumente se quiser usar Shift
        public float groundAcceleration = 200f;
        public float airAcceleration = 120f;
        public float turnSmoothTime = 0.03f;

        [Header("Pulo")]
        public float jumpHeight = 2f;
        public float gravity = -55f;
        [Tooltip("1 = pulo sempre completo (Roblox). Maior que 1 = soltar o botão encurta o pulo.")]
        public float lowJumpMultiplier = 1f;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.12f;
        [Tooltip("Segurar Espaço = pular de novo assim que pisar no chão.")]
        public bool autoJumpWhileHeld = true;
        public float maxFallSpeed = 40f;

        [Header("Física")]
        [Tooltip("Força com que o jogador empurra objetos com Rigidbody (as caixas).")]
        public float pushPower = 1.2f;

        public bool IsGrounded => cc.isGrounded;
        public Vector3 Velocity => horizontalVel + Vector3.up * verticalVel;

        CharacterController cc;
        Vector3 horizontalVel;
        float verticalVel;
        float turnVel;
        float coyoteCounter;
        float bufferCounter;
        float controlLock;      // tempo sem controle depois de levar um empurrão
        float hitCooldown;      // evita empurrões repetidos no mesmo frame

        // plataforma móvel em que estamos pisando (preenchida em OnControllerColliderHit)
        MovingPlatform platform;

        // cache do último collider tocado (evita GetComponentInParent a cada frame)
        Collider lastCollider;
        MovingPlatform lastPlatform;
        Hazard lastHazard;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            if (!GameManager.Instance.Playing) return;   // menu, pausa, game over ou vitória
            float dt = Time.deltaTime;

            // 1) vai junto com a plataforma móvel em que estava
            if (platform != null) cc.Move(platform.Delta);
            platform = null;

            bool grounded = cc.isGrounded;
            controlLock -= dt;
            hitCooldown -= dt;

            // 2) movimento horizontal relativo à câmera
            Vector3 wishDir = Vector3.zero;
            if (controlLock <= 0f)
            {
                Vector2 input = GameInput.Move;
                Vector3 camF = Vector3.forward, camR = Vector3.right;
                if (cameraTransform)
                {
                    camF = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                    camR = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                }
                wishDir = camF * input.y + camR * input.x;

                float speed = GameInput.Sprint ? sprintSpeed : walkSpeed;
                float accel = grounded ? groundAcceleration : airAcceleration;
                horizontalVel = Vector3.MoveTowards(horizontalVel, wishDir * speed, accel * dt);
            }
            else
            {
                // levou empurrão: perde velocidade aos poucos, sem controle por um instante
                horizontalVel = Vector3.MoveTowards(horizontalVel, Vector3.zero, 4f * dt);
            }

            // 3) vira o personagem para onde está andando
            if (wishDir.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(wishDir.x, wishDir.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVel, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // 4) pulo (coyote time + buffer + pular segurando)
            bool jumpWanted = controlLock <= 0f &&
                              (GameInput.JumpPressed || (autoJumpWhileHeld && GameInput.JumpHeld));
            coyoteCounter = grounded ? coyoteTime : coyoteCounter - dt;
            bufferCounter = jumpWanted ? jumpBufferTime : bufferCounter - dt;

            if (grounded && verticalVel < 0f) verticalVel = -2f;   // gruda no chão

            if (bufferCounter > 0f && coyoteCounter > 0f)
            {
                verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity);
                bufferCounter = 0f;
                coyoteCounter = 0f;
            }

            // 5) gravidade
            float g = gravity;
            if (verticalVel > 0f && !GameInput.JumpHeld) g *= lowJumpMultiplier;
            verticalVel = Mathf.Max(verticalVel + g * dt, -maxFallSpeed);

            cc.Move((horizontalVel + Vector3.up * verticalVel) * dt);
        }

        /// <summary>Chamado pela Unity quando o CharacterController encosta em algo durante o Move.</summary>
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // empurra objetos de física (Rigidbody não-kinemático, como as caixas)
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body != null && !body.isKinematic && hit.moveDirection.y > -0.3f)
            {
                Vector3 push = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
                body.AddForceAtPosition(push * pushPower, hit.point, ForceMode.Impulse);
                return;
            }

            if (hit.collider != lastCollider)
            {
                lastCollider = hit.collider;
                lastPlatform = hit.collider.GetComponentInParent<MovingPlatform>();
                lastHazard = hit.collider.GetComponentInParent<Hazard>();
            }

            if (lastHazard != null)
            {
                Knockback(hit.normal, lastHazard);
                return;
            }
            if (lastPlatform != null && hit.normal.y > 0.5f) platform = lastPlatform;
        }

        void Knockback(Vector3 normal, Hazard hazard)
        {
            if (hitCooldown > 0f) return;
            hitCooldown = 0.4f;

            // a normal aponta do obstáculo para o jogador
            Vector3 dir = new Vector3(normal.x, 0f, normal.z);
            if (dir.sqrMagnitude < 0.01f) dir = -transform.forward;
            dir.Normalize();

            horizontalVel = dir * hazard.knockback;
            verticalVel = hazard.upwardForce;
            controlLock = hazard.controlLock;
        }

        /// <summary>Lança o jogador para cima (trampolim). "height" = altura do pulo em unidades.</summary>
        public void Launch(float height)
        {
            verticalVel = Mathf.Sqrt(height * -2f * gravity);
            coyoteCounter = 0f;
            bufferCounter = 0f;
        }

        /// <summary>Move o jogador para outro lugar (respawn / checkpoints).</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            cc.enabled = true;
            horizontalVel = Vector3.zero;
            verticalVel = 0f;
            controlLock = 0f;
            platform = null;
        }
    }
}
