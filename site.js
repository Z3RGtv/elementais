// Dicionário de mapeamento idêntico ao do teu jogo para detetar os ficheiros na pasta Sprites
const elementaisMap = [
    { id: "1_1", file: "T_Icon_BR_Creature_Sprite_Water_Unvault_Ch7S3_ui_L.webp" },
    { id: "1_2", file: "T_Icon_BR_Creature_Sprite_Water_Gold_ui_L.webp" },
    { id: "1_3", file: "T_Icon_BR_Creature_Sprite_Water_Candy_ui_L.webp" },
    { id: "1_4", file: "T_Icon_BR_Creature_Sprite_Water_Galaxy_ui_L.webp" },
    { id: "1_5", file: "T_Icon_BR_Creature_Sprite_Water_Holofoil_ui_L.webp" },
    { id: "2_1", file: "T_Icon_BR_Creature_Sprite_Earth_Ch7S3_UI_L.webp" },
    { id: "2_2", file: "T_Icon_BR_Creature_Sprite_Earth_Gold_ui_L.webp" },
    { id: "2_3", file: "T_Icon_BR_Creature_Sprite_Earth_Candy_ui_L.webp" },
    { id: "2_4", file: "T_Icon_BR_Creature_Sprite_Earth_Galaxy_ui_L.webp" },
    { id: "3_1", file: "T_Icon_BR_Creature_Sprite_Fire_Unvault_Ch7S3_ui_L.webp" },
    { id: "3_2", file: "T_Icon_BR_Creature_Sprite_Fire_Gold_ui_L.webp" },
    { id: "3_3", file: "T_Icon_BR_Creature_Sprite_Fire_Candy_ui_L.webp" },
    { id: "3_4", file: "T_Icon_BR_Creature_Sprite_Fire_Galaxy_ui_L.webp" },
    { id: "3_5", file: "T_Icon_BR_Creature_Sprite_Fire_Holofoil_ui_L.webp" },
    { id: "4_1", file: "T_Icon_BR_Duck_Default_L.webp" },
    { id: "4_2", file: "T_Icon_BR_Duck_Gold_L.webp" },
    { id: "4_3", file: "T_Icon_BR_Duck_Candy_L.webp" },
    { id: "4_4", file: "T_Icon_BR_Duck_Galaxy_L.webp" },
    { id: "5_1", file: "T_Icon_BR_Creature_Sprite_Ghost_Unvault_L.webp" },
    { id: "5_2", file: "T_Icon_BR_Creature_Sprite_Ghost_Gold_L.webp" },
    { id: "5_3", file: "T_Icon_BR_Creature_Sprite_Ghost_Candy_L.webp" },
    { id: "5_4", file: "T_Icon_BR_Creature_Sprite_Ghost_Galaxy_L.webp" },
    { id: "5_5", file: "T_Icon_BR_Creature_Sprite_Ghost_Holo_L.webp" },
    { id: "6_1", file: "T_Icon_BR_Creature_Sprite_Sleepy_ui_L.webp" },
    { id: "6_2", file: "T_Icon_BR_Creature_Sprite_Sleepy_Gold_ui_L.webp" },
    { id: "6_3", file: "T_Icon_BR_Creature_Sprite_Sleepy_Candy_ui_L.webp" },
    { id: "6_4", file: "T_Icon_BR_Creature_Sprite_Sleepy_Galaxy_ui_L.webp" },
    { id: "7_1", file: "T_Icon_BR_RedDemon_Default_L.webp" },
    { id: "7_2", file: "T_Icon_BR_RedDemon_Gold_L.webp" },
    { id: "7_3", file: "T_Icon_BR_RedDemon_Candy_L.webp" },
    { id: "7_4", file: "T_Icon_BR_RedDemon_Galaxy_L.webp" },
    { id: "8_1", file: "T_Icon_BR_Creature_Sprite_Punk_ui_L.webp" },
    { id: "8_2", file: "T_Icon_BR_Creature_Sprite_Punk_Gold_ui_L.webp" },
    { id: "8_3", file: "T_Icon_BR_Creature_Sprite_Punk_Candy_ui_L.webp" },
    { id: "8_4", file: "T_Icon_BR_Creature_Sprite_Punk_Galaxy_ui_L.webp" },
    { id: "9_1", file: "T_Icon_BR_Creature_Sprite_King_ui_L.webp" },
    { id: "9_2", file: "T_Icon_BR_Creature_Sprite_King_Gold_ui_L.webp" },
    { id: "9_3", file: "T_Icon_BR_Creature_Sprite_King_Candy_ui_L.webp" },
    { id: "9_4", file: "T_Icon_BR_Creature_Sprite_King_Galaxy_ui_L.webp" },
    { id: "9_5", file: "T_Icon_BR_Creature_Sprite_King_Holofoil_ui_L.webp" },
    { id: "10_1", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_ui_L.webp" },
    { id: "10_2", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Gold_ui_L.webp" },
    { id: "10_3", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Candy_ui_L.webp" },
    { id: "10_4", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Galaxy_ui_L.webp" },
    { id: "11_1", file: "T_Icon_BR_Creature_Sprite_BurntPeanut_ui_L.webp" },
    { id: "12_1", file: "T_Icon_BR_Creature_Sprite_Fishy_ui_L.webp" },
    { id: "12_2", file: "T_Icon_BR_Creature_Sprite_Fishy_Gold_ui_L.webp" },
    { id: "12_3", file: "T_Icon_BR_Creature_Sprite_Fishy_Candy_ui_L.webp" },
    { id: "12_4", file: "T_Icon_BR_Creature_Sprite_Fishy_Galaxy_ui_L.webp" },
    { id: "13_1", file: "T_Icon_BR_Creature_Sprite_Soccer_ui_L.webp" },
    { id: "13_2", file: "T_Icon_BR_Creature_Sprite_Soccer_Gold_L.webp" },
    { id: "13_3", file: "T_Icon_BR_Creature_Sprite_Soccer_Candy_L.webp" },
    { id: "13_4", file: "T_Icon_BR_Creature_Sprite_Soccer_Galaxy_L.webp" },
    { id: "13_5", file: "T_Icon_BR_Creature_Sprite_Soccer_Holofoil_L.webp" },
    { id: "14_1", file: "T_Icon_BR_Creature_Sprite_Drifter_ui_L.webp" },
    { id: "14_2", file: "T_Icon_BR_Creature_Sprite_Drifter_Gold_ui_L.webp" },
    { id: "14_3", file: "T_Icon_BR_Creature_Sprite_Drifter_Candy_ui_L.webp" },
    { id: "14_4", file: "T_Icon_BR_Creature_Sprite_Drifter_Galaxy_ui_L.webp" },
    { id: "15_1", file: "T_Icon_BR_Creature_Sprite_Boss_ui_L.webp" },
    { id: "15_2", file: "T_Icon_BR_Creature_Sprite_Boss_Gold_ui_L.webp" },
    { id: "15_3", file: "T_Icon_BR_Creature_Sprite_Boss_Candy_ui_L.webp" },
    { id: "15_4", file: "T_Icon_BR_Creature_Sprite_Boss_Galaxy_ui_L.webp" },
    { id: "16_1", file: "T_Icon_BR_GrimReaper_Default_L.webp" },
    { id: "16_2", file: "T_Icon_BR_GrimReaper_Gold_L.webp" },
    { id: "16_3", file: "T_Icon_BR_GrimReaper_Candy_L.webp" },
    { id: "16_4", file: "T_Icon_BR_GrimReaper_Galaxy_L.webp" },
    { id: "17_1", file: "T_Icon_BR_Air_Default_L.webp" },
    { id: "17_2", file: "T_Icon_BR_Air_Gold_L.webp" },
    { id: "17_3", file: "T_Icon_BR_Air_Candy_L.webp" },
    { id: "17_4", file: "T_Icon_BR_Air_Galaxy_L.webp" },
    { id: "17_5", file: "T_Icon_BR_Air_Holo_L.webp" },
    { id: "18_1", file: "T_Icon_BR_Creature_Sprite_Seven_ui_L.webp" },
    { id: "18_2", file: "T_Icon_BR_Creature_Sprite_Seven_Gold_ui_L.webp" },
    { id: "18_3", file: "T_Icon_BR_Creature_Sprite_Seven_Candy_ui_L.webp" },
    { id: "18_4", file: "T_Icon_BR_Creature_Sprite_Seven_Galaxy_ui_L.webp" },
    { id: "18_5", file: "T_Icon_BR_Creature_Sprite_Seven_Holofoil_ui_L.webp" },
    { id: "19_1", file: "T_Icon_BR_FossilMeal_Default_L.webp" },
    { id: "19_2", file: "T_Icon_BR_FossilMeal_Gold_L.webp" },
    { id: "19_3", file: "T_Icon_BR_FossilMeal_Candy_L.webp" },
    { id: "19_4", file: "T_Icon_BR_FossilMeal_Galaxy_L.webp" },
    { id: "19_5", file: "T_Icon_BR_FossilMeal_Holofoil_L.webp" },
    { id: "20_1", file: "T_Icon_BR_CompanyStargazer_Default_L.webp" },
    { id: "21_1", file: "T_Icon_BR_CokeParmesan_Default_L.webp" }
];

let dadosGlobais = [];
let meuUsername = localStorage.getItem('meuUsername') || '';
let jogadorSelecionado = null; // Jogador cuja coleção estamos a ver
let jogadorSelecionadoTroca = null; // Alvo selecionado dentro do modal
let propostasAtivas = [];

// Seletores do Assistente de Troca
let oferecidoId = null;
let pedidoId = null;
let cacheOferecido = null;
let cachePedido = null;

async function carregarDados() {
    try {
        const res = await fetch(`inventario.json?t=${new Date().getTime()}`);
        const data = await res.json();
        dadosGlobais = data.ranking || [];
        propostasAtivas = data.propostas || [];
        
        document.getElementById('update-timer').textContent = `Última sincronização com a Stream: ${data.updatedAt || 'Pendente'}`;
        
        const availableUsers = data.availableUsers || [];

        // Limpar e re-mapear elementaisMap
        const baseLength = 85;
        elementaisMap.length = baseLength;

        availableUsers.forEach(u => {
            elementaisMap.push({
                id: u.id,
                file: u.file,
                isUser: true,
                name: u.name
            });
        });
        
        renderizarRanking(dadosGlobais);
        atualizarUIConta();
        renderizarEstatisticas(data.estatisticas_bolas || {}, data.recentes_lancamentos || []);
        
        // Se já tivermos um jogador selecionado, atualiza a exibição dele, senão mostra o Inventário Global por defeito
        if (jogadorSelecionado) {
            if (jogadorSelecionado.username === "Inventário Global") {
                selecionarInventarioGlobal();
            } else {
                const atualizado = dadosGlobais.find(p => p.username.toLowerCase() === jogadorSelecionado.username.toLowerCase());
                if (atualizado) {
                    const items = document.querySelectorAll('.ranking-item');
                    let foundEl = null;
                    items.forEach(el => {
                        if (el.textContent.includes(`@${atualizado.username}`)) {
                            foundEl = el;
                        }
                    });
                    selecionarUtilizador(atualizado, foundEl);
                }
            }
        } else {
            selecionarInventarioGlobal();
        }
        
        renderizarPropostas();
    } catch (e) {
        document.getElementById('update-timer').textContent = "A aguardar a primeira captura da stream para gerar o catálogo...";
    }
}

function renderizarRanking(jogadores) {
    const lista = document.getElementById('ranking-list');
    lista.innerHTML = '';

    jogadores.forEach((player, index) => {
        const item = document.createElement('div');
        item.className = 'ranking-item';
        
        const isMe = meuUsername && player.username.toLowerCase() === meuUsername.toLowerCase();
        if (isMe) {
            item.classList.add('my-account');
        }
        
        if (jogadorSelecionado && jogadorSelecionado.username.toLowerCase() === player.username.toLowerCase()) {
            item.classList.add('active');
        }

        let rankPrefix = `<strong>${index + 1}º</strong>`;
        if (index >= 0 && index <= 4) {
            rankPrefix = `<img src="badges/${index + 1}.png" class="rank-badge-img" title="${index + 1}º Classificado" alt="${index + 1}º">`;
        }

        item.innerHTML = `
            <span>${rankPrefix} @${player.username} ${isMe ? '<span class="my-account-badge">Tu</span>' : ''}</span>
            <span>${player.pontos} pts</span>
        `;
        item.onclick = () => selecionarUtilizador(player, item);
        lista.appendChild(item);
    });
}

function selecionarUtilizador(player, elementoDOM) {
    document.querySelectorAll('.ranking-item').forEach(i => i.classList.remove('active'));
    if (elementoDOM) elementoDOM.classList.add('active');

    jogadorSelecionado = player;

    document.getElementById('view-title').textContent = `Coleção de @${player.username}`;
    document.getElementById('user-points-label-container').style.display = 'inline';
    document.getElementById('user-points-val').textContent = player.pontos;

    // Calcular o progresso de elementais obtidos em relação ao catálogo total dinâmico
    const totalCatalog = elementaisMap.length;
    const totalObtidos = Object.keys(player.inventario).filter(key => player.inventario[key] > 0).length;
    
    document.getElementById('user-progress-val').textContent = `${totalObtidos} / ${totalCatalog}`;
    
    const pct = totalCatalog > 0 ? (totalObtidos / totalCatalog * 100) : 0;
    const progressBar = document.getElementById('user-progress-bar');
    if (progressBar) {
        progressBar.style.width = `${pct}%`;
        progressBar.title = `${totalObtidos} de ${totalCatalog} elementais (${Math.round(pct)}%)`;
    }

    document.getElementById('user-points-panel').classList.remove('hidden');

    const btnPropor = document.getElementById('btn-propor-troca');
    const btnDefinir = document.getElementById('btn-definir-conta');

    const isTwitchLoggedIn = !!localStorage.getItem('twitch_access_token');
    const autenticadas = JSON.parse(localStorage.getItem('contas_autenticadas_twitch') || '[]');
    const contaJaAutenticada = autenticadas.includes(player.username.toLowerCase());

    // O botão "Esta é a minha conta" só aparece nas contas que NUNCA fizeram login com a Twitch neste navegador
    // e que também não são a nossa conta atualmente selecionada (caso tenhamos uma ativa)
    if (contaJaAutenticada || (meuUsername && meuUsername.toLowerCase() === player.username.toLowerCase())) {
        btnDefinir.classList.add('hidden');
    } else {
        btnDefinir.classList.remove('hidden');
        btnDefinir.textContent = "Esta é a minha Conta 👤";
    }

    // O botão "Propor Troca" só aparece se tivermos uma conta ativa configurada (meuUsername)
    // e essa conta for diferente da conta do jogador selecionado
    if (meuUsername && meuUsername.toLowerCase() !== player.username.toLowerCase()) {
        btnPropor.classList.remove('hidden');
    } else {
        btnPropor.classList.add('hidden');
    }

    renderizarGridColecao(player, 'site-grid', false);
    renderizarPropostas();
}

function renderizarGridColecao(player, targetGridId, isSelectionMode, selectCallback) {
    const grid = document.getElementById(targetGridId);
    grid.innerHTML = '';

    elementaisMap.forEach(elem => {
        const qty = player.inventario[elem.id] || 0;
        
        // No modo de seleção do assistente, só mostramos as cartas que o jogador de facto tem
        if (isSelectionMode && qty === 0) return;

        const slot = document.createElement('div');
        slot.className = 'grid-item';
        if (elem.isUser) {
            slot.classList.add('user-slot');
        }

        const img = document.createElement('img');
        img.src = `Sprites/${elem.file}`;

        if (qty === 0) {
            slot.classList.add('not-owned');
            slot.appendChild(img);
        } else if (qty === 1) {
            slot.appendChild(img);
        } else if (qty >= 2) {
            slot.classList.add('stacked');
            img.className = 'card-front';
            
            const imgBack = document.createElement('img');
            imgBack.src = `Sprites/${elem.file}`;
            imgBack.className = 'card-back';

            const badge = document.createElement('span');
            badge.className = 'qty-badge';
            badge.textContent = `x${qty}`;

            slot.appendChild(imgBack);
            slot.appendChild(img);
            slot.appendChild(badge);
        }

        if (elem.isUser) {
            const nameTag = document.createElement('span');
            nameTag.className = 'user-name-tag';
            nameTag.textContent = elem.name;
            slot.appendChild(nameTag);
        }

        if (isSelectionMode && selectCallback) {
            // Se for o grid de pedidos do alvo e eu já tiver 2 cópias desse bicho, desativa visualmente
            if (targetGridId === 'target-request-card-selection') {
                const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase()) || { inventario: {} };
                const qtyEu = meuPerfil.inventario[elem.id] || 0;
                if (qtyEu >= 2) {
                    slot.classList.add('disabled-selection');
                    slot.title = "Já tens o limite de 2 cópias deste elemental na tua coleção!";
                }
            }

            slot.onclick = () => {
                if (targetGridId === 'target-request-card-selection') {
                    const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase()) || { inventario: {} };
                    const qtyEu = meuPerfil.inventario[elem.id] || 0;
                    if (qtyEu >= 2) {
                        alert(`Não podes propor receber este elemental porque já tens o limite máximo de 2 cópias na tua coleção!`);
                        return;
                    }
                }
                grid.querySelectorAll('.grid-item').forEach(i => i.classList.remove('selected'));
                slot.classList.add('selected');
                selectCallback(elem.id, elem);
            };
        } else if (!isSelectionMode) {
            slot.onclick = () => {
                if (player.username === "Inventário Global") {
                    mostrarPossuidoresCard(elem);
                    return;
                }
                if (qty >= 1) {
                    const isTwitchLoggedIn = !!localStorage.getItem('twitch_access_token');
                    const autenticadas = JSON.parse(localStorage.getItem('contas_autenticadas_twitch') || '[]');
                    
                    // Caso 1: Clicar no elemental de outra pessoa (Proposta direta se estiver logado)
                    if (meuUsername && meuUsername.toLowerCase() !== player.username.toLowerCase()) {
                        const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase()) || { inventario: {} };
                        
                        // Verificar limites de trocas do próprio utilizador e do alvo
                        const meuDisp = meuPerfil.trocasDisponiveis !== undefined ? meuPerfil.trocasDisponiveis : 3;
                        if (meuDisp < 1) {
                            alert(`Não tens trocas disponíveis de momento (máximo 3 por semana)!`);
                            return;
                        }
                        const targetDisp = player.trocasDisponiveis !== undefined ? player.trocasDisponiveis : 3;
                        if (targetDisp < 1) {
                            alert(`O jogador @${player.username} não tem trocas disponíveis esta semana!`);
                            return;
                        }

                        const minhasCopias = meuPerfil.inventario[elem.id] || 0;
                        if (minhasCopias >= 2) {
                            alert(`Não podes propor receber este elemental porque já tens o limite máximo de 2 cópias na tua coleção!`);
                            return;
                        }
                        abrirModalTrocaComCardPedidoPreSelecionado(player, elem);
                        return;
                    }

                    // Caso 2: Clicar no meu próprio elemental (Definir o que oferecer primeiro)
                    const contaJaAutenticada = autenticadas.includes(player.username.toLowerCase());
                    if (contaJaAutenticada) {
                        if (meuUsername && meuUsername.toLowerCase() === player.username.toLowerCase()) {
                            oferecidoId = elem.id;
                            abrirModalTrocaComCardPreSelecionado(elem);
                        } else {
                            alert(`Esta conta (@${player.username}) já foi autenticada via Twitch neste navegador. Por favor, faz login no topo da página para a usar.`);
                        }
                    } else {
                        if (isTwitchLoggedIn) {
                            if (meuUsername && meuUsername.toLowerCase() === player.username.toLowerCase()) {
                                oferecidoId = elem.id;
                                abrirModalTrocaComCardPreSelecionado(elem);
                            }
                        } else {
                            // Fluxo Livre: Assumir identidade automaticamente
                            meuUsername = player.username;
                            localStorage.setItem('meuUsername', meuUsername);
                            atualizarUIConta();
                            
                            // Destacar visualmente no ranking e atualizar botões
                            selecionarUtilizador(player, document.querySelector('.ranking-item.active'));

                            oferecidoId = elem.id;
                            abrirModalTrocaComCardPreSelecionado(elem);
                        }
                    }
                }
            };
        }

        grid.appendChild(slot);
    });
}

// Lógica de Conta Local (Minha Conta / Twitch)
function atualizarUIConta() {
    const container = document.getElementById('my-profile-container');
    const avatar = localStorage.getItem('meuProfileImage') || '';
    const isTwitchLoggedIn = !!localStorage.getItem('twitch_access_token');
    
    if (meuUsername) {
        let tradeWidgetHtml = '';
        if (dadosGlobais) {
            const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase());
            if (meuPerfil) {
                const disp = meuPerfil.trocasDisponiveis !== undefined ? meuPerfil.trocasDisponiveis : 3;
                const prox = meuPerfil.proximaRecuperacao || null;
                
                let timerHtml = '';
                if (disp < 3 && prox) {
                    timerHtml = `<span class="trade-timer-badge" data-endtime="${prox}">...</span>`;
                }
                
                tradeWidgetHtml = `
                    <div class="user-trade-status">
                        <span class="trade-charges-label">🔄 Trocas: <strong>${disp}/3</strong></span>
                        ${timerHtml}
                    </div>
                `;
            }
        }

        if (isTwitchLoggedIn) {
            // Utilizador Logado via Twitch
            container.innerHTML = `
                <div class="profile-info">
                    ${avatar ? `<img src="${avatar}" class="twitch-avatar" alt="Avatar">` : ''}
                    <span>Olá, <strong>@${meuUsername}</strong>!</span>
                    ${tradeWidgetHtml}
                </div>
                <button class="logout-btn" onclick="limparConta()">Sair ✖</button>
            `;
        } else {
            // Utilizador definiu a conta manualmente (sem login)
            const clientId = "9xa7l560hxwruznfapxxrwm3543fk0";
            const redirectUri = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
                ? window.location.origin + '/'
                : 'https://z3rgtv.github.io/elementais/';
            const authUrl = `https://id.twitch.tv/oauth2/authorize?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&response_type=token&scope=`;

            container.innerHTML = `
                <div class="profile-info">
                    <span>Conta manual: <strong>@${meuUsername}</strong></span>
                    ${tradeWidgetHtml}
                    <button class="logout-btn" onclick="limparConta()" style="margin-left: 5px; margin-right: 15px;">Alterar ✖</button>
                </div>
                <a href="${authUrl}" class="twitch-login-btn">
                    <svg class="twitch-icon" viewBox="0 0 24 24" width="16" height="16" style="vertical-align: middle; margin-right: 6px;">
                        <path fill="currentColor" d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"/>
                    </svg>
                    Entrar com a Twitch
                </a>
            `;
        }
    } else {
        // Nenhuma identidade selecionada
        const clientId = "9xa7l560hxwruznfapxxrwm3543fk0";
        const redirectUri = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
            ? window.location.origin + '/'
            : 'https://z3rgtv.github.io/elementais/';
        const authUrl = `https://id.twitch.tv/oauth2/authorize?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&response_type=token&scope=`;
        
        container.innerHTML = `
            <span style="font-size: 12px; color: var(--text-muted); margin-right: 15px;">Sem conta selecionada.</span>
            <a href="${authUrl}" class="twitch-login-btn">
                <svg class="twitch-icon" viewBox="0 0 24 24" width="16" height="16" style="vertical-align: middle; margin-right: 6px;">
                    <path fill="currentColor" d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"/>
                </svg>
                Entrar com a Twitch
            </a>
        `;
    }
}

