using UnityEngine;

// Quando o Player chega no fim do mapa, avisa o HUD para mostrar a vitoria.
public class Chegada : MonoBehaviour
{
    void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player") && HUD.instancia != null)
        {
            HUD.instancia.Vencer();
        }
    }
}
