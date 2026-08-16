const ESTADO_FILE = 'jogo_estado.txt';
const POLLING_INTERVAL = 200; // 200ms
let ultimoComando = "";
let colecaoTimeout = null;
let colecaoInterval = null;
let alertTimeout = null;

// Precarregar imagens das Pokébolas para evitar flicker e atrasos
const preloadedImages = [];
function precarregarBolas() {
    for (let i = 1; i <= 4; i++) {
        const imgClose = new Image();
        imgClose.src = `balls/close_${i}.png`;
        preloadedImages.push(imgClose);
        const imgOpen = new Image();
        imgOpen.src = `balls/open_${i}.png`;
        preloadedImages.push(imgOpen);
    }
}
precarregarBolas();

// Gestão de estado das animações de arremesso
let isThrowing = false;
let pendingResult = null;
let currentUsername = "";
let currentBolaIndex = 1;
let currentGhostAtivo = false;
let ghostSmokeInterval = null;
let ghostGlideInterval = null;
let ghostRevelado = false;

// Lista estática de todos os 41 elementais agrupados por espécie (4 variantes por espécie, BurntPeanut no final)
const allElementais = [
    // 1. Água
    { id: "1_1", file: "T_Icon_BR_Creature_Sprite_Water_Unvault_Ch7S3_ui_L.webp", name: "Água Normal" },
    { id: "1_2", file: "T_Icon_BR_Creature_Sprite_Water_Gold_ui_L.webp", name: "Água Gold" },
    { id: "1_3", file: "T_Icon_BR_Creature_Sprite_Water_Candy_ui_L.webp", name: "Água Gummy" },
    { id: "1_4", file: "T_Icon_BR_Creature_Sprite_Water_Galaxy_ui_L.webp", name: "Água Galaxy" },
    { id: "1_5", file: "T_Icon_BR_Creature_Sprite_Water_Holofoil_ui_L.webp", name: "Água Holofoil" },
    { id: "1_7", file: "T_Icon_BR_Creature_Sprite_Water_Gem_ui_L.webp", name: "Água Gem" },
    { id: "1_8", file: "T_Icon_BR_Creature_Sprite_Water_Quack_ui_L.webp", name: "Água Quack" },
    
    // 2. Terra
    { id: "2_1", file: "T_Icon_BR_Creature_Sprite_Earth_Ch7S3_UI_L.webp", name: "Terra Normal" },
    { id: "2_2", file: "T_Icon_BR_Creature_Sprite_Earth_Gold_ui_L.webp", name: "Terra Gold" },
    { id: "2_3", file: "T_Icon_BR_Creature_Sprite_Earth_Candy_ui_L.webp", name: "Terra Gummy" },
    { id: "2_4", file: "T_Icon_BR_Creature_Sprite_Earth_Galaxy_ui_L.webp", name: "Terra Galaxy" },
    { id: "2_6", file: "T_Icon_BR_Creature_Sprite_Earth_Cube_ui_L.webp", name: "Terra Cube" },
    { id: "2_7", file: "T_Icon_BR_Creature_Sprite_Earth_Gem_ui_L.webp", name: "Terra Gem" },
    { id: "2_8", file: "T_Icon_BR_Creature_Sprite_Earth_Quack_ui_L.webp", name: "Terra Quack" },
    
    // 3. Fogo
    { id: "3_1", file: "T_Icon_BR_Creature_Sprite_Fire_Unvault_Ch7S3_ui_L.webp", name: "Fogo Normal" },
    { id: "3_2", file: "T_Icon_BR_Creature_Sprite_Fire_Gold_ui_L.webp", name: "Fogo Gold" },
    { id: "3_3", file: "T_Icon_BR_Creature_Sprite_Fire_Candy_ui_L.webp", name: "Fogo Gummy" },
    { id: "3_4", file: "T_Icon_BR_Creature_Sprite_Fire_Galaxy_ui_L.webp", name: "Fogo Galaxy" },
    { id: "3_5", file: "T_Icon_BR_Creature_Sprite_Fire_Holofoil_ui_L.webp", name: "Fogo Holofoil" },
    { id: "3_6", file: "T_Icon_BR_Creature_Sprite_Fire_Cube_ui_L.webp", name: "Fogo Cube" },
    { id: "3_8", file: "T_Icon_BR_Creature_Sprite_Fire_Quack_ui_L.webp", name: "Fogo Quack" },
    
    // 4. Pato
    { id: "4_1", file: "T_Icon_BR_Duck_Default_L.webp", name: "Pato Normal" },
    { id: "4_2", file: "T_Icon_BR_Duck_Gold_L.webp", name: "Pato Gold" },
    { id: "4_3", file: "T_Icon_BR_Duck_Candy_L.webp", name: "Pato Gummy" },
    { id: "4_4", file: "T_Icon_BR_Duck_Galaxy_L.webp", name: "Pato Galaxy" },
    { id: "4_7", file: "T_Icon_BR_Duck_Gem_L.webp", name: "Pato Gem" },
    
    // 5. Fantasma
    { id: "5_1", file: "T_Icon_BR_Creature_Sprite_Ghost_Unvault_L.webp", name: "Fantasma Normal" },
    { id: "5_2", file: "T_Icon_BR_Creature_Sprite_Ghost_Gold_L.webp", name: "Fantasma Gold" },
    { id: "5_3", file: "T_Icon_BR_Creature_Sprite_Ghost_Candy_L.webp", name: "Fantasma Gummy" },
    { id: "5_4", file: "T_Icon_BR_Creature_Sprite_Ghost_Galaxy_L.webp", name: "Fantasma Galaxy" },
    { id: "5_5", file: "T_Icon_BR_Creature_Sprite_Ghost_Holo_L.webp", name: "Fantasma Holofoil" },
    
    // 6. Dos Sonhos
    { id: "6_1", file: "T_Icon_BR_Creature_Sprite_Sleepy_ui_L.webp", name: "Dos Sonhos Normal" },
    { id: "6_2", file: "T_Icon_BR_Creature_Sprite_Sleepy_Gold_ui_L.webp", name: "Dos Sonhos Gold" },
    { id: "6_3", file: "T_Icon_BR_Creature_Sprite_Sleepy_Candy_ui_L.webp", name: "Dos Sonhos Gummy" },
    { id: "6_4", file: "T_Icon_BR_Creature_Sprite_Sleepy_Galaxy_ui_L.webp", name: "Dos Sonhos Galaxy" },
    { id: "6_6", file: "T_Icon_BR_Creature_Sprite_Sleepy_Cube_ui_L.webp", name: "Dos Sonhos Cube" },
    
    // 7. Demónio
    { id: "7_1", file: "T_Icon_BR_RedDemon_Default_L.webp", name: "Demónio Normal" },
    { id: "7_2", file: "T_Icon_BR_RedDemon_Gold_L.webp", name: "Demónio Gold" },
    { id: "7_3", file: "T_Icon_BR_RedDemon_Candy_L.webp", name: "Demónio Gummy" },
    { id: "7_4", file: "T_Icon_BR_RedDemon_Galaxy_L.webp", name: "Demónio Galaxy" },
    { id: "7_7", file: "T_Icon_BR_RedDemon_Gem_L.webp", name: "Demónio Gem" },
    
    // 8. Punk
    { id: "8_1", file: "T_Icon_BR_Creature_Sprite_Punk_ui_L.webp", name: "Punk Normal" },
    { id: "8_2", file: "T_Icon_BR_Creature_Sprite_Punk_Gold_ui_L.webp", name: "Punk Gold" },
    { id: "8_3", file: "T_Icon_BR_Creature_Sprite_Punk_Candy_ui_L.webp", name: "Punk Gummy" },
    { id: "8_4", file: "T_Icon_BR_Creature_Sprite_Punk_Galaxy_ui_L.webp", name: "Punk Galaxy" },
    { id: "8_6", file: "T_Icon_BR_Creature_Sprite_Punk_Cube_ui_L.webp", name: "Punk Cube" },
    
    // 9. Rei
    { id: "9_1", file: "T_Icon_BR_Creature_Sprite_King_ui_L.webp", name: "Rei Normal" },
    { id: "9_2", file: "T_Icon_BR_Creature_Sprite_King_Gold_ui_L.webp", name: "Rei Gold" },
    { id: "9_3", file: "T_Icon_BR_Creature_Sprite_King_Candy_ui_L.webp", name: "Rei Gummy" },
    { id: "9_4", file: "T_Icon_BR_Creature_Sprite_King_Galaxy_ui_L.webp", name: "Rei Galaxy" },
    { id: "9_5", file: "T_Icon_BR_Creature_Sprite_King_Holofoil_ui_L.webp", name: "Rei Holofoil" },
    
    // 10. Ponto Zero
    { id: "10_1", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_ui_L.webp", name: "Ponto Zero Normal" },
    { id: "10_2", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Gold_ui_L.webp", name: "Ponto Zero Gold" },
    { id: "10_3", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Candy_ui_L.webp", name: "Ponto Zero Gummy" },
    { id: "10_4", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Galaxy_ui_L.webp", name: "Ponto Zero Galaxy" },
    { id: "10_5", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Holofoil_ui_L.webp", name: "Ponto Zero Holofoil" },
    { id: "10_6", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Cube_ui_L.webp", name: "Ponto Zero Cube" },
    { id: "10_7", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Gem_ui_L.webp", name: "Ponto Zero Gem" },
    { id: "10_8", file: "T_Icon_BR_Creature_Sprite_ZeroPoint_Quack_ui_L.webp", name: "Ponto Zero Quack" },
    
    // 11. BurntPeanut
    { id: "11_1", file: "T_Icon_BR_Creature_Sprite_BurntPeanut_ui_L.webp", name: "BurntPeanut" },

    // 12. Peixoto
    { id: "12_1", file: "T_Icon_BR_Creature_Sprite_Fishy_ui_L.webp", name: "Peixoto Normal" },
    { id: "12_2", file: "T_Icon_BR_Creature_Sprite_Fishy_Gold_ui_L.webp", name: "Peixoto Gold" },
    { id: "12_3", file: "T_Icon_BR_Creature_Sprite_Fishy_Candy_ui_L.webp", name: "Peixoto Gummy" },
    { id: "12_4", file: "T_Icon_BR_Creature_Sprite_Fishy_Galaxy_ui_L.webp", name: "Peixoto Galaxy" },
    { id: "12_6", file: "T_Icon_BR_Creature_Sprite_Fishy_Cube_L.webp", name: "Peixoto Cube" },

    // 13. Atacante
    { id: "13_1", file: "T_Icon_BR_Creature_Sprite_Soccer_ui_L.webp", name: "Atacante Normal" },
    { id: "13_2", file: "T_Icon_BR_Creature_Sprite_Soccer_Gold_L.webp", name: "Atacante Gold" },
    { id: "13_3", file: "T_Icon_BR_Creature_Sprite_Soccer_Candy_L.webp", name: "Atacante Gummy" },
    { id: "13_4", file: "T_Icon_BR_Creature_Sprite_Soccer_Galaxy_L.webp", name: "Atacante Galaxy" },
    { id: "13_5", file: "T_Icon_BR_Creature_Sprite_Soccer_Holofoil_L.webp", name: "Atacante Holofoil" },

    // 14. Aura
    { id: "14_1", file: "T_Icon_BR_Creature_Sprite_Drifter_ui_L.webp", name: "Aura Normal" },
    { id: "14_2", file: "T_Icon_BR_Creature_Sprite_Drifter_Gold_ui_L.webp", name: "Aura Gold" },
    { id: "14_3", file: "T_Icon_BR_Creature_Sprite_Drifter_Candy_ui_L.webp", name: "Aura Gummy" },
    { id: "14_4", file: "T_Icon_BR_Creature_Sprite_Drifter_Galaxy_ui_L.webp", name: "Aura Galaxy" },
    { id: "14_7", file: "T_Icon_BR_Creature_Sprite_Drifter_Gem_ui_L.webp", name: "Aura Gem" },

    // 15. Boss
    { id: "15_1", file: "T_Icon_BR_Creature_Sprite_Boss_ui_L.webp", name: "Boss Normal" },
    { id: "15_2", file: "T_Icon_BR_Creature_Sprite_Boss_Gold_ui_L.webp", name: "Boss Gold" },
    { id: "15_3", file: "T_Icon_BR_Creature_Sprite_Boss_Candy_ui_L.webp", name: "Boss Gummy" },
    { id: "15_4", file: "T_Icon_BR_Creature_Sprite_Boss_Galaxy_ui_L.webp", name: "Boss Galaxy" },
    { id: "15_6", file: "T_Icon_BR_Creature_Sprite_Boss_Cube_ui_L.webp", name: "Boss Cube" },

    // 16. Grim
    { id: "16_1", file: "T_Icon_BR_GrimReaper_Default_L.webp", name: "Grim Normal" },
    { id: "16_2", file: "T_Icon_BR_GrimReaper_Gold_L.webp", name: "Grim Gold" },
    { id: "16_3", file: "T_Icon_BR_GrimReaper_Candy_L.webp", name: "Grim Gummy" },
    { id: "16_4", file: "T_Icon_BR_GrimReaper_Galaxy_L.webp", name: "Grim Galaxy" },
    { id: "16_5", file: "T_Icon_BR_GrimReaper_Holofoil_L.webp", name: "Grim Holofoil" },
    { id: "16_6", file: "T_Icon_BR_GrimReaper_Cube_L.webp", name: "Grim Cube" },
    { id: "16_7", file: "T_Icon_BR_GrimReaper_Gem_L.webp", name: "Grim Gem" },

    // 17. Vento
    { id: "17_1", file: "T_Icon_BR_Air_Default_L.webp", name: "Vento Normal" },
    { id: "17_2", file: "T_Icon_BR_Air_Gold_L.webp", name: "Vento Gold" },
    { id: "17_3", file: "T_Icon_BR_Air_Candy_L.webp", name: "Vento Gummy" },
    { id: "17_4", file: "T_Icon_BR_Air_Galaxy_L.webp", name: "Vento Galaxy" },
    { id: "17_5", file: "T_Icon_BR_Air_Holo_L.webp", name: "Vento Holofoil" },

    // 18. Seven
    { id: "18_1", file: "T_Icon_BR_Creature_Sprite_Seven_ui_L.webp", name: "Seven Normal" },
    { id: "18_2", file: "T_Icon_BR_Creature_Sprite_Seven_Gold_ui_L.webp", name: "Seven Gold" },
    { id: "18_3", file: "T_Icon_BR_Creature_Sprite_Seven_Candy_ui_L.webp", name: "Seven Gummy" },
    { id: "18_4", file: "T_Icon_BR_Creature_Sprite_Seven_Galaxy_ui_L.webp", name: "Seven Galaxy" },
    { id: "18_5", file: "T_Icon_BR_Creature_Sprite_Seven_Holofoil_ui_L.webp", name: "Seven Holofoil" },

    // 19. Batman
    { id: "19_1", file: "T_Icon_BR_FossilMeal_Default_L.webp", name: "Batman Normal" },
    { id: "19_2", file: "T_Icon_BR_FossilMeal_Gold_L.webp", name: "Batman Gold" },
    { id: "19_3", file: "T_Icon_BR_FossilMeal_Candy_L.webp", name: "Batman Gummy" },
    { id: "19_4", file: "T_Icon_BR_FossilMeal_Galaxy_L.webp", name: "Batman Galaxy" },
    { id: "19_5", file: "T_Icon_BR_FossilMeal_Holofoil_L.webp", name: "Batman Holofoil" },
    { id: "19_6", file: "T_Icon_BR_FossilMeal_Cube_L.webp", name: "Batman Cube" },

    // 20. Vini JR
    { id: "20_1", file: "T_Icon_BR_CokeParmesan_Default_L.webp", name: "Vini JR" },

    // 21. Pollo
    { id: "21_1", file: "T_Icon_BR_CompanyStargazer_Default_L.webp", name: "Pollo" },

    // 22. Llama
    { id: "22_1", file: "T_Icon_BR_Creature_Sprite_Llama_ui_L.webp", name: "Llama Normal" },
    { id: "22_2", file: "T_Icon_BR_Creature_Sprite_Llama_Gold_ui_L.webp", name: "Llama Gold" },
    { id: "22_3", file: "T_Icon_BR_Creature_Sprite_Llama_Candy_ui_L.webp", name: "Llama Gummy" },
    { id: "22_4", file: "T_Icon_BR_Creature_Sprite_Llama_Galaxy_ui_L.webp", name: "Llama Galaxy" },
    { id: "22_7", file: "T_Icon_BR_Creature_Sprite_Llama_Gem_ui_L.webp", name: "Llama Gem" },

    // 23. Peely
    { id: "23_1", file: "T_Icon_BR_Creature_Sprite_Peely_ui_L.webp", name: "Peely Normal" },
    { id: "23_2", file: "T_Icon_BR_Creature_Sprite_Peely_Gold_ui_L.webp", name: "Peely Gold" },
    { id: "23_3", file: "T_Icon_BR_Creature_Sprite_Peely_Candy_ui_L.webp", name: "Peely Gummy" },
    { id: "23_4", file: "T_Icon_BR_Creature_Sprite_Peely_Galaxy_ui_L.webp", name: "Peely Galaxy" },
    { id: "23_5", file: "T_Icon_BR_Creature_Sprite_Peely_Holofoil_ui_L.webp", name: "Peely Holofoil" },

    // 24. John Wick
    { id: "24_1", file: "T_Icon_Reload_FillerGrunt_icon_L.webp", name: "John Wick Normal" },

    // 25. Ironmouse
    { id: "25_1", file: "T_Icon_BR_PedicureAntacid_L.webp", name: "Ironmouse" }
];

if (typeof userElementais !== 'undefined') {
    allElementais.push(...userElementais);
}

const elementaisMap = {};
allElementais.forEach(item => {
    elementaisMap[item.id] = item;
});

// Referências UI
const alertBox = document.getElementById('alert-box');
const alertText = document.getElementById('alert-text');
const elementalImg = document.getElementById('elemental-img');
const ballImg = document.getElementById('ball-img');
const collectionPanel = document.getElementById('collection-panel');
const collectionTitle = document.getElementById('collection-title');
const collectionGrid = document.getElementById('collection-grid');

// Referências UI para Trocas
const tradeArea = document.getElementById('trade-area');
const tradeUser1 = document.getElementById('trade-user1');
const tradeUser2 = document.getElementById('trade-user2');
const tradeImg1 = document.getElementById('trade-img1');
const tradeImg2 = document.getElementById('trade-img2');
const tradeCard1 = document.getElementById('trade-card1');
const tradeCard2 = document.getElementById('trade-card2');
let tradeTimeout = null;

// Referências UI para Sprits
const spritArea = document.getElementById('sprit-area');
const spritNotification = document.getElementById('sprit-notification');
const spritParticles = document.getElementById('sprit-particles');
const spritBall = document.getElementById('sprit-ball');
const spritCreature = document.getElementById('sprit-creature');

let spritTimeouts = [];
let spritAmbientInterval = null;

function resetSpritSystem() {
    spritTimeouts.forEach(t => clearTimeout(t));
    spritTimeouts = [];
    if (spritAmbientInterval) {
        clearInterval(spritAmbientInterval);
        spritAmbientInterval = null;
    }
    if (spritArea) spritArea.classList.add('hidden');
    if (spritNotification) spritNotification.classList.remove('show');
    if (spritBall) {
        spritBall.className = 'hidden';
        spritBall.src = '';
    }
    if (spritCreature) {
        spritCreature.className = 'hidden';
        spritCreature.src = '';
        spritCreature.style.transform = '';
    }
    if (spritParticles) spritParticles.innerHTML = '';
}

// Sintetizar som retro "plim" de captura com Web Audio API
function tocarSomPlim() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const playNote = (freq, delay, duration) => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.value = freq;
            gain.gain.setValueAtTime(0, ctx.currentTime + delay);
            gain.gain.linearRampToValueAtTime(0.15, ctx.currentTime + delay + 0.05);
            gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + delay + duration);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(ctx.currentTime + delay);
            osc.stop(ctx.currentTime + delay + duration);
        };
        // Arpejo ascendente rápido e brilhante
        playNote(523.25, 0.0, 0.4);  // C5
        playNote(659.25, 0.06, 0.4); // E5
        playNote(783.99, 0.12, 0.4); // G5
        playNote(1046.50, 0.18, 0.6); // C6
    } catch (e) {
        console.error("Erro ao tocar som:", e);
    }
}

// Sintetizar som retro de troca
function tocarSomTroca() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        // Swoosh (slide de frequências)
        const oscS = ctx.createOscillator();
        const gainS = ctx.createGain();
        oscS.type = 'triangle';
        oscS.frequency.setValueAtTime(250, ctx.currentTime);
        oscS.frequency.exponentialRampToValueAtTime(750, ctx.currentTime + 0.45);
        gainS.gain.setValueAtTime(0, ctx.currentTime);
        gainS.gain.linearRampToValueAtTime(0.08, ctx.currentTime + 0.05);
        gainS.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.45);
        oscS.connect(gainS);
        gainS.connect(ctx.destination);
        oscS.start();
        oscS.stop(ctx.currentTime + 0.45);
        
        // Notas brilhantes de sucesso
        const playChime = (freq, delay, duration) => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.value = freq;
            gain.gain.setValueAtTime(0, ctx.currentTime + delay);
            gain.gain.linearRampToValueAtTime(0.12, ctx.currentTime + delay + 0.02);
            gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + delay + duration);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(ctx.currentTime + delay);
            osc.stop(ctx.currentTime + delay + duration);
        };
        playChime(659.25, 0.4, 0.35); // E5
        playChime(987.77, 0.48, 0.45); // B5
    } catch (e) {
        console.error("Erro ao tocar som de troca:", e);
    }
}