// Configurar o clique no botão Definir Conta
document.getElementById('btn-definir-conta').onclick = () => {
    if (jogadorSelecionado) {
        // Se estiver logado pela Twitch, ao definir uma conta manual, fazemos logout automático do Twitch
        const isTwitchLoggedIn = !!localStorage.getItem('twitch_access_token');
        if (isTwitchLoggedIn) {
            localStorage.removeItem('twitch_access_token');
            localStorage.removeItem('meuProfileImage');
        }

        meuUsername = jogadorSelecionado.username;
        localStorage.setItem('meuUsername', meuUsername);
        atualizarUIConta();
        selecionarUtilizador(jogadorSelecionado, document.querySelector('.ranking-item.active'));
    }
};

function limparConta() {
    meuUsername = '';
    localStorage.removeItem('meuUsername');
    localStorage.removeItem('twitch_access_token');
    localStorage.removeItem('meuProfileImage');
    atualizarUIConta();
    if (jogadorSelecionado) {
        selecionarUtilizador(jogadorSelecionado, document.querySelector('.ranking-item.active'));
    }
}

// Renderização de Propostas Pendentes
function renderizarPropostas() {
    const container = document.getElementById('propostas-pendentes-container');
    
    if (!meuUsername) {
        container.classList.add('hidden');
        return;
    }

    const minhasPropostas = propostasAtivas.filter(p => 
        p.proposerName.toLowerCase() === meuUsername.toLowerCase() || 
        p.targetName.toLowerCase() === meuUsername.toLowerCase()
    );

    if (minhasPropostas.length === 0) {
        container.classList.add('hidden');
        return;
    }

    container.innerHTML = `<h3>Visualizador de Trocas Pendentes 🔄</h3>`;
    
    minhasPropostas.forEach(p => {
        const souAlvo = p.targetName.toLowerCase() === meuUsername.toLowerCase();
        const card = document.createElement('div');
        card.className = 'proposta-card';

        const elemPropInfo = elementaisMap.find(e => e.id === p.elemProposer) || { file: 'default.png', id: p.elemProposer };
        const elemTargInfo = elementaisMap.find(e => e.id === p.elemTarget) || { file: 'default.png', id: p.elemTarget };

        const propNomeLegivel = elemPropInfo.isUser ? elemPropInfo.name : obterNomeSimplesBicho(p.elemProposer);
        const targNomeLegivel = elemTargInfo.isUser ? elemTargInfo.name : obterNomeSimplesBicho(p.elemTarget);

        let content = '';
        if (souAlvo) {
            content = `
                <div class="prop-players-info">
                    <span><strong>@${p.proposerName}</strong> propõe:</span>
                    <div class="prop-visual-swap">
                        <span class="prop-action-label give">Oferece</span>
                        <img src="Sprites/${elemPropInfo.file}" class="prop-card-mini" title="${propNomeLegivel}">
                        <span class="prop-item-name">${propNomeLegivel}</span>
                        
                        <span class="prop-arrow">➡️</span>
                        
                        <span class="prop-action-label take">Pede-te</span>
                        <img src="Sprites/${elemTargInfo.file}" class="prop-card-mini" title="${targNomeLegivel}">
                        <span class="prop-item-name">${targNomeLegivel}</span>
                    </div>
                </div>
                <div class="prop-actions">
                    <button class="btn btn-sm" onclick="copiarComandoResposta('sim', '${p.proposerName}', this)">Aceitar (sim) 🤝</button>
                    <button class="btn btn-sm btn-danger" onclick="copiarComandoResposta('nao', '${p.proposerName}', this)">Recusar (nao) ❌</button>
                </div>
            `;
        } else {
            content = `
                <div class="prop-players-info">
                    <span>Propuseste a <strong>@${p.targetName}</strong>:</span>
                    <div class="prop-visual-swap">
                        <span class="prop-action-label give">Dás</span>
                        <img src="Sprites/${elemPropInfo.file}" class="prop-card-mini" title="${propNomeLegivel}">
                        <span class="prop-item-name">${propNomeLegivel}</span>
                        
                        <span class="prop-arrow">➡️</span>
                        
                        <span class="prop-action-label take">Pedes</span>
                        <img src="Sprites/${elemTargInfo.file}" class="prop-card-mini" title="${targNomeLegivel}">
                        <span class="prop-item-name">${targNomeLegivel}</span>
                    </div>
                </div>
                <div class="prop-actions">
                    <span style="font-size: 12px; color: var(--text-muted); font-style: italic;">Aguardando resposta no chat</span>
                </div>
            `;
        }

        card.innerHTML = content;
        container.appendChild(card);
    });

    container.classList.remove('hidden');
}

