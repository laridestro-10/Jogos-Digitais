# Parkour nas Alturas

Obby (parkour no estilo Roblox) em 3D feito em Unity para a disciplina de Jogos Digitais, Fase 2.
O jogador sobe 7 fases baseadas no mapa desenhado pelo grupo (zonas 4 a 1), pegando moedas e
evitando lava, ate chegar na plataforma final.

---

## 1. Como abrir e jogar

| Item | Valor |
| --- | --- |
| Unity | 6000.6 (projeto salvo na 6000.6.5f1; funciona na 6000.6.3f1) |
| Pipeline | URP (Universal Render Pipeline), preset "PC" |
| Pasta do projeto | `Jogos-Digitais/fase 2` |
| Cena inicial | `Assets/ParkourNasAlturas/Cenas/Menu.unity` |
| Input | "Active Input Handling" = **Both** (Project Settings > Player) |

1. Abra a pasta `fase 2` no Unity Hub.
2. Abra a cena **Menu** e aperte Play.
3. Para gerar o executavel: File > Build Profiles > Build (as cenas Menu e Obby ja estao na lista).

### Controles

| Tecla | Acao |
| --- | --- |
| W A S D / setas | Andar (na direcao da camera) |
| Espaco | Pular |
| Botao direito do mouse + arrastar | Girar a camera |
| Q / E | Girar a camera pelo teclado |
| Rodinha do mouse | Zoom |
| Esc | Pausar / fechar janelas |

---

## 2. Estrutura de pastas

```
Assets/
  ParkourNasAlturas/          <- tudo do jogo
    Cenas/
      Menu.unity              tela inicial (cenario 3D girando + botoes)
      Obby.unity              o jogo (7 fases)
    Scripts/
      Jogo/                   GerenciadorJogo, Configuracoes
      Jogador/                Personagem, AnimacaoBoneco, CameraJogador
      Fases/                  Checkpoint, Perigo, Trampolim, PlataformaMovel, Girar, Moeda, LinhaDeChegada
      Interface/              MenuPrincipal, MenuOpcoes, BotaoAnimado, Flutuar, Confete, TransicaoCena, CameraMenu
      Legado/                 VoltarAoInicio (so a SampleScene usa)
    Materiais/                cores das fases (Obby_*), lava, vidro, ceu, agua...
    Sons/                     musica e efeitos (.wav)
    Texturas/                 icones, estrelas, vinheta, degrade
    Fontes/                   Poppins-Bold + licenca
    LEIA-ME.md                este arquivo
  Scenes/SampleScene.unity    cena original da equipe (fora do jogo)
  TorusGenerator.cs           script da equipe que gera os aneis (usado na fase 5)
  PlayerController.cs         controle original da equipe (nao usado no jogo final)
  Settings/                   configuracoes do URP (qualidade, pos-processamento)
```

---

## 3. Fluxo do jogo

```
Menu --JOGAR--> Obby --chega no fim--> Vitoria --JOGAR DE NOVO--> Obby
  |                |                       \--MENU--> Menu
  |                \--Esc--> Pausa (Continuar / Opcoes / Reiniciar / Menu)
  \--COMO JOGAR / OPCOES / SAIR
```

Toda troca de cena passa pelo `TransicaoCena` (fade preto).

---

## 4. As 7 fases (cena Obby, objeto "Obby")

| Fase | Zona do desenho | Desafio | Objetos / scripts |
| --- | --- | --- | --- |
| 1 Blocos | 4 | Blocos azuis sobre lava | `Lava` com `Perigo` |
| 2 A curva | 4 | Tabuas amarelas em curva, subindo | so plataformas |
| 3 Bolinhas | 3 | Discos rosa em zigue-zague | so plataformas |
| 4 Barra giratoria | 3 | Postes, disco com barra que gira, trampolim | `Girar`, `Perigo`, `Trampolim` |
| 5 Aneis | 2 | Aneis ciano | `TorusGenerator` (raio 0,9 / tubo 0,45) |
| 6 Plataformas moveis | 2 | Barras em U, metade se mexe | `PlataformaMovel` |
| 7 Ponte de vidro | 1 | 7 colunas x 3 vidros, so 1 verdadeiro por coluna | vidro falso = BoxCollider "Is Trigger" |