// Sintetizar som retro "swoosh" do arremesso da bola
function tocarSomAtirar() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        // Oscilador para o tom básico de vento/lançamento (onda triângulo)
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(120, ctx.currentTime);
        // Subida rápida de frequência
        osc.frequency.exponentialRampToValueAtTime(700, ctx.currentTime + 0.3);
        
        gain.gain.setValueAtTime(0, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.12, ctx.currentTime + 0.05);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.35);
        
        // Gerador de ruído branco para dar textura de "vento"
        const bufferSize = ctx.sampleRate * 0.35;
        const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }
        
        const noise = ctx.createBufferSource();
        noise.buffer = buffer;
        
        const filter = ctx.createBiquadFilter();
        filter.type = 'bandpass';
        filter.frequency.setValueAtTime(250, ctx.currentTime);
        filter.frequency.exponentialRampToValueAtTime(1200, ctx.currentTime + 0.3);
        filter.Q.value = 3.0;
        
        const noiseGain = ctx.createGain();
        noiseGain.gain.setValueAtTime(0, ctx.currentTime);
        noiseGain.gain.linearRampToValueAtTime(0.04, ctx.currentTime + 0.05);
        noiseGain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.35);
        
        // Ligar oscilador
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        // Ligar ruído
        noise.connect(filter);
        filter.connect(noiseGain);
        noiseGain.connect(ctx.destination);
        
        osc.start();
        noise.start();
        osc.stop(ctx.currentTime + 0.35);
        noise.stop(ctx.currentTime + 0.35);
    } catch (e) {
        console.error("Erro ao tocar som de arremesso:", e);
    }
}

// Sintetizar som retro de chute do Atacante
function tocarSomChutoAtacante() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(150, ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(30, ctx.currentTime + 0.15);
        
        gain.gain.setValueAtTime(0.35, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.25);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.start();
        osc.stop(ctx.currentTime + 0.3);
    } catch (e) {
        console.error("Erro ao tocar som de chuto:", e);
    }
}

// Sintetizar som retro de sucção/feixe de energia (elemental a entrar para a bola)
function tocarSomSugado() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        const duration = 0.75;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        
        // Som metálico/sci-fi usando onda dente-de-serra
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(1400, ctx.currentTime);
        // Descida rápida de frequência
        osc.frequency.exponentialRampToValueAtTime(150, ctx.currentTime + duration);
        
        // Modulador LFO para criar o efeito tremido do feixe laser clássico do Pokémon
        const lfo = ctx.createOscillator();
        const lfoGain = ctx.createGain();
        lfo.frequency.value = 45; // 45Hz vibrato rápido
        lfoGain.gain.value = 250; // Largura do vibrato
        
        // Filtro passa-baixo para tornar o som mais "retro" de consola e menos estridente
        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(1200, ctx.currentTime);
        filter.frequency.exponentialRampToValueAtTime(300, ctx.currentTime + duration);
        filter.Q.value = 5.0;
        
        gain.gain.setValueAtTime(0, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.1, ctx.currentTime + 0.05);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + duration);
        
        // Ligar modulação LFO ao pitch do oscilador
        lfo.connect(lfoGain);
        lfoGain.connect(osc.frequency);
        
        // Ligar sinal principal
        osc.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        
        lfo.start();
        osc.start();
        lfo.stop(ctx.currentTime + duration);
        osc.stop(ctx.currentTime + duration);
    } catch (e) {
        console.error("Erro ao tocar som de sucção:", e);
    }
}

// Sintetizar som de falha/abertura da bola (quando o elemental foge de dentro)
function tocarSomFalha() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        // 1. Som de pop/descompressão (onda triângulo descendente rápida)
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(350, ctx.currentTime);
        osc.frequency.linearRampToValueAtTime(70, ctx.currentTime + 0.22);
        
        gain.gain.setValueAtTime(0.15, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.22);
        
        // 2. Ruído de despressurização ("puff" de libertação)
        const bufferSize = ctx.sampleRate * 0.15;
        const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }
        
        const noise = ctx.createBufferSource();
        noise.buffer = buffer;
        
        const filter = ctx.createBiquadFilter();
        filter.type = 'highpass';
        filter.frequency.value = 800; // Ruído limpo agudo
        
        const noiseGain = ctx.createGain();
        noiseGain.gain.setValueAtTime(0.08, ctx.currentTime);
        noiseGain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.15);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        noise.connect(filter);
        filter.connect(noiseGain);
        noiseGain.connect(ctx.destination);
        
        osc.start();
        noise.start();
        osc.stop(ctx.currentTime + 0.22);
        noise.stop(ctx.currentTime + 0.22);
    } catch (e) {
        console.error("Erro ao tocar som de falha:", e);
    }
}

// Sintetizar som de abano da bola no chão (pequeno clique/tique clássico)
function tocarSomAbano() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'sine';
        osc.frequency.setValueAtTime(950, ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(200, ctx.currentTime + 0.04);
        
        gain.gain.setValueAtTime(0.08, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.04);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.start();
        osc.stop(ctx.currentTime + 0.04);
    } catch (e) {
        console.error("Erro ao tocar som de abano:", e);
    }
}