function copiarComandoResposta(resposta, proponenteName, botao) {
    const comando = `${resposta} ${proponenteName}`;
    navigator.clipboard.writeText(comando).then(() => {
        const textoOriginal = botao.textContent;
        botao.textContent = "Copiado! ✔";
        setTimeout(() => {
            botao.textContent = textoOriginal;
        }, 1500);
    });
}

// Lógica do Modal / Assistente de Troca
const modal = document.getElementById('modal-troca');
const btnFecharModal = document.getElementById('btn-fechar-modal');

// FLUXO A: Iniciar clicando num elemental meu
function abrirModalTrocaComCardPreSelecionado(elemOferecido) {
    document.getElementById('modal-target-username').textContent = "outro jogador";
    
    const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase()) || { inventario: {} };

    pedidoId = null;
    jogadorSelecionadoTroca = null;
    cacheOferecido = elemOferecido;
    cachePedido = null;
    document.getElementById('troca-resumo-container').classList.add('hidden');

    // Mostrar bloco de seleção de jogador e esconder cartas do alvo
    document.getElementById('modal-player-select-block').classList.remove('hidden');
    document.getElementById('modal-target-cards-block').classList.add('hidden');
    document.getElementById('modal-search-player').value = '';

    // Renderizar meu inventário no modal
    renderizarGridColecao(meuPerfil, 'my-offer-card-selection', true, (id, elem) => {
        oferecidoId = id;
        cacheOferecido = elem;
        atualizarResumoTroca(elem, null);
        renderizarListaJogadoresModal(); // Re-renderiza a lista para atualizar quem fica indisponível
    });

    // Destacar visualmente o card oferecido que foi clicado
    setTimeout(() => {
        const gridOferta = document.getElementById('my-offer-card-selection');
        gridOferta.querySelectorAll('.grid-item').forEach(slot => {
            const img = slot.querySelector('img');
            if (img && img.src.includes(elemOferecido.file)) {
                slot.classList.add('selected');
            }
        });
    }, 50);

    // Listar outros jogadores
    renderizarListaJogadoresModal();

    modal.classList.remove('hidden');
}

