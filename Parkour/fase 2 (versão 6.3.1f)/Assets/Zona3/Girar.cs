using UnityEngine;

// Faz o objeto girar sem parar (ex: barra giratoria).
public class Girar : MonoBehaviour
{
    public Vector3 velocidade = new Vector3(0, 60, 0);   // graus por segundo em cada eixo

    void Update()
    {
        transform.Rotate(velocidade * Time.deltaTime);
    }
}