// Cria pequenas partículas de brilho coloridas flutuando em redor da bola
function criarParticulasBrilho() {
    const container = document.getElementById('game-container');
    const numParticulas = 20;
    
    for (let i = 0; i < numParticulas; i++) {
        const p = document.createElement('div');
        p.className = 'sparkle-particle';
        p.style.left = `calc(50% - 100px)`;
        p.style.top = `calc(45% + 80px)`;
        const angulo = (i / numParticulas) * 2 * Math.PI + (Math.random() - 0.5) * 0.4;
        const dist = 50 + Math.random() * 70;
        const tx = Math.cos(angulo) * dist;
        const ty = Math.sin(angulo) * dist;
        p.style.setProperty('--tx', `${tx}px`);
        p.style.setProperty('--ty', `${ty}px`);
        
        const cores = ['#ffe600', '#2ecc71', '#a154f2', '#ffffff', '#00e5ff'];
        p.style.backgroundColor = cores[Math.floor(Math.random() * cores.length)];
        container.appendChild(p);
        setTimeout(() => p.remove(), 1200);
    }
}

// Cria fumo ao fugir
function criarParticulasFumo() {
    const container = document.getElementById('game-container');
    const numParticulas = 15;
    
    for (let i = 0; i < numParticulas; i++) {
        const p = document.createElement('div');
        p.className = 'smoke-particle';
        const angulo = (i / numParticulas) * 2 * Math.PI + (Math.random() - 0.5) * 0.5;
        const dist = 30 + Math.random() * 60;
        const tx = Math.cos(angulo) * dist;
        const ty = Math.sin(angulo) * dist;
        p.style.setProperty('--tx', `${tx}px`);
        p.style.setProperty('--ty', `${ty}px`);
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 1500);
    }
}

// Função de utilidade para mostrar alertas no ecrã (Desativada: alertas são lidos apenas no chat da Twitch)
function mostrarAlerta(texto, tipo, duracao = 3000) {
    // No-op
}

// Lida com a resolução final (Sucesso ou Falha) após a animação de arremesso terminar
function processarResolucaoFinal(resultado) {
    if (resultado === 'SUCESSO') {
        mostrarAlerta(`@${currentUsername} CAPTUROU!`, 'success', 4000);
        
        ballImg.src = `balls/close_${currentBolaIndex}.png`;
        ballImg.className = 'anim-success-glow';
        
        tocarSomPlim();
        criarParticulasBrilho();
        
        currentGhostAtivo = false;
        if (ghostGlideInterval) {
            clearInterval(ghostGlideInterval);
            ghostGlideInterval = null;
        }
        
        setTimeout(() => {
            ballImg.classList.add('hidden');
            elementalImg.classList.add('hidden');
            ocultarEfeitosAtivos();
            pararFumacaAmbient();
        }, 2200);
    } 
    else if (resultado === 'FALHA') {
        mostrarAlerta(`@${currentUsername} falhou...`, 'danger', 3000);
        tocarSomFalha();
        
        // Abre a bola e ejeta o elemental
        ballImg.src = `balls/open_${currentBolaIndex}.png`;
        ballImg.className = 'anim-fail-release';
        
        elementalImg.classList.remove('sucked');
        elementalImg.classList.add('spawn');
        
        if (currentGhostAtivo && !ghostRevelado) {
            setTimeout(() => {
                if (currentGhostAtivo && !ghostRevelado) {
                    elementalImg.classList.add('hidden');
                    elementalImg.className = 'hidden';
                }
            }, 2000);
        }
        
        setTimeout(() => {
            ballImg.classList.add('hidden');
        }, 1000);
    }
    
    // Reset de estados
    isThrowing = false;
    pendingResult = null;
}

// Lida com a lógica central
function processarComando(comandoCru) {
    if (!comandoCru) return;
    
    const partesPrincipais = comandoCru.split('|');
    const comandoReal = partesPrincipais[0];
    
    if (partesPrincipais.length > 1) {
        const raw = partesPrincipais[1];
        let agua, terra, fogo, pato, ghost, sleepy, demon, punk, king, aura, boss, peixe, atacante, vento, peely, seven;
        if (raw.includes('=')) {
            const map = {};
            raw.split(';').forEach(pair => {
                const kv = pair.split('=');
                if (kv.length === 2) map[kv[0].trim().toLowerCase()] = kv[1].trim();
            });
            agua = map["agua"];
            terra = map["terra"];
            fogo = map["fogo"] === 'true' || map["fogo"] === 'True';
            pato = map["pato"];
            ghost = map["ghost"] === 'true' || map["ghost"] === 'True';
            sleepy = map["sleepy"];
            demon = map["demon"];
            punk = map["punk"];
            king = map["king"] === 'true' || map["king"] === 'True';
            aura = map["aura"] === 'true' || map["aura"] === 'True';
            boss = map["boss"];
            peixe = map["peixe"];
            atacante = map["atacante"] === 'true' || map["atacante"] === 'True';
            vento = map["vento"];
            peely = map["peely"];
            seven = map["seven"];
        } else {
            const efeitosPartes = raw.split(';');
            agua = efeitosPartes[0];
            terra = efeitosPartes[1];
            fogo = efeitosPartes[2] === 'True' || efeitosPartes[2] === 'true';
            pato = efeitosPartes[3];
            ghost = efeitosPartes[4] === 'True' || efeitosPartes[4] === 'true';
            sleepy = efeitosPartes[5];
            demon = efeitosPartes[6];
            punk = efeitosPartes[7];
            king = efeitosPartes[8] === 'True' || efeitosPartes[8] === 'true';
            aura = efeitosPartes[9] === 'True' || efeitosPartes[9] === 'true';
            boss = efeitosPartes[10];
            peixe = efeitosPartes[11];
            atacante = efeitosPartes[12] === 'True' || efeitosPartes[12] === 'true';
            vento = efeitosPartes[13];
            peely = efeitosPartes[14];
            seven = efeitosPartes[15];
        }
        
        atualizarEfeitosAtivos(agua, terra, fogo, pato, ghost, sleepy, demon, punk, king, aura, boss, peixe, atacante, vento, peely, seven);
    }

    const partes = comandoReal.split(';');
    const acao = partes[0];

    switch(acao) {
        case 'SPAWN':
            const ficheiro = partes[1];
            const nome = partes[2];
            const aguaAtivo = partes[3] === 'True' || partes[3] === 'true';
            const terraAtivo = partes[4] === 'True' || partes[4] === 'true';
            const fogoAtivo = partes[5] === 'True' || partes[5] === 'true';
            const patoAtivo = partes[6] === 'True' || partes[6] === 'true';
            currentGhostAtivo = partes[7] === 'True' || partes[7] === 'true';
            const sleepyAtivo = partes[8] === 'True' || partes[8] === 'true';
            
            ballImg.className = 'hidden';
            elementalImg.className = 'hidden';
            elementalImg.src = `Sprites/${ficheiro}`;
            
            setTimeout(() => {
                if (currentGhostAtivo) {
                    elementalImg.className = 'hidden';
                    elementalImg.classList.add('hidden');
                    
                    if (ghostGlideInterval) {
                        clearInterval(ghostGlideInterval);
                        ghostGlideInterval = null;
                    }
                    executarDesfileFantasma();
                    ghostGlideInterval = setInterval(executarDesfileFantasma, 60000);
                } else {
                    elementalImg.className = 'spawn';
                    elementalImg.classList.remove('hidden');
                    const alertaNome = nome;
                    mostrarAlerta(`Apareceu um ${alertaNome}!`, 'success', 5000);
                }
                if (currentGhostAtivo) {
                    iniciarFumacaAmbient();
                } else {
                    pararFumacaAmbient();
                }
            }, 50);
            break;

        case 'REBOUND':
            currentUsername = partes[1];
            const bolaReb = partes[2];
            
            currentBolaIndex = 1;
            if (bolaReb === "super") currentBolaIndex = 2;
            else if (bolaReb === "ultra") currentBolaIndex = 3;
            else if (bolaReb === "master") currentBolaIndex = 4;
            
            if (currentGhostAtivo && !ghostRevelado) {
                ghostRevelado = true;
                if (ghostGlideInterval) {
                    clearInterval(ghostGlideInterval);
                    ghostGlideInterval = null;
                }
                elementalImg.className = 'spawn ghost-smoke';
            }
            
            mostrarAlerta(`⚽ REBOUND! @${currentUsername} chuta de novo!`, 'danger', 0);
            
            // Iniciar fluxo de animação
            isThrowing = true;
            pendingResult = null;
            
            elementalImg.classList.remove('hidden', 'sucked');
            
            // 1. Mostrar o Atacante a chutar
            const strikerKickImg = document.getElementById('striker-overlay-kick');
            if (strikerKickImg) {
                strikerKickImg.classList.remove('hidden');
                strikerKickImg.classList.add('striker-kick-active');
                
                // Toca som de chuto
                tocarSomChutoAtacante();
                
                // Esconder após animação terminar
                setTimeout(() => {
                    strikerKickImg.classList.remove('striker-kick-active');
                    strikerKickImg.classList.add('hidden');
                }, 1600);
            }
            
            // 2. A Pokébola é atirada (pontapeada) fechada com delay de 200ms após o chuto
            setTimeout(() => {
                ballImg.src = `balls/close_${currentBolaIndex}.png`;
                ballImg.className = 'anim-throw';
                ballImg.classList.remove('hidden');
                tocarSomAtirar();
            }, 200);
            
            // 3. Aos 1.7s: a bola ABRE e começa a vibrar. O elemental é sugado
            setTimeout(() => {
                ballImg.src = `balls/open_${currentBolaIndex}.png`;
                ballImg.classList.remove('anim-throw');
                ballImg.classList.add('anim-vibrate');
                elementalImg.classList.add('sucked');
                tocarSomSugado();
            }, 1700);
 
            // 4. Aos 3.7s: O elemental foi sugado por completo. A bola FECHA e faz a transição de queda
            setTimeout(() => {
                ballImg.src = `balls/close_${currentBolaIndex}.png`;
                ballImg.classList.remove('anim-vibrate');
                ballImg.classList.add('anim-close-seal');
            }, 3700);
 
            // 5. Aos 4.1s: A bola cai e executa o abano (shake) de captura no chão
            setTimeout(() => {
                ballImg.classList.remove('anim-close-seal');
                ballImg.classList.add('anim-shake');
                // Sincronizar tiques clássicos com a animação de abano
                setTimeout(tocarSomAbano, 200);   // Primeiro abano
                setTimeout(tocarSomAbano, 1600);  // Segundo abano
                setTimeout(tocarSomAbano, 3000);  // Terceiro abano
            }, 4100);
 
            // 6. Aos 7.7s: Termina a simulação do arremesso. Se já houver resultado pendente, processa-o
            setTimeout(() => {
                if (pendingResult) {
                    processarResolucaoFinal(pendingResult);
                } else {
                    isThrowing = false;
                }
            }, 7700);
            break;

        case 'ATIRAR':
            currentUsername = partes[1];
            const bola = partes[2];
            
            currentBolaIndex = 1;
            if (bola === "super") currentBolaIndex = 2;
            else if (bola === "ultra") currentBolaIndex = 3;
            else if (bola === "master") currentBolaIndex = 4;
            
            if (currentGhostAtivo && !ghostRevelado) {
                ghostRevelado = true;
                if (ghostGlideInterval) {
                    clearInterval(ghostGlideInterval);
                    ghostGlideInterval = null;
                }
                elementalImg.className = 'spawn ghost-smoke';
            }
            
            mostrarAlerta(`@${currentUsername} atirou uma ${bola}!`, 'danger', 0);
            
            // Iniciar fluxo de animação
            isThrowing = true;
            pendingResult = null;
            
            elementalImg.classList.remove('hidden', 'sucked');
            
            // 1. Bola é atirada FECHADA
            ballImg.src = `balls/close_${currentBolaIndex}.png`;
            ballImg.className = 'anim-throw';
            ballImg.classList.remove('hidden');
            tocarSomAtirar();
            
            // 2. Aos 1.5s (chegada no elemental): a bola ABRE e começa a vibrar. O elemental é sugado
            setTimeout(() => {
                ballImg.src = `balls/open_${currentBolaIndex}.png`;
                ballImg.classList.remove('anim-throw');
                ballImg.classList.add('anim-vibrate');
                elementalImg.classList.add('sucked');
                tocarSomSugado();
            }, 1500);

            // 3. Aos 3.5s: O elemental foi sugado por completo. A bola FECHA e faz a transição de queda
            setTimeout(() => {
                ballImg.src = `balls/close_${currentBolaIndex}.png`;
                ballImg.classList.remove('anim-vibrate');
                ballImg.classList.add('anim-close-seal');
            }, 3500);

            // 4. Aos 3.9s: A bola cai e executa o abano (shake) de captura no chão
            setTimeout(() => {
                ballImg.classList.remove('anim-close-seal');
                ballImg.classList.add('anim-shake');
                // Sincronizar tiques clássicos com a animação de abano
                setTimeout(tocarSomAbano, 200);   // Primeiro abano (~4.1s)
                setTimeout(tocarSomAbano, 1600);  // Segundo abano (~5.5s)
                setTimeout(tocarSomAbano, 3000);  // Terceiro abano (~6.9s)
            }, 3900);

            // 5. Aos 7.5s: Termina a simulação do arremesso. Se já houver resultado pendente, processa-o
            setTimeout(() => {
                if (pendingResult) {
                    processarResolucaoFinal(pendingResult);
                } else {
                    // Se o resultado ainda não chegou (lag de rede ou atraso do bot), aguarda
                    isThrowing = false;
                }
            }, 7500);
            break;

        case 'SUCESSO':
            if (isThrowing) {
                pendingResult = 'SUCESSO';
            } else {
                processarResolucaoFinal('SUCESSO');
            }
            break;

        case 'FALHA':
            if (isThrowing) {
                pendingResult = 'FALHA';
            } else {
                processarResolucaoFinal('FALHA');
            }
            break;

        case 'COLECAO':
            const userColecao = partes[1];
            const idsStr = partes[2];
            mostrarPainelColecao(userColecao, idsStr);
            break;

        case 'FUGIU':
            const bichoFugiuNome = partes[1];
            mostrarAlerta(`O ${bichoFugiuNome} fugiu...`, 'danger', 3000);
            elementalImg.className = 'anim-escape' + (currentGhostAtivo ? ' ghost-smoke' : '');
            criarParticulasFumo();
            pararFumacaAmbient();
            currentGhostAtivo = false;
            ghostRevelado = false;
            if (ghostGlideInterval) {
                clearInterval(ghostGlideInterval);
                ghostGlideInterval = null;
            }
            setTimeout(() => {
                elementalImg.classList.add('hidden');
                elementalImg.className = 'hidden';
                ocultarEfeitosAtivos();
            }, 1500);
            break;

        case 'TROCA':
            const viewer1 = partes[1];
            const viewer2 = partes[2];
            const bichoFile1 = partes[3];
            const bichoFile2 = partes[4];
            
            if (tradeTimeout) clearTimeout(tradeTimeout);
            
            tradeUser1.textContent = viewer1;
            tradeUser2.textContent = viewer2;
            tradeImg1.src = `Sprites/${bichoFile1}`;
            tradeImg2.src = `Sprites/${bichoFile2}`;
            
            tradeCard1.className = 'trade-card';
            tradeCard2.className = 'trade-card';
            tradeArea.classList.remove('hidden');
            tradeArea.style.opacity = '1';
            
            setTimeout(() => {
                tradeCard1.classList.add('anim-swap-left');
                tradeCard2.classList.add('anim-swap-right');
                tocarSomTroca();
            }, 50);
            
            mostrarAlerta(`Troca Concluída entre @${viewer1} e @${viewer2}!`, 'success', 4000);
            
            tradeTimeout = setTimeout(() => {
                tradeArea.style.transition = 'opacity 0.5s ease';
                tradeArea.style.opacity = '0';
                setTimeout(() => {
                    tradeArea.classList.add('hidden');
                    tradeCard1.className = 'trade-card';
                    tradeCard2.className = 'trade-card';
                }, 500);
            }, 4500);
            break;

        case 'SPRIT':
            const numeroSprit = parseInt(partes[1]);
            const jogadorNome = partes[2];
            mostrarEfeitoSprit(numeroSprit, jogadorNome);
            break;

        case 'GRIM':
            const grimConjurador = partes[1];
            const grimVítima = partes[2];
            const grimElemId = partes[3];
            const grimCandidatos = partes[4] ? partes[4].split(',') : [];
            mostrarEfeitoGrim(grimConjurador, grimVítima, grimElemId, grimCandidatos);
            break;

        case 'PESCA':
            const pescaUser = partes[1];
            const pescaElemId = partes[2];
            mostrarPescaExtra(pescaUser, pescaElemId);
            break;

        case 'QUACK':
            const quackUser = partes[1];
            const quackElemId = partes[2];
            const quackReq = partes[3];
            const delayQuack = isThrowing ? 4500 : 200;
            mostrarAnimacaoQuack(quackUser, quackElemId, quackReq, delayQuack);
            break;

        case 'LIMPAR':
            elementalImg.className = 'hidden';
            elementalImg.src = '';
            
            ballImg.className = 'hidden';
            ballImg.src = '';
            
            alertBox.classList.add('hidden');
            collectionPanel.classList.add('hidden');
            tradeArea.classList.add('hidden');
            
            document.getElementById('grim-panel').classList.add('hidden');
            document.getElementById('grim-reaper-img').classList.remove('grim-enter');
            document.getElementById('grim-reaper-img').classList.add('hidden');
            document.getElementById('grim-reveal-area').classList.add('hidden');
            document.getElementById('grim-card-slash').className = 'hidden';
            document.getElementById('grim-target-card').className = '';
            document.querySelectorAll('.grim-soul-particle').forEach(p => p.remove());

            document.getElementById('fishing-panel').classList.add('hidden');
            document.getElementById('fishing-line-container').style.top = '-280px';
            document.getElementById('fishing-card-holder').classList.remove('card-hooked');
            document.getElementById('fishing-card-holder').classList.add('hidden');
            document.querySelectorAll('.fishing-bubble').forEach(p => p.remove());

            resetSpritSystem();
            ocultarEfeitosAtivos();
            pararFumacaAmbient();
            currentGhostAtivo = false;
            ghostRevelado = false;
            if (ghostGlideInterval) {
                clearInterval(ghostGlideInterval);
                ghostGlideInterval = null;
            }
            
            if (colecaoTimeout) clearTimeout(colecaoTimeout);
            if (colecaoInterval) clearInterval(colecaoInterval);
            if (alertTimeout) clearTimeout(alertTimeout);
            if (tradeTimeout) clearTimeout(tradeTimeout);
            
            document.querySelectorAll('.sparkle-particle').forEach(p => p.remove());
            document.querySelectorAll('.smoke-particle').forEach(p => p.remove());
            
            isThrowing = false;
            pendingResult = null;
            break;
    }
}