function abrirModalTrocaComCardPedidoPreSelecionado(targetPlayer, elemPedido) {
    if (!meuUsername) return;
    
    const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase()) || { inventario: {} };

    oferecidoId = null;
    pedidoId = elemPedido.id;
    cacheOferecido = null;
    cachePedido = elemPedido;
    document.getElementById('troca-resumo-container').classList.add('hidden');

    // Renderizar meu inventário para eu escolher o que ofereço
    renderizarGridColecao(meuPerfil, 'my-offer-card-selection', true, (id, elem) => {
        oferecidoId = id;
        cacheOferecido = elem;
        atualizarResumoTroca(elem, null);
        renderizarListaJogadoresModal(); // Re-renderiza a lista de jogadores caso decida mudar
    });

    // Selecionar diretamente o jogador cuja página estamos a visualizar
    selecionarJogadorTrocaNoModal(targetPlayer);

    // Destacar visualmente o card pedido que foi clicado no grid do alvo
    setTimeout(() => {
        const gridPedido = document.getElementById('target-request-card-selection');
        if (gridPedido) {
            gridPedido.querySelectorAll('.grid-item').forEach(slot => {
                const img = slot.querySelector('img');
                if (img && img.src.includes(elemPedido.file)) {
                    slot.classList.add('selected');
                }
            });
        }
    }, 50);

    // Atualizar o resumo da troca já com o item pedido
    atualizarResumoTroca(null, elemPedido);

    modal.classList.remove('hidden');
}

