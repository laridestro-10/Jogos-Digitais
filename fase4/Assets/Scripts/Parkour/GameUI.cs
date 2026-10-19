using UnityEngine;
using UnityEngine.UI;

namespace Parkour
{
    /// <summary>
    /// Toda a interface do jogo, montada por código num único Canvas, dentro da mesma cena do percurso:
    /// Menu inicial, Como jogar, Configurações (volume / sensibilidade / tela cheia), HUD com botões,
    /// Pausa, Game Over e Vitória. Cada tela é só um painel que liga/desliga conforme o GameState.
    /// "Como jogar" e "Configurações" são sub-telas: abrem por cima do menu, da pausa ou do HUD.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        enum Sub { None, HowTo, Settings }

        static readonly Color Yellow = new Color(1f, 0.9f, 0.2f);

        GameManager gm;
        Sub sub;
        bool resumeAfterSub;     // sub-tela aberta pelo botão do HUD: ao voltar, o jogo continua

        GameObject hud, menu, pause, gameOver, victory, howTo, settings;
        Text progressText, timeText, livesText, toast;
        Text menuBest, overInfo, overTime, winTime, winBest;
        Text volumeLabel, sensitivityLabel, fullscreenLabel;
        Slider volumeSlider, sensitivitySlider;

        float toastTimer;
        int lastLives = -1, lastSecond = -1, lastProgress = -1;

        void Start()
        {
            gm = GameManager.Instance;
            Transform c = UiBuilder.CreateCanvas("Interface", 10).transform;

            // a ordem importa: o que é criado depois fica por cima
            hud = BuildHud(c);
            menu = BuildMenu(c);
            pause = BuildPause(c);
            gameOver = BuildGameOver(c);
            victory = BuildVictory(c);
            howTo = BuildHowTo(c);
            settings = BuildSettings(c);

            gm.StateChanged += OnStateChanged;
            gm.CheckpointReached += OnCheckpointReached;
            Refresh();
        }

        void OnDestroy()
        {
            if (gm == null) return;
            gm.StateChanged -= OnStateChanged;
            gm.CheckpointReached -= OnCheckpointReached;
        }

        void Update()
        {
            if (GameInput.PausePressed) OnEscape();
            if (hud.activeSelf) UpdateHud();
            UpdateToast();
        }

        // ====================================================================
        //  CONSTRUÇÃO DAS TELAS
        // ====================================================================

        /// <summary>Painel de tela cheia (fica desligado até o Refresh ligar).</summary>
        static GameObject NewScreen(Transform parent, string name, Color bg)
        {
            Image img = UiBuilder.FullPanel(parent, bg);
            img.gameObject.name = name;
            return img.gameObject;
        }

        GameObject BuildHud(Transform c)
        {
            // contêiner transparente que não bloqueia cliques (só os botões bloqueiam)
            var go = new GameObject("HUD", typeof(RectTransform));
            go.transform.SetParent(c, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            progressText = HudText(go.transform, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(30f, -25f));
            timeText = HudText(go.transform, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -25f));
            livesText = HudText(go.transform, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-30f, -25f));

            // botões dentro do jogo: configurações e como jogar (clique com o mouse solto)
            Vector2 corner = new Vector2(1f, 1f);
            Vector2 size = new Vector2(200f, 64f);
            UiBuilder.MakeButton(go.transform, "Config.", new Vector2(-30f, -105f), size,
                                 () => OpenFromHud(Sub.Settings), 30, corner);
            UiBuilder.MakeButton(go.transform, "Ajuda", new Vector2(-250f, -105f), size,
                                 () => OpenFromHud(Sub.HowTo), 30, corner);

            Text hint = UiBuilder.Label(go.transform,
                "WASD mover  |  Espaço pular  |  Botão direito: girar câmera  |  Scroll: zoom  |  R: checkpoint  |  Esc: pausa",
                24, TextAnchor.LowerLeft, new Vector2(30f, 20f), new Vector2(1700f, 40f),
                new Color(1f, 1f, 1f, 0.8f), new Vector2(0f, 0f));
            hint.gameObject.AddComponent<Outline>();

