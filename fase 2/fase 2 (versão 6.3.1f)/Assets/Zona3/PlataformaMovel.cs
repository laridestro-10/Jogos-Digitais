using UnityEngine;

// Plataforma que vai e volta. O Player vira "filho" dela enquanto esta em cima,
// assim ele anda junto com a plataforma.
public class PlataformaMovel : MonoBehaviour
{
    public Vector3 deslocamento = new Vector3(0, 0, 2);   // ate onde ela vai
    public float velocidade = 0.5f;

    private Vector3 inicio;

    void Start()
    {
        inicio = transform.position;
    }

    void Update()
    {
        // PingPong vai de 0 ate 1 e volta para 0, sem parar
        float t = Mathf.PingPong(Time.time * velocidade, 1f);
        transform.position = inicio + deslocamento * t;
    }

    void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            outro.transform.SetParent(transform);
        }
    }

    void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag("Player") && outro.transform.parent == transform)
        {
            outro.transform.SetParent(null);
        }
    }
}
