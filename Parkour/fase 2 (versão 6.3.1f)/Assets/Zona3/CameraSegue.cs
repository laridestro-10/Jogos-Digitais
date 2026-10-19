using UnityEngine;

// Camera em terceira pessoa (estilo Roblox):
// botao direito do mouse + arrastar = girar, rodinha = zoom, Q/E tambem giram.
// Se tiver uma plataforma entre a camera e o Player, a camera chega mais perto.
public class CameraSegue : MonoBehaviour
{
    public Transform alvo;                 // o Player
    public float distancia = 9f;
    public float sensibilidade = 3f;
    public float anguloHorizontal = 90f;
    public float anguloVertical = 25f;
    public float suavidade = 12f;

    private float distanciaAtual;

    void Start()
    {
        distanciaAtual = distancia;
    }

    void LateUpdate()
    {
        if (alvo == null) return;
        float sens = sensibilidade * Configuracoes.Sensibilidade;

        if (Input.GetMouseButton(1))
        {
            anguloHorizontal += Input.GetAxis("Mouse X") * sens;
            anguloVertical -= Input.GetAxis("Mouse Y") * sens;
        }
        if (Input.GetKey(KeyCode.Q)) anguloHorizontal -= 90f * Configuracoes.Sensibilidade * Time.unscaledDeltaTime;
        if (Input.GetKey(KeyCode.E)) anguloHorizontal += 90f * Configuracoes.Sensibilidade * Time.unscaledDeltaTime;
        anguloVertical = Mathf.Clamp(anguloVertical, 5f, 75f);

        distancia -= Input.GetAxis("Mouse ScrollWheel") * 8f;
        distancia = Mathf.Clamp(distancia, 4f, 16f);

        Quaternion rotacao = Quaternion.Euler(anguloVertical, anguloHorizontal, 0);
        Vector3 foco = alvo.position + Vector3.up * 1f;
        Vector3 direcao = -(rotacao * Vector3.forward);

        // nao deixa a camera entrar dentro das plataformas
        float alvoDist = distancia;
        RaycastHit[] hits = Physics.SphereCastAll(foco, 0.3f, direcao, distancia, ~0, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit h in hits)
        {
            if (h.collider.transform == alvo || h.collider.transform.IsChildOf(alvo)) continue;
            if (h.distance > 0f && h.distance - 0.2f < alvoDist) alvoDist = Mathf.Max(1.5f, h.distance - 0.2f);
        }
        // chega perto rapido, volta para longe devagar
        float vel = alvoDist < distanciaAtual ? 30f : suavidade * 0.4f;
        distanciaAtual = Mathf.Lerp(distanciaAtual, alvoDist, 1f - Mathf.Exp(-vel * Time.unscaledDeltaTime));

        transform.position = foco + direcao * distanciaAtual;
        transform.LookAt(foco);
    }
}