function renderizarListaJogadoresModal() {
    const list = document.getElementById('modal-player-list');
    list.innerHTML = '';

    const termo = document.getElementById('modal-search-player').value.toLowerCase().trim();

    // Filtrar todos os jogadores do ranking exceto o proponente (meuUsername)
    const outrosJogadores = dadosGlobais.filter(p => 
        p.username.toLowerCase() !== meuUsername.toLowerCase() &&
        p.username.toLowerCase().includes(termo)
    );

    if (outrosJogadores.length === 0) {
        list.innerHTML = '<div style="font-size: 12px; color: var(--text-muted); text-align: center; padding: 10px;">Nenhum outro jogador encontrado.</div>';
        return;
    }

    outrosJogadores.forEach(player => {
        const item = document.createElement('div');
        item.className = 'modal-player-item';

        // Verificar se o jogador alvo já possui 2 ou mais cópias do elemental oferecido
        const qtyTarget = player.inventario[oferecidoId] || 0;
        const jaTemLimite = oferecidoId && (qtyTarget >= 2);
        const semTrocasTarget = player.trocasDisponiveis !== undefined && player.trocasDisponiveis < 1;

        if (jaTemLimite || semTrocasTarget) {
            item.classList.add('disabled-trade');
            let labelExtra = jaTemLimite ? "(Já tem 2)" : "(Sem trocas)";
            item.innerHTML = `
                <span><strong style="color: #ff5555;">@${player.username}</strong> <small style="color: #ff5555; font-size: 10px; margin-left: 5px;">${labelExtra}</small></span>
                <span style="font-size: 11px; color: var(--text-muted);">${Object.keys(player.inventario).length} espécies</span>
            `;
            item.onclick = (e) => {
                e.stopPropagation();
            };
        } else {
            item.innerHTML = `
                <span><strong>@${player.username}</strong></span>
                <span style="font-size: 11px; color: var(--text-muted);">${Object.keys(player.inventario).length} espécies</span>
            `;
            item.onclick = () => selecionarJogadorTrocaNoModal(player);
        }
        list.appendChild(item);
    });
}