function executarDesfileFantasma() {
    if (!currentGhostAtivo || ghostRevelado) return;
    
    elementalImg.classList.remove('hidden', 'sucked');
    elementalImg.className = 'ghost-smoke ghost-glide';
    
    setTimeout(() => {
        if (currentGhostAtivo && !ghostRevelado && elementalImg.className.includes('ghost-glide')) {
            elementalImg.classList.add('hidden');
            elementalImg.className = 'hidden';
        }
    }, 12000);
}

function mostrarPainelColecao(username, idsStr) {
    if (colecaoTimeout) clearTimeout(colecaoTimeout);
    if (colecaoInterval) clearInterval(colecaoInterval);
    
    collectionTitle.textContent = username;
    
    const playerInventory = {};
    if (idsStr && idsStr.trim() !== "") {
        const pairs = idsStr.split(',');
        pairs.forEach(p => {
            const parts = p.split(':');
            if (parts.length === 2) {
                playerInventory[parts[0]] = parseInt(parts[1]) || 0;
            }
        });
    }

    const ITEMS_PER_PAGE = 8;
    const totalPages = Math.ceil(allElementais.length / ITEMS_PER_PAGE);
    let currentPage = 0;

    // Criar os dots de paginação
    const dotsContainer = document.getElementById('collection-dots');
    if (dotsContainer) {
        dotsContainer.innerHTML = '';
        for (let i = 0; i < totalPages; i++) {
            const dot = document.createElement('div');
            dot.className = 'collection-dot' + (i === 0 ? ' active' : '');
            dotsContainer.appendChild(dot);
        }
    }

    const setupImgOnError = (imgElement, container) => {
        imgElement.onerror = () => {
            imgElement.style.display = 'none';
            container.classList.add('broken-img');
        };
    };

    function renderizarPagina(pageIndex) {
        collectionGrid.innerHTML = '';
        
        // Atualizar dots
        if (dotsContainer) {
            const dots = dotsContainer.querySelectorAll('.collection-dot');
            dots.forEach((dot, idx) => {
                if (idx === pageIndex) {
                    dot.classList.add('active');
                } else {
                    dot.classList.remove('active');
                }
            });
        }

        const startIdx = pageIndex * ITEMS_PER_PAGE;
        const endIdx = Math.min(startIdx + ITEMS_PER_PAGE, allElementais.length);

        for (let i = startIdx; i < endIdx; i++) {
            const elem = allElementais[i];
            const qty = playerInventory[elem.id] || 0;
            const divSlot = document.createElement('div');
            divSlot.className = 'collection-item';

            if (qty === 0) {
                const img = document.createElement('img');
                img.src = `Sprites/${elem.file}`;
                img.className = 'not-owned';
                img.alt = elem.name;
                setupImgOnError(img, divSlot);
                divSlot.appendChild(img);
            }
            else if (qty === 1) {
                const img = document.createElement('img');
                img.src = `Sprites/${elem.file}`;
                img.alt = elem.name;
                setupImgOnError(img, divSlot);
                divSlot.appendChild(img);
            }
            else if (qty >= 2) {
                divSlot.classList.add('stacked');
                
                const imgBack = document.createElement('img');
                imgBack.src = `Sprites/${elem.file}`;
                imgBack.className = 'card-back';
                imgBack.alt = elem.name;
                setupImgOnError(imgBack, divSlot);
                
                const imgFront = document.createElement('img');
                imgFront.src = `Sprites/${elem.file}`;
                imgFront.className = 'card-front';
                imgFront.alt = elem.name;
                setupImgOnError(imgFront, divSlot);
                
                divSlot.appendChild(imgBack);
                divSlot.appendChild(imgFront);
                
                const badge = document.createElement('span');
                badge.className = 'qty-badge';
                badge.textContent = `${qty}x`;
                divSlot.appendChild(badge);
            }
            collectionGrid.appendChild(divSlot);
        }

        // Preencher com slots vazios se houver menos de 8 itens (ex: última página)
        const numItems = endIdx - startIdx;
        if (numItems < ITEMS_PER_PAGE) {
            for (let j = numItems; j < ITEMS_PER_PAGE; j++) {
                const divSlot = document.createElement('div');
                divSlot.className = 'collection-item empty-slot';
                collectionGrid.appendChild(divSlot);
            }
        }
    }

    // Renderizar página inicial
    renderizarPagina(0);
    collectionGrid.style.opacity = '1';

    collectionPanel.style.animation = 'fadeIn 0.5s ease-out forwards';
    collectionPanel.classList.remove('hidden');

    // Intervalo de Rotação (a cada 2.5 segundos)
    colecaoInterval = setInterval(() => {
        // Se já mostrámos a última página, fechamos o painel sem voltar ao início
        if (currentPage === totalPages - 1) {
            clearInterval(colecaoInterval);
            if (colecaoTimeout) clearTimeout(colecaoTimeout);
            collectionPanel.style.animation = 'fadeOut 0.5s ease-out forwards';
            setTimeout(() => collectionPanel.classList.add('hidden'), 500);
            return;
        }

        // Iniciar fade out 250ms antes da transição da página
        collectionGrid.style.opacity = '0';
        
        setTimeout(() => {
            currentPage = (currentPage + 1) % totalPages;
            renderizarPagina(currentPage);
            collectionGrid.style.opacity = '1';
        }, 250);
    }, 2500);

    // Timeout de Fecho Total do Painel (segurança de fallback)
    colecaoTimeout = setTimeout(() => {
        if (colecaoInterval) clearInterval(colecaoInterval);
        collectionPanel.style.animation = 'fadeOut 0.5s ease-out forwards';
        setTimeout(() => collectionPanel.classList.add('hidden'), 500);
    }, 18000);
}

// Loop de leitura do ficheiro
async function lerFicheiro() {
    try {
        const url = window.location.protocol === 'file:' ? ESTADO_FILE : `${ESTADO_FILE}?t=${new Date().getTime()}`;
        const resposta = await fetch(url, { cache: 'no-store' });
        if (!resposta.ok) throw new Error('Network response was not ok');
        
        const texto = await resposta.text();
        const linhaCrua = texto.trim();
        
        if (linhaCrua !== "" && linhaCrua !== ultimoComando) {
            ultimoComando = linhaCrua;
            processarComando(linhaCrua);
        }
    } catch (e) {
        console.error("Erro ao ler jogo_estado.txt:", e);
    }
}

// Iniciar Polling
setInterval(lerFicheiro, POLLING_INTERVAL);

// Funções para testes (Debug)
function testarComando(comando) {
    console.log("A testar comando localmente:", comando);
    processarComando(comando);
}
function toggleDebug() {
    const debugPanel = document.getElementById('debug-panel');
    if (debugPanel) {
        debugPanel.classList.add('hidden');
    }
}

// =========================================================================
// SISTEMA DO GRIM REAPER (ROLETA, CEIFEIRO E EXTERMÍNIO)
// =========================================================================

