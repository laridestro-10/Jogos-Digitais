using UnityEngine;

// Se o jogador cair, ele volta para onde comecou.
public class VoltarAoInicio : MonoBehaviour
{
    public float alturaMinima = -5f;   // abaixo disso conta como queda

    private Vector3 inicio;
    private CharacterController controller;

    void Start()
    {
        inicio = transform.position;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (transform.position.y < alturaMinima)
        {
            // o CharacterController precisa ser desligado para teleportar
            controller.enabled = false;
            transform.position = inicio;
            controller.enabled = true;
        }
    }
}
