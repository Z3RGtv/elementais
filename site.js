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

async function carregarDados() {
    try {
        // Evita que o browser faça cache do ficheiro antigo injetando um carimbo de tempo (?t=...)
        const res = await fetch(`inventario.json?t=${new Date().getTime()}`);
        const data = await res.json();
        dadosGlobais = data.ranking || [];
        
        document.getElementById('update-timer').textContent = `Última sincronização com a Stream: ${data.updatedAt || 'Pendente'}`;
        
        // Fazer varredura de elementais de utilizadores dinamicamente a partir dos inventários
        const userSet = new Set();
        dadosGlobais.forEach(p => {
            if (p.inventario) {
                Object.keys(p.inventario).forEach(id => {
                    if (id.startsWith('u_')) {
                        userSet.add(id);
                    }
                });
            }
        });

        // Limpar entradas anteriores de utilizadores em elementaisMap (o mapa base tem 43 itens até BurntPeanut)
        const baseLength = 43;
        elementaisMap.length = baseLength;

        // Inserir os utilizadores encontrados ordenados alfabeticamente a seguir ao BurntPeanut
        Array.from(userSet).sort().forEach(id => {
            const nameWithoutPrefix = id.substring(2);
            elementaisMap.push({
                id: id,
                file: `Users/${nameWithoutPrefix}.png`,
                isUser: true,
                name: nameWithoutPrefix
            });
        });
        
        renderizarRanking(dadosGlobais);
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
    if(elementoDOM) elementoDOM.classList.add('active');

    document.getElementById('view-title').textContent = `Coleção de @${player.username}`;
    document.getElementById('user-points-val').textContent = player.pontos;
    document.getElementById('user-points-panel').classList.remove('hidden');

    const grid = document.getElementById('site-grid');
    grid.innerHTML = '';

    elementaisMap.forEach(elem => {
        const qty = player.inventario[elem.id] || 0;
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

        // Adicionar uma etiqueta pequena com o nome nos elementais de User
        if (elem.isUser) {
            const nameTag = document.createElement('span');
            nameTag.className = 'user-name-tag';
            nameTag.textContent = elem.name;
            slot.appendChild(nameTag);
        }

        grid.appendChild(slot);
    });
}

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