function mostrarEfeitoGrim(conjurador, vitima, elemId, candidatos = []) {
    const panel = document.getElementById('grim-panel');
    const strip = document.getElementById('grim-slot-strip');
    const revealArea = document.getElementById('grim-reveal-area');
    const victimNameDiv = document.getElementById('grim-victim-name');
    const targetImg = document.getElementById('grim-target-img');
    const targetCard = document.getElementById('grim-target-card');
    const slashDiv = document.getElementById('grim-card-slash');
    const reaperImg = document.getElementById('grim-reaper-img');

    // 1. Reset state
    panel.classList.remove('hidden');
    panel.style.opacity = '1';
    revealArea.classList.add('hidden');
    slashDiv.className = 'hidden';
    targetCard.className = '';
    reaperImg.className = 'hidden';
    strip.style.transition = 'none';
    strip.style.transform = 'translateY(0px)';

    // 2. Preencher a roleta de nomes (nomes dos candidatos do Top 3)
    const topCandidates = (candidatos && candidatos.length > 0) ? candidatos.filter(c => c && c.trim() !== '') : [];
    const fallbackNomes = ['Z3RGtv', 'SheisDani', 'MarquesrCarol', 'gui_z0', 'manu12321_'];
    
    // Garantir que a pool de nomes tem pelo menos 3 nomes
    let poolNomes = [...topCandidates];
    fallbackNomes.forEach(f => {
        if (poolNomes.length < 3 && !poolNomes.includes(f)) poolNomes.push(f);
    });
    poolNomes.sort(() => Math.random() - 0.5);
    
    const itemsCount = 25;
    const itemHeight = 55; // Altura exata definida em CSS para .grim-slot-item
    let html = '';
    for (let i = 0; i < itemsCount; i++) {
        let nome = poolNomes[i % poolNomes.length];
        if (i === itemsCount - 3) {
            nome = vitima;
        }
        html += `<div class="grim-slot-item ${i === itemsCount - 3 ? 'winner-item' : ''}">@${nome}</div>`;
    }
    strip.innerHTML = html;

    tocarSomSpookyStart();

    // 3. Iniciar o spin (Slot Machine)
    setTimeout(() => {
        strip.style.transition = 'transform 3.5s cubic-bezier(0.1, 0.8, 0.25, 1)';
        const targetOffset = -(itemsCount - 3) * itemHeight;
        strip.style.transform = `translateY(${targetOffset}px)`;
        
        let tickInterval = setInterval(() => {
            tocarSomTick();
        }, 125);
        
        setTimeout(() => {
            clearInterval(tickInterval);
        }, 3200);
        
    }, 100);

    // 4. Parar na Vítima e revelar
    setTimeout(() => {
        tocarSomImpacto();
        const slotMachine = document.getElementById('grim-slot-machine');
        if (slotMachine) {
            slotMachine.style.borderColor = '#f1c40f';
            slotMachine.style.boxShadow = '0 0 35px #f1c40f';
            
            setTimeout(() => {
                slotMachine.style.borderColor = '#a154f2';
                slotMachine.style.boxShadow = '0 0 25px rgba(161, 84, 242, 0.6)';
            }, 800);
        }

        revealArea.classList.remove('hidden');
        victimNameDiv.textContent = `💀 Vítima Escolhida: @${vitima}`;
        
        const bichoInfo = elementaisMap[elemId] || { file: 'T_Icon_BR_Creature_Sprite_ZeroPoint_ui_L.webp' };
        targetImg.src = `Sprites/${bichoInfo.file}`;
        tocarSomRevelaCard();
    }, 3800);

    // 5. Ceifeiro Grim entra
    setTimeout(() => {
        reaperImg.classList.remove('hidden');
        reaperImg.classList.add('grim-enter');
        tocarSomGrimRiso();
    }, 4800);

    // 6. O Slash (corte) e a destruição da carta
    setTimeout(() => {
        slashDiv.className = 'slash-active';
        tocarSomSlashCorte();
        
        panel.style.animation = 'sprit-creature-shake 0.3s ease-out';
        setTimeout(() => {
            panel.style.animation = '';
        }, 300);

        targetCard.classList.add('card-dissolve');
        criarParticulasAlma();
    }, 6000);

    // 7. Fade-out e fechar o painel
    setTimeout(() => {
        panel.style.opacity = '0';
        setTimeout(() => {
            panel.classList.add('hidden');
            revealArea.classList.add('hidden');
            reaperImg.classList.remove('grim-enter');
            reaperImg.classList.add('hidden');
        }, 500);
    }, 8500);
}

function criarParticulasAlma() {
    const container = document.getElementById('grim-card-container');
    const numParticles = 45;
    for (let i = 0; i < numParticles; i++) {
        const p = document.createElement('div');
        p.className = 'grim-soul-particle';
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * Math.PI * 2;
        const dist = 70 + Math.random() * 160;
        const dx = Math.cos(angle) * dist;
        const dy = Math.sin(angle) * dist - (40 + Math.random() * 80);
        
        p.style.setProperty('--dx', `${dx}px`);
        p.style.setProperty('--dy', `${dy}px`);
        
        p.style.animationDelay = `${Math.random() * 0.4}s`;
        p.style.animationDuration = `${1.2 + Math.random() * 0.8}s`;
        
        container.appendChild(p);
        
        setTimeout(() => {
            p.remove();
        }, 2200);
    }
}

function tocarSomSpookyStart() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const osc2 = ctx.createOscillator();
        const gain = ctx.createGain();
        
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(100, ctx.currentTime);
        osc.frequency.linearRampToValueAtTime(70, ctx.currentTime + 3.0);
        
        osc2.type = 'sine';
        osc2.frequency.setValueAtTime(102, ctx.currentTime);
        osc2.frequency.linearRampToValueAtTime(72, ctx.currentTime + 3.0);

        gain.gain.setValueAtTime(0.01, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.18, ctx.currentTime + 0.5);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 3.0);
        
        osc.connect(gain);
        osc2.connect(gain);
        gain.connect(ctx.destination);
        
        osc.start();
        osc2.start();
        osc.stop(ctx.currentTime + 3.0);
        osc2.stop(ctx.currentTime + 3.0);
    } catch (e) {}
}

function tocarSomTick() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(600, ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(150, ctx.currentTime + 0.05);
        
        gain.gain.setValueAtTime(0.08, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.05);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        osc.start();
        osc.stop(ctx.currentTime + 0.06);
    } catch (e) {}
}

function tocarSomImpacto() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        
        osc.type = 'sine';
        osc.frequency.setValueAtTime(220, ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(55, ctx.currentTime + 0.4);
        
        gain.gain.setValueAtTime(0.3, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.4);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        osc.start();
        osc.stop(ctx.currentTime + 0.4);
    } catch (e) {}
}

function tocarSomRevelaCard() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        
        osc.type = 'sine';
        osc.frequency.setValueAtTime(330, ctx.currentTime);
        osc.frequency.linearRampToValueAtTime(660, ctx.currentTime + 0.3);
        
        gain.gain.setValueAtTime(0.01, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.12, ctx.currentTime + 0.1);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.3);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        osc.start();
        osc.stop(ctx.currentTime + 0.3);
    } catch (e) {}
}

function tocarSomGrimRiso() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(180, ctx.currentTime);
        
        const lfo = ctx.createOscillator();
        const lfoGain = ctx.createGain();
        lfo.frequency.setValueAtTime(10, ctx.currentTime);
        lfoGain.gain.setValueAtTime(30, ctx.currentTime);
        
        lfo.connect(lfoGain);
        lfoGain.connect(osc.frequency);
        
        gain.gain.setValueAtTime(0.01, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.15, ctx.currentTime + 0.3);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 1.2);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        lfo.start();
        osc.start();
        
        lfo.stop(ctx.currentTime + 1.2);
        osc.stop(ctx.currentTime + 1.2);
    } catch (e) {}
}

function tocarSomSlashCorte() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        const bufferSize = ctx.sampleRate * 0.4;
        const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }
        const noise = ctx.createBufferSource();
        noise.buffer = buffer;
        
        const filter = ctx.createBiquadFilter();
        filter.type = 'bandpass';
        filter.frequency.setValueAtTime(1200, ctx.currentTime);
        filter.frequency.exponentialRampToValueAtTime(300, ctx.currentTime + 0.3);
        
        const noiseGain = ctx.createGain();
        noiseGain.gain.setValueAtTime(0.2, ctx.currentTime);
        noiseGain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.35);
        
        noise.connect(filter);
        filter.connect(noiseGain);
        noiseGain.connect(ctx.destination);
        
        const osc = ctx.createOscillator();
        const oscGain = ctx.createGain();
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(900, ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(100, ctx.currentTime + 0.35);
        
        oscGain.gain.setValueAtTime(0.25, ctx.currentTime);
        oscGain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.38);
        
        osc.connect(oscGain);
        oscGain.connect(ctx.destination);
        
        noise.start();
        osc.start();
        noise.stop(ctx.currentTime + 0.4);
        osc.stop(ctx.currentTime + 0.4);
    } catch (e) {}
}

// =========================================================================
// SISTEMA DE SPRITS - LÓGICA E SÍNTESE DE SOM (NOVO)
// =========================================================================

