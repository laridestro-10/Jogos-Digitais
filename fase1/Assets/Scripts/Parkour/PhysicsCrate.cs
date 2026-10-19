using System.Collections.Generic;
using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Caixa com física de verdade (Rigidbody NÃO kinemático): tem massa, gravidade e colisão,
    /// e o jogador pode empurrar. Se cair no vazio volta para o lugar de origem, e o
    /// GameManager arruma todas as caixas quando um jogo novo começa.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsCrate : MonoBehaviour
    {
        [Tooltip("Altura (Y) abaixo da qual a caixa volta para o lugar de origem.")]
        public float killY = -40f;

        /// <summary>Todas as caixas ativas (para reiniciar de uma vez).</summary>
        public static readonly List<PhysicsCrate> All = new List<PhysicsCrate>();

        Rigidbody body;
        Vector3 startPos;
        Quaternion startRot;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            startPos = transform.position;
            startRot = transform.rotation;
        }

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        void Update()
        {
            if (transform.position.y < killY) ResetToStart();
        }

        public void ResetToStart()
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(startPos, startRot);
        }
    }
}
