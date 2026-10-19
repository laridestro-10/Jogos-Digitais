# Parkour 3D

Jogo de parkour em terceira pessoa, no estilo dos obbies do Roblox, feito na **Unity 6** (template Universal 3D / URP). O jogador sai da plataforma **START**, atravessa um percurso de plataformas arredondadas e chega ao portal **GOAL**, sem perder as 3 vidas.

Tudo acontece em **uma única cena** (`Scenes/Parkour/Parkour.unity`): menu, jogo, pausa, game over e vitória são telas que ligam e desligam, sem troca de cena.

- **Equipe:** André, Larissa, Murilo e Valeska
- **Disciplina / professor:** Jogos Digitais
- **IA utilizada no desenvolvimento:** Claude (Anthropic). O registro dos prompts está no documento de análise de prompts entregue junto.

---

## Como jogar

**Objetivo:** siga o percurso da plataforma START até o portal GOAL. Você tem **3 vidas**: cada queda no vazio tira uma. O tempo corre durante a partida e o **melhor tempo** fica salvo.

### Controles

| Ação | Teclado e mouse | Controle |
|---|---|---|
| Andar | W A S D ou setas | Analógico esquerdo |
| Pular (segure para pular de novo ao pisar no chão) | Espaço | Botão A |
| Girar a câmera | Segurar o botão direito do mouse e mover | Analógico direito |
| Zoom da câmera | Scroll do mouse | — |
| Voltar ao checkpoint (sem perder vida) | R | Botão Y |
| Pausa | Esc ou P | Start |

Durante o jogo, os botões **Config.** e **Ajuda** (canto superior direito) pausam o jogo e abrem as configurações e a tela Como jogar. Ao voltar, o jogo continua.

### Dicas

- As plataformas **roxas** são checkpoints: se você cair, volta para a última que tocou.
- **Tentar de novo** (Game Over) volta ao último checkpoint, com as 3 vidas.
- Na escadinha final, a placa mostra o **código** de qual bloco pisar em cada coluna. Os blocos errados tremem e caem.

---

## O percurso

O percurso é contínuo e tem **6 checkpoints** antes do portal. O HUD mostra o progresso como *Checkpoint n/6*.

| Ordem | Trecho | O que tem |
|---|---|---|
| 1 | Largada | Plataforma START, 6 caixas com física para empurrar e 7 blocos arredondados espaçados, até o checkpoint 1 |
| 2 | Curva de tábuas | Pedra de ligação, 5 tábuas em arco de 90 graus e 8 tábuas retas, até o checkpoint 2 |
| 3 | Bolinhas | Discos em zigue-zague, em duas fileiras, até o checkpoint 3 |
| 4 | Pilares | Pilares e blocos sobre pilares, até o checkpoint 4 |
| 5 | Argolas | Fileira de argolas, até o checkpoint 5 |
| 6 | Vigas | Duas fileiras de vigas ligadas por uma viga em curva, até o checkpoint 6 |
| 7 | Escadinha numerada | 3 fileiras de 7 blocos numerados (1, 2, 3) e a placa com o código, até o **portal GOAL** |

As fileiras ficam separadas e ligadas por pedras de ligação, então não dá para pular um trecho inteiro. O espaço entre as peças foi calculado para caber no pulo do jogador (altura 2, gravidade -55, velocidade 7).

---

## Telas

Todas ficam em `GameUI.cs`, montadas por código em um único Canvas.

| Tela | Quando aparece |
|---|---|
| Menu inicial (start) | Ao abrir o jogo: Jogar, Como jogar, Configurações e Sair, com o melhor tempo |
| Como jogar | Pelo menu, pela pausa ou pelo botão Ajuda do HUD |
| Configurações | Volume, sensibilidade da câmera e tela cheia; valores salvos |
| HUD | Durante o jogo: vidas, tempo, checkpoint, botões Config. e Ajuda e aviso grande ao alcançar um checkpoint |
| Pausa | Esc / P / Start: Continuar, Como jogar, Configurações, Reiniciar jogo e Menu principal |
| Game Over | Quando as 3 vidas acabam: Tentar de novo ou Menu principal |
| Vitória | Ao chegar ao portal GOAL: tempo total e melhor tempo |

---

## Como abrir o projeto