function mostrarEfeitoSprit(numeroSprit, jogadorNome) {
    if (!spritArea || !spritNotification || !spritParticles || !spritBall || !spritCreature) return;

    // 1. Limpar estados e timeouts anteriores
    resetSpritSystem();
    
    let titulo = "";
    let themeClass = "";
    
    let ballIndex = 1; // 1: normal, 2: super, 3: ultra, 4: master
    let creatureFile = "";
    let themeGlow = "#ffffff";
    
    let ambientParticleFunc = null;
    let climaxParticleFunc = null;
    let soundClimaxFunc = null;

    switch(numeroSprit) {
        case 1: // Água
            titulo = `💧 @${jogadorNome} invocou o Elemental de Água! 💧`;
            themeClass = 'theme-1';
            ballIndex = 2; // Great Ball
            creatureFile = 'T_Icon_BR_Creature_Sprite_Water_Unvault_Ch7S3_ui_L.webp';
            themeGlow = '#00e5ff';
            ambientParticleFunc = spawnWaterParticles;
            climaxParticleFunc = spawnWaterClimax;
            soundClimaxFunc = tocarSomSplashAgua;
            break;
        case 2: // Terra
            titulo = `🌿 @${jogadorNome} invocou o Elemental de Terra! 🌿`;
            themeClass = 'theme-2';
            ballIndex = 3; // Ultra Ball
            creatureFile = 'T_Icon_BR_Creature_Sprite_Earth_Ch7S3_UI_L.webp';
            themeGlow = '#2ecc71';
            ambientParticleFunc = spawnEarthParticles;
            climaxParticleFunc = spawnEarthClimax;
            soundClimaxFunc = tocarSomCrumbleTerra;
            break;
        case 3: // Fogo
            titulo = `🔥 @${jogadorNome} invocou o Elemental de Fogo! 🔥`;
            themeClass = 'theme-3';
            ballIndex = 1; // Pokéball
            creatureFile = 'T_Icon_BR_Creature_Sprite_Fire_Unvault_Ch7S3_ui_L.webp';
            themeGlow = '#ff5100';
            ambientParticleFunc = spawnFireParticles;
            climaxParticleFunc = spawnFireClimax;
            soundClimaxFunc = tocarSomExplosaoFogo;
            break;
        case 4: // Estelar / Pato
            titulo = `🦆 @${jogadorNome} invocou o Elemental de Pato! 🦆`;
            themeClass = 'theme-generic';
            ballIndex = 4; // Master Ball
            creatureFile = 'T_Icon_BR_Duck_Default_L.webp';
            themeGlow = '#fbc02d';
            ambientParticleFunc = spawnGenericParticles;
            climaxParticleFunc = spawnDuckClimax;
            soundClimaxFunc = tocarSomSpritGenerico;
            break;
        case 5: // Ghost
            titulo = `👻 @${jogadorNome} invocou o Elemental de Fantasma! 👻`;
            themeClass = 'theme-generic';
            ballIndex = 4; // Master Ball
            creatureFile = 'T_Icon_BR_Creature_Sprite_Ghost_Unvault_L.webp';
            themeGlow = '#a154f2';
            ambientParticleFunc = spawnGhostSmokeParticles;
            climaxParticleFunc = spawnGhostClimax;
            soundClimaxFunc = tocarSomSpritGhost;
            break;
        case 6: // Sleepy
            titulo = `💤 @${jogadorNome} invocou o Elemental dos Sonhos! 💤`;
            themeClass = 'theme-generic';
            ballIndex = 4; // Master Ball
            creatureFile = 'T_Icon_BR_Creature_Sprite_Sleepy_ui_L.webp';
            themeGlow = '#5e5ce6';
            ambientParticleFunc = spawnSleepyParticles;
            climaxParticleFunc = spawnSleepyClimax;
            soundClimaxFunc = tocarSomSpritSleepy;
            break;
        case 7: // Demon
            titulo = `😈 @${jogadorNome} invocou o Elemental de Demónio! 😈`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_RedDemon_Default_L.webp';
            themeGlow = '#ff003c';
            ambientParticleFunc = spawnFireParticles;
            climaxParticleFunc = spawnDemonClimax;
            soundClimaxFunc = tocarSomSpritGenerico;
            break;
        case 8: // Punk
            titulo = `🎸 @${jogadorNome} invocou o Elemental de Punk! 🎸`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_Punk_ui_L.webp';
            themeGlow = '#ff007f';
            ambientParticleFunc = spawnGenericParticles;
            climaxParticleFunc = spawnPunkClimax;
            soundClimaxFunc = tocarSomSpritGenerico;
            break;
        case 9: // King
            titulo = `👑 @${jogadorNome} invocou o Elemental de Rei! 👑`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_King_ui_L.webp';
            themeGlow = '#ffd700';
            ambientParticleFunc = spawnGenericParticles;
            climaxParticleFunc = spawnKingClimax;
            soundClimaxFunc = tocarSomSpritGenerico;
            break;
        case 10: // Ponto Zero
            titulo = `🌌 @${jogadorNome} invocou o Elemental de Ponto Zero! 🌌`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_ZeroPoint_ui_L.webp';
            themeGlow = '#8e44ad'; // Roxo galáctico
            ambientParticleFunc = spawnAuroraParticles;
            climaxParticleFunc = spawnAuroraClimax;
            soundClimaxFunc = tocarSomChimeAura;
            break;
        case 12: // Peixoto
            titulo = `🎣 @${jogadorNome} invocou o Elemental de Peixoto! 🎣`;
            themeClass = 'theme-1';
            ballIndex = 2; // Great Ball
            creatureFile = 'T_Icon_BR_Creature_Sprite_Fishy_ui_L.webp';
            themeGlow = '#00aaff';
            ambientParticleFunc = spawnWaterParticles;
            climaxParticleFunc = spawnWaterClimax;
            soundClimaxFunc = tocarSomSplashAgua;
            break;
        case 13: // Atacante (Futebol)
            titulo = `⚽ @${jogadorNome} invocou o Elemental Atacante! ⚽`;
            themeClass = 'theme-generic';
            ballIndex = 4; // Master Ball class glow
            creatureFile = 'T_Icon_BR_Creature_Sprite_Soccer_ui_L.webp';
            themeGlow = '#2ecc71'; // Verde relvado
            ambientParticleFunc = spawnGenericParticles;
            climaxParticleFunc = spawnGenericClimax;
            soundClimaxFunc = tocarSomChutoAtacante;
            break;
        case 14: // Aura
            titulo = `✨ @${jogadorNome} invocou o Elemental de Aura! ✨`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_Drifter_ui_L.webp';
            themeGlow = '#00ffcc';
            ambientParticleFunc = spawnAuroraParticles;
            climaxParticleFunc = spawnAuroraClimax;
            soundClimaxFunc = tocarSomChimeAura;
            break;
        case 15: // Boss
            titulo = `👑 @${jogadorNome} invocou o Elemental de Boss! 👑`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_Boss_ui_L.webp';
            themeGlow = '#e67e22';
            ambientParticleFunc = spawnFireParticles;
            climaxParticleFunc = spawnBossClimax;
            soundClimaxFunc = tocarSomSismoBoss;
            break;
        case 17: // Ar
            titulo = `🌪️ @${jogadorNome} invocou o Elemental de Ar! 🌪️`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Air_Default_L.webp';
            themeGlow = '#a8e6cf';
            ambientParticleFunc = spawnAuroraParticles;
            climaxParticleFunc = spawnAuroraClimax;
            soundClimaxFunc = tocarSomChimeAura;
            break;
        case 18: // Seven
            titulo = `⚡ @${jogadorNome} invocou o Elemental Seven! ⚡`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_Seven_ui_L.webp';
            themeGlow = '#3498db';
            ambientParticleFunc = spawnAuroraParticles;
            climaxParticleFunc = spawnAuroraClimax;
            soundClimaxFunc = tocarSomChimeAura;
            break;
        case 19: // Batman
            titulo = `🦇 @${jogadorNome} invocou o Elemental Batman! 🦇`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_FossilMeal_Default_L.webp';
            themeGlow = '#34495e';
            ambientParticleFunc = spawnGhostSmokeParticles;
            climaxParticleFunc = spawnGhostClimax;
            soundClimaxFunc = tocarSomSpritGhost;
            break;
        case 20: // Vini JR
            titulo = `⚽ @${jogadorNome} invocou o Elemental Vini JR! ⚽`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_CokeParmesan_Default_L.webp';
            themeGlow = '#f1c40f';
            ambientParticleFunc = spawnGenericParticles;
            climaxParticleFunc = spawnGenericClimax;
            soundClimaxFunc = tocarSomChutoAtacante;
            break;
        case 21: // Pollo
            titulo = `🍕 @${jogadorNome} invocou o Elemental Pollo! 🍕`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_CompanyStargazer_Default_L.webp';
            themeGlow = '#e74c3c';
            ambientParticleFunc = spawnFireParticles;
            climaxParticleFunc = spawnFireClimax;
            soundClimaxFunc = tocarSomSpritGenerico;
            break;
        case 18: // Seven
            titulo = `⚡ @${jogadorNome} invocou o Elemental Seven! ⚡`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_Seven_ui_L.webp';
            themeGlow = '#00d2d3';
            ambientParticleFunc = spawnAuroraParticles;
            climaxParticleFunc = spawnAuroraClimax;
            soundClimaxFunc = tocarSomChimeAura;
            break;
        case 22: // Llama
            titulo = `🦙 @${jogadorNome} invocou o Elemental Llama! 🦙`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_Llama_ui_L.webp';
            themeGlow = '#f39c12';
            ambientParticleFunc = spawnAuroraParticles;
            climaxParticleFunc = spawnAuroraClimax;
            soundClimaxFunc = tocarSomChimeAura;
            break;
        case 23: // Peely
            titulo = `🍌 @${jogadorNome} invocou o Elemental Peely! 🍌`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Creature_Sprite_Peely_ui_L.webp';
            themeGlow = '#f1c40f';
            ambientParticleFunc = spawnGenericParticles;
            climaxParticleFunc = spawnGenericClimax;
            soundClimaxFunc = tocarSomSpritGenerico;
            break;
        default:
            titulo = `✨ @${jogadorNome} invocou um Elemental! ✨`;
            themeClass = 'theme-generic';
            ballIndex = 4;
            creatureFile = 'T_Icon_BR_Duck_Default_L.webp';
            themeGlow = '#a154f2';
            ambientParticleFunc = spawnGenericParticles;
            climaxParticleFunc = spawnGenericClimax;
            soundClimaxFunc = tocarSomSpritGenerico;
    }

    // 2. TIMELINE DA ANIMAÇÃO (6.5s totais)
    
    // --- 0.0s: Arremesso da Bola ---
    spritNotification.textContent = titulo;
    spritNotification.classList.add('show', themeClass);
    spritArea.classList.remove('hidden');

    spritBall.src = `balls/close_${ballIndex}.png`;
    spritBall.className = 'sprit-ball-throw';
    tocarSomAtirar();

    // --- 1.2s: Bola abre e Criatura sai ---
    spritTimeouts.push(setTimeout(() => {
        spritBall.src = `balls/open_${ballIndex}.png`;
        tocarSomEject();

        spritCreature.src = `Sprites/${creatureFile}`;
        spritCreature.style.setProperty('--theme-glow', themeGlow);
        spritCreature.className = 'sprit-creature-emerge';

        // Iniciar partículas de ambiente suaves
        if (ambientParticleFunc) {
            spritAmbientInterval = ambientParticleFunc(spritParticles);
        }
    }, 1200));

    // --- 2.2s: Ocultar Bola ---
    spritTimeouts.push(setTimeout(() => {
        spritBall.className = 'hidden';
        spritBall.classList.remove('sprit-ball-throw');
    }, 2200));

    // --- 2.5s: Começa a Abanar / Efeito Ondas (Sprit aparece limpo primeiro) ---
    spritTimeouts.push(setTimeout(() => {
        spritCreature.classList.remove('sprit-creature-emerge');
        if (numeroSprit === 1) {
            // Efeito líquido ondulado para Água
            spritCreature.classList.add('sprit-creature-wave-shake');
        } else {
            // Vibração acumulante para outros
            spritCreature.classList.add('sprit-creature-shake');
        }
    }, 2500));

    // --- 4.0s: Clímax (PUFF! Desintegração em partículas a partir do centro) ---
    spritTimeouts.push(setTimeout(() => {
        spritCreature.classList.remove('sprit-creature-shake', 'sprit-creature-wave-shake');
        spritCreature.classList.add('sprit-creature-puff');

        // Parar partículas de ambiente suaves
        if (spritAmbientInterval) {
            clearInterval(spritAmbientInterval);
            spritAmbientInterval = null;
        }

        // Tocar som de clímax e gerar partículas massivas vindas do centro
        if (soundClimaxFunc) soundClimaxFunc();
        if (climaxParticleFunc) climaxParticleFunc(spritParticles);
    }, 4000));

    // --- 5.5s: Fecho do Banner ---
    spritTimeouts.push(setTimeout(() => {
        spritNotification.classList.remove('show');
    }, 5500));

    // --- 6.5s: Ocultar e Limpar Tudo ---
    spritTimeouts.push(setTimeout(() => {
        resetSpritSystem();
    }, 6500));
}

// 🔊 Sínteses de Som Retrô (Web Audio API)
function tocarSomEject() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(100, ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(1200, ctx.currentTime + 0.35);
        
        gain.gain.setValueAtTime(0, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.08, ctx.currentTime + 0.05);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.35);
        
        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.value = 1000;
        
        osc.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        
        osc.start();
        osc.stop(ctx.currentTime + 0.35);
    } catch (e) {}
}

function tocarSomExplosaoFogo() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        const bufferSize = ctx.sampleRate * 1.5;
        const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }
        
        const noise = ctx.createBufferSource();
        noise.buffer = buffer;
        
        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(800, ctx.currentTime);
        filter.frequency.exponentialRampToValueAtTime(40, ctx.currentTime + 1.2);
        filter.Q.value = 4.0;
        
        const gain = ctx.createGain();
        gain.gain.setValueAtTime(0, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.28, ctx.currentTime + 0.05);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 1.4);
        
        noise.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        
        noise.start();
        noise.stop(ctx.currentTime + 1.4);
        
        const osc = ctx.createOscillator();
        const oscGain = ctx.createGain();
        osc.type = 'triangle';
        osc.frequency.setValueAtTime(90, ctx.currentTime);
        osc.frequency.linearRampToValueAtTime(30, ctx.currentTime + 0.4);
        oscGain.gain.setValueAtTime(0.25, ctx.currentTime);
        oscGain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.45);
        osc.connect(oscGain);
        oscGain.connect(ctx.destination);
        osc.start();
        osc.stop(ctx.currentTime + 0.45);
    } catch (e) {}
}

function tocarSomSplashAgua() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        const bufferSize = ctx.sampleRate * 0.9;
        const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }
        
        const noise = ctx.createBufferSource();
        noise.buffer = buffer;
        
        const filter = ctx.createBiquadFilter();
        filter.type = 'bandpass';
        filter.frequency.setValueAtTime(1000, ctx.currentTime);
        filter.frequency.exponentialRampToValueAtTime(250, ctx.currentTime + 0.8);
        filter.Q.value = 3.0;
        
        const gain = ctx.createGain();
        gain.gain.setValueAtTime(0, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.18, ctx.currentTime + 0.05);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.8);
        
        noise.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        
        noise.start();
        noise.stop(ctx.currentTime + 0.8);
        
        for (let i = 0; i < 12; i++) {
            setTimeout(() => {
                const osc = ctx.createOscillator();
                const g = ctx.createGain();
                osc.type = 'sine';
                osc.frequency.setValueAtTime(1200 + Math.random() * 800, ctx.currentTime);
                osc.frequency.exponentialRampToValueAtTime(400, ctx.currentTime + 0.1);
                g.gain.setValueAtTime(0.05, ctx.currentTime);
                g.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.1);
                osc.connect(g);
                g.connect(ctx.destination);
                osc.start();
                osc.stop(ctx.currentTime + 0.1);
            }, i * 35);
        }
    } catch (e) {}
}

function tocarSomCrumbleTerra() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        
        const bufferSize = ctx.sampleRate * 1.6;
        const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
        const data = buffer.getChannelData(0);
        for (let i = 0; i < bufferSize; i++) {
            data[i] = Math.random() * 2 - 1;
        }
        
        const noise = ctx.createBufferSource();
        noise.buffer = buffer;
        
        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(180, ctx.currentTime);
        filter.Q.value = 1.0;
        
        const gain = ctx.createGain();
        gain.gain.setValueAtTime(0, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.15, ctx.currentTime + 0.25);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 1.5);
        
        noise.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        
        noise.start();
        noise.stop(ctx.currentTime + 1.5);
        
        for (let i = 0; i < 8; i++) {
            setTimeout(() => {
                const osc = ctx.createOscillator();
                const g = ctx.createGain();
                osc.type = 'triangle';
                osc.frequency.setValueAtTime(120 + Math.random() * 60, ctx.currentTime);
                osc.frequency.linearRampToValueAtTime(40, ctx.currentTime + 0.2);
                g.gain.setValueAtTime(0.06, ctx.currentTime);
                g.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.2);
                osc.connect(g);
                g.connect(ctx.destination);
                osc.start();
                osc.stop(ctx.currentTime + 0.2);
            }, i * 180);
        }
    } catch (e) {}
}

function tocarSomSpritGenerico() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const playNote = (freq, delay) => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.value = freq;
            gain.gain.setValueAtTime(0, ctx.currentTime + delay);
            gain.gain.linearRampToValueAtTime(0.08, ctx.currentTime + delay + 0.02);
            gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + delay + 0.35);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(ctx.currentTime + delay);
            osc.stop(ctx.currentTime + delay + 0.35);
        };
        playNote(523.25, 0); // C5
        playNote(659.25, 0.08); // E5
        playNote(783.99, 0.16); // G5
        playNote(987.77, 0.24); // B5
    } catch (e) {}
}

// ☄️ Partículas de Ambiente Suave (Localizadas no Boneco)
// ☄️ Partículas de Ambiente Suave (Localizadas no Boneco)
function spawnWaterParticles(container) {
    const interval = setInterval(() => {
        const p = document.createElement('div');
        p.className = 'particle-water';
        const size = 8 + Math.random() * 12;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = `calc(50% - 130px + ${Math.random() * 260}px)`;
        p.style.top = `calc(50% + 110px - ${Math.random() * 30}px)`;
        p.style.setProperty('--drift', `${-70 + Math.random() * 140}px`);
        p.style.animationDuration = `${2.0 + Math.random() * 1.5}s`;
        container.appendChild(p);
        setTimeout(() => p.remove(), 4000);
    }, 110);
    return interval;
}

function spawnEarthParticles(container) {
    const interval = setInterval(() => {
        const p = document.createElement('div');
        p.className = 'particle-earth';
        p.style.left = `calc(50% - 130px + ${Math.random() * 260}px)`;
        p.style.top = `calc(50% - 120px + ${Math.random() * 30}px)`;
        p.style.setProperty('--drift', `${-70 + Math.random() * 140}px`);
        p.style.animationDuration = `${2.5 + Math.random() * 1.5}s`;
        const leafColors = ['#2e7d32', '#4caf50', '#8bc34a', '#8d6e63', '#5d4037'];
        p.style.background = leafColors[Math.floor(Math.random() * leafColors.length)];
        container.appendChild(p);
        setTimeout(() => p.remove(), 4500);
    }, 180);
    return interval;
}

function spawnFireParticles(container) {
    const interval = setInterval(() => {
        const p = document.createElement('div');
        p.className = 'particle-fire';
        const size = 4 + Math.random() * 8;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = `calc(50% - 120px + ${Math.random() * 240}px)`;
        p.style.top = `calc(50% + 110px - ${Math.random() * 30}px)`;
        p.style.setProperty('--drift', `${-70 + Math.random() * 140}px`);
        p.style.animationDuration = `${1.2 + Math.random() * 0.8}s`;
        container.appendChild(p);
        setTimeout(() => p.remove(), 2500);
    }, 70);
    return interval;
}

