using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Cuida do jogo: moedas, tempo, fases, mortes, checkpoints, sons, pausa e vitoria.
// Outros scripts acham ele com HUD.instancia.
public class HUD : MonoBehaviour
{
    public static HUD instancia;

    [Header("Topo da tela")]
    public Text textoMoedas;
    public Text textoTempo;
    public Text textoMortes;
    public Text textoFase;
    public RectTransform pilulaMoedas;     // pula quando pega moeda
    public Image[] segmentos;              // barra de progresso das fases
    public Color corFeito = new Color(0.35f, 0.9f, 0.45f);
    public Color corAtual = new Color(1f, 0.82f, 0.2f);
    public Color corFalta = new Color(1f, 1f, 1f, 0.18f);

    [Header("Aviso no meio da tela")]
    public CanvasGroup aviso;
    public Text textoAviso;

    [Header("Telas")]
    public GameObject painelPausa;
    public GameObject painelVitoria;
    public GameObject painelOpcoes;
    public RectTransform cartaoVitoria;
    public Image flashMorte;

    [Header("Vitoria")]
    public RawImage[] estrelas;
    public Texture estrelaCheia;
    public Texture estrelaVazia;
    public Text textoTempoFinal;
    public Text textoMoedasFinal;
    public Text textoMortesFinal;
    public GameObject seloRecorde;

    [Header("Jogo")]
    public Transform player;
    public int totalFases = 7;
    public float alturaMorte = -1f;

    [Header("Sons")]
    public AudioClip musica;
    public AudioClip somMoeda;
    public AudioClip somCheckpoint;
    public AudioClip somMorte;
    public AudioClip somVitoria;

    [HideInInspector] public int moedas = 0;
    [HideInInspector] public int total = 0;

    private float tempo;
    private bool pausado, terminou;
    private int faseAtual = 1;
    private int mortes;
    private Vector3 pontoDeVolta;
    private float tempoAviso = -1f;
    private float pulsoMoeda;
    private float tempoVitoria = -1f;
    private int estrelasGanhas;
    private int estrelasMostradas;
    private AudioSource efeitos, fundo;

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        painelPausa.SetActive(false);
        painelVitoria.SetActive(false);
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
        Time.timeScale = 1f;
        pontoDeVolta = player.position;

        efeitos = gameObject.AddComponent<AudioSource>();
        efeitos.ignoreListenerPause = true;
        fundo = gameObject.AddComponent<AudioSource>();
        fundo.clip = musica;
        fundo.loop = true;
        fundo.volume = 0.45f * Configuracoes.Musica;
        if (musica != null) fundo.Play();

        AtualizarBarra();
        MostrarAviso("FASE 1");
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (!terminou && !pausado) tempo += Time.deltaTime;

        textoMoedas.text = moedas + " / " + total;
        textoTempo.text = FormatarTempo(tempo);
        textoMortes.text = mortes.ToString();
        textoFase.text = "FASE " + faseAtual;

        // moeda: pulinho na pilula
        if (pilulaMoedas != null)
        {
            pulsoMoeda = Mathf.Max(0, pulsoMoeda - dt * 4f);
            pilulaMoedas.localScale = Vector3.one * (1f + Mathf.Sin(pulsoMoeda * Mathf.PI) * 0.18f);
        }

        // aviso: entra crescendo, fica, e some
        if (tempoAviso >= 0)
        {
            tempoAviso += dt;
            float entrada = Mathf.Clamp01(tempoAviso / 0.25f);
            float saida = Mathf.Clamp01((tempoAviso - 2.4f) / 0.4f);
            aviso.alpha = entrada * (1f - saida);
            float escala = Mathf.Lerp(1.4f, 1f, EaseOutBack(entrada));
            aviso.transform.localScale = Vector3.one * escala;
            if (saida >= 1f) { tempoAviso = -1f; aviso.gameObject.SetActive(false); }
        }

        // tela vermelha sumindo
        if (flashMorte.color.a > 0)
        {
            Color c = flashMorte.color;
            c.a = Mathf.Max(0, c.a - dt * 1.5f);
            flashMorte.color = c;
        }

        // caiu do mapa
        if (!terminou && player.position.y < alturaMorte) Morrer();

        // volumes (podem mudar na tela de opcoes)
        fundo.volume = 0.45f * Configuracoes.Musica * (pausado ? 0.4f : 1f);
        efeitos.volume = Configuracoes.Efeitos;

        // pausa (se as opcoes estiverem abertas, o Esc fecha elas primeiro)
        bool opcoesAbertas = painelOpcoes != null && painelOpcoes.activeSelf;
        if (Input.GetKeyDown(KeyCode.Escape) && opcoesAbertas)
        {
            painelOpcoes.GetComponent<OpcoesUI>().Fechar();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !terminou)
        {
            if (pausado) Continuar(); else Pausar();
        }

