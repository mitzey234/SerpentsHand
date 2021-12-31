using Exiled.API.Features;
using Exiled.Loader;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace SerpentsHand
{
    public class SerpentsHand : Plugin<Config>
    {
        public EventHandlers EventHandlers;

        public static SerpentsHand instance;
        private Harmony hInstance;

        public static bool isScp035 = false;

        private bool state = false;

        public static List<int> FFGrants = new List<int>();

        public override void OnEnabled()
        {
            if (state) return;

            if (!Config.IsEnabled) return;

            hInstance = new Harmony("cyanox.serpentshand");
            hInstance.PatchAll();

            instance = this;
            EventHandlers = new EventHandlers();
            Check035();

            Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnRoundStart;
            Exiled.Events.Handlers.Server.RespawningTeam += EventHandlers.OnTeamRespawn;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += EventHandlers.OnPocketDimensionEnter;
            Exiled.Events.Handlers.Player.FailingEscapePocketDimension += EventHandlers.OnPocketDimensionDie;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += EventHandlers.OnPocketDimensionExit;
            Exiled.Events.Handlers.Player.Hurting += EventHandlers.OnPlayerHurt;
            //Exiled.Events.Handlers.Server.EndingRound += EventHandlers.OnCheckRoundEnd;
            Exiled.Events.Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;
            Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnSetRole;
            Exiled.Events.Handlers.Player.Left += EventHandlers.OnDisconnect;
            Exiled.Events.Handlers.Scp106.Containing += EventHandlers.OnContain106;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += EventHandlers.OnRACommand;
            Exiled.Events.Handlers.Player.ActivatingGenerator += EventHandlers.OnActivatingGenerator;
            Exiled.Events.Handlers.Player.EnteringFemurBreaker += EventHandlers.OnFemurEnter;
            Exiled.Events.Handlers.Player.Died += EventHandlers.OnPlayerDeath;
            Exiled.Events.Handlers.Player.Shot += EventHandlers.OnShoot;
            Exiled.Events.Handlers.Player.Spawning += EventHandlers.OnSpawning;
            Exiled.Events.Handlers.Server.RestartingRound += EventHandlers.OnRoundRestart;

            state = true;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {

            Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnRoundStart;
            Exiled.Events.Handlers.Server.RespawningTeam -= EventHandlers.OnTeamRespawn;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= EventHandlers.OnPocketDimensionEnter;
            Exiled.Events.Handlers.Player.FailingEscapePocketDimension -= EventHandlers.OnPocketDimensionDie;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= EventHandlers.OnPocketDimensionExit;
            Exiled.Events.Handlers.Player.Hurting -= EventHandlers.OnPlayerHurt;
            //Exiled.Events.Handlers.Server.EndingRound -= EventHandlers.OnCheckRoundEnd;
            Exiled.Events.Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;
            Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnSetRole;
            Exiled.Events.Handlers.Player.Left -= EventHandlers.OnDisconnect;
            Exiled.Events.Handlers.Scp106.Containing -= EventHandlers.OnContain106;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= EventHandlers.OnRACommand;
            Exiled.Events.Handlers.Player.ActivatingGenerator -= EventHandlers.OnActivatingGenerator;
            Exiled.Events.Handlers.Player.EnteringFemurBreaker -= EventHandlers.OnFemurEnter;
            Exiled.Events.Handlers.Player.Died -= EventHandlers.OnPlayerDeath;
            Exiled.Events.Handlers.Player.Shot -= EventHandlers.OnShoot;
            Exiled.Events.Handlers.Player.Spawning -= EventHandlers.OnSpawning;
            Exiled.Events.Handlers.Server.RestartingRound -= EventHandlers.OnRoundRestart;

            hInstance.UnpatchAll(hInstance.Id);
            EventHandlers = null;

            state = true;
            base.OnDisabled();
        }

        public override string Name => "SerpentsHand";
        public override string Author => "Cyanox";

        internal void Check035()
        {
            foreach (var plugin in Loader.Plugins)
            {
                if (plugin.Name == "scp035")
                {
                    isScp035 = true;
                    return;
                }
            }
        }
    }
}