function spawnGenericParticles(container) {
    const interval = setInterval(() => {
        const p = document.createElement('div');
        p.className = 'particle-generic';
        const size = 4 + Math.random() * 7;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = `calc(50% - 110px + ${Math.random() * 220}px)`;
        p.style.top = `calc(50% - 110px + ${Math.random() * 220}px)`;
        
        const angle = Math.random() * 2 * Math.PI;
        const dist = 50 + Math.random() * 90;
        p.style.setProperty('--tx', `${Math.cos(angle) * dist}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * dist}px`);
        p.style.animationDuration = `${1.5 + Math.random() * 1.0}s`;
        
        const starColors = ['#ffffff', '#a154f2', '#ffe600', '#00e5ff'];
        p.style.backgroundColor = starColors[Math.floor(Math.random() * starColors.length)];
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 2800);
    }, 140);
    return interval;
}

// ☄️ Partículas de Clímax (Explosões / Dissolução no Ecrã)
function spawnFireClimax(container) {
    for (let i = 0; i < 80; i++) {
        const p = document.createElement('div');
        p.className = 'particle-fire sprit-particle-explode';
        const size = 5 + Math.random() * 9;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * 2 * Math.PI;
        const speed = 30 + Math.random() * 90;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.animationDuration = `${0.6 + Math.random() * 0.7}s`;
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 1400);
    }
}

function spawnWaterClimax(container) {
    for (let i = 0; i < 110; i++) {
        const p = document.createElement('div');
        p.className = 'particle-water-drop sprit-particle-explode';
        const size = 4 + Math.random() * 8;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * 2 * Math.PI;
        const speed = 25 + Math.random() * 85;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.animationDuration = `${0.6 + Math.random() * 0.7}s`;
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 1400);
    }
}

function spawnEarthClimax(container) {
    for (let i = 0; i < 80; i++) {
        const p = document.createElement('div');
        p.className = 'particle-earth-square';
        
        // Cores terrosas e folhas para os quadradinhos
        const earthColors = ['#8d6e63', '#5d4037', '#795548', '#3e2723', '#a1887f', '#2e7d32', '#4caf50', '#8bc34a'];
        p.style.background = earthColors[Math.floor(Math.random() * earthColors.length)];
        
        const size = 5 + Math.random() * 10;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * 2 * Math.PI;
        const speed = 35 + Math.random() * 105;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.setProperty('--rot', `${-360 + Math.random() * 720}deg`);
        p.style.animationDuration = `${1.2 + Math.random() * 0.8}s`;
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 2000);
    }
}

function spawnGenericClimax(container) {
    for (let i = 0; i < 70; i++) {
        const p = document.createElement('div');
        p.className = 'particle-generic sprit-particle-explode';
        const size = 5 + Math.random() * 9;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * 2 * Math.PI;
        const speed = 40 + Math.random() * 115;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.animationDuration = `${0.8 + Math.random() * 0.6}s`;
        
        const starColors = ['#ffffff', '#a154f2', '#ffe600', '#00e5ff', '#ff5100'];
        p.style.backgroundColor = starColors[Math.floor(Math.random() * starColors.length)];
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 1500);
    }
}

function spawnDuckClimax(container) {
    for (let i = 0; i < 45; i++) {
        const p = document.createElement('div');
        p.className = 'particle-feather';
        p.style.left = '50%';
        p.style.top = '50%';
        const angle = Math.random() * 2 * Math.PI;
        const speed = 40 + Math.random() * 120;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.setProperty('--rot', `${-180 + Math.random() * 360}deg`);
        container.appendChild(p);
        setTimeout(() => p.remove(), 1900);
    }
}

function spawnDemonClimax(container) {
    for (let i = 0; i < 60; i++) {
        const p = document.createElement('div');
        p.className = 'particle-demon-fire';
        const size = 10 + Math.random() * 20;
        p.style.setProperty('--size', `${size}px`);
        p.style.left = '50%';
        p.style.top = '50%';
        const angle = Math.random() * 2 * Math.PI;
        const speed = 50 + Math.random() * 140;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        container.appendChild(p);
        setTimeout(() => p.remove(), 1600);
    }
}

function spawnPunkClimax(container) {
    for (let i = 0; i < 50; i++) {
        const p = document.createElement('div');
        p.className = 'particle-punk-spark';
        p.style.left = '50%';
        p.style.top = '50%';
        const angle = Math.random() * 2 * Math.PI;
        const speed = 60 + Math.random() * 160;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.setProperty('--rot', `${angle * (180 / Math.PI) + 90}deg`);
        container.appendChild(p);
        setTimeout(() => p.remove(), 1300);
    }
}

function spawnKingClimax(container) {
    for (let i = 0; i < 20; i++) {
        const c = document.createElement('div');
        c.className = 'particle-crown';
        c.textContent = '👑';
        c.style.left = '50%';
        c.style.top = '50%';
        const angle = Math.random() * 2 * Math.PI;
        const speed = 50 + Math.random() * 130;
        c.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        c.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        container.appendChild(c);
        setTimeout(() => c.remove(), 2100);
    }
    for (let i = 0; i < 35; i++) {
        const coin = document.createElement('div');
        coin.className = 'particle-coin';
        coin.style.left = '50%';
        coin.style.top = '50%';
        const angle = Math.random() * 2 * Math.PI;
        const speed = 35 + Math.random() * 110;
        coin.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        coin.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        container.appendChild(coin);
        setTimeout(() => coin.remove(), 1700);
    }
}

function spawnPeelyClimax(container) {
    for (let i = 0; i < 60; i++) {
        const p = document.createElement('div');
        p.className = 'particle-peely';
        p.style.left = '50%';
        p.style.top = '50%';
        const angle = Math.random() * 2 * Math.PI;
        const speed = 40 + Math.random() * 120;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.setProperty('--rot', `${-180 + Math.random() * 360}deg`);
        container.appendChild(p);
        setTimeout(() => p.remove(), 1800);
    }
}

function spawnGenericClimaxExtra(container) {
    for (let i = 0; i < 50; i++) {
        const p = document.createElement('div');
        p.className = 'particle-generic sprit-particle-explode';
        const size = 5 + Math.random() * 9;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        const angle = Math.random() * 2 * Math.PI;
        const speed = 30 + Math.random() * 100;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        container.appendChild(p);
        setTimeout(() => p.remove(), 1500);
    }
}

function atualizarEfeitosAtivos(agua, terra, fogo, pato, ghost, sleepy, demon, punk, king, aura, boss, peixe, atacante, vento, peely, seven) {
    const container = document.getElementById('active-effects-container') || document.getElementById('active-effects-bar');
    if (!container) return;
    
    container.innerHTML = '';
    let temEfeito = false;

    if (fogo) {
        const circle = document.createElement('div');
        circle.className = 'effect-circle fire';
        circle.title = 'Fogo Ativo (Spawns rápidos)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_Fire_Unvault_Ch7S3_ui_L.webp" alt="Fogo">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (agua && (agua === 'True' || agua === 'true' || agua === 'Super' || agua === 'super' || agua === true)) {
        const isSuper = (agua === 'Super' || agua === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle water';
        circle.title = isSuper ? 'Água Ativa [SUPER] (-60% Taxa de Captura para todos exceto conjurador)' : 'Água Ativa (-40% Taxa de Captura para todos exceto conjurador)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_Water_Unvault_Ch7S3_ui_L.webp" alt="Água">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (terra && (terra === 'True' || terra === 'true' || terra === 'Super' || terra === 'super' || terra === true)) {
        const isSuper = (terra === 'Super' || terra === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle earth';
        circle.title = isSuper ? 'Terra Ativa [SUPER] (Garante elemental Mítico)' : 'Terra Ativa (Garante Épico, Lendário ou Mítico)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_Earth_Ch7S3_UI_L.webp" alt="Terra">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (pato && (pato === 'True' || pato === 'true' || pato === 'Super' || pato === 'super' || pato === true)) {
        const isSuper = (pato === 'Super' || pato === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle duck';
        circle.title = isSuper ? 'Pato Ativo [SUPER] (Garante variante Gummy ou superior)' : 'Pato Ativo (Garante variante Gold ou superior)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Duck_Default_L.webp" alt="Pato">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (ghost) {
        const circle = document.createElement('div');
        circle.className = 'effect-circle ghost';
        circle.title = 'Fantasma Ativo (Spawn oculto)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_Ghost_Unvault_L.webp" alt="Fantasma">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (sleepy && (sleepy === 'True' || sleepy === 'true' || sleepy === 'Super' || sleepy === 'super' || sleepy === true)) {
        const isSuper = (sleepy === 'Super' || sleepy === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle sleepy';
        circle.title = isSuper ? 'Sonhos Ativo [SUPER] (Adormece 2 pessoas no sorteio)' : 'Sonhos Ativo (Adormece 1 pessoa no sorteio)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_Sleepy_ui_L.webp" alt="Sonhos">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (demon && (demon === 'True' || demon === 'true' || demon === 'Super' || demon === 'super' || demon === true)) {
        const isSuper = (demon === 'Super' || demon === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle demon';
        circle.title = isSuper ? 'Demónio Ativo [SUPER] (Apenas conjurador pode usar Master/Ultra Ball)' : 'Demónio Ativo (Apenas conjurador pode usar Master Ball)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_RedDemon_Default_L.webp" alt="Demónio">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (punk && (punk === 'True' || punk === 'true' || punk === 'Super' || punk === 'super' || punk === true)) {
        const isSuper = (punk === 'Super' || punk === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle punk';
        circle.title = isSuper ? 'Punk Ativo [SUPER] (Rouba elementais de até 2 participantes)' : 'Punk Ativo (Rouba 1 elemental de participante)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_Punk_ui_L.webp" alt="Punk">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (king) {
        const circle = document.createElement('div');
        circle.className = 'effect-circle king';
        circle.title = 'Rei Ativo (Apenas conjurador pode arremessar)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_King_ui_L.webp" alt="Rei">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (peely && (peely === 'True' || peely === 'true' || peely === 'Super' || peely === 'super' || peely === true)) {
        const isSuper = (peely === 'Super' || peely === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle peely';
        circle.title = isSuper ? 'Peely Ativo [SUPER] (Primeiros 2 lugares da fila escorregam)' : 'Peely Ativo (1º lugar da fila escorrega)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_Peely_ui_L.webp" alt="Peely">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (seven && (seven === 'True' || seven === 'true' || seven === 'Super' || seven === 'super' || seven === true)) {
        const isSuper = (seven === 'Super' || seven === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle seven';
        circle.title = isSuper ? 'Seven Ativo [SUPER] (Traz elemental dos últimos 7 com Upgrade Duplo)' : 'Seven Ativo (Traz elemental dos últimos 7 com Upgrade de Variante)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Creature_Sprite_Seven_ui_L.webp" alt="Seven">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (vento && (vento === 'True' || vento === 'true' || vento === 'Super' || vento === 'super' || vento === true)) {
        const isSuper = (vento === 'Super' || vento === 'super');
        const circle = document.createElement('div');
        circle.className = 'effect-circle wind';
        circle.title = isSuper ? 'Vento Ativo [SUPER] (Garante Ar/Vento com chances lendárias/míticas)' : 'Vento Ativo (Garante Ar/Vento no Spawn)';
        circle.innerHTML = '<img src="Sprites/T_Icon_BR_Air_Default_L.webp" alt="Vento">';
        container.appendChild(circle);
        temEfeito = true;
    }

    if (temEfeito) {
        container.classList.remove('hidden');
    } else {
        container.classList.add('hidden');
    }
}

function ocultarEfeitosAtivos() {
    // No-op: mantemos os efeitos ativos visíveis de forma persistente
}

// =========================================================================
// GHOST EFFECT PARTICLES & SOUND FUNCTIONS
// =========================================================================
function spawnGhostSmokeParticles(container) {
    const interval = setInterval(() => {
        const p = document.createElement('div');
        p.className = 'particle-smoke';
        const size = 35 + Math.random() * 45;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = `calc(50% - ${size/2}px - 40px + ${Math.random() * 80}px)`;
        p.style.top = `calc(50% - ${size/2}px + 60px - ${Math.random() * 30}px)`;
        p.style.setProperty('--drift', `${-60 + Math.random() * 120}px`);
        p.style.animationName = 'smokeFloat';
        p.style.animationDuration = `${2.0 + Math.random() * 1.5}s`;
        container.appendChild(p);
        setTimeout(() => p.remove(), 3500);
    }, 130);
    return interval;
}

function spawnGhostClimax(container) {
    for (let i = 0; i < 40; i++) {
        const p = document.createElement('div');
        p.className = 'particle-generic sprit-particle-explode';
        const size = 5 + Math.random() * 9;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * 2 * Math.PI;
        const speed = 40 + Math.random() * 115;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.animationDuration = `${0.8 + Math.random() * 0.6}s`;
        
        const starColors = ['#ffffff', '#a154f2', '#800080'];
        p.style.backgroundColor = starColors[Math.floor(Math.random() * starColors.length)];
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 1500);
    }
    for (let i = 0; i < 40; i++) {
        const p = document.createElement('div');
        p.className = 'particle-smoke sprit-particle-explode';
        const size = 30 + Math.random() * 50;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * 2 * Math.PI;
        const speed = 30 + Math.random() * 100;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.animationDuration = `${1.0 + Math.random() * 1.0}s`;
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 2000);
    }
}

function tocarSomSpritGhost() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(300, ctx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(60, ctx.currentTime + 1.2);
        
        gain.gain.setValueAtTime(0, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.12, ctx.currentTime + 0.1);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 1.2);
        
        const filter = ctx.createBiquadFilter();
        filter.type = 'lowpass';
        filter.frequency.setValueAtTime(400, ctx.currentTime);
        
        osc.connect(filter);
        filter.connect(gain);
        gain.connect(ctx.destination);
        
        osc.start();
        osc.stop(ctx.currentTime + 1.2);
        
        const playChime = (freq, delay) => {
            const oscC = ctx.createOscillator();
            const gainC = ctx.createGain();
            oscC.type = 'sine';
            oscC.frequency.value = freq;
            gainC.gain.setValueAtTime(0, ctx.currentTime + delay);
            gainC.gain.linearRampToValueAtTime(0.05, ctx.currentTime + delay + 0.05);
            gainC.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + delay + 0.8);
            oscC.connect(gainC);
            gainC.connect(ctx.destination);
            oscC.start(ctx.currentTime + delay);
            oscC.stop(ctx.currentTime + delay + 0.8);
        };
        
        playChime(880, 0.2);
        playChime(660, 0.4);
        playChime(440, 0.6);
    } catch (e) {}
}

function iniciarFumacaAmbient() {
    pararFumacaAmbient();
    const container = document.getElementById('elemental-area');
    if (!container) return;
    ghostSmokeInterval = setInterval(() => {
        const p = document.createElement('div');
        p.className = 'particle-smoke';
        const size = 60 + Math.random() * 80;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = `calc(50% - ${size/2}px + ${-60 + Math.random() * 120}px)`;
        p.style.top = `calc(50% - ${size/2}px + ${-40 + Math.random() * 100}px)`;
        p.style.setProperty('--drift', `${-80 + Math.random() * 160}px`);
        p.style.animationName = 'smokeFloat';
        p.style.animationDuration = `${3.0 + Math.random() * 2.0}s`;
        container.appendChild(p);
        setTimeout(() => p.remove(), 4500);
    }, 250);
}

function pararFumacaAmbient() {
    if (ghostSmokeInterval) {
        clearInterval(ghostSmokeInterval);
        ghostSmokeInterval = null;
    }
    const container = document.getElementById('elemental-area');
    if (container) {
        container.querySelectorAll('.particle-smoke').forEach(p => p.remove());
    }
}

function spawnSleepyParticles(container) {
    const interval = setInterval(() => {
        const p = document.createElement('div');
        p.className = 'particle-sleepy';
        p.textContent = 'Z';
        const size = 12 + Math.random() * 14;
        p.style.fontSize = `${size}px`;
        p.style.left = `calc(50% - 100px + ${Math.random() * 200}px)`;
        p.style.top = `calc(50% + 80px - ${Math.random() * 40}px)`;
        p.style.setProperty('--drift', `${-50 + Math.random() * 100}px`);
        p.style.animationName = 'floatSleepy';
        p.style.animationDuration = `${2.5 + Math.random() * 1.5}s`;
        container.appendChild(p);
        setTimeout(() => p.remove(), 4000);
    }, 250);
    return interval;
}

function spawnSleepyClimax(container) {
    for (let i = 0; i < 40; i++) {
        const p = document.createElement('div');
        p.className = 'particle-sleepy sprit-particle-explode';
        p.textContent = 'Z';
        const size = 14 + Math.random() * 16;
        p.style.fontSize = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * 2 * Math.PI;
        const speed = 40 + Math.random() * 110;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.animationDuration = `${1.0 + Math.random() * 0.6}s`;
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 1600);
    }
    for (let i = 0; i < 40; i++) {
        const p = document.createElement('div');
        p.className = 'particle-generic sprit-particle-explode';
        const size = 5 + Math.random() * 8;
        p.style.width = `${size}px`;
        p.style.height = `${size}px`;
        p.style.left = '50%';
        p.style.top = '50%';
        
        const angle = Math.random() * 2 * Math.PI;
        const speed = 35 + Math.random() * 105;
        p.style.setProperty('--tx', `${Math.cos(angle) * speed}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * speed}px`);
        p.style.animationDuration = `${0.8 + Math.random() * 0.6}s`;
        p.style.backgroundColor = '#5e5ce6';
        
        container.appendChild(p);
        setTimeout(() => p.remove(), 1400);
    }
}

function tocarSomSpritSleepy() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'sine';
        osc.frequency.setValueAtTime(220, ctx.currentTime);
        
        const lfo = ctx.createOscillator();
        const lfoGain = ctx.createGain();
        lfo.frequency.value = 4;
        lfoGain.gain.value = 15;
        
        lfo.connect(lfoGain);
        lfoGain.connect(osc.frequency);
        
        gain.gain.setValueAtTime(0, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.15, ctx.currentTime + 0.2);
        gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 1.5);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        lfo.start();
        osc.start();
        lfo.stop(ctx.currentTime + 1.5);
        osc.stop(ctx.currentTime + 1.5);
    } catch (e) {}
}

