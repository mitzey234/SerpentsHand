using System.Collections.Generic;
using System.Linq;
using MEC;
using UnityEngine;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.API.Extensions;

namespace SerpentsHand
{
    public partial class EventHandlers
    {
        private List<int> shPocketPlayers = new List<int>();
        internal static Dictionary<Player, Vector3> PositionsToSpawn = new Dictionary<Player, Vector3>();

        private int teamRespawnCount = 0;
        private int serpentsRespawnCount = 0;

        public static List<Player> shPlayers = new List<Player>();

        private static System.Random rand = new System.Random();

        private static Vector3 shSpawnPos = new Vector3(0, 1002, 8);

        public void OnRoundStart()
        {
            PositionsToSpawn.Clear();
            shPlayers.Clear();
            shPocketPlayers.Clear();
            teamRespawnCount = 0;
            serpentsRespawnCount = 0;
        }

        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            int numScps = Player.List.Count(p => p.Team == Team.SCP && (p.Role != RoleType.Scp0492 ||
                (p.SessionVariables.ContainsKey("is966") && (bool)p.SessionVariables["is966"])));
            if (serpentsRespawnCount < SerpentsHand.instance.Config.MaxSpawns &&
                teamRespawnCount >= SerpentsHand.instance.Config.RespawnDelay &&
                (numScps <= 2 && numScps > 0) &&
                (Player.List.Count(p => p.IsHuman) > 6))
            {
                if (rand.Next(1, 101) <= SerpentsHand.instance.Config.SpawnChance)
                {
                    if (ev.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox)
                    {
                        // Prevent announcement
                        ev.NextKnownTeam = Respawning.SpawnableTeamType.ChaosInsurgency;
                    }

                    List<Player> SHPlayers = new List<Player>();
                    List<Player> CIPlayers = new List<Player>(ev.Players);
                    ev.Players.Clear();

                    for (int i = 0; i < SerpentsHand.instance.Config.MaxSquad && CIPlayers.Count > 0; i++)
                    {
                        Player player = CIPlayers[rand.Next(CIPlayers.Count)];
                        SHPlayers.Add(player);
                        CIPlayers.Remove(player);
                    }
                    Timing.CallDelayed(0.1f, () => SpawnSquad(SHPlayers));

                    serpentsRespawnCount++;
                }
                else if (ev.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency)
                {
                    string ann = SerpentsHand.instance.Config.CiEntryAnnouncement;
                    if (ann != string.Empty)
                    {
                        Cassie.Message(ann, true, true);
                    }
                }
            }
            teamRespawnCount++;
        }

