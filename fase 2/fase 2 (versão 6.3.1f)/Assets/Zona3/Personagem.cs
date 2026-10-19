using UnityEngine;

// Controla o personagem: andar, pular e virar para o lado que esta andando.
// Ele anda para onde a camera esta olhando (W = para frente da camera).
//
// COMO TROCAR O VISUAL DO PERSONAGEM:
// 1. Dentro do objeto "Player", apague o filho "Modelo".
// 2. Arraste o modelo novo para dentro do "Player" (como filho).
// 3. Arraste esse modelo para o campo "Modelo" deste script no Inspector.
[RequireComponent(typeof(CharacterController))]
public class Personagem : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 7f;
    public float alturaPulo = 2.2f;
    public float gravidade = -25f;
    public float ajudaNoPulo = 0.15f;   // segundos de "folga" para apertar o pulo

    [Header("Visual")]
    public Transform modelo;            // filho com o desenho do personagem
    public Transform cameraJogo;        // a camera, para andar na direcao dela
    public float velocidadeGiro = 12f;

    [Header("Sons")]
    public AudioClip somTrampolim;

    private CharacterController controller;
    private AudioSource som;
    private float velocidadeY;          // velocidade para cima/baixo
    private float tempoFora = 0f;       // tempo desde que saiu do chao
    private float tempoBotao = 99f;     // tempo desde que apertou pular

    void Start()
    {
        controller = GetComponent<CharacterController>();
        som = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // 1. Ler o teclado (WASD ou setas)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // frente e direita da camera, sem inclinar para cima/baixo
        Vector3 frente = Vector3.forward;
        Vector3 direita = Vector3.right;
        if (cameraJogo != null)
        {
            frente = cameraJogo.forward;
            frente.y = 0;
            frente.Normalize();
            direita = cameraJogo.right;
            direita.y = 0;
            direita.Normalize();
        }
        Vector3 direcao = frente * z + direita * x;
        if (direcao.magnitude > 1)
        {
            direcao.Normalize();   // andar na diagonal nao fica mais rapido
        }

        // 2. Pulo (com uma pequena ajuda: da para pular logo depois de sair da beirada)
        if (controller.isGrounded) tempoFora = 0;
        else tempoFora = tempoFora + Time.deltaTime;

        if (Input.GetButtonDown("Jump")) tempoBotao = 0;
        else tempoBotao = tempoBotao + Time.deltaTime;

        if (tempoFora < ajudaNoPulo && tempoBotao < ajudaNoPulo)
        {
            velocidadeY = Mathf.Sqrt(alturaPulo * -2f * gravidade);
            tempoFora = 99f;
            tempoBotao = 99f;
        }

        // 3. Gravidade
        if (controller.isGrounded && velocidadeY < 0)
        {
            velocidadeY = -2f;     // mantem o personagem colado no chao
        }
        velocidadeY = velocidadeY + gravidade * Time.deltaTime;

        // 4. Mover (um Move so por frame)
        Vector3 movimento = direcao * velocidade;
        movimento.y = velocidadeY;
        controller.Move(movimento * Time.deltaTime);

        // 5. Virar o modelo para onde esta andando
        if (modelo != null && direcao.magnitude > 0.1f)
        {
            Quaternion alvo = Quaternion.LookRotation(direcao);
            modelo.rotation = Quaternion.Slerp(modelo.rotation, alvo, velocidadeGiro * Time.deltaTime);
        }
    }

    // Usado pelo Trampolim: joga o personagem para cima
    public void Impulso(float altura)
    {
        velocidadeY = Mathf.Sqrt(altura * -2f * gravidade);
        if (somTrampolim != null) som.PlayOneShot(somTrampolim, Configuracoes.Efeitos);
    }
}