- Cada fase comeca num **Checkpoint** (plataforma roxa com quadrado verde neon). Ao pisar, o quadrado fica dourado.
- A **Chegada** (plataforma laranja com bandeiras) chama a vitoria.
- Resposta da ponte de vidro (para o grupo, NAO aparece no jogo): linhas 3 2 3 3 2 1 1, da direita para a esquerda
  (linha 1 = mais perto da camera inicial). Os cubinhos brancos no comeco marcam o numero de cada linha.
- Se cair na agua (altura < -1) ou encostar em lava/barra: volta ao ultimo checkpoint e conta 1 morte.

### Pontuacao
- 16 moedas espalhadas.
- Estrelas (0 a 3): 1 por terminar, +1 por pegar 75% das moedas, +1 por morrer 3 vezes ou menos.
- Recorde de tempo e melhor numero de estrelas salvos com `PlayerPrefs` (chaves `recorde` e `estrelas`).

---

## 5. Scripts

### Jogo
**GerenciadorJogo** (no objeto `HUD` da cena Obby) - o "cerebro" da partida.
- Acesso de qualquer script: `GerenciadorJogo.instancia`.
- Controla: moedas, tempo, fase atual, mortes, ponto de volta (checkpoint), sons, pausa, opcoes e vitoria.
- Metodos usados por outros scripts: `PegouMoeda()`, `Checkpoint(fase, posicao, mensagem)`, `Morrer()`,
  `Vencer()`, `MostrarAviso(texto)`, `TocarSom(clip)`.
- Metodos dos botoes: `Continuar()`, `AbrirOpcoes()`, `JogarDeNovo()`, `IrParaMenu()`.
- Campos no Inspector: textos do HUD, barra de fases (`segmentos`), paineis, estrelas, `player`,
  `totalFases` (7), `alturaMorte` (-1) e os sons.

**Configuracoes** (classe estatica) - opcoes salvas no computador.
- `Musica` (0-1, padrao 0,7), `Efeitos` (0-1, padrao 0,8), `Sensibilidade` (0,3-2, padrao 1), `TelaCheia`.

### Jogador
**Personagem** (no `Player`) - movimento com `CharacterController`.
- Anda na direcao da camera, pula, gravidade, "ajuda no pulo" (0,15 s depois de sair da beirada).
- Vira o filho `Modelo` para onde esta andando. `Impulso(altura)` e usado pelo trampolim.
- Valores: velocidade 7, alturaPulo 2,2, gravidade -25.

**AnimacaoBoneco** (no `Player/Modelo`) - balanca bracos e pernas do boneco de blocos quando ele anda.

**CameraJogador** (na `Main Camera` da Obby) - camera em terceira pessoa.
- Botao direito / Q / E giram, rodinha da zoom (4 a 16), multiplicada pela sensibilidade das Opcoes.
- Faz um SphereCast para nao atravessar plataformas.

### Fases
| Script | O que faz |
| --- | --- |
| Checkpoint | Trigger: ao entrar o Player, avisa o gerenciador (`fase`, `mensagem`) e pinta a `marca` |
| Perigo | Trigger: ao entrar o Player, chama `Morrer()` (lava e barra giratoria) |
| Trampolim | Trigger: chama `Personagem.Impulso(alturaDoPulo)` (5 m) |
| PlataformaMovel | Vai e volta (`deslocamento`, `velocidade`) com `Mathf.PingPong`; o Player vira filho dela enquanto esta em cima |
| Girar | Gira o objeto sem parar (`velocidade` em graus/s por eixo) |
| Moeda | Gira; ao encostar, `PegouMoeda()` e some. Cada moeda se soma ao total no `Start` |
| LinhaDeChegada | Trigger no fim do mapa: chama `Vencer()` |
| TorusGenerator (equipe) | Gera a malha e o colisor do anel |

