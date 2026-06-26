// Dicionário de mapeamento idêntico ao do teu jogo para detetar os ficheiros na pasta Sprites
const elementaisMap = [
    { id: "1_1", file: "T_Icon_BR_Creature_Sprite_Water_Unvault_Ch7S3_ui_L.webp" },
    { id: "1_2", file: "T_Icon_BR_Creature_Sprite_Water_Gold_ui_L.webp" },
    { id: "1_3", file: "T_Icon_BR_Creature_Sprite_Water_Candy_ui_L.webp" },
    { id: "1_4", file: "T_Icon_BR_Creature_Sprite_Water_Galaxy_ui_L.webp" },
    { id: "2_1", file: "T_Icon_BR_Creature_Sprite_Earth_Ch7S3_UI_L.webp" },
    { id: "2_2", file: "T_Icon_BR_Creature_Sprite_Earth_Gold_ui_L.webp" },
    { id: "2_3", file: "T_Icon_BR_Creature_Sprite_Earth_Candy_ui_L.webp" },
    { id: "2_4", file: "T_Icon_BR_Creature_Sprite_Earth_Galaxy_ui_L.webp" },
    { id: "3_1", file: "T_Icon_BR_Creature_Sprite_Fire_Unvault_Ch7S3_ui_L.webp" },
    { id: "3_2", file: "T_Icon_BR_Creature_Sprite_Fire_Gold_ui_L.webp" },
    { id: "3_3", file: "T_Icon_BR_Creature_Sprite_Fire_Candy_ui_L.webp" },
    { id: "3_4", file: "T_Icon_BR_Creature_Sprite_Fire_Galaxy_ui_L.webp" },
    { id: "4_1", file: "T_Icon_BR_Duck_Default_L.webp" },
    { id: "4_2", file: "T_Icon_BR_Duck_Gold_L.webp" },
    { id: "4_3", file: "T_Icon_BR_Duck_Candy_L.webp" },
    { id: "4_4", file: "T_Icon_BR_Duck_Galaxy_L.webp" },
    { id: "5_1", file: "T_Icon_BR_Creature_Sprite_Ghost_Unvault_L.webp" },
    { id: "5_2", file: "T_Icon_BR_Creature_Sprite_Ghost_Gold_L.webp" },
    { id: "5_3", file: "T_Icon_BR_Creature_Sprite_Ghost_Candy_L.webp" },
    { id: "5_4", file: "T_Icon_BR_Creature_Sprite_Ghost_Galaxy_L.webp" },
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
    { id: "10_1", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_ui_L.webp" },
    { id: "10_2", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Gold_ui_L.webp" },
    { id: "10_3", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Candy_ui_L.webp" },
    { id: "10_4", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Galaxy_ui_L.webp" },
    { id: "11_1", file: "T_Icon_BR_Creature_Sprite_BurntPeanut_ui_L.webp" }
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
        const baseLength = 43;
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
        
        // Se já tivermos um jogador selecionado, atualiza a exibição dele
        if (jogadorSelecionado) {
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
        if (jogadorSelecionado && jogadorSelecionado.username.toLowerCase() === player.username.toLowerCase()) {
            item.classList.add('active');
        }
        item.innerHTML = `
            <span><strong>${index + 1}º</strong> @${player.username}</span>
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
    document.getElementById('user-points-val').textContent = player.pontos;
    document.getElementById('user-points-panel').classList.remove('hidden');

    const btnPropor = document.getElementById('btn-propor-troca');
    const btnDefinir = document.getElementById('btn-definir-conta');

    btnDefinir.classList.add('hidden'); // Ocultar, pois usamos login da Twitch seguro

    if (meuUsername) {
        if (meuUsername.toLowerCase() === player.username.toLowerCase()) {
            btnPropor.classList.add('hidden');
        } else {
            btnPropor.classList.remove('hidden');
        }
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
            slot.onclick = () => {
                grid.querySelectorAll('.grid-item').forEach(i => i.classList.remove('selected'));
                slot.classList.add('selected');
                selectCallback(elem.id, elem);
            };
        } else if (!isSelectionMode) {
            // FLUXO DE ATALHO DE CLIQUE EM ELEMENTAL QUE EU TENHO (Apenas se for o dono autenticado)
            slot.onclick = () => {
                if (qty >= 1) {
                    if (!meuUsername) {
                        alert("Por favor, entra com a tua conta da Twitch no topo da página para poderes propor trocas!");
                        return;
                    }
                    if (meuUsername.toLowerCase() === player.username.toLowerCase()) {
                        oferecidoId = elem.id;
                        abrirModalTrocaComCardPreSelecionado(elem);
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
    
    if (meuUsername) {
        container.innerHTML = `
            <div class="profile-info">
                ${avatar ? `<img src="${avatar}" class="twitch-avatar" alt="Avatar">` : ''}
                <span>Olá, <strong>@${meuUsername}</strong>!</span>
            </div>
            <button class="logout-btn" onclick="limparConta()">Sair ✖</button>
        `;
    } else {
        const clientId = "9xa7l560hxwruznfapxxrwm3543fk0";
        // Usa o URL de produção ou localhost dinamicamente dependendo de onde o site está a rodar
        const redirectUri = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
            ? window.location.origin + '/'
            : 'https://z3rgtv.github.io/elementais/';
            
        const authUrl = `https://id.twitch.tv/oauth2/authorize?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&response_type=token&scope=`;
        
        container.innerHTML = `
            <a href="${authUrl}" class="twitch-login-btn">
                <svg class="twitch-icon" viewBox="0 0 24 24" width="16" height="16" style="vertical-align: middle; margin-right: 6px;">
                    <path fill="currentColor" d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"/>
                </svg>
                Entrar com a Twitch
            </a>
        `;
    }
}

// Ocultar e ignorar botão legado
document.getElementById('btn-definir-conta').classList.add('hidden');

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

        const propNomeLegivel = elemPropInfo.isUser ? elemPropInfo.name : p.elemProposer;
        const targNomeLegivel = elemTargInfo.isUser ? elemTargInfo.name : p.elemTarget;

        let content = '';
        if (souAlvo) {
            content = `
                <div class="prop-players-info">
                    <span><strong>@${p.proposerName}</strong> quer:</span>
                    <div class="prop-visual-swap">
                        <img src="Sprites/${elemPropInfo.file}" class="prop-card-mini" title="${propNomeLegivel}">
                        <span class="prop-arrow">➡️</span>
                        <img src="Sprites/${elemTargInfo.file}" class="prop-card-mini" title="${targNomeLegivel}">
                    </div>
                    <span>pelo teu elemental.</span>
                </div>
                <div class="prop-actions">
                    <button class="btn btn-sm" onclick="copiarComandoResposta('sim', '${p.proposerName}', this)">Aceitar (sim) 🤝</button>
                    <button class="btn btn-sm btn-danger" onclick="copiarComandoResposta('nao', '${p.proposerName}', this)">Recusar (nao) ❌</button>
                </div>
            `;
        } else {
            content = `
                <div class="prop-players-info">
                    <span>Ofereceste a <strong>@${p.targetName}</strong>:</span>
                    <div class="prop-visual-swap">
                        <img src="Sprites/${elemPropInfo.file}" class="prop-card-mini" title="${propNomeLegivel}">
                        <span class="prop-arrow">➡️</span>
                        <img src="Sprites/${elemTargInfo.file}" class="prop-card-mini" title="${targNomeLegivel}">
                    </div>
                    <span>(Aguardando resposta dele no chat).</span>
                </div>
                <div class="prop-actions">
                    <span style="font-size: 12px; color: var(--text-muted);">Enviada</span>
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

        if (jaTemLimite) {
            item.classList.add('disabled-trade');
            item.innerHTML = `
                <span><strong style="color: #ff5555;">@${player.username}</strong> <small style="color: #ff5555; font-size: 10px; margin-left: 5px;">(Já tem 2)</small></span>
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
        const jaTemLimite = qtyTarget >= 2;

        const commandInput = document.getElementById('twitch-command-input');
        const btnCopiar = document.getElementById('btn-copiar-comando');

        if (jaTemLimite) {
            commandInput.value = `Inválido: @${targetUser} já tem o limite de 2 cópias!`;
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
    setInterval(carregarDados, 15000);
}

// Iniciar a aplicação
inicializarApp();
