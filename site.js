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
    { id: "5_3", file: "T_Icon_BR_Creature_Sprite_Ghost_Candy_ui_L.webp" },
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
let jogadorSelecionado = null;
let propostasAtivas = [];

// Seletores do Assistente de Troca
let oferecidoId = null;
let pedidoId = null;

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
                // Manter o DOM destacado encontrando o elemento correspondente
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

    // Controlar visibilidade dos botões de ação de troca
    const btnPropor = document.getElementById('btn-propor-troca');
    const btnDefinir = document.getElementById('btn-definir-conta');

    if (meuUsername) {
        if (meuUsername.toLowerCase() === player.username.toLowerCase()) {
            btnPropor.classList.add('hidden');
            btnDefinir.classList.add('hidden');
        } else {
            btnPropor.classList.remove('hidden');
            btnDefinir.classList.remove('hidden');
            btnDefinir.textContent = "Esta é a minha Conta 👤";
        }
    } else {
        btnPropor.classList.add('hidden');
        btnDefinir.classList.remove('hidden');
        btnDefinir.textContent = "Esta é a minha Conta 👤";
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
        }

        grid.appendChild(slot);
    });
}

// Lógica de Conta Local (Minha Conta)
function atualizarUIConta() {
    const container = document.getElementById('my-profile-container');
    if (meuUsername) {
        container.innerHTML = `
            <span>Olá, <strong>@${meuUsername}</strong>!</span>
            <button onclick="limparConta()">Alterar ✖</button>
        `;
    } else {
        container.innerHTML = `
            <span style="font-size: 12px; color: var(--text-muted);">Identidade Twitch não configurada.</span>
        `;
    }
}

document.getElementById('btn-definir-conta').onclick = () => {
    if (jogadorSelecionado) {
        meuUsername = jogadorSelecionado.username;
        localStorage.setItem('meuUsername', meuUsername);
        atualizarUIConta();
        
        // Atualiza botões
        selecionarUtilizador(jogadorSelecionado, document.querySelector('.ranking-item.active'));
    }
};

function limparConta() {
    meuUsername = '';
    localStorage.removeItem('meuUsername');
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

    // Filtrar propostas onde eu sou o proponente ou o alvo
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

document.getElementById('btn-propor-troca').onclick = () => {
    if (!meuUsername || !jogadorSelecionado) return;
    
    // Configurar nomes no modal
    document.getElementById('modal-target-username').textContent = `@${jogadorSelecionado.username}`;
    
    // Obter objeto do proponente (eu)
    const meuPerfil = dadosGlobais.find(p => p.username.toLowerCase() === meuUsername.toLowerCase()) || { inventario: {} };

    // Limpar estados
    oferecidoId = null;
    pedidoId = null;
    document.getElementById('troca-resumo-container').classList.add('hidden');

    // Renderizar seleções de cartas
    renderizarGridColecao(meuPerfil, 'my-offer-card-selection', true, (id, elem) => {
        oferecidoId = id;
        atualizarResumoTroca(elem, null);
    });

    renderizarGridColecao(jogadorSelecionado, 'target-request-card-selection', true, (id, elem) => {
        pedidoId = id;
        atualizarResumoTroca(null, elem);
    });

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

let cacheOferecido = null;
let cachePedido = null;

function atualizarResumoTroca(elemOferecido, elemPedido) {
    if (elemOferecido) cacheOferecido = elemOferecido;
    if (elemPedido) cachePedido = elemPedido;

    if (oferecidoId && pedidoId && cacheOferecido && cachePedido) {
        // Exibir previews
        const divOferecido = document.getElementById('resumo-oferecido-img');
        const divPedido = document.getElementById('resumo-pedido-img');

        divOferecido.innerHTML = `<img src="Sprites/${cacheOferecido.file}">`;
        divPedido.innerHTML = `<img src="Sprites/${cachePedido.file}">`;

        document.getElementById('resumo-oferecido-nome').textContent = cacheOferecido.isUser ? cacheOferecido.name : cacheOferecido.id;
        document.getElementById('resumo-pedido-nome').textContent = cachePedido.isUser ? cachePedido.name : cachePedido.id;

        // Comando Twitch formatado
        const command = `${jogadorSelecionado.username} ${oferecidoId} ${pedidoId}`;
        document.getElementById('twitch-command-input').value = command;

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

// Configuração da Barra de Pesquisa Dinâmica
document.getElementById('search-input').addEventListener('input', (e) => {
    const termo = e.target.value.toLowerCase().trim();
    if (!termo) {
        renderizarRanking(dadosGlobais);
        return;
    }
    const filtrados = dadosGlobais.filter(p => p.username.toLowerCase().includes(termo));
    renderizarRanking(filtrados);
});

// Execução inicial e Polling em background (15 segundos)
carregarDados();
setInterval(carregarDados, 15000);