1. Instale a **Unity 6** com o template Universal 3D (URP).
2. Abra a pasta do projeto no Unity Hub.
3. Abra a cena `Assets/Scenes/Parkour/Parkour.unity`.
4. Aperte **Play**.

O jogo funciona com o **Input System novo** e com o **Input Manager antigo** (o `GameInput.cs` detecta qual está ativo).

### Ao atualizar os scripts

Apague a pasta antiga `Scripts/Parkour` **antes** de copiar a nova. Se sobrar algum script velho (por exemplo `GameHud` ou `PauseMenu`), o projeto não compila.

---

## Estrutura do projeto

| Pasta | Conteúdo |
|---|---|
| `Scripts/Parkour` | 16 scripts C# comentados |
| `Scenes/Parkour/Parkour.unity` | A cena do jogo: jogador, câmera, percurso, GameManager e Interface |
| `Parkour/Materiais` | 7 materiais URP Lit |
| `Parkour/Malhas` | Malhas arredondadas geradas por código |
| `Settings` | Perfis do template URP (PC e Mobile) |
| `Scenes/SampleScene` | Cena de exemplo do template (não usada no jogo) |

### Scripts

| Script | O que faz |
|---|---|
| `GameManager.cs` | Estado do jogo (Menu, Playing, Paused, GameOver, Victory), vidas, tempo, melhor tempo, checkpoints e reinício |
| `GameSettings.cs` | Volume e sensibilidade salvos em PlayerPrefs, com cache para a câmera |
| `GameInput.cs` | Lê teclado, mouse e controle num lugar só |
| `ThirdPersonController.cs` | Movimento em terceira pessoa com CharacterController: pulo com coyote time e jump buffer, empurrão de caixas e teleporte |
| `ThirdPersonCamera.cs` | Câmera orbital com colisão, zoom e giro com o botão direito do mouse |
| `PlayerRespawn.cs` | Guarda o último checkpoint, tira vida ao cair no vazio e trata a tecla R |
| `Checkpoint.cs` | Checkpoint numerado: ao tocar, vira o ponto de renascimento |
| `GoalZone.cs` | Portal GOAL: ao tocar, mostra a tela de vitória |
| `PhysicsCrate.cs` | Caixas com Rigidbody dinâmico que voltam ao lugar se caírem ou num jogo novo |
| `FallingPlatform.cs` | Blocos que tremem e caem quando o jogador pisa (blocos errados da escadinha) |
| `MovingPlatform.cs`, `SpinningBar.cs`, `Hazard.cs`, `JumpPad.cs` | Obstáculos do kit (plataforma móvel, barra giratória, empurrão e trampolim) |
| `UiBuilder.cs` | Cria Canvas, textos, botões e sliders por código, sem prefabs |
| `GameUI.cs` | Todas as telas do jogo, ligadas e desligadas conforme o estado do GameManager |

---

## Requisitos do trabalho

| Requisito | Onde foi atendido |
|---|---|
| Menu inicial | Painel Menu em `GameUI.cs` |
| Configurações | Sliders de volume e sensibilidade e botão de tela cheia, salvos em PlayerPrefs |
| Game Over e vitória | Painéis em `GameUI.cs`, controlados pelo `GameManager.cs` |
| Física | Jogador com CharacterController e caixas com Rigidbody (`PhysicsCrate.cs`) |
| Código comentado | Todos os scripts têm comentários explicando o que fazem |
| README | Este arquivo |

---

## Assets e créditos

O jogo **não usa assets externos**: não há modelos, texturas, músicas ou sons baixados. Malhas e materiais foram criados por código. A fonte da interface é a `LegacyRuntime`, embutida na Unity.

Materiais (cor base RGB): Plataforma_Rosa (0,90 / 0,05 / 0,45), Checkpoint_Roxo (0,45 / 0,05 / 0,65), Start_Goal_Laranja (1,00 / 0,50 / 0,00), Caixa (0,62 / 0,40 / 0,18), Jogador (0,20 / 0,80 / 0,40), Jogador_Frente (1,00 / 1,00 / 1,00), Placa_Escura (0,08 / 0,08 / 0,12).

## Limitações conhecidas

- O projeto **não tem áudio**. O slider de volume funciona, mas ainda não há som para tocar.
- O código foi gerado com ajuda de IA; o processo, os erros encontrados e as correções estão no registro de desenvolvimento.
