using UnityEngine;

// Lava, barra giratoria... encostou, volta para o ultimo checkpoint.
public class Perigo : MonoBehaviour
{
    void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            HUD.instancia.Morrer();
        }
    }
}
