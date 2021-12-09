using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MEC;
using Exiled.API.Extensions;

namespace SerpentsHand
{
    using Exiled.API.Enums;
	using Exiled.API.Features.Items;
    using Exiled.API.Interfaces;
    using Exiled.Loader;
	using InventorySystem.Items.Firearms.Attachments;
    using System;
    using System.Reflection;

	partial class EventHandlers
    {
        internal static void SpawnPlayer(Player player, bool full = true)
        {
            shPlayers.Add(player);
            PositionsToSpawn.Add(player, shSpawnPos);
            player.SetRole(RoleType.Tutorial);
            player.Broadcast(10, SerpentsHand.instance.Config.SpawnBroadcast);

            Log.Info("Serpants count: " + shPlayers.Count);

            if (full)
            {
                Timing.CallDelayed(1f, () =>
                {
                    for (int i = 0; i < SerpentsHand.instance.Config.SpawnItems.Count; i++)
                    {
                        Item item = player.AddItem(SerpentsHand.instance.Config.SpawnItems[i]);
                        if (item is Firearm firearm)
                        {
                            if (!AttachmentsServerHandler.PlayerPreferences.TryGetValue(player.ReferenceHub, out Dictionary<ItemType, uint> dictionary) || !dictionary.TryGetValue(item.Base.ItemTypeId, out uint num))
                            {
                                num = 0U;
                            }
                            num = firearm.Base.ValidateAttachmentsCode(num);
                            firearm.Base.ApplyAttachmentsCode(num, false);
                        }
                    }
                    player.Health = SerpentsHand.instance.Config.Health;
                    foreach (ItemType ammoType in SerpentsHand.instance.Config.SpawnAmmo.Keys)
                    {
                        player.Inventory.UserInventory.ReserveAmmo[ammoType] = SerpentsHand.instance.Config.SpawnAmmo[ammoType];
                        player.Inventory.SendAmmoNextFrame = true;
                    }
                });
                // Prevent Serpents Hand from taking up Chaos spawn tickets
                //Respawning.RespawnTickets.Singleton.GrantTickets(Respawning.SpawnableTeamType.ChaosInsurgency, 1);
            }

            Player scp966 = Player.List.FirstOrDefault(p => p.SessionVariables.ContainsKey("is966") && (bool)p.SessionVariables["is966"]);
            if (scp966 != null)
            {
                player.TargetGhostsHashSet.Remove(scp966.Id);
            }
        }

        internal static void CreateSquad(int size)
        {
            List<Player> spec = new List<Player>();
            List<Player> pList = Player.List.ToList();

            foreach (Player player in pList)
            {
                if (player.Team == Team.RIP)
                {
                    spec.Add(player);
                }
            }

            int spawnCount = 1;
            while (spec.Count > 0 && spawnCount <= size)
            {
                int index = rand.Next(0, spec.Count);
                if (spec[index] != null)
                {
                    SpawnPlayer(spec[index], true);
                    spec.RemoveAt(index);
                    spawnCount++;
                }
            }
        }

        internal static void SpawnSquad(List<Player> players)
        {
            foreach (Player player in players)
            {
                SpawnPlayer(player);
            }

            Cassie.Message(SerpentsHand.instance.Config.EntryAnnouncement, true, true);
        }

        internal static List<Player> GetSHPlayers()
        {
            return shPlayers;
        }

        private List<Player> TryGet035()
        {
            List<Player> scp035 = null;
            if (SerpentsHand.isScp035)
            {
                try
                {
                    scp035 = (List<Player>)Loader.Plugins.First(pl => pl.Name == "scp035").Assembly.GetType("scp035.API.Scp035Data").GetMethod("GetScp035s", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
                }
                catch (Exception e)
                {
                    Log.Debug("Failed getting 035s: " + e);
                    scp035 = new List<Player>();
                }
            } else
            {
                scp035 = new List<Player>();
            }
            return scp035;
        }

        private int CountRoles(Team team)
        {
            List<Player> scp035 = null;

            if (SerpentsHand.isScp035)
            {
                scp035 = TryGet035();
            } else
            {
                scp035 = new List<Player>();
            }

            int count = 0;
            foreach (Player pl in Player.List)
            {
                if (pl.Team == team)
                {
                    if (scp035 != null && scp035.Select(s=>s.Id).ToList().Contains(pl.Id)) continue;
                    count++;
                }
            }
            return count;
        }

        private void TeleportTo106(Player player)
        {
            Player scp106 = Player.List.Where(x => x.Role == RoleType.Scp106).FirstOrDefault();
            if (scp106 != null)
            {
                player.Position = scp106.Position;
            }
            else
            {
                player.Position = RoleType.Scp096.GetRandomSpawnProperties().Item1;
            }
        }
    }
}