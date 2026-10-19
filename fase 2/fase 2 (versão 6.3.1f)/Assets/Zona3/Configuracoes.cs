using UnityEngine;

// Opcoes do jogador, salvas no computador (PlayerPrefs).
// Qualquer script pode ler: Configuracoes.Musica, Configuracoes.Efeitos...
public static class Configuracoes
{
    public static float Musica
    {
        get { return PlayerPrefs.GetFloat("opc_musica", 0.7f); }
        set { PlayerPrefs.SetFloat("opc_musica", Mathf.Clamp01(value)); }
    }

    public static float Efeitos
    {
        get { return PlayerPrefs.GetFloat("opc_efeitos", 0.8f); }
        set { PlayerPrefs.SetFloat("opc_efeitos", Mathf.Clamp01(value)); }
    }

    // multiplicador da velocidade da camera (0.3 a 2)
    public static float Sensibilidade
    {
        get { return PlayerPrefs.GetFloat("opc_sensibilidade", 1f); }
        set { PlayerPrefs.SetFloat("opc_sensibilidade", Mathf.Clamp(value, 0.3f, 2f)); }
    }

    public static bool TelaCheia
    {
        get { return Screen.fullScreen; }
        set { Screen.fullScreen = value; }
    }
}