            // aviso grande ao alcançar um checkpoint
            toast = UiBuilder.Label(go.transform, "", 90, TextAnchor.MiddleCenter,
                                    new Vector2(0f, 300f), new Vector2(1200f, 140f), Yellow);
            toast.gameObject.AddComponent<Outline>();
            toast.gameObject.SetActive(false);
            return go;
        }

        static Text HudText(Transform parent, TextAnchor align, Vector2 anchor, Vector2 pos)
        {
            Text t = UiBuilder.Label(parent, "", 44, align, pos, new Vector2(600f, 70f), Color.white, anchor);
            t.gameObject.AddComponent<Outline>();   // contorno para ler em qualquer fundo
            return t;
        }

        GameObject BuildMenu(Transform c)
        {
            GameObject p = NewScreen(c, "Menu", new Color(0.08f, 0.05f, 0.15f, 0.72f));   // deixa o percurso aparecer atrás
            Transform t = p.transform;

            UiBuilder.Label(t, "PARKOUR", 150, TextAnchor.MiddleCenter,
                            new Vector2(0f, 330f), new Vector2(1400f, 200f), UiBuilder.Pink);
            UiBuilder.Label(t, "3 vidas  •  chegue à plataforma roxa do final!", 40, TextAnchor.MiddleCenter,
                            new Vector2(0f, 225f), new Vector2(1400f, 60f), Color.white);

            Vector2 size = new Vector2(580f, 95f);
            UiBuilder.MakeButton(t, "Jogar", new Vector2(0f, 110f), size, gm.StartGame);
            UiBuilder.MakeButton(t, "Como jogar", new Vector2(0f, -5f), size, () => OpenSub(Sub.HowTo));
            UiBuilder.MakeButton(t, "Configurações", new Vector2(0f, -120f), size, () => OpenSub(Sub.Settings));
            UiBuilder.MakeButton(t, "Sair", new Vector2(0f, -235f), size, gm.Quit);

            menuBest = UiBuilder.Label(t, "", 34, TextAnchor.MiddleCenter,
                                       new Vector2(0f, -340f), new Vector2(900f, 50f), Yellow);
            return p;
        }

        GameObject BuildPause(Transform c)
        {
            GameObject p = NewScreen(c, "Pausa", new Color(0.04f, 0.02f, 0.10f, 0.82f));
            Transform t = p.transform;

            UiBuilder.Label(t, "PAUSADO", 110, TextAnchor.MiddleCenter,
                            new Vector2(0f, 340f), new Vector2(1200f, 160f), UiBuilder.Pink);

            Vector2 size = new Vector2(620f, 95f);
            UiBuilder.MakeButton(t, "Continuar", new Vector2(0f, 190f), size, gm.Resume);
            UiBuilder.MakeButton(t, "Como jogar", new Vector2(0f, 75f), size, () => OpenSub(Sub.HowTo));
            UiBuilder.MakeButton(t, "Configurações", new Vector2(0f, -40f), size, () => OpenSub(Sub.Settings));
            UiBuilder.MakeButton(t, "Reiniciar jogo", new Vector2(0f, -155f), size, gm.StartGame);
            UiBuilder.MakeButton(t, "Menu principal", new Vector2(0f, -270f), size, gm.GoToMenu);
            return p;
        }

        GameObject BuildGameOver(Transform c)
        {
            GameObject p = NewScreen(c, "GameOver", new Color(0.15f, 0.02f, 0.05f, 0.88f));
            Transform t = p.transform;

            UiBuilder.Label(t, "GAME OVER", 150, TextAnchor.MiddleCenter,
                            new Vector2(0f, 280f), new Vector2(1400f, 200f), new Color(1f, 0.25f, 0.25f));
            overInfo = UiBuilder.Label(t, "", 50, TextAnchor.MiddleCenter,
                                       new Vector2(0f, 150f), new Vector2(1200f, 70f), Color.white);
            overTime = UiBuilder.Label(t, "", 40, TextAnchor.MiddleCenter,
                                       new Vector2(0f, 90f), new Vector2(1200f, 60f), Color.white);

            Vector2 size = new Vector2(620f, 110f);
            UiBuilder.MakeButton(t, "Tentar de novo", new Vector2(0f, -40f), size, gm.Retry);
            UiBuilder.MakeButton(t, "Menu principal", new Vector2(0f, -180f), size, gm.GoToMenu);
            return p;
        }

        GameObject BuildVictory(Transform c)
        {
            GameObject p = NewScreen(c, "Vitoria", new Color(0.03f, 0.12f, 0.10f, 0.85f));
            Transform t = p.transform;

            UiBuilder.Label(t, "VOCÊ VENCEU!", 150, TextAnchor.MiddleCenter,
                            new Vector2(0f, 280f), new Vector2(1500f, 200f), new Color(0.3f, 1f, 0.5f));
            winTime = UiBuilder.Label(t, "", 56, TextAnchor.MiddleCenter,
                                      new Vector2(0f, 150f), new Vector2(1200f, 80f), Color.white);
            winBest = UiBuilder.Label(t, "", 44, TextAnchor.MiddleCenter,
                                      new Vector2(0f, 80f), new Vector2(1200f, 60f), Yellow);

            Vector2 size = new Vector2(620f, 110f);
            UiBuilder.MakeButton(t, "Jogar de novo", new Vector2(0f, -50f), size, gm.StartGame);
            UiBuilder.MakeButton(t, "Menu principal", new Vector2(0f, -190f), size, gm.GoToMenu);
            return p;
        }

        GameObject BuildHowTo(Transform c)
        {
            GameObject p = NewScreen(c, "ComoJogar", new Color(0.08f, 0.05f, 0.15f, 0.97f));
            Transform t = p.transform;

            UiBuilder.Label(t, "COMO JOGAR", 110, TextAnchor.MiddleCenter,
                            new Vector2(0f, 400f), new Vector2(1400f, 160f), UiBuilder.Pink);

            const string body =
                "OBJETIVO\n" +
                "Siga o percurso da plataforma START até a plataforma roxa do final (o segundo checkpoint). Você tem 3 vidas: cada queda no vazio tira uma.\n\n" +
                "CONTROLES\n" +
                "W A S D ou setas: mover        Espaço: pular (segure para pular de novo)\n" +
                "Botão direito do mouse (segurar): girar a câmera        Scroll: zoom\n" +
                "R: voltar ao checkpoint sem perder vida        Esc ou P: pausa\n\n" +
                "DICAS\n" +
                "A primeira plataforma roxa é um checkpoint: se cair depois dela, você volta para ela.\n" +
                "Depois dela vem a curva de tábuas; a segunda plataforma roxa é o fim desta parte e mostra a tela de vitória.\n" +
                "Tentar de novo (Game Over) volta ao último checkpoint.";
            UiBuilder.Label(t, body, 36, TextAnchor.UpperLeft,
                            new Vector2(0f, 20f), new Vector2(1600f, 700f), Color.white);

            UiBuilder.MakeButton(t, "Voltar", new Vector2(0f, -400f), new Vector2(520f, 95f), Back);
            return p;
        }

        GameObject BuildSettings(Transform c)
        {
            GameObject p = NewScreen(c, "Configuracoes", new Color(0.08f, 0.05f, 0.15f, 0.97f));
            Transform t = p.transform;

            UiBuilder.Label(t, "CONFIGURAÇÕES", 110, TextAnchor.MiddleCenter,
                            new Vector2(0f, 340f), new Vector2(1400f, 160f), UiBuilder.Pink);

            // volume
            volumeLabel = UiBuilder.Label(t, "", 44, TextAnchor.MiddleCenter,
                                          new Vector2(0f, 190f), new Vector2(900f, 70f), Color.white);
            volumeSlider = UiBuilder.MakeSlider(t, new Vector2(0f, 120f), new Vector2(700f, 50f),
                                                0f, 1f, GameSettings.Volume, OnVolume);

            // sensibilidade da câmera (vale na hora, até durante a partida)
            sensitivityLabel = UiBuilder.Label(t, "", 44, TextAnchor.MiddleCenter,
                                               new Vector2(0f, 20f), new Vector2(900f, 70f), Color.white);
            sensitivitySlider = UiBuilder.MakeSlider(t, new Vector2(0f, -50f), new Vector2(700f, 50f),
                                                     0.5f, 6f, GameSettings.Sensitivity, OnSensitivity);

            // tela cheia
            Button full = UiBuilder.MakeButton(t, "", new Vector2(0f, -170f), new Vector2(620f, 100f), ToggleFullscreen);
            fullscreenLabel = full.GetComponentInChildren<Text>();

            UiBuilder.MakeButton(t, "Voltar", new Vector2(0f, -310f), new Vector2(620f, 100f), Back);
            SyncSettings();
            return p;
        }

        // ====================================================================
        //  NAVEGAÇÃO ENTRE TELAS
        // ====================================================================

        void OnStateChanged(GameState state) => Refresh();

        /// <summary>Liga só os painéis que valem para o estado atual.</summary>
        void Refresh()
        {
            GameState s = gm.State;
            hud.SetActive(s == GameState.Playing || s == GameState.Paused);
            menu.SetActive(s == GameState.Menu && sub == Sub.None);
            pause.SetActive(s == GameState.Paused && sub == Sub.None);
            gameOver.SetActive(s == GameState.GameOver);
            victory.SetActive(s == GameState.Victory);
            howTo.SetActive(sub == Sub.HowTo);
            settings.SetActive(sub == Sub.Settings);

            if (s == GameState.Menu)
            {
                float best = GameManager.BestTime;
                menuBest.text = best > 0f ? "Melhor tempo: " + GameManager.FormatTime(best) : "Ainda sem recorde";
            }
            else if (s == GameState.GameOver)
            {
                overInfo.text = $"Acabaram as vidas  •  Checkpoint {gm.CheckpointsReached}/{gm.checkpointTotal}";
                overTime.text = "Tempo: " + GameManager.FormatTime(gm.Elapsed);
            }
            else if (s == GameState.Victory)
            {
                winTime.text = "Tempo total: " + GameManager.FormatTime(gm.Elapsed);
                winBest.text = "Melhor tempo: " + GameManager.FormatTime(GameManager.BestTime);
            }
        }

        void OpenSub(Sub s)
        {
            sub = s;
            if (s == Sub.Settings) SyncSettings();
            Refresh();
        }

        /// <summary>Botões Config./Ajuda do HUD: pausa o jogo e abre a sub-tela; ao voltar, o jogo continua.</summary>
        void OpenFromHud(Sub s)
        {
            if (gm.State != GameState.Playing) return;
            resumeAfterSub = true;
            sub = s;
            if (s == Sub.Settings) SyncSettings();
            gm.Pause();     // dispara o Refresh
        }

        void Back()
        {
            GameSettings.Save();
            sub = Sub.None;
            if (resumeAfterSub)
            {
                resumeAfterSub = false;
                gm.Resume();    // dispara o Refresh
            }
            else Refresh();
        }

        void OnEscape()
        {
            if (sub != Sub.None) Back();
            else if (gm.State == GameState.Playing) gm.Pause();
            else if (gm.State == GameState.Paused) gm.Resume();
        }

        // ====================================================================
        //  HUD E AVISO DE FASE
        // ====================================================================

        /// <summary>Só reescreve os textos quando o valor muda (evita criar strings novas todo frame).</summary>
        void UpdateHud()
        {
            if (gm.Lives != lastLives)
            {
                lastLives = gm.Lives;
                livesText.text = "Vidas: " + lastLives;
                livesText.color = lastLives <= 1 ? new Color(1f, 0.35f, 0.35f) : Color.white;
            }

            int sec = (int)gm.Elapsed;
            if (sec != lastSecond)
            {
                lastSecond = sec;
                timeText.text = GameManager.FormatTime(gm.Elapsed);
            }

            if (gm.CheckpointsReached != lastProgress)
            {
                lastProgress = gm.CheckpointsReached;
                progressText.text = $"Checkpoint {lastProgress}/{gm.checkpointTotal}";
            }
        }

        void OnCheckpointReached(int number)
        {
            toast.text = $"CHECKPOINT {number}/{gm.checkpointTotal}";
            toast.color = Yellow;
            toast.gameObject.SetActive(true);
            toastTimer = 2.5f;
        }

        void UpdateToast()
        {
            if (toastTimer <= 0f) return;
            toastTimer -= Time.unscaledDeltaTime;
            if (toastTimer <= 0f)
            {
                toast.gameObject.SetActive(false);
                return;
            }
            Color col = Yellow;
            col.a = Mathf.Clamp01(toastTimer / 0.8f);   // some devagar no final
            toast.color = col;
        }

        // ====================================================================
        //  CONFIGURAÇÕES
        // ====================================================================

        void SyncSettings()
        {
            volumeSlider.SetValueWithoutNotify(GameSettings.Volume);
            sensitivitySlider.SetValueWithoutNotify(GameSettings.Sensitivity);
            volumeLabel.text = VolumeText(GameSettings.Volume);
            sensitivityLabel.text = SensitivityText(GameSettings.Sensitivity);
            fullscreenLabel.text = FullscreenText(UnityEngine.Screen.fullScreen);
        }

        static string VolumeText(float v) => "Volume: " + Mathf.RoundToInt(v * 100f) + "%";
        static string SensitivityText(float v) => "Sensibilidade da câmera: " + v.ToString("0.0");
        static string FullscreenText(bool on) => "Tela cheia: " + (on ? "Sim" : "Não");

        void OnVolume(float v)
        {
            GameSettings.Volume = v;
            volumeLabel.text = VolumeText(v);
        }

        void OnSensitivity(float v)
        {
            GameSettings.Sensitivity = v;
            sensitivityLabel.text = SensitivityText(v);
        }

        void ToggleFullscreen()
        {
            bool next = !UnityEngine.Screen.fullScreen;
            UnityEngine.Screen.fullScreen = next;
            fullscreenLabel.text = FullscreenText(next);
        }
    }
}
