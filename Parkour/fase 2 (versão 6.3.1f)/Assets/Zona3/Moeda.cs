using UnityEngine;

// Moeda que fica girando. Quando o Player encosta, soma no placar e some.
public class Moeda : MonoBehaviour
{
    public float velocidadeGiro = 90f;   // graus por segundo

    void Start()
    {
        if (HUD.instancia != null) HUD.instancia.total = HUD.instancia.total + 1;
    }

    void Update()
    {
        transform.Rotate(0, velocidadeGiro * Time.deltaTime, 0);
    }

    void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            HUD.instancia.PegouMoeda();
            Destroy(gameObject);
        }
    }
}
