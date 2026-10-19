using UnityEngine;

namespace Parkour
{
    /// <summary>
    /// Configurações do jogador salvas em PlayerPrefs (sobrevivem ao fechar o jogo).
    /// Os valores ficam em cache: a câmera lê a sensibilidade todo frame sem tocar no PlayerPrefs.
    /// </summary>
    public static class GameSettings
    {
        const string VolumeKey = "pk_volume";
        const string SensitivityKey = "pk_sensitivity";

        static float volume = -1f;        // -1 = ainda não lido
        static float sensitivity = -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { volume = -1f; sensitivity = -1f; }

        /// <summary>Volume geral, de 0 a 1.</summary>
        public static float Volume
        {
            get
            {
                if (volume < 0f) volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
                return volume;
            }
            set
            {
                volume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(VolumeKey, volume);
                AudioListener.volume = volume;
            }
        }

        /// <summary>Sensibilidade do mouse na câmera (vale na hora, até durante o jogo).</summary>
        public static float Sensitivity
        {
            get
            {
                if (sensitivity < 0f) sensitivity = PlayerPrefs.GetFloat(SensitivityKey, 2f);
                return sensitivity;
            }
            set
            {
                sensitivity = Mathf.Clamp(value, 0.5f, 6f);
                PlayerPrefs.SetFloat(SensitivityKey, sensitivity);
            }
        }

        public static void Apply() => AudioListener.volume = Volume;

        public static void Save() => PlayerPrefs.Save();
    }
}
