using System;
using System.IO;

public class CPHInline
{
    private string caminhoEstado = @"I:\Twitch\Games\elementais\jogo_estado.txt";

    public bool Execute()
    {
        // Força a caça a terminar na memória do bot
        CPH.SetGlobalVar("cacaAtiva", false);
        CPH.SetGlobalVar("ultimoSpawnTicks", 0L);
        
        // Envia o comando de limpeza total para o teu HTML com os efeitos ativos acumulados
        EscreverEstado("LIMPAR");
        
        CPH.RunAction("Elementais - Desativar Caça");
        CPH.LogInfo("[Elementais] Ecrã limpo de emergência.");
        
        return true;
    }

    private void EscreverEstado(string cmd)
    {
        try
        {
            bool cacaAtiva = CPH.GetGlobalVar<bool>("cacaAtiva");
            bool aguaAtiva = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAguaAtiva") : CPH.GetGlobalVar<bool>("spritAguaAtivo");
            bool aguaSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAguaSuper") : CPH.GetGlobalVar<bool>("spritAguaSuper");
            string aguaVal = aguaAtiva ? (aguaSuper ? "Super" : "True") : "False";

            bool terraAtiva = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritTerraAtiva") : CPH.GetGlobalVar<bool>("spritTerraAtivo");
            bool terraSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritTerraSuper") : CPH.GetGlobalVar<bool>("spritTerraSuper");
            string terraVal = terraAtiva ? (terraSuper ? "Super" : "True") : "False";

            bool fogo = CPH.GetGlobalVar<bool>("spritFogoAtivo");

            bool patoAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPatoAtiva") : CPH.GetGlobalVar<bool>("spritPatoAtivo");
            bool patoSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPatoSuper") : CPH.GetGlobalVar<bool>("spritPatoSuper");
            string patoVal = patoAtivo ? (patoSuper ? "Super" : "True") : "False";

            bool ghost = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritGhostAtiva") : CPH.GetGlobalVar<bool>("spritGhostAtivo");

            bool sleepyAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritSleepyAtiva") : CPH.GetGlobalVar<bool>("spritSleepyAtivo");
            int sleepyCount = CPH.GetGlobalVar<int>(cacaAtiva ? "cacaSpritSleepyCount" : "spritSleepyCount");
            string sleepyVal = sleepyAtivo ? (sleepyCount > 1 ? "Super" : "True") : "False";

            bool demonAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritDemonAtiva") : CPH.GetGlobalVar<bool>("spritDemonAtivo");
            bool demonSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritDemonSuper") : CPH.GetGlobalVar<bool>("spritDemonSuper");
            string demonVal = demonAtivo ? (demonSuper ? "Super" : "True") : "False";

            bool punkAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPunkAtiva") : CPH.GetGlobalVar<bool>("spritPunkAtivo");
            bool punkSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPunkSuper") : CPH.GetGlobalVar<bool>("spritPunkSuper");
            string punkVal = punkAtivo ? (punkSuper ? "Super" : "True") : "False";

            bool king = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritKingAtiva") : CPH.GetGlobalVar<bool>("spritKingAtivo");
            bool aura = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAuraAtiva") : CPH.GetGlobalVar<bool>("spritAuraAtivo");
            bool bossAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritBossAtiva") : CPH.GetGlobalVar<bool>("spritBossAtivo");
            bool bossSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritBossSuper") : CPH.GetGlobalVar<bool>("spritBossSuper");
            string bossVal = bossAtivo ? (bossSuper ? "Super" : "True") : "False";

            bool peixeAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPeixeAtiva") : CPH.GetGlobalVar<bool>("spritPeixeAtivo");
            bool peixeSuper = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritPeixeSuper") : CPH.GetGlobalVar<bool>("spritPeixeSuper");
            string peixeVal = peixeAtivo ? (peixeSuper ? "Super" : "True") : "False";

            bool atacanteAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritAtacanteAtiva") : CPH.GetGlobalVar<bool>("spritAtacanteAtivo");
            string cacaAtacanteSuperStr = CPH.GetGlobalVar<string>(cacaAtiva ? "cacaSpritAtacanteSuper" : "spritAtacanteSuper") ?? "";
            bool atacanteSuper = !string.IsNullOrEmpty(cacaAtacanteSuperStr);
            string atacanteVal = atacanteAtivo ? (atacanteSuper ? "Super" : "True") : "False";

            bool ventoAtivo = cacaAtiva ? CPH.GetGlobalVar<bool>("cacaSpritVentoAtiva") : CPH.GetGlobalVar<bool>("spritVentoAtivo");
            string cacaVentoSuperStr = CPH.GetGlobalVar<string>(cacaAtiva ? "cacaSpritVentoSuper" : "spritVentoSuper") ?? "";
            bool ventoSuper = !string.IsNullOrEmpty(cacaVentoSuperStr);
            string ventoVal = ventoAtivo ? (ventoSuper ? "Super" : "True") : "False";

            string suffix = string.Format("|agua={0};terra={1};fogo={2};pato={3};ghost={4};sleepy={5};demon={6};punk={7};king={8};aura={9};boss={10};peixe={11};atacante={12};vento={13}",
                aguaVal, terraVal, fogo, patoVal, ghost, sleepyVal, demonVal, punkVal, king, aura, bossVal, peixeVal, atacanteVal, ventoVal);
            File.WriteAllText(caminhoEstado, cmd + suffix);
        }
        catch {}
    }
}