function selecionarJogadorTrocaNoModal(player) {
    jogadorSelecionadoTroca = player;
    document.getElementById('modal-target-username').textContent = `@${player.username}`;
    document.getElementById('modal-active-target-label').textContent = `Cartas de @${player.username}`;

    // Mostrar bloco de cartas do alvo
    document.getElementById('modal-player-select-block').classList.add('hidden');
    document.getElementById('modal-target-cards-block').classList.remove('hidden');

    // Renderizar as cartas dele
    renderizarGridColecao(player, 'target-request-card-selection', true, (id, elem) => {
        pedidoId = id;
        cachePedido = elem;
        atualizarResumoTroca(null, elem);
    });
}

// Botão de voltar à lista de jogadores no modal
document.getElementById('btn-modal-mudar-jogador').onclick = () => {
    jogadorSelecionadoTroca = null;
    pedidoId = null;
    document.getElementById('modal-target-username').textContent = "outro jogador";
    document.getElementById('modal-player-select-block').classList.remove('hidden');
    document.getElementById('modal-target-cards-block').classList.add('hidden');
    document.getElementById('modal-search-player').value = '';
    renderizarListaJogadoresModal();
    atualizarResumoTroca(null, null);
};

// Escuta input de pesquisa de jogadores no modal
document.getElementById('modal-search-player').addEventListener('input', renderizarListaJogadoresModal);


// FLUXO B: Iniciar clicando no botão "Propor Troca" no perfil de outro jogador
document.getElementById('btn-propor-troca').onclick = () => {
    if (!meuUsername || !jogadorSelecionado) return;
    
    const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase()) || { inventario: {} };

    // Validar se o proponente tem trocas disponíveis
    const meuDisp = meuPerfil.trocasDisponiveis !== undefined ? meuPerfil.trocasDisponiveis : 3;
    if (meuDisp < 1) {
        alert(`Não tens trocas disponíveis de momento (máximo 3 por semana)!`);
        return;
    }

    // Validar se o alvo tem trocas disponíveis
    const targetDisp = jogadorSelecionado.trocasDisponiveis !== undefined ? jogadorSelecionado.trocasDisponiveis : 3;
    if (targetDisp < 1) {
        alert(`O jogador @${jogadorSelecionado.username} não tem trocas disponíveis esta semana!`);
        return;
    }

    oferecidoId = null;
    pedidoId = null;
    cacheOferecido = null;
    cachePedido = null;
    document.getElementById('troca-resumo-container').classList.add('hidden');

    // Renderizar meu inventário
    renderizarGridColecao(meuPerfil, 'my-offer-card-selection', true, (id, elem) => {
        oferecidoId = id;
        cacheOferecido = elem;
        atualizarResumoTroca(elem, null);
        renderizarListaJogadoresModal(); // Re-renderiza a lista de jogadores caso o utilizador decida mudar de jogador
    });

    // Selecionar diretamente o jogador cuja página estamos a visualizar
    selecionarJogadorTrocaNoModal(jogadorSelecionado);

    modal.classList.remove('hidden');
};

btnFecharModal.onclick = () => {
    modal.classList.add('hidden');
};

window.onclick = (event) => {
    if (event.target === modal) {
        modal.classList.add('hidden');
    }
};

function atualizarResumoTroca(elemOferecido, elemPedido) {
    if (elemOferecido) cacheOferecido = elemOferecido;
    if (elemPedido) cachePedido = elemPedido;

    if (oferecidoId && pedidoId && cacheOferecido && cachePedido) {
        const divOferecido = document.getElementById('resumo-oferecido-img');
        const divPedido = document.getElementById('resumo-pedido-img');

        divOferecido.innerHTML = `<img src="Sprites/${cacheOferecido.file}">`;
        divPedido.innerHTML = `<img src="Sprites/${cachePedido.file}">`;

        document.getElementById('resumo-oferecido-nome').textContent = cacheOferecido.isUser ? cacheOferecido.name : cacheOferecido.id;
        document.getElementById('resumo-pedido-nome').textContent = cachePedido.isUser ? cachePedido.name : cachePedido.id;

        const targetPlayer = jogadorSelecionadoTroca || jogadorSelecionado;
        const targetUser = targetPlayer ? targetPlayer.username : "";
        
        const qtyTarget = targetPlayer && targetPlayer.inventario ? (targetPlayer.inventario[oferecidoId] || 0) : 0;
        const jaTemLimiteAlvo = qtyTarget >= 2;

        const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase()) || { inventario: {} };
        const qtyEu = meuPerfil.inventario[pedidoId] || 0;
        const jaTemLimiteEu = qtyEu >= 2;

        const myDisp = meuPerfil.trocasDisponiveis !== undefined ? meuPerfil.trocasDisponiveis : 3;
        const targetDisp = targetPlayer.trocasDisponiveis !== undefined ? targetPlayer.trocasDisponiveis : 3;

        const commandInput = document.getElementById('twitch-command-input');
        const btnCopiar = document.getElementById('btn-copiar-comando');

        if (myDisp < 1) {
            commandInput.value = `Inválido: Não tens trocas disponíveis de momento!`;
            commandInput.style.color = '#ff5555';
            btnCopiar.disabled = true;
            btnCopiar.classList.add('disabled-btn');
        } else if (targetDisp < 1) {
            commandInput.value = `Inválido: @${targetUser} não tem trocas disponíveis!`;
            commandInput.style.color = '#ff5555';
            btnCopiar.disabled = true;
            btnCopiar.classList.add('disabled-btn');
        } else if (jaTemLimiteAlvo) {
            commandInput.value = `Inválido: @${targetUser} já tem o limite de 2 cópias!`;
            commandInput.style.color = '#ff5555';
            btnCopiar.disabled = true;
            btnCopiar.classList.add('disabled-btn');
        } else if (jaTemLimiteEu) {
            commandInput.value = `Inválido: Já tens o limite de 2 cópias!`;
            commandInput.style.color = '#ff5555';
            btnCopiar.disabled = true;
            btnCopiar.classList.add('disabled-btn');
        } else {
            const command = `${targetUser} ${oferecidoId} ${pedidoId}`;
            commandInput.value = command;
            commandInput.style.color = '';
            btnCopiar.disabled = false;
            btnCopiar.classList.remove('disabled-btn');
        }

        document.getElementById('troca-resumo-container').classList.remove('hidden');
    } else {
        document.getElementById('troca-resumo-container').classList.add('hidden');
    }
}

