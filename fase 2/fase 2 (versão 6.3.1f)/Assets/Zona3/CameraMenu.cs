using UnityEngine;

// Camera do menu: gira devagar em volta do cenario.
public class CameraMenu : MonoBehaviour
{
    public Vector3 centro = new Vector3(4f, 1.5f, 0f);
    public float raio = 11f;
    public float altura = 4f;
    public float velocidade = 6f;   // graus por segundo
    public float anguloInicial = 200f;

    void LateUpdate()
    {
        float a = (anguloInicial + Time.unscaledTime * velocidade) * Mathf.Deg2Rad;
        Vector3 pos = centro + new Vector3(Mathf.Sin(a) * raio, altura + Mathf.Sin(Time.unscaledTime * 0.4f) * 0.6f, Mathf.Cos(a) * raio);
        transform.position = pos;
        transform.LookAt(centro);
    }
}
