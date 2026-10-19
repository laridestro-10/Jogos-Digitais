using UnityEngine;

// Animacao suave de "flutuar" para textos e elementos da interface.
public class Flutuar : MonoBehaviour
{
    public float alturaOnda = 10f;      // pixels para cima/baixo
    public float velocidade = 1.5f;
    public float inclinacao = 0f;       // graus de balanco
    public float pulsar = 0f;           // 0.03 = cresce 3%
    public float atraso = 0f;           // desencontra elementos iguais

    private Vector3 posBase;
    private Vector3 escalaBase;
    private Quaternion rotBase;

    void Start()
    {
        posBase = transform.localPosition;
        escalaBase = transform.localScale;
        rotBase = transform.localRotation;
    }

    void Update()
    {
        float t = Time.unscaledTime * velocidade + atraso;
        transform.localPosition = posBase + Vector3.up * Mathf.Sin(t) * alturaOnda;
        transform.localRotation = rotBase * Quaternion.Euler(0, 0, Mathf.Sin(t * 0.7f) * inclinacao);
        transform.localScale = escalaBase * (1f + Mathf.Sin(t * 1.3f) * pulsar);
    }
}