document.getElementById('btn-copiar-comando').onclick = () => {
    const input = document.getElementById('twitch-command-input');
    navigator.clipboard.writeText(input.value).then(() => {
        const btn = document.getElementById('btn-copiar-comando');
        btn.textContent = "Copiado! ✔";
        setTimeout(() => {
            btn.textContent = "Copiar 📋";
        }, 1500);
    });
};

document.getElementById('btn-ver-globais').addEventListener('click', () => {
    selecionarInventarioGlobal();
});

// Configuração da Barra de Pesquisa Dinâmica (Filtro Geral)
document.getElementById('search-input').addEventListener('input', (e) => {
    const termo = e.target.value.toLowerCase().trim();
    if (!termo) {
        renderizarRanking(dadosGlobais);
        return;
    }
    const filtrados = dadosGlobais.filter(p => p.username.toLowerCase().includes(termo));
    renderizarRanking(filtrados);
});

// Processar o token de autenticação da Twitch no URL hash
function verificarLoginTwitch() {
    const hash = window.location.hash;
    if (hash && hash.includes('access_token=')) {
        const params = new URLSearchParams(hash.substring(1));
        const token = params.get('access_token');
        if (token) {
            localStorage.setItem('twitch_access_token', token);
            // Limpa o hash do URL para ficar limpo
            history.replaceState(null, document.title, window.location.pathname + window.location.search);
        }
    }
}

// Obter dados do perfil do utilizador autenticado
async function obterPerfilTwitch() {
    const token = localStorage.getItem('twitch_access_token');
    if (!token) return;

    try {
        const res = await fetch('https://api.twitch.tv/helix/users', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Client-Id': '9xa7l560hxwruznfapxxrwm3543fk0'
            }
        });
        
        if (res.status === 401) {
            // Token expirou ou é inválido, limpa a sessão
            limparConta();
            return;
        }

        const data = await res.json();
        if (data && data.data && data.data.length > 0) {
            const user = data.data[0];
            meuUsername = user.login; // Guardado em minúsculas (igual ao username da BD)
            localStorage.setItem('meuUsername', meuUsername);
            localStorage.setItem('meuProfileImage', user.profile_image_url);
            
            // Adicionar esta conta à lista local de contas que já se autenticaram via Twitch
            let autenticadas = JSON.parse(localStorage.getItem('contas_autenticadas_twitch') || '[]');
            if (!autenticadas.includes(meuUsername.toLowerCase())) {
                autenticadas.push(meuUsername.toLowerCase());
                localStorage.setItem('contas_autenticadas_twitch', JSON.stringify(autenticadas));
            }
        }
    } catch (e) {
        console.error("Erro ao carregar perfil da Twitch:", e);
    }
}

// Inicialização segura
async function inicializarApp() {
    verificarLoginTwitch();
    if (localStorage.getItem('twitch_access_token')) {
        await obterPerfilTwitch();
    }
    await carregarDados();
    iniciarTemporizadoresTrocas();
    setInterval(carregarDados, 15000);
}

// Iniciar a aplicação
inicializarApp();

// Controlo do Painel Lateral (Sidebar) de Pontos e Raridades
document.getElementById('btn-abrir-sidebar').onclick = (e) => {
    e.stopPropagation();
    document.getElementById('sidebar-pontos').classList.add('active');
};

document.getElementById('btn-fechar-sidebar').onclick = () => {
    document.getElementById('sidebar-pontos').classList.remove('active');
};

// Fechar a sidebar se clicar fora dela no ecrã
window.addEventListener('click', (event) => {
    const sidebar = document.getElementById('sidebar-pontos');
    if (sidebar.classList.contains('active') && event.target !== sidebar && !sidebar.contains(event.target)) {
        sidebar.classList.remove('active');
    }
});

