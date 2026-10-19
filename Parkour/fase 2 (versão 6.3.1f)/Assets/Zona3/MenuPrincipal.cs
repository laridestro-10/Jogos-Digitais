using UnityEngine;
using UnityEngine.UI;

// Menu inicial: botoes, recorde, tela "Como jogar" e musica.
public class MenuPrincipal : MonoBehaviour
{
    public Text textoRecorde;
    public Text textoEstrelas;
    public GameObject painelComoJogar;
    public GameObject painelOpcoes;
    public AudioClip musica;

    private AudioSource fundo;

    void Start()
    {
        Time.timeScale = 1f;
        painelComoJogar.SetActive(false);
        if (painelOpcoes != null) painelOpcoes.SetActive(false);

        if (PlayerPrefs.HasKey("recorde"))
            textoRecorde.text = HUD.FormatarTempo(PlayerPrefs.GetFloat("recorde"));
        else
            textoRecorde.text = "--:--";

        if (textoEstrelas != null)
            textoEstrelas.text = PlayerPrefs.GetInt("estrelas", 0) + " / 3 estrelas";

        fundo = gameObject.AddComponent<AudioSource>();
        fundo.clip = musica;
        fundo.loop = true;
        fundo.volume = 0.45f * Configuracoes.Musica;
        if (musica != null) fundo.Play();
    }

    void Update()
    {
        if (fundo != null) fundo.volume = 0.45f * Configuracoes.Musica;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (painelOpcoes != null && painelOpcoes.activeSelf) painelOpcoes.GetComponent<OpcoesUI>().Fechar();
            else if (painelComoJogar.activeSelf) FecharComoJogar();
        }
    }

    public void Jogar()           { Transicao.IrPara("Obby"); }
    public void ComoJogar()       { painelComoJogar.SetActive(true); }
    public void FecharComoJogar() { painelComoJogar.SetActive(false); }
    public void AbrirOpcoes()     { if (painelOpcoes != null) painelOpcoes.SetActive(true); }

    public void Sair()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
