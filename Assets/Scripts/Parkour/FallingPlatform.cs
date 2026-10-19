using System.Collections;
using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Plataforma que treme e cai quando o jogador pisa, e volta depois de um tempo.
    /// Funciona em um objeto com BoxCollider normal (ela cria sozinha o gatilho em cima).
    /// </summary>
    public class FallingPlatform : MonoBehaviour
    {
        [Tooltip("Tempo tremendo antes de cair.")]
        public float delay = 0.35f;
        [Tooltip("Tempo até a plataforma voltar.")]
        public float resetTime = 4f;
        public float fallSpeed = 18f;
        public float shake = 0.05f;

        Collider solid;
        Vector3 startPos;
        bool triggered;

        void Awake()
        {
            solid = GetComponent<Collider>();
            startPos = transform.position;

            // gatilho fino logo acima da superfície, para detectar quem pisa
            var trig = gameObject.AddComponent<BoxCollider>();
            trig.isTrigger = true;
            var box = solid as BoxCollider;
            Vector3 size = box ? box.size : new Vector3(2f, 0.5f, 2f);
            trig.center = new Vector3(0f, size.y * 0.5f + 1f, 0f);
            trig.size = new Vector3(size.x * 0.9f, 2f, size.z * 0.9f);
        }

        void OnTriggerEnter(Collider other)
        {
            if (triggered || !other.TryGetComponent(out PlayerRespawn _)) return;
            StartCoroutine(Fall());
        }

        IEnumerator Fall()
        {
            triggered = true;

            // 1) treme
            for (float t = 0f; t < delay; t += Time.deltaTime)
            {
                transform.position = startPos + Random.insideUnitSphere * shake;
                yield return null;
            }

            // 2) cai
            solid.enabled = false;
            for (float t = 0f; t < resetTime; t += Time.deltaTime)
            {
                transform.position += Vector3.down * fallSpeed * Time.deltaTime;
                yield return null;
            }

            // 3) volta ao lugar
            transform.position = startPos;
            solid.enabled = true;
            triggered = false;
        }
    }
}