// Renderizar estatísticas de lançamentos e feed recente
function renderizarEstatisticas(estatisticas, recentes) {
    const ballsContainer = document.getElementById('stats-balls-container');
    const feedBody = document.getElementById('stats-recent-feed');

    const ballLabels = {
        normal: { name: "Pokébola", img: "balls/close_1.png", class: "ball-normal" },
        super: { name: "Super Bola", img: "balls/close_2.png", class: "ball-super" },
        ultra: { name: "Ultra Bola", img: "balls/close_3.png", class: "ball-ultra" },
        master: { name: "Master Bola", img: "balls/close_4.png", class: "ball-master" }
    };

    if (ballsContainer) {
        ballsContainer.innerHTML = '';
        const keys = ["normal", "super", "ultra", "master"];
        keys.forEach(k => {
            const info = ballLabels[k];
            const data = estatisticas[k] || { total: 0, sucesso: 0, rate: "0.0" };
            
            const card = document.createElement('div');
            card.className = `stats-ball-card ${info.class}`;
            card.innerHTML = `
                <div class="ball-card-header">
                    <span>${info.name}</span>
                    <div class="ball-img-wrapper">
                        <img src="${info.img}" alt="${info.name}">
                    </div>
                </div>
                <div class="ball-stats-numbers">
                    Lançamentos: <strong>${data.total}</strong><br>
                    Capturas: <strong>${data.sucesso}</strong>
                </div>
                <div>
                    <div class="ball-rate-value">${data.rate}%</div>
                    <div class="stats-progress-bar-container">
                        <div class="stats-progress-bar" style="width: ${data.rate}%"></div>
                    </div>
                </div>
            `;
            ballsContainer.appendChild(card);
        });
    }

    if (feedBody) {
        if (recentes.length === 0) {
            feedBody.innerHTML = `
                <tr>
                    <td colspan="5" style="text-align: center; color: var(--text-muted); padding: 20px;">Nenhum lançamento registado ainda.</td>
                </tr>
            `;
            return;
        }

        feedBody.innerHTML = '';
        recentes.forEach(row => {
            const elemInfo = elementaisMap.find(e => e.id === row.elementalId);
            const elemName = elemInfo ? (elemInfo.isUser ? elemInfo.name : obterNomeSimplesBicho(row.elementalId)) : row.elementalId;

            const info = ballLabels[row.bola] || { name: row.bola, img: "balls/close_1.png" };
            const dataFormatada = row.date ? row.date.substring(5, 16) : 'Pendente'; // Mostrar "MM-DD HH:MM"

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="recent-time">${dataFormatada}</td>
                <td><strong>@${row.username}</strong></td>
                <td>${elemName}</td>
                <td>
                    <div class="recent-ball-wrapper">
                        <img src="${info.img}" class="recent-ball-icon" alt="${info.name}">
                    </div>
                    ${info.name}
                </td>
                <td>
                    <span class="badge-outcome ${row.sucesso ? 'sucesso' : 'falha'}">
                        ${row.sucesso ? 'Sucesso' : 'Falhou'}
                    </span>
                </td>
            `;
            feedBody.appendChild(tr);
        });
    }
}

function obterNomeSimplesBicho(id) {
    const partes = id.split('_');
    const especie = parseInt(partes[0]);
    const variante = parseInt(partes[1]);
    
    const nomesEspecies = {
        1: "Água", 2: "Terra", 3: "Fogo", 4: "Pato", 5: "Fantasma", 
        6: "Dos Sonhos", 7: "Demónio", 8: "Punk", 9: "Rei", 10: "Ponto Zero",
        11: "BurntPeanut", 12: "Peixoto", 13: "Atacante", 14: "Aura", 
        15: "Boss", 16: "Grim", 17: "Ar", 18: "Seven", 19: "Batman",
        20: "Vini JR", 21: "Pollo"
    };

    const nomesVariantes = {
        1: "Normal", 2: "Gold", 3: "Gummy", 4: "Galaxy", 5: "Holofoil"
    };

    const nomeBase = nomesEspecies[especie] || id;
    if (especie === 11 || especie === 20 || especie === 21) return nomesEspecies[especie] || id;
    const nomeVar = nomesVariantes[variante] || "";
    return `${nomeBase} (${nomeVar})`;
}

let intervalTemporizadoresTrocas = null;
function iniciarTemporizadoresTrocas() {
    if (intervalTemporizadoresTrocas) clearInterval(intervalTemporizadoresTrocas);
    
    const atualizarTimers = () => {
        const badges = document.querySelectorAll('.trade-timer-badge');
        badges.forEach(badge => {
            const endtimeStr = badge.getAttribute('data-endtime');
            if (!endtimeStr || endtimeStr === 'null') {
                badge.style.display = 'none';
                return;
            }
            const endTime = new Date(endtimeStr).getTime();
            const now = new Date().getTime();
            const diff = endTime - now;
            
            if (diff <= 0) {
                badge.innerText = "Pronta!";
                badge.classList.add('ready');
                return;
            }
            
            const dias = Math.floor(diff / (1000 * 60 * 60 * 24));
            const horas = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            const minutos = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
            const segundos = Math.floor((diff % (1000 * 60)) / 1000);
            
            let texto = '';
            if (dias > 0) {
                texto = `+1 em ${dias}d ${horas}h`;
            } else if (horas > 0) {
                texto = `+1 em ${horas}h ${minutos}m`;
            } else {
                texto = `+1 em ${minutos}m ${segundos}s`;
            }
            badge.innerText = texto;
        });
    };
    
    atualizarTimers();
    intervalTemporizadoresTrocas = setInterval(atualizarTimers, 1000);
}

function calcularInventarioGlobal() {
    const globalInv = {};
    elementaisMap.forEach(elem => {
        globalInv[elem.id] = 0;
    });

    dadosGlobais.forEach(player => {
        if (!player.inventario) return;
        for (let elemId in player.inventario) {
            const qty = player.inventario[elemId] || 0;
            if (qty > 0) {
                globalInv[elemId] = (globalInv[elemId] || 0) + qty;
            }
        }
    });

    return globalInv;
}

function selecionarInventarioGlobal() {
    document.querySelectorAll('.ranking-list .ranking-item').forEach(item => item.classList.remove('active'));

    const globalInv = calcularInventarioGlobal();
    const totalCatalog = elementaisMap.length;
    
    let uniqueOwned = 0;
    let totalPoints = 0;
    elementaisMap.forEach(elem => {
        const qty = globalInv[elem.id] || 0;
        if (qty > 0) uniqueOwned++;
    });

    dadosGlobais.forEach(p => totalPoints += (p.pontos || 0));

    const globalPlayer = {
        username: "Inventário Global",
        pontos: totalPoints,
        inventario: globalInv
    };

    jogadorSelecionado = globalPlayer;

    document.getElementById('view-title').textContent = "Álbum Global (Treinadores Acumulados)";
    document.getElementById('user-points-panel').classList.remove('hidden');
    document.getElementById('user-points-label-container').style.display = 'none';
    document.getElementById('user-progress-val').textContent = `${uniqueOwned} / ${totalCatalog}`;

    const pct = totalCatalog > 0 ? (uniqueOwned / totalCatalog) * 100 : 0;
    document.getElementById('user-progress-bar').style.width = `${pct}%`;

    document.getElementById('btn-propor-troca').classList.add('hidden');
    document.getElementById('btn-definir-conta').classList.add('hidden');

    renderizarGridColecao(globalPlayer, 'site-grid', false);
    renderizarPropostas();
}

function mostrarPossuidoresCard(elem) {
    const title = document.getElementById('modal-owners-title');
    const list = document.getElementById('modal-owners-list');
    if (!title || !list) return;

    const nomeAmigavel = obterNomeSimplesBicho(elem.id);
    title.textContent = `Quem possui ${nomeAmigavel}?`;

    list.innerHTML = '';

    const possuidores = [];
    dadosGlobais.forEach(player => {
        const qty = player.inventario[elem.id] || 0;
        if (qty > 0) {
            possuidores.push({ username: player.username, qty, playerObj: player });
        }
    });

    possuidores.sort((a, b) => b.qty - a.qty);

    if (possuidores.length === 0) {
        list.innerHTML = '<p style="text-align: center; color: var(--text-muted); margin: 20px 0; font-size: 12px;">Nenhum treinador possui este elemental ainda. 😢</p>';
    } else {
        possuidores.forEach(pos => {
            const item = document.createElement('div');
            item.className = 'modal-owner-item';
            item.innerHTML = `
                <span>@${pos.username}</span>
                <span class="censo-badge-total">x${pos.qty}</span>
            `;
            item.onclick = () => {
                document.getElementById('modal-owners').classList.add('hidden');
                
                const rankingItems = document.querySelectorAll('.ranking-list .ranking-item');
                let foundEl = null;
                rankingItems.forEach(el => {
                    if (el.textContent.includes(`@${pos.username}`)) {
                        foundEl = el;
                    }
                });
                
                selecionarUtilizador(pos.playerObj, foundEl);
            };
            list.appendChild(item);
        });
    }

    document.getElementById('modal-owners').classList.remove('hidden');
}

document.getElementById('btn-fechar-modal-owners').onclick = () => {
    document.getElementById('modal-owners').classList.add('hidden');
};

window.addEventListener('click', (event) => {
    const modalOwners = document.getElementById('modal-owners');
    if (event.target === modalOwners) {
        modalOwners.classList.add('hidden');
    }
});