### Interface
| Script | O que faz |
| --- | --- |
| MenuPrincipal | Botoes do menu (Jogar, Como jogar, Opcoes, Sair), recorde, estrelas e musica |
| MenuOpcoes | Tela de opcoes (+/- de musica, efeitos, sensibilidade; tela cheia) |
| BotaoAnimado | Botao cresce no hover, afunda no clique e toca som |
| Flutuar | Animacao de flutuar/balancar/pulsar (logo, selo de recorde) |
| Confete | Chuva de confete na tela de vitoria |
| TransicaoCena | Fade preto; `TransicaoCena.IrPara("NomeDaCena")` |
| CameraMenu | Camera do menu girando em volta do cenario |

Todas as animacoes de interface usam `Time.unscaledDeltaTime`, entao funcionam com o jogo pausado (`timeScale = 0`).

---

## 6. Configuracoes do projeto que foram alteradas
| Onde | O que | Por que |
| --- | --- | --- |
| Project Settings > Player | Active Input Handling = Both | o codigo usa `Input.GetAxis` |
| Settings/PC_RPAsset | MSAA 4x, sombra 4096, distancia de sombra 70 | qualidade alta |
| Project Settings > Quality (PC) | V-Sync ligado | sem "rasgar" a imagem |
| Settings/SampleSceneProfile | Color Adjustments (+saturacao, contraste), bloom 0,35 | cores mais vivas |
| Build Profiles | Menu, Obby (SampleScene desligada) | ordem do jogo |

---

## 7. Como mexer no jogo
- **Trocar o personagem:** dentro de `Player`, apague o filho `Modelo`, coloque o modelo novo como filho
  (pes em Y = -1) e arraste-o para o campo `Modelo` do `Personagem`. Se for de outro tamanho, ajuste o
  `CharacterController` (Height/Radius). Pode remover o `AnimacaoBoneco` se o modelo tiver animacoes proprias.
- **Mudar uma fase:** tudo fica no objeto `Obby`, um grupo por fase. Mova/duplique (Ctrl+D) as plataformas.
- **Novo checkpoint:** duplique um "Checkpoint N", troque `fase` e `mensagem` no filho `Ponto` e aumente
  `totalFases` no `GerenciadorJogo` (e um `Segmento` na barra do HUD).
- **Nova moeda:** duplique uma `Moeda` (o total conta sozinho).
- **Cores:** arraste outro material de `Materiais/` na peca (`Tampa` = parte de cima, `Base` = parte de baixo).
- **Volume padrao / sensibilidade:** valores padrao em `Configuracoes.cs`.

---

## 8. Creditos e assets
| Asset | Origem | Licenca |
| --- | --- | --- |
| Fonte Poppins Bold | The Poppins Project Authors (github.com/itfoundry/Poppins) | SIL Open Font License 1.1 |
| Musica e efeitos sonoros | Gerados por IA (Claude) com sintese em Python | originais do projeto |
| Icones, estrelas, vinheta | Gerados por IA (Claude) com Python/Pillow | originais do projeto |
| Modelos 3D | Formas basicas do Unity (cubo, cilindro, esfera) | Unity |
| TorusGenerator, PlayerController, mapa das zonas | Grupo | do grupo |
| Unity 6, URP, UGUI | Unity Technologies | licenca Unity |

Desenvolvido com auxilio da IA Claude (Anthropic). O registro do uso da IA esta no documento do grupo.

---

## 9. Problemas conhecidos / pendencias
- A `SampleScene` (original da equipe) da erro se rodar: ela usa uma versao antiga do HUD. Ela nao faz parte do jogo.
- O repositorio nao tem `.gitignore`: adicione o de Unity antes de commitar (senao a pasta `Library` vai junto).
- O projeto foi aberto numa versao mais antiga do Unity (6000.6.3) do que a salva (6000.6.5).