        // animacao da vitoria
        if (tempoVitoria >= 0) AnimarVitoria(dt);
    }

    // ---------- eventos do jogo ----------

    public void TocarSom(AudioClip clip, float volume = 1f)
    {
        if (clip != null && efeitos != null) efeitos.PlayOneShot(clip, volume);
    }

    public void PegouMoeda()
    {
        moedas++;
        pulsoMoeda = 1f;
        TocarSom(somMoeda);
    }

    public bool Checkpoint(int fase, Vector3 posicao, string mensagem)
    {
        if (fase <= faseAtual) return false;
        faseAtual = fase;
        pontoDeVolta = posicao;
        AtualizarBarra();
        MostrarAviso(mensagem);
        TocarSom(somCheckpoint);
        return true;
    }

    public void Morrer()
    {
        mortes++;
        TocarSom(somMorte);
        flashMorte.color = new Color(1f, 0.1f, 0.15f, 0.55f);
        player.SetParent(null);
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.position = pontoDeVolta;
        cc.enabled = true;
    }

    public void MostrarAviso(string texto)
    {
        textoAviso.text = texto;
        aviso.gameObject.SetActive(true);
        tempoAviso = 0f;
    }

    void AtualizarBarra()
    {
        for (int i = 0; i < segmentos.Length; i++)
        {
            if (i < faseAtual - 1) segmentos[i].color = corFeito;
            else if (i == faseAtual - 1) segmentos[i].color = corAtual;
            else segmentos[i].color = corFalta;
        }
    }

    // ---------- pausa ----------

    public void Pausar()
    {
        pausado = true;
        painelPausa.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Continuar()
    {
        pausado = false;
        painelPausa.SetActive(false);
        Time.timeScale = 1f;
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
    }

    public void AbrirOpcoes()
    {
        if (painelOpcoes != null) painelOpcoes.SetActive(true);
    }

    // ---------- vitoria ----------

    public void Vencer()
    {
        if (terminou) return;
        terminou = true;
        Time.timeScale = 0f;
        fundo.Stop();
        TocarSom(somVitoria);

        float recorde = PlayerPrefs.GetFloat("recorde", 0f);
        bool novoRecorde = recorde == 0f || tempo < recorde;
        if (novoRecorde) PlayerPrefs.SetFloat("recorde", tempo);

        // estrelas: 1 por terminar, +1 por 75% das moedas, +1 por morrer 3 vezes ou menos
        estrelasGanhas = 1;
        if (total > 0 && moedas >= Mathf.CeilToInt(total * 0.75f)) estrelasGanhas++;
        if (mortes <= 3) estrelasGanhas++;
        int melhor = PlayerPrefs.GetInt("estrelas", 0);
        if (estrelasGanhas > melhor) PlayerPrefs.SetInt("estrelas", estrelasGanhas);

        textoTempoFinal.text = FormatarTempo(tempo);
        textoMoedasFinal.text = moedas + " / " + total;
        textoMortesFinal.text = mortes.ToString();
        seloRecorde.SetActive(novoRecorde);
        foreach (RawImage e in estrelas) { e.texture = estrelaVazia; e.transform.localScale = Vector3.one; }
        estrelasMostradas = 0;

        painelVitoria.SetActive(true);
        tempoVitoria = 0f;
    }

    void AnimarVitoria(float dt)
    {
        tempoVitoria += dt;
        float k = Mathf.Clamp01(tempoVitoria / 0.45f);
        cartaoVitoria.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, EaseOutBack(k));

        // estrelas aparecem uma de cada vez
        float inicio = 0.6f, intervalo = 0.35f;
        while (estrelasMostradas < estrelasGanhas && tempoVitoria > inicio + estrelasMostradas * intervalo)
        {
            estrelas[estrelasMostradas].texture = estrelaCheia;
            TocarSom(somMoeda, 0.8f);
            estrelasMostradas++;
        }
        for (int i = 0; i < estrelas.Length; i++)
        {
            float t = tempoVitoria - (inicio + i * intervalo);
            float s = (i < estrelasGanhas && t > 0) ? Mathf.Lerp(1.6f, 1f, EaseOutBack(Mathf.Clamp01(t / 0.3f))) : 1f;
            estrelas[i].transform.localScale = Vector3.one * s;
        }
    }

    // ---------- botoes ----------

    public void JogarDeNovo() { Transicao.IrPara(SceneManager.GetActiveScene().name); }
    public void IrParaMenu()  { Transicao.IrPara("Menu"); }

    // ---------- ajudantes ----------

    public static string FormatarTempo(float segundos)
    {
        int min = (int)(segundos / 60);
        int seg = (int)(segundos % 60);
        return min + ":" + seg.ToString("00");
    }

    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
    }
}
