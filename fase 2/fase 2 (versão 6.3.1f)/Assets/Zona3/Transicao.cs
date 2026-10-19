using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Tela preta que aparece/desaparece entre as cenas (fade).
// Uso: Transicao.IrPara("Obby");
[RequireComponent(typeof(CanvasGroup))]
public class Transicao : MonoBehaviour
{
    public float duracao = 0.45f;

    private static Transicao instancia;
    private CanvasGroup grupo;
    private bool trocando;

    void Awake()
    {
        instancia = this;
        grupo = GetComponent<CanvasGroup>();
        grupo.alpha = 1f;
        grupo.blocksRaycasts = true;
    }

    void Start()
    {
        StartCoroutine(Fade(1f, 0f, null));
    }

    public static void IrPara(string cena)
    {
        Time.timeScale = 1f;
        if (instancia == null) { SceneManager.LoadScene(cena); return; }
        if (instancia.trocando) return;
        instancia.trocando = true;
        instancia.StartCoroutine(instancia.Fade(0f, 1f, cena));
    }

    IEnumerator Fade(float de, float para, string cena)
    {
        grupo.blocksRaycasts = true;
        float t = 0f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duracao);
            grupo.alpha = Mathf.Lerp(de, para, k);
            yield return null;
        }
        grupo.alpha = para;
        grupo.blocksRaycasts = para > 0.5f;
        if (cena != null) SceneManager.LoadScene(cena);
    }
}
