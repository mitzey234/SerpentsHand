using Exiled.API.Features;
using HarmonyLib;
using UnityEngine;

namespace SerpentsHand.Patches
{
	[HarmonyPatch(typeof(PlayableScps.Scp939), nameof(PlayableScps.Scp939.ServerAttack))]
	class Scp939DamagePatch
	{
		public static bool Prefix(GameObject target) => !EventHandlers.shPlayers.Contains(Player.Get(target)) || (EventHandlers.shPlayers.Contains(Player.Get(target)) && SerpentsHand.instance.Config.FriendlyFire) || (EventHandlers.RoundEnded && SerpentsHand.instance.Config.EndRoundFriendlyFire);
	}

	[HarmonyPatch(typeof(PlayableScps.Scp173), nameof(PlayableScps.Scp173.ServerKillPlayer))]
	class Scp173DamagePatch
	{
		public static bool Prefix(ReferenceHub target) => !EventHandlers.shPlayers.Contains(Player.Get(target)) || (EventHandlers.shPlayers.Contains(Player.Get(target)) && SerpentsHand.instance.Config.FriendlyFire) || (EventHandlers.RoundEnded && SerpentsHand.instance.Config.EndRoundFriendlyFire);
	}

	[HarmonyPatch(typeof(PlayableScps.Scp173), nameof(PlayableScps.Scp173.UpdateObservers))]
	class Scp173LookingPatch
	{
		public static void Postfix(PlayableScps.Scp173 __instance)
		{
			if (__instance._isObserved) __instance._observingPlayers.RemoveWhere(target => EventHandlers.shPlayers.Contains(Player.Get(target)) && SerpentsHand.instance.Config.FriendlyFire);
			__instance._isObserved = (__instance._observingPlayers.Count > 0 || __instance.StareAtDuration > 0f);
		}
	}

	[HarmonyPatch(typeof(PlayerStatsSystem.AttackerDamageHandler), nameof(PlayerStatsSystem.AttackerDamageHandler.ProcessDamage))]
	class FriendlyFirePatch
	{
		public static bool Prefix(ReferenceHub ply, PlayerStatsSystem.AttackerDamageHandler __instance)
		{
			RoleType curClass = ply.characterClassManager.CurClass;
			if (SerpentsHand.FFGrants.Contains(__instance.GetHashCode()))
			{
				__instance.ForceFullFriendlyFire = true;
				SerpentsHand.FFGrants.Remove(__instance.GetHashCode());
			}
			return true;
		}
	}
}
