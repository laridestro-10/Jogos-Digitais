using System;
using UnityEngine;

namespace Parkour
{
    /// <summary>Em que momento o jogo está (tudo acontece em UMA cena só).</summary>
    public enum GameState { Menu, Playing, Paused, GameOver, Victory }

    /// <summary>
    /// Cérebro do jogo: estado, vidas, tempo, checkpoints e reinício.
    /// Só existe um percurso, dentro da mesma cena. O que muda é só o estado:
    /// Menu -> Playing -> (Paused) -> GameOver ou Victory. A interface (GameUI) escuta os eventos.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class GameManager : MonoBehaviour
    {
        public const int MaxLives = 3;
        const string BestTimeKey = "pk_best_time";

        [Tooltip("Onde o jogador começa (a plataforma START). Preenchido pelo gerador.")]
        public Transform startPoint;
        [Tooltip("Quantos checkpoints existem no percurso (aparece no HUD). Preenchido pelo gerador.")]
        public int checkpointTotal = 6;

        static GameManager instance;

        /// <summary>Acesso global. Se ainda não existir, procura na cena (ou cria).</summary>
        public static GameManager Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<GameManager>();
                if (instance == null) instance = new GameObject("GameManager").AddComponent<GameManager>();
                return instance;
            }
        }

        public GameState State { get; private set; } = GameState.Menu;
        public bool Playing => State == GameState.Playing;
        public int Lives { get; private set; } = MaxLives;
        public int CheckpointsReached { get; private set; }
        public float Elapsed { get; private set; }

        /// <summary>Melhor tempo salvo (0 = ainda não zerou o jogo).</summary>
        public static float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0f);

        /// <summary>Disparado quando o estado muda (a interface troca de painel).</summary>
        public event Action<GameState> StateChanged;
        /// <summary>Disparado ao alcançar um checkpoint novo (a interface mostra o aviso).</summary>
        public event Action<int> CheckpointReached;

        PlayerRespawn player;
        ThirdPersonCamera cam;

        // com "Enter Play Mode sem recarregar domínio" o static sobraria da vez anterior
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => instance = null;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            Time.timeScale = 1f;
            GameSettings.Apply();
            FreeCursor();
        }

        void Start()
        {
            player = FindFirstObjectByType<PlayerRespawn>();
            cam = FindFirstObjectByType<ThirdPersonCamera>();
            ResetRun();     // jogador na largada, caixas no lugar
        }

        void Update()
        {
            if (State == GameState.Playing) Elapsed += Time.deltaTime;
        }

        // ---------------- estado ----------------

        void SetState(GameState next)
        {
            if (State == next) return;
            State = next;
            Time.timeScale = next == GameState.Paused ? 0f : 1f;
            if (next != GameState.Playing) FreeCursor();   // ao jogar, a câmera cuida do cursor
            StateChanged?.Invoke(next);
        }

        static void FreeCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // ---------------- fluxo do jogo ----------------

        /// <summary>Começa um jogo novo do zero, na largada (Jogar / Reiniciar / Jogar de novo).</summary>
        public void StartGame()
        {
            ResetRun();
            SetState(GameState.Playing);
        }

        public void Pause()
        {
            if (State == GameState.Playing) SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (State == GameState.Paused) SetState(GameState.Playing);
        }

        /// <summary>Depois do Game Over: volta a jogar do último checkpoint, com todas as vidas.</summary>
        public void Retry()
        {
            Lives = MaxLives;
            SetState(GameState.Playing);
        }

        public void GoToMenu()
        {
            ResetRun();
            SetState(GameState.Menu);
        }

        /// <summary>Perde uma vida. Devolve false se acabaram (e abre o Game Over).</summary>
        public bool LoseLife()
        {
            Lives = Mathf.Max(0, Lives - 1);
            if (Lives > 0) return true;

            if (player) player.Respawn();   // o fundo do Game Over mostra o percurso, não o vazio
            SetState(GameState.GameOver);
            return false;
        }

        /// <summary>Chamado pelos checkpoints (1 = primeiro). Só conta se for um novo.</summary>
        public void ReachCheckpoint(int number)
        {
            if (number <= CheckpointsReached) return;
            CheckpointsReached = number;
            CheckpointReached?.Invoke(number);
        }

        /// <summary>Chamado pelo portal GOAL.</summary>
        public void Win()
        {
            if (State != GameState.Playing) return;
            float best = BestTime;
            if (best <= 0f || Elapsed < best)
            {
                PlayerPrefs.SetFloat(BestTimeKey, Elapsed);
                PlayerPrefs.Save();
            }
            SetState(GameState.Victory);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------------- reinício do mundo ----------------

        /// <summary>Zera vidas, tempo e checkpoints; coloca o jogador na largada e arruma as caixas.</summary>
        void ResetRun()
        {
            Lives = MaxLives;
            Elapsed = 0f;
            CheckpointsReached = 0;

            for (int i = PhysicsCrate.All.Count - 1; i >= 0; i--) PhysicsCrate.All[i].ResetToStart();

            if (player && startPoint) player.ResetTo(startPoint);
            if (cam) cam.ResetView(0f);
        }

        /// <summary>Formata segundos como mm:ss.</summary>
        public static string FormatTime(float seconds)
        {
            int s = Mathf.Max(0, (int)seconds);
            return $"{s / 60:00}:{s % 60:00}";
        }
    }
}
