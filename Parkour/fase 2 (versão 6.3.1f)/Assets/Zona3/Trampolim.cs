using UnityEngine;

// Joga o Player para cima quando ele pisa.
public class Trampolim : MonoBehaviour
{
    public float alturaDoPulo = 5f;

    void OnTriggerEnter(Collider outro)
    {
        Personagem personagem = outro.GetComponent<Personagem>();
        if (personagem != null)
        {
            personagem.Impulso(alturaDoPulo);
        }
    }
}
