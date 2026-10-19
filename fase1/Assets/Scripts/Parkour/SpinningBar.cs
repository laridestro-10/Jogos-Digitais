using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Barra giratória física: Rigidbody KINEMÁTICO girado no FixedUpdate (jeito correto de mover
    /// colisores pela física). O collider filho tem o componente Hazard, que empurra o jogador.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SpinningBar : MonoBehaviour
    {
        public float degreesPerSecond = 90f;

        Rigidbody body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;   // movimento suave na tela
        }

        void FixedUpdate()
        {
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, degreesPerSecond * Time.fixedDeltaTime, 0f));
        }
    }
}
