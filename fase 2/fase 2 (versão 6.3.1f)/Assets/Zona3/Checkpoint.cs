using UnityEngine;

// Checkpoint: quando o Player pisa aqui, este vira o novo lugar de voltar.
public class Checkpoint : MonoBehaviour
{
    public int fase = 1;                      // numero da fase que comeca aqui
    public string mensagem = "Checkpoint!";   // texto que aparece na tela
    public Renderer marca;                    // quadrado que muda de cor
    public Color corAtivado = new Color(1f, 0.85f, 0.1f);

    void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            // o Player volta um pouco acima do centro do checkpoint
            bool novo = HUD.instancia.Checkpoint(fase, transform.position + Vector3.up * 1.2f, mensagem);
            if (novo && marca != null)
            {
                marca.material.color = corAtivado;
            }
        }
    }
}
