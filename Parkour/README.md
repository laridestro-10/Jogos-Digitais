# Parkour nas Alturas

Obby em 3D (parkour no estilo Roblox) feito em Unity para a disciplina de Jogos Digitais.
O jogador sobe por **7 fases** baseadas no mapa desenhado pelo grupo, pega moedas, desvia de lava,
barra giratória e vidros falsos até chegar à linha de chegada.

- **Motor:** Unity 6000.3.6f1 (URP 17.3)
- **Cenas:** `Menu` e `Obby`
- **Scripts:** 19 scripts C# em `Assets/Zona3`

## Como abrir

1. Clone o repositório e troque para a branch `parkour-nas-alturas`.
2. No Unity Hub, clique em **Add → Add project from disk** e selecione a pasta
   `Parkour/fase 2 (versão 6.3.1f)`.
3. Abra com o Unity **6000.3.6f1**.
4. Abra a cena `Assets/Scenes/Menu.unity` e aperte **Play**.

## Controles

| Tecla | Ação |
|---|---|
| W A S D / setas | Andar (na direção da câmera) |
| Espaço | Pular |
| Botão direito do mouse (ou Q / E) | Girar a câmera |
| Rodinha do mouse | Zoom |
| Esc | Pausar / fechar janelas |

## Fases

| Fase | Zona | Desafio |
|---|---|---|
| 1 | 4 | Blocos sobre a lava |
| 2 | 4 | A curva (tábuas em sequência) |
| 3 | 3 | Bolinhas (discos em zigue-zague) |
| 4 | 3 | Barra giratória e trampolim |
| 5 | 2 | Anéis (gerados pelo `TorusGenerator`) |
| 6 | 2 | Plataformas que se mexem |
| 7 | 1 | Ponte de vidro (só um vidro por coluna aguenta) |

## Regras

- **Checkpoints** no início de cada fase: ao morrer, o jogador volta para o último.
- **Mortes:** encostar na lava ou na barra giratória, ou cair na água.
- **14 moedas** espalhadas pelo mapa, com tempo e mortes contados no HUD.
- **Estrelas:** 1 por terminar, +1 por pegar pelo menos 75% das moedas e +1 por morrer 3 vezes ou menos.
- O melhor tempo, as estrelas e as opções ficam salvos (`PlayerPrefs`) e aparecem no menu.

## Telas

Menu inicial, Como jogar, Opções (volume da música, efeitos, sensibilidade e tela cheia),
HUD, Pausa e Vitória. Toda troca de cena tem fade.

## Scripts principais

| Script | Função |
|---|---|
| `HUD` | Gerencia a partida: moedas, tempo, mortes, checkpoints, pausa e vitória |
| `Personagem` | Movimento com CharacterController, pulo com coyote time |
| `CameraSegue` | Câmera em 3ª pessoa que não atravessa plataformas |
| `Checkpoint` / `Perigo` | Salva o progresso / manda de volta |
| `Trampolim` / `Girar` | Impulso para cima / objeto girando |
| `PlataformaMovel` | Vai e volta levando o jogador |
| `Moeda` / `Chegada` | Soma moedas / termina a partida |
| `MenuPrincipal` / `OpcoesUI` | Menu e opções salvas |
| `Transicao` / `BotaoAnimado` | Fade entre cenas / botões animados |

## Documentos

Na pasta [`documentos`](documentos):

- `Parkour_nas_Alturas_relatorio.pdf`: relatório com prints das telas e fases e os erros corrigidos
- `Parkour_nas_Alturas_tecnico.pdf`: documentação técnica (arquitetura, scripts, física, interface)
- `Parkour_nas_Alturas_gameplay.mp4`: vídeo da gameplay completa
