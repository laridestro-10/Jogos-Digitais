using UnityEngine;

// Balanca bracos e pernas do boneco quando ele anda.
public class AnimacaoBoneco : MonoBehaviour
{
    public Transform bracoEsquerdo;
    public Transform bracoDireito;
    public Transform pernaEsquerda;
    public Transform pernaDireita;
    public float velocidadeDoPasso = 10f;
    public float angulo = 35f;

    private Vector3 ultimaPosicao;

    void Start()
    {
        ultimaPosicao = transform.position;
    }

    void Update()
    {
        if (Time.deltaTime <= 0) return;   // jogo pausado

        // quanto o boneco andou desde o ultimo frame (sem contar subir/descer)
        Vector3 andou = transform.position - ultimaPosicao;
        andou.y = 0;
        float velocidade = andou.magnitude / Time.deltaTime;
        ultimaPosicao = transform.position;

        float a = 0;
        if (velocidade > 0.5f)
        {
            a = Mathf.Sin(Time.time * velocidadeDoPasso) * angulo;
        }

        bracoEsquerdo.localRotation = Quaternion.Euler(a, 0, 0);
        bracoDireito.localRotation = Quaternion.Euler(-a, 0, 0);
        pernaEsquerda.localRotation = Quaternion.Euler(-a, 0, 0);
        pernaDireita.localRotation = Quaternion.Euler(a, 0, 0);
    }
}
