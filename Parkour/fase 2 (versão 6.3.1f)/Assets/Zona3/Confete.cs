using UnityEngine;
using UnityEngine.UI;

// Chuva de confete na interface (usada na tela de vitoria).
public class Confete : MonoBehaviour
{
    public int quantidade = 90;
    public float duracao = 4f;

    private RectTransform[] pecas;
    private Vector2[] velocidades;
    private float[] giros;
    private float tempo;

    static readonly Color[] cores = {
        new Color(1f, 0.35f, 0.6f), new Color(0.3f, 0.75f, 1f), new Color(1f, 0.85f, 0.2f),
        new Color(0.4f, 0.9f, 0.4f), new Color(0.7f, 0.45f, 1f), new Color(1f, 0.55f, 0.15f)
    };

    void OnEnable()
    {
        RectTransform area = (RectTransform)transform;
        if (pecas == null)
        {
            pecas = new RectTransform[quantidade];
            velocidades = new Vector2[quantidade];
            giros = new float[quantidade];
            for (int i = 0; i < quantidade; i++)
            {
                GameObject g = new GameObject("Confete", typeof(RectTransform), typeof(Image));
                g.transform.SetParent(transform, false);
                Image img = g.GetComponent<Image>();
                img.color = cores[i % cores.Length];
                img.raycastTarget = false;
                pecas[i] = (RectTransform)g.transform;
                pecas[i].sizeDelta = new Vector2(Random.Range(12f, 22f), Random.Range(20f, 34f));
            }
        }
        float w = area.rect.width, h = area.rect.height;
        for (int i = 0; i < quantidade; i++)
        {
            pecas[i].gameObject.SetActive(true);
            pecas[i].anchoredPosition = new Vector2(Random.Range(-w / 2, w / 2), h / 2 + Random.Range(20f, 500f));
            velocidades[i] = new Vector2(Random.Range(-60f, 60f), -Random.Range(220f, 420f));
            giros[i] = Random.Range(-360f, 360f);
        }
        tempo = 0f;
    }

    void Update()
    {
        if (pecas == null) return;
        tempo += Time.unscaledDeltaTime;
        float dt = Time.unscaledDeltaTime;
        float h = ((RectTransform)transform).rect.height;
        for (int i = 0; i < pecas.Length; i++)
        {
            RectTransform p = pecas[i];
            if (!p.gameObject.activeSelf) continue;
            Vector2 v = velocidades[i];
            v.x += Mathf.Sin(tempo * 3f + i) * 40f * dt;
            p.anchoredPosition += v * dt;
            p.Rotate(0, 0, giros[i] * dt);
            if (p.anchoredPosition.y < -h / 2 - 60f)
            {
                if (tempo < duracao) p.anchoredPosition = new Vector2(p.anchoredPosition.x, h / 2 + 40f);
                else p.gameObject.SetActive(false);
            }
        }
    }
}