        public void OnPocketDimensionEnter(EnteringPocketDimensionEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player))
            {
                shPocketPlayers.Add(ev.Player.Id);
            }
        }

        public void OnPocketDimensionDie(FailingEscapePocketDimensionEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player))
            {
                if (!SerpentsHand.instance.Config.FriendlyFire)
                {
                    ev.IsAllowed = false;
                }
                if (SerpentsHand.instance.Config.TeleportTo106)
                {
                    TeleportTo106(ev.Player);
                }
                shPocketPlayers.Remove(ev.Player.Id);
            }
        }

        public void OnPocketDimensionExit(EscapingPocketDimensionEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player))
            {
                ev.IsAllowed = false;
                if (SerpentsHand.instance.Config.TeleportTo106)
                {
                    TeleportTo106(ev.Player);
                }
                shPocketPlayers.Remove(ev.Player.Id);
            }
        }

        //Untested: Recontainment, RagdollLess, FriendlyFireDetector, Flying, Contain
        //Unfixable: Hemorrhage
        public enum DamageTypes
        {
            None,
            Unknown,
            AK,
            Asphyxiation,
            Bleeding,
            Com15,
            Com18,
            Contain,
            CrossVec,
            Decont,
            E11SR,
            Falldown,
            Flying,
            FriendlyFireDetector,
            FSP9,
            Grenade,
            Hemorrhage,
            Logicer,
            Lure,
            MicroHID,
            Nuke,
            Pocket,
            Poison,
            RagdollLess,
            Recontainment,
            Revolver,
            Scp018,
            Scp049,
            Scp0492,
            Scp096,
            Scp096Charge,
            Scp096Pry,
            Scp106,
            Scp173,
            Scp207,
            Scp939,
            Shotgun,
            Tesla,
            Wall
        }

        //Returns the most likely applicable damage type for the damage handler given
        public DamageTypes ParseHandler(PlayerStatsSystem.DamageHandlerBase d)
        {
            if (d.ServerLogsText == null) return DamageTypes.None;
            if (d.ServerLogsText.Contains("Micro H.I.D.")) return DamageTypes.MicroHID;
            if (d.ServerLogsText.Contains("Fall damage")) return DamageTypes.Falldown;
            if (d.ServerLogsText.Contains("Crushed.")) return DamageTypes.Wall;
            if (d.ServerLogsText.Contains("SCP-207")) return DamageTypes.Scp207;
            if (d.ServerLogsText.Contains("SCP-096's charge")) return DamageTypes.Scp096Charge;
            if (d.ServerLogsText.Contains("Melted by a highly corrosive substance")) return DamageTypes.Decont;
            if (d.ServerLogsText.Contains("Tried to pass through a gate being breached by SCP-096")) return DamageTypes.Scp096Pry;
            if (d.ServerLogsText.Contains("Got slapped by SCP-096")) return DamageTypes.Scp096;
            if (d.ServerLogsText.Contains("SCP-018")) return DamageTypes.Scp018;
            if (d.ServerLogsText.Contains("Scp0492")) return DamageTypes.Scp0492;
            if (d.ServerLogsText.Contains("bait for SCP-106")) return DamageTypes.Lure;
            if (d.ServerLogsText.Contains("Died to alpha warhead")) return DamageTypes.Nuke;
            if (d.ServerLogsText.Contains("Friendly Fire")) return DamageTypes.FriendlyFireDetector;
            if (d.ServerLogsText.Contains("Asphyxiated")) return DamageTypes.Asphyxiation;
            if (d.ServerLogsText.Contains("GunCrossvec")) return DamageTypes.CrossVec;
            if (d.ServerLogsText.Contains("GunCOM18")) return DamageTypes.Com18;
            if (d.ServerLogsText.Contains("GunCOM15")) return DamageTypes.Com15;
            if (d.ServerLogsText.Contains("GunShotgun")) return DamageTypes.Shotgun;
            if (d.ServerLogsText.Contains("Explosion.")) return DamageTypes.Grenade;
            foreach (DamageTypes dmgtyp in DamageTypes.GetValues(typeof(DamageTypes)))
            {
                if (d.ServerLogsText.Contains(dmgtyp.ToString())) return dmgtyp;
            }
            return DamageTypes.Unknown;
        }

        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.Target == null || ev.Attacker == null) return;
            DamageTypes damageType = ParseHandler(ev.Handler.Base);
            List<Player> scp035 = null;

            if (SerpentsHand.isScp035)
            {
                scp035 = TryGet035();
            } else
            {
                scp035 = new List<Player>();
            }

            if (((shPlayers.Contains(ev.Target) && (ev.Attacker.Team == Team.SCP || damageType == DamageTypes.Pocket)) ||
                (shPlayers.Contains(ev.Attacker) && (ev.Target.Team == Team.SCP || (scp035 != null && scp035.Select(s => s.Id).ToList().Contains(ev.Target.Id)))) ||
                (shPlayers.Contains(ev.Target) && shPlayers.Contains(ev.Attacker) && ev.Target != ev.Attacker)) && !SerpentsHand.instance.Config.FriendlyFire) ev.IsAllowed = false;

            if (shPlayers.Contains(ev.Target) && damageType == DamageTypes.Pocket) ev.IsAllowed = false;

            if (SerpentsHand.instance.Config.EndRoundFriendlyFire && RoundEnded)
            {
                ev.IsAllowed = true;
                SerpentsHand.FFGrants.Add(ev.Handler.Base.GetHashCode());
            }
        }

        public void OnPlayerDeath(DiedEventArgs ev)
        {
            if (shPlayers.Contains(ev.Target))
            {
                ev.Target.CustomInfo = string.Empty;
                ev.Target.ReferenceHub.nicknameSync.ShownPlayerInfo |= PlayerInfoArea.Role;
                shPlayers.Remove(ev.Target);
            }

            if (ev.Target.Role == RoleType.Scp106 && !SerpentsHand.instance.Config.FriendlyFire)
            {
                foreach (Player player in Player.List.Where(x => shPocketPlayers.Contains(x.Id)))
                {
                    //Is broke
                    //player.ReferenceHub.playerStats.HurtPlayer(new PlayerStatsSystem.PlayerStats.HitInfo(50000, "WORLD", ev.HitInformations.Tool, player.Id, true), player.GameObject);
                    player.Kill("Died with Scp106 in the Pocket Dimension");
                }
            }
        }

        public void OnCheckRoundEnd(EndingRoundEventArgs ev)
        {
            List<Player> scp035 = null;

            if (SerpentsHand.isScp035)
            {
                scp035 = TryGet035();
            } else
            {
                scp035 = new List<Player>();
            }

            bool MTFAlive = CountRoles(Team.MTF) > 0;
            bool CiAlive = CountRoles(Team.CHI) > 0;
            bool ScpAlive = CountRoles(Team.SCP) + scp035.Count > 0;
            bool DClassAlive = CountRoles(Team.CDP) > 0;
            bool ScientistsAlive = CountRoles(Team.RSC) > 0;
            bool SHAlive = shPlayers.Count > 0;

            if (SHAlive && ((CiAlive && !SerpentsHand.instance.Config.ScpsWinWithChaos) || DClassAlive || MTFAlive || ScientistsAlive))
            {
                Log.Info("Block1");
                ev.IsAllowed = false;
            }
            else if (SHAlive && ScpAlive && !MTFAlive && !DClassAlive && !ScientistsAlive)
            {
                if (!SerpentsHand.instance.Config.ScpsWinWithChaos)
                {
                    if (!CiAlive)
                    {
                        ev.LeadingTeam = Exiled.API.Enums.LeadingTeam.Anomalies;
                        ev.IsAllowed = true;
                        ev.IsRoundEnded = true;
                        Log.Info("Allow1");

                        RoundEnded = true;
                    }
                }
                else
                {
                    ev.LeadingTeam = Exiled.API.Enums.LeadingTeam.Anomalies;
                    ev.IsAllowed = true;
                    ev.IsRoundEnded = true;
                    Log.Info("Allow2");

                    RoundEnded = true;
                }
            }
            else if (SHAlive && !ScpAlive && !MTFAlive && !DClassAlive && !ScientistsAlive)
            {
                ev.LeadingTeam = Exiled.API.Enums.LeadingTeam.Anomalies;
                ev.IsAllowed = true;
                ev.IsRoundEnded = true;
                Log.Info("Allow3");

                RoundEnded = true;
            }
        }

        public static bool RoundEnded = false;

        public void OnRoundEnd (RoundEndedEventArgs ev)
        {
            RoundEnded = true;
        }

        public void OnSetRole(ChangingRoleEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player))
            {
                if (ev.NewRole.GetTeam() != Team.TUT)
                {
                    shPlayers.Remove(ev.Player);
                    ev.Player.CustomInfo = string.Empty;
                    ev.Player.ReferenceHub.nicknameSync.ShownPlayerInfo |= PlayerInfoArea.Role;
                }
                else
                {
                    ev.Player.CustomInfo = "<color=#00FF58>Serpents Hand</color>";
                    ev.Player.ReferenceHub.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;
                }
            }
        }

        public void OnShoot(ShotEventArgs ev)
        {
            List<Player> scp035s = TryGet035();

            if (ev.Target != null && ev.Shooter != null)
            {
                if (ev.Target.Team == Team.SCP && shPlayers.Contains(ev.Shooter) && !SerpentsHand.instance.Config.FriendlyFire && !(RoundEnded && SerpentsHand.instance.Config.EndRoundFriendlyFire)) ev.CanHurt = false;
                if (scp035s.Contains(ev.Target) && shPlayers.Contains(ev.Shooter) && !SerpentsHand.instance.Config.FriendlyFire && !(RoundEnded && SerpentsHand.instance.Config.EndRoundFriendlyFire)) ev.CanHurt = false;
                if (ev.Shooter.Team == Team.SCP && shPlayers.Contains(ev.Target) && !SerpentsHand.instance.Config.FriendlyFire && !(RoundEnded && SerpentsHand.instance.Config.EndRoundFriendlyFire)) ev.CanHurt = false;
            }
        }

        public void OnRoundRestart ()
        {
            RoundEnded = false;
        }

        public void OnDisconnect(LeftEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player))
            {
                shPlayers.Remove(ev.Player);
                ev.Player.CustomInfo = string.Empty;
                ev.Player.ReferenceHub.nicknameSync.ShownPlayerInfo |= PlayerInfoArea.Role;
            }
        }

        public void OnContain106(ContainingEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player) && !SerpentsHand.instance.Config.FriendlyFire)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
		{
            if (shPlayers.Contains(ev.Player) && !SerpentsHand.instance.Config.FriendlyFire)
			{
                ev.IsAllowed = false;
			}
		}

        public void OnFemurEnter(EnteringFemurBreakerEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player) && !SerpentsHand.instance.Config.FriendlyFire)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnSpawning(SpawningEventArgs ev)
		{
            if (PositionsToSpawn.ContainsKey(ev.Player))
            {
                ev.Position = PositionsToSpawn[ev.Player];
                PositionsToSpawn.Remove(ev.Player);
            }
        }
    }
}