function mostrarPescaExtra(usuario, elemId) {
    const panel = document.getElementById('fishing-panel');
    const lineContainer = document.getElementById('fishing-line-container');
    const cardHolder = document.getElementById('fishing-card-holder');
    const cardImg = document.getElementById('fishing-card-img');
    const userMsg = document.getElementById('fishing-user-msg');
    
    // 1. Reset state
    panel.classList.remove('hidden');
    panel.style.opacity = '1';
    lineContainer.style.top = '-280px';
    cardHolder.classList.remove('card-hooked');
    cardHolder.classList.add('hidden');
    userMsg.textContent = ``;
    
    // 2. Anunciar quem está a pescar
    userMsg.textContent = `🎣 @${usuario} lançou a cana de pesca...`;

    // 3. Descer o anzol até à água
    setTimeout(() => {
        lineContainer.style.top = '0px'; 
        tocarSomAtirar(); 
    }, 500);

    // 4. Splash de água e bolhas
    setTimeout(() => {
        tocarSomSplashAgua();
        tocarSomBubbles();
        criarParticulasAguaPesca();
    }, 1800);

    // 5. Prender o peixe no anzol
    setTimeout(() => {
        const bichoInfo = elementaisMap[elemId] || { file: 'T_Icon_BR_Creature_Sprite_Fishy_ui_L.webp' };
        cardImg.src = `Sprites/${bichoInfo.file}`;
        
        cardHolder.classList.remove('hidden');
        setTimeout(() => {
            cardHolder.classList.add('card-hooked');
        }, 50);
        
        userMsg.textContent = `🎣 @${usuario} pescou algo!`;
        tocarSomRevelaCard();
        criarParticulasAguaPesca();
    }, 3000);

    // 6. Puxar o anzol com o cromo
    setTimeout(() => {
        lineContainer.style.top = '-400px';
        cardHolder.style.transition = 'all 1.5s cubic-bezier(0.175, 0.885, 0.32, 1)';
        cardHolder.style.top = '-190px'; 
        tocarSomSplashAgua();
    }, 4500);

    // 7. Fechar o painel
    setTimeout(() => {
        panel.style.opacity = '0';
        setTimeout(() => {
            panel.classList.add('hidden');
            cardHolder.style.transition = '';
            cardHolder.style.top = '210px'; 
        }, 500);
    }, 6500);
}

function criarParticulasAguaPesca() {
    const container = document.getElementById('fishing-particles');
    const count = 35;
    for (let i = 0; i < count; i++) {
        const b = document.createElement('div');
        b.className = 'fishing-bubble';
        b.style.left = `${40 + Math.random() * 20}%`;
        b.style.bottom = `${30 + Math.random() * 20}%`;
        const size = 6 + Math.random() * 14;
        b.style.width = `${size}px`;
        b.style.height = `${size}px`;
        b.style.setProperty('--rx', `${(Math.random() - 0.5) * 120}px`);
        b.style.animationDelay = `${Math.random() * 0.3}s`;
        b.style.animationDuration = `${0.8 + Math.random() * 0.7}s`;
        container.appendChild(b);
        setTimeout(() => b.remove(), 1600);
    }
}

function tocarSomBubbles() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        for (let i = 0; i < 8; i++) {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(800 + Math.random() * 600, ctx.currentTime + i * 0.08);
            osc.frequency.exponentialRampToValueAtTime(300, ctx.currentTime + i * 0.08 + 0.06);
            gain.gain.setValueAtTime(0.04, ctx.currentTime + i * 0.08);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + i * 0.08 + 0.06);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(ctx.currentTime + i * 0.08);
            osc.stop(ctx.currentTime + i * 0.08 + 0.07);
        }
    } catch (e) {}
}

function spawnAuroraParticles() {
    if (!spritParticles) return;
    const interval = setInterval(() => {
        if (!spritArea.classList.contains('hidden')) {
            const p = document.createElement('div');
            p.className = 'aurora-particle';
            p.style.left = `${20 + Math.random() * 60}%`;
            p.style.top = `${30 + Math.random() * 40}%`;
            p.style.setProperty('--size', `${15 + Math.random() * 30}px`);
            p.style.setProperty('--tx', `${(Math.random() - 0.5) * 200}px`);
            p.style.setProperty('--ty', `${-(80 + Math.random() * 120)}px`);
            p.style.animationDuration = `${2.5 + Math.random() * 1.5}s`;
            spritParticles.appendChild(p);
            setTimeout(() => p.remove(), 4000);
        }
    }, 180);
    activeSpritIntervals.push(interval);
}

function spawnAuroraClimax() {
    if (!spritParticles) return;
    for (let i = 0; i < 45; i++) {
        const p = document.createElement('div');
        p.className = 'aurora-particle';
        p.style.left = '50%';
        p.style.top = '50%';
        p.style.setProperty('--size', `${20 + Math.random() * 40}px`);
        const angle = Math.random() * Math.PI * 2;
        const dist = 60 + Math.random() * 160;
        p.style.setProperty('--tx', `${Math.cos(angle) * dist}px`);
        p.style.setProperty('--ty', `${Math.sin(angle) * dist}px`);
        p.style.animationDuration = `${1.5 + Math.random() * 1.0}s`;
        spritParticles.appendChild(p);
        setTimeout(() => p.remove(), 2500);
    }
}

function spawnBossClimax() {
    const container = document.getElementById('sprit-area');
    container.classList.add('sismo-active');
    setTimeout(() => {
        container.classList.remove('sismo-active');
    }, 2500);
    
    if (spritParticles) {
        for (let i = 0; i < 50; i++) {
            const p = document.createElement('div');
            p.className = 'sprit-particle fire';
            p.style.left = '50%';
            p.style.top = '50%';
            const angle = Math.random() * Math.PI * 2;
            const dist = 40 + Math.random() * 180;
            p.style.setProperty('--dx', `${Math.cos(angle) * dist}px`);
            p.style.setProperty('--dy', `${Math.sin(angle) * dist - (30 + Math.random() * 100)}px`);
            p.style.animationDelay = `${Math.random() * 0.2}s`;
            p.style.animationDuration = `${1.0 + Math.random() * 0.8}s`;
            spritParticles.appendChild(p);
            setTimeout(() => p.remove(), 2000);
        }
    }
}

function tocarSomChimeAura() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const notes = [523.25, 659.25, 783.99, 1046.50];
        notes.forEach((freq, index) => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(freq, ctx.currentTime + index * 0.08);
            gain.gain.setValueAtTime(0.01, ctx.currentTime + index * 0.08);
            gain.gain.linearRampToValueAtTime(0.1, ctx.currentTime + index * 0.08 + 0.03);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + index * 0.08 + 0.4);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(ctx.currentTime + index * 0.08);
            osc.stop(ctx.currentTime + index * 0.08 + 0.45);
        });
    } catch (e) {}
}

function tocarSomSismoBoss() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(80, ctx.currentTime);
        osc.frequency.linearRampToValueAtTime(35, ctx.currentTime + 2.0);
        
        const lfo = ctx.createOscillator();
        const lfoGain = ctx.createGain();
        lfo.frequency.setValueAtTime(8, ctx.currentTime); 
        lfoGain.gain.setValueAtTime(15, ctx.currentTime);
        lfo.connect(lfoGain);
        lfoGain.connect(osc.frequency);
        
        gain.gain.setValueAtTime(0.01, ctx.currentTime);
        gain.gain.linearRampToValueAtTime(0.35, ctx.currentTime + 0.4);
        gain.gain.linearRampToValueAtTime(0.2, ctx.currentTime + 1.2);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 2.2);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        
        lfo.start();
        osc.start();
        lfo.stop(ctx.currentTime + 2.2);
    } catch (e) {}
}

function mostrarAnimacaoQuack(userName, elemId, req, delayMs = 4500) {
    const item = elementaisMap[elemId];
    if (!item) return;

    setTimeout(() => {
        const quackPanel = document.getElementById('quack-panel');
        const quackCardImg = document.getElementById('quack-card-img');
        const quackReqNum = document.getElementById('quack-req-num');
        const quackUserMsg = document.getElementById('quack-user-msg');

        if (quackReqNum) quackReqNum.textContent = req;
        if (quackCardImg) quackCardImg.src = `Sprites/${item.file}`;
        if (quackUserMsg) quackUserMsg.textContent = `@${userName} ganhou +1 ${item.name}!`;

        if (quackPanel) {
            quackPanel.classList.remove('hidden');
            quackPanel.style.opacity = '1';
        }
        tocarSomRevelaCard();

        setTimeout(() => {
            if (quackPanel) {
                quackPanel.style.transition = 'opacity 0.5s ease';
                quackPanel.style.opacity = '0';
                setTimeout(() => {
                    quackPanel.classList.add('hidden');
                }, 500);
            }
        }, 5500);
    }, delayMs);
}
