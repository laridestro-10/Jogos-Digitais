using UnityEngine;
using UnityEngine.UI;

// Tela de opcoes: volume da musica, dos efeitos, sensibilidade da camera e tela cheia.
public class OpcoesUI : MonoBehaviour
{
    public Text textoMusica;
    public Text textoEfeitos;
    public Text textoSensibilidade;
    public Text textoTelaCheia;

    const float passoVolume = 0.1f;
    const float passoSens = 0.1f;

    void OnEnable() { Atualizar(); }

    public void MusicaMais()   { Configuracoes.Musica += passoVolume; Atualizar(); }
    public void MusicaMenos()  { Configuracoes.Musica -= passoVolume; Atualizar(); }
    public void EfeitosMais()  { Configuracoes.Efeitos += passoVolume; Atualizar(); }
    public void EfeitosMenos() { Configuracoes.Efeitos -= passoVolume; Atualizar(); }
    public void SensMais()     { Configuracoes.Sensibilidade += passoSens; Atualizar(); }
    public void SensMenos()    { Configuracoes.Sensibilidade -= passoSens; Atualizar(); }

    public void AlternarTelaCheia()
    {
        Configuracoes.TelaCheia = !Configuracoes.TelaCheia;
        // a troca so acontece no proximo frame, entao mostramos o valor novo direto
        textoTelaCheia.text = !Screen.fullScreen ? "LIGADA" : "DESLIGADA";
    }

    public void Fechar()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }

    void Atualizar()
    {
        textoMusica.text = Mathf.RoundToInt(Configuracoes.Musica * 100) + "%";
        textoEfeitos.text = Mathf.RoundToInt(Configuracoes.Efeitos * 100) + "%";
        textoSensibilidade.text = Configuracoes.Sensibilidade.ToString("0.0") + "x";
        textoTelaCheia.text = Screen.fullScreen ? "LIGADA" : "DESLIGADA";
    }
}
