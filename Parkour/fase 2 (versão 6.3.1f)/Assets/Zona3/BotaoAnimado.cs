using UnityEngine;
using UnityEngine.EventSystems;

// Deixa o botao "vivo": cresce quando o mouse passa, afunda no clique e toca sons.
// Usa tempo real (unscaled) para funcionar tambem com o jogo pausado.
public class BotaoAnimado : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float escalaHover = 1.07f;
    public float escalaClique = 0.94f;
    public AudioClip somHover;
    public AudioClip somClique;

    private Vector3 escalaBase;
    private float alvo = 1f;
    private bool dentro;
    private static AudioSource som;   // uma caixa de som para todos os botoes

    void Awake()
    {
        escalaBase = transform.localScale;
        if (som == null)
        {
            GameObject g = new GameObject("SomDosBotoes");
            DontDestroyOnLoad(g);
            som = g.AddComponent<AudioSource>();
            som.ignoreListenerPause = true;
        }
    }

    void OnDisable()
    {
        alvo = 1f; dentro = false;
        transform.localScale = escalaBase;
    }

    void Update()
    {
        float atual = transform.localScale.x / escalaBase.x;
        float novo = Mathf.Lerp(atual, alvo, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
        transform.localScale = escalaBase * novo;
    }

    public void OnPointerEnter(PointerEventData e) { dentro = true; alvo = escalaHover; Tocar(somHover, 0.5f); }
    public void OnPointerExit(PointerEventData e)  { dentro = false; alvo = 1f; }
    public void OnPointerDown(PointerEventData e)  { alvo = escalaClique; Tocar(somClique, 0.8f); }
    public void OnPointerUp(PointerEventData e)    { alvo = dentro ? escalaHover : 1f; }

    void Tocar(AudioClip c, float vol)
    {
        if (c != null && som != null) som.PlayOneShot(c, vol * Configuracoes.Efeitos);
    }
}
