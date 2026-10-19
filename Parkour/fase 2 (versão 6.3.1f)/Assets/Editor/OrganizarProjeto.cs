#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Roda UMA vez quando o Unity abre/recompila: organiza as pastas do jogo,
// renomeia alguns scripts para nomes mais claros e depois se apaga sozinho.
// Usa AssetDatabase.MoveAsset, que mantem os GUIDs: nenhuma referencia das cenas se perde.
[InitializeOnLoad]
public static class OrganizarProjeto
{
    const string Base = "Assets/ParkourNasAlturas";
    const string Velho = "Assets/Zona3";

    static OrganizarProjeto()
    {
        EditorApplication.delayCall += Executar;
    }

    static void Executar()
    {
        if (!AssetDatabase.IsValidFolder(Velho))
        {
            Apagar("Assets/Editor/OrganizarProjeto.cs");
            return;
        }
        Debug.Log("[Organizar] Organizando o projeto...");

        foreach (var p in new[] { "Cenas", "Scripts", "Scripts/Jogo", "Scripts/Jogador", "Scripts/Fases", "Scripts/Interface", "Scripts/Legado", "Materiais", "Sons", "Texturas", "Fontes" })
            CriarPasta(Base + "/" + p);

        // cenas
        Mover("Assets/Scenes/Menu.unity", Base + "/Cenas/Menu.unity");
        Mover("Assets/Scenes/Obby.unity", Base + "/Cenas/Obby.unity");

        // scripts (alguns ganham nome novo)
        string s = Base + "/Scripts/";
        Mover(Velho + "/HUD.cs", s + "Jogo/GerenciadorJogo.cs");
        Mover(Velho + "/Configuracoes.cs", s + "Jogo/Configuracoes.cs");
        Mover(Velho + "/Personagem.cs", s + "Jogador/Personagem.cs");
        Mover(Velho + "/AnimacaoBoneco.cs", s + "Jogador/AnimacaoBoneco.cs");
        Mover(Velho + "/CameraSegue.cs", s + "Jogador/CameraJogador.cs");
        Mover(Velho + "/Checkpoint.cs", s + "Fases/Checkpoint.cs");
        Mover(Velho + "/Perigo.cs", s + "Fases/Perigo.cs");
        Mover(Velho + "/Trampolim.cs", s + "Fases/Trampolim.cs");
        Mover(Velho + "/PlataformaMovel.cs", s + "Fases/PlataformaMovel.cs");
        Mover(Velho + "/Girar.cs", s + "Fases/Girar.cs");
        Mover(Velho + "/Moeda.cs", s + "Fases/Moeda.cs");
        Mover(Velho + "/Chegada.cs", s + "Fases/LinhaDeChegada.cs");
        Mover(Velho + "/MenuPrincipal.cs", s + "Interface/MenuPrincipal.cs");
        Mover(Velho + "/OpcoesUI.cs", s + "Interface/MenuOpcoes.cs");
        Mover(Velho + "/BotaoAnimado.cs", s + "Interface/BotaoAnimado.cs");
        Mover(Velho + "/Flutuar.cs", s + "Interface/Flutuar.cs");
        Mover(Velho + "/Confete.cs", s + "Interface/Confete.cs");
        Mover(Velho + "/Transicao.cs", s + "Interface/TransicaoCena.cs");
        Mover(Velho + "/CameraMenu.cs", s + "Interface/CameraMenu.cs");
        Mover(Velho + "/VoltarAoInicio.cs", s + "Legado/VoltarAoInicio.cs");

        // materiais, sons, texturas e fonte
        foreach (string guid in AssetDatabase.FindAssets("", new[] { Velho + "/Materiais" }))
        {
            string c = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(c)) Mover(c, Base + "/Materiais/" + Path.GetFileName(c));
        }
        foreach (var n in new[] { "Musica", "Moeda", "Checkpoint", "Morte", "Trampolim", "Vitoria" })
            Mover(Velho + "/Sons/" + n + ".wav", Base + "/Sons/" + n + ".wav");
        Mover(Velho + "/UI/Clique.wav", Base + "/Sons/Clique.wav");
        Mover(Velho + "/UI/Hover.wav", Base + "/Sons/Hover.wav");
        foreach (var n in new[] { "Vinheta", "DegradeLateral", "Estrela", "EstrelaVazia", "IconeMoeda", "IconeRelogio", "IconeMortes" })
            Mover(Velho + "/UI/" + n + ".png", Base + "/Texturas/" + n + ".png");
        Mover(Velho + "/UI/Poppins-Bold.ttf", Base + "/Fontes/Poppins-Bold.ttf");
        Mover(Velho + "/UI/Poppins-LICENCA.txt", Base + "/Fontes/Poppins-LICENCA.txt");

        // nomes novos das classes dentro dos scripts
        string[,] nomes = {
            { "HUD", "GerenciadorJogo" }, { "CameraSegue", "CameraJogador" }, { "Chegada", "LinhaDeChegada" },
            { "OpcoesUI", "MenuOpcoes" }, { "Transicao", "TransicaoCena" }
        };
        foreach (string arq in Directory.GetFiles(Base + "/Scripts", "*.cs", SearchOption.AllDirectories))
        {
            string texto = File.ReadAllText(arq), novo = texto;
            for (int i = 0; i < nomes.GetLength(0); i++)
                novo = Regex.Replace(novo, @"\b" + nomes[i, 0] + @"\b", nomes[i, 1]);
            if (novo != texto) File.WriteAllText(arq, novo);
        }

        // ordem das cenas no jogo (a SampleScene da equipe fica desligada)
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Base + "/Cenas/Menu.unity", true),
            new EditorBuildSettingsScene(Base + "/Cenas/Obby.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", false),
        };

        // sobras: som de pulo sem uso, LEIA-ME antigo e pastas vazias
        Apagar(Velho + "/Sons/Pulo.wav");
        Apagar(Velho + "/LEIA-ME.txt");
        foreach (var p in new[] { Velho + "/Sons", Velho + "/UI", Velho + "/Materiais" })
            if (AssetDatabase.FindAssets("", new[] { p }).Length == 0) Apagar(p);
        if (AssetDatabase.FindAssets("", new[] { Velho }).Length == 0) Apagar(Velho);

        Apagar("Assets/Editor/OrganizarProjeto.cs");
        if (AssetDatabase.FindAssets("", new[] { "Assets/Editor" }).Length == 0) Apagar("Assets/Editor");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Organizar] Pronto! Veja Assets/ParkourNasAlturas/LEIA-ME.md");
    }

    static void CriarPasta(string caminho)
    {
        if (AssetDatabase.IsValidFolder(caminho)) return;
        string pai = Path.GetDirectoryName(caminho).Replace('\\', '/');
        CriarPasta(pai);
        AssetDatabase.CreateFolder(pai, Path.GetFileName(caminho));
    }

    static void Mover(string de, string para)
    {
        if (AssetDatabase.LoadMainAssetAtPath(de) == null) return;
        string erro = AssetDatabase.MoveAsset(de, para);
        if (!string.IsNullOrEmpty(erro)) Debug.LogWarning("[Organizar] " + de + " -> " + para + ": " + erro);
    }

    static void Apagar(string caminho)
    {
        if (AssetDatabase.LoadMainAssetAtPath(caminho) != null || AssetDatabase.IsValidFolder(caminho))
            AssetDatabase.MoveAssetToTrash(caminho);
    }
}
#endif
