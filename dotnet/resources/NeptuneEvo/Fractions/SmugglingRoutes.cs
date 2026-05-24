using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using NeptuneEvo.Handles;
using NeptuneEvo.Core;
using Redage.SDK;
using NeptuneEvo.GUI;
using NeptuneEvo.Functions;
using NeptuneEvo.Chars;
using NeptuneEvo.Chars.Models;
using NeptuneEvo.Accounts;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Players;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Fractions.Models;
using NeptuneEvo.Fractions.Player;
using NeptuneEvo.Players.Popup.List.Models;
using NeptuneEvo.Table.Models;

namespace NeptuneEvo.Fractions
{
    /// <summary>
    /// Master Smuggling Routes system - manages cross-territory smuggling between
    /// Cayo Perico and Los Santos for all criminal factions.
    /// Police, Water Police, Coast Guard, Sheriff, FBI can intercept smugglers.
    /// If caught, smugglers get arrested and sent to jail.
    /// </summary>
    class SmugglingRoutes : Script
    {
        private static readonly nLog Log = new nLog("Fractions.SmugglingRoutes");

        // Active smuggling missions tracked globally
        public static Dictionary<ExtPlayer, SmugglingMission> ActiveMissions = new Dictionary<ExtPlayer, SmugglingMission>();

        // Police checkpoint positions (road checkpoints where cops can bust smugglers)
        public static List<Vector3> PoliceCheckpoints = new List<Vector3>()
        {
            new Vector3(1705.5f, 3598.2f, 35.5f),       // Sandy Shores highway
            new Vector3(2570.8f, 2645.3f, 37.8f),       // Route 68
            new Vector3(1231.5f, 1849.2f, 79.5f),       // Great Ocean Highway north
            new Vector3(785.3f, -91.8f, 72.5f),          // Vinewood Hills road
            new Vector3(122.5f, -760.3f, 31.4f),         // Downtown LS
            new Vector3(-385.2f, -1155.8f, 30.2f),       // South LS checkpoint
            new Vector3(-1577.8f, -842.5f, 10.2f),       // Del Perro Freeway
        };

        // Coast Guard patrol zones (water areas)
        public static List<Vector3> CoastGuardPatrolZones = new List<Vector3>()
        {
            new Vector3(-1664.5f, -1116.8f, 0.5f),      // Del Perro Beach
            new Vector3(1346.2f, -3298.5f, 0.5f),        // Terminal coast
            new Vector3(3895.8f, 4508.2f, 0.5f),         // Paleto Bay water
            new Vector3(-2972.5f, 353.2f, 0.5f),          // Chumash Beach
            new Vector3(1467.8f, 6567.5f, 0.5f),          // North coast
        };

        // Cargo types that can be smuggled
        public enum CargoType
        {
            Drugs,
            Weapons,
            StolenGoods,
            Contraband,
            CounterfeitMoney,
            IllegalElectronics
        }

        // Cargo values
        public static Dictionary<CargoType, int> CargoValues = new Dictionary<CargoType, int>()
        {
            {CargoType.Drugs, 30000},
            {CargoType.Weapons, 50000},
            {CargoType.StolenGoods, 15000},
            {CargoType.Contraband, 20000},
            {CargoType.CounterfeitMoney, 25000},
            {CargoType.IllegalElectronics, 35000},
        };

        // Jail times per cargo type (minutes)
        public static Dictionary<CargoType, int> JailTimes = new Dictionary<CargoType, int>()
        {
            {CargoType.Drugs, 30},
            {CargoType.Weapons, 45},
            {CargoType.StolenGoods, 15},
            {CargoType.Contraband, 20},
            {CargoType.CounterfeitMoney, 25},
            {CargoType.IllegalElectronics, 20},
        };

        // Wanted levels per cargo type
        public static Dictionary<CargoType, int> WantedLevels = new Dictionary<CargoType, int>()
        {
            {CargoType.Drugs, 3},
            {CargoType.Weapons, 5},
            {CargoType.StolenGoods, 2},
            {CargoType.Contraband, 3},
            {CargoType.CounterfeitMoney, 4},
            {CargoType.IllegalElectronics, 2},
        };

        // Cayo Perico departure points
        public static List<Vector3> CayoPericoDocks = new List<Vector3>()
        {
            new Vector3(4840.2f, -5175.5f, 2.0f),
            new Vector3(4958.3f, -5122.8f, 2.0f),
            new Vector3(4720.5f, -5251.2f, 2.0f),
        };

        // Los Santos arrival points
        public static List<Vector3> LoseSantosArrivalDocks = new List<Vector3>()
        {
            new Vector3(1299.8f, -3264.5f, 5.5f),       // Terminal
            new Vector3(-1624.5f, -1052.8f, 1.2f),      // Del Perro
            new Vector3(1467.8f, 6567.5f, 1.5f),        // North coast
            new Vector3(3848.5f, 4463.2f, 2.5f),        // Paleto Bay
        };

        // Smuggling start NPC location
        public static Vector3 SmugglingNPC = new Vector3(1280.5f, -3272.8f, 5.5f);

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                // Smuggling mission NPC
                CustomColShape.CreateCylinderColShape(SmugglingNPC, 2, 2, 0, ColShapeEnums.SmugglingMissionStart);
                NAPI.TextLabel.CreateTextLabel("~o~Mysterious Contact\n~w~'I have a job for you...'",
                    SmugglingNPC + new Vector3(0, 0, 0.5), 10F, 0.4F, 0, new Color(255, 140, 0));
                NAPI.Marker.CreateMarker(1, SmugglingNPC - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(255, 140, 0, 180));

                // Police checkpoints (visible to police, invisible to criminals)
                foreach (var cp in PoliceCheckpoints)
                {
                    CustomColShape.CreateCylinderColShape(cp, 15, 5, 0, ColShapeEnums.PoliceSmuggleCheckpoint);
                }

                // Coast Guard patrol zones
                foreach (var zone in CoastGuardPatrolZones)
                {
                    CustomColShape.CreateCylinderColShape(zone, 100, 10, 0, ColShapeEnums.CoastGuardPatrol);
                }

                // Cayo Perico departure points
                foreach (var dock in CayoPericoDocks)
                {
                    CustomColShape.CreateCylinderColShape(dock, 5, 3, 0, ColShapeEnums.CayoPericoDeparture);
                    NAPI.Marker.CreateMarker(1, dock - new Vector3(0, 0, 0.5), new Vector3(), new Vector3(), 3.0f, new Color(0, 200, 200, 100));
                }

                // Los Santos arrival
                foreach (var dock in LoseSantosArrivalDocks)
                {
                    CustomColShape.CreateCylinderColShape(dock, 5, 3, 0, ColShapeEnums.LSArrivalDock);
                }

                Log.Write("SmugglingRoutes system initialized with police intercept capability.");
            }
            catch (Exception e)
            {
                Log.Write($"Event_ResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.SmugglingMissionStart)]
        public static void OnSmugglingMissionStart(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                var fracType = Manager.FractionTypes.ContainsKey(fracId) ? Manager.FractionTypes[fracId] : FractionsType.None;

                // Only criminal factions can smuggle
                if (fracType != FractionsType.Gangs && fracType != FractionsType.Mafia && 
                    fracType != FractionsType.Bikers && fracType != FractionsType.None)
                {
                    // If they're law enforcement, give them a hint
                    if (fracType == FractionsType.Gov)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "This person looks suspicious... Keep an eye on this area for smuggling activity.", 5000);
                        return;
                    }
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't look like someone I'd do business with.", 3000);
                    return;
                }

                // Check if player already has active mission
                if (ActiveMissions.ContainsKey(player))
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, 
                        "You already have an active smuggling mission! Complete it first.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "Smuggling Operations";
                frameList.Callback = callback_smugglingMission;

                frameList.List.Add(new ListData($"Smuggle Drugs (Payout: ${CargoValues[CargoType.Drugs]})", "drugs"));
                frameList.List.Add(new ListData($"Smuggle Weapons (Payout: ${CargoValues[CargoType.Weapons]})", "weapons"));
                frameList.List.Add(new ListData($"Smuggle Stolen Goods (Payout: ${CargoValues[CargoType.StolenGoods]})", "stolengoods"));
                frameList.List.Add(new ListData($"Smuggle Contraband (Payout: ${CargoValues[CargoType.Contraband]})", "contraband"));
                frameList.List.Add(new ListData($"Smuggle Counterfeit Money (Payout: ${CargoValues[CargoType.CounterfeitMoney]})", "counterfeit"));
                frameList.List.Add(new ListData($"Smuggle Electronics (Payout: ${CargoValues[CargoType.IllegalElectronics]})", "electronics"));
                frameList.List.Add(new ListData("Cancel Current Mission", "cancel"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnSmugglingMissionStart Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_smugglingMission")]
        public static void callback_smugglingMission(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                if (value == "cancel")
                {
                    if (ActiveMissions.ContainsKey(player))
                    {
                        ActiveMissions.Remove(player);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Mission cancelled. You lost the cargo.", 3000);
                    }
                    return;
                }

                CargoType cargo;
                switch (value)
                {
                    case "drugs": cargo = CargoType.Drugs; break;
                    case "weapons": cargo = CargoType.Weapons; break;
                    case "stolengoods": cargo = CargoType.StolenGoods; break;
                    case "contraband": cargo = CargoType.Contraband; break;
                    case "counterfeit": cargo = CargoType.CounterfeitMoney; break;
                    case "electronics": cargo = CargoType.IllegalElectronics; break;
                    default: return;
                }

                // Create mission
                Random rnd = new Random();
                int pickupIdx = rnd.Next(0, CayoPericoDocks.Count);
                int dropoffIdx = rnd.Next(0, LoseSantosArrivalDocks.Count);

                var mission = new SmugglingMission
                {
                    Cargo = cargo,
                    PickupPoint = CayoPericoDocks[pickupIdx],
                    DropoffPoint = LoseSantosArrivalDocks[dropoffIdx],
                    Payout = CargoValues[cargo],
                    StartTime = DateTime.Now,
                    PickedUp = false,
                    Delivered = false
                };

                ActiveMissions[player] = mission;

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"SMUGGLING MISSION: Transport {cargo} from Cayo Perico to Los Santos.\n" +
                    $"Payout: ${mission.Payout}\n" +
                    $"WARNING: If caught by police/coast guard, you will be ARRESTED and JAILED for {JailTimes[cargo]} minutes!\n" +
                    $"Head to the pickup point marked on your GPS.", 10000);

                Trigger.ClientEvent(player, "createWaypoint", mission.PickupPoint.X, mission.PickupPoint.Y);
            }
            catch (Exception e)
            {
                Log.Write($"callback_smugglingMission Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.CayoPericoDeparture)]
        public static void OnCayoPericoDeparture(ExtPlayer player)
        {
            try
            {
                if (!ActiveMissions.ContainsKey(player)) return;
                var mission = ActiveMissions[player];

                if (mission.PickedUp)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "You already picked up the cargo!", 3000);
                    return;
                }

                if (!player.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You need a vehicle or boat to transport the cargo!", 3000);
                    return;
                }

                mission.PickedUp = true;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Cargo loaded! {mission.Cargo} crate secured in your vehicle.\n" +
                    $"Now deliver to the drop-off point in Los Santos. Watch out for the Coast Guard!", 8000);

                Trigger.ClientEvent(player, "createWaypoint", mission.DropoffPoint.X, mission.DropoffPoint.Y);

                // 40% chance coast guard is alerted immediately
                Random rnd = new Random();
                if (rnd.Next(100) < 40)
                {
                    BroadcastToLawEnforcement($"[COAST GUARD ALERT] Suspicious cargo loading detected at Cayo Perico docks! " +
                        $"Possible {mission.Cargo} smuggling in progress!");
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnCayoPericoDeparture Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.LSArrivalDock)]
        public static void OnLSArrivalDock(ExtPlayer player)
        {
            try
            {
                if (!ActiveMissions.ContainsKey(player)) return;
                var mission = ActiveMissions[player];

                if (!mission.PickedUp)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You haven't picked up the cargo yet!", 3000);
                    return;
                }

                // Successfully delivered
                mission.Delivered = true;
                MoneySystem.Wallet.Change(player, mission.Payout);

                // Bonus for fast delivery
                TimeSpan elapsed = DateTime.Now - mission.StartTime;
                int bonus = 0;
                if (elapsed.TotalMinutes < 10)
                {
                    bonus = mission.Payout / 2; // 50% speed bonus
                    MoneySystem.Wallet.Change(player, bonus);
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"SMUGGLING COMPLETE! Cargo delivered successfully!\n" +
                    $"Earned: ${mission.Payout}" + (bonus > 0 ? $" + ${bonus} speed bonus!" : ""), 8000);

                ActiveMissions.Remove(player);
            }
            catch (Exception e)
            {
                Log.Write($"OnLSArrivalDock Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.PoliceSmuggleCheckpoint)]
        public static void OnPoliceCheckpoint(ExtPlayer player, int index)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                // Only affects smugglers passing through
                if (!ActiveMissions.ContainsKey(player)) return;
                if (!ActiveMissions[player].PickedUp) return;

                var mission = ActiveMissions[player];

                // 50% chance of being detected at checkpoint
                Random rnd = new Random();
                if (rnd.Next(100) < 50)
                {
                    BroadcastToLawEnforcement($"[CHECKPOINT ALERT] Vehicle carrying suspected {mission.Cargo} " +
                        $"detected at police checkpoint! Location: {player.Position.X:F0}, {player.Position.Y:F0}");
                    
                    characterData.WantedLVL = (characterData.WantedLVL ?? 0) + WantedLevels[mission.Cargo];
                    
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"You've been spotted at a police checkpoint! Wanted level increased! FLEE!", 5000);
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnPoliceCheckpoint Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.CoastGuardPatrol)]
        public static void OnCoastGuardPatrol(ExtPlayer player, int index)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                if (!ActiveMissions.ContainsKey(player)) return;
                if (!ActiveMissions[player].PickedUp) return;

                var mission = ActiveMissions[player];

                // 40% chance of being detected in patrol zone
                Random rnd = new Random();
                if (rnd.Next(100) < 40)
                {
                    BroadcastToLawEnforcement($"[COAST GUARD] Vessel carrying illegal {mission.Cargo} detected in patrol zone! " +
                        $"Position: {player.Position.X:F0}, {player.Position.Y:F0}. Water Police and Coast Guard respond!");

                    characterData.WantedLVL = (characterData.WantedLVL ?? 0) + WantedLevels[mission.Cargo];

                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"Coast Guard has spotted you! They're calling for backup! Speed up!", 5000);
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnCoastGuardPatrol Exception: {e.ToString()}");
            }
        }

        /// <summary>
        /// Called by police/sheriff/FBI when they arrest a smuggler.
        /// Confiscates cargo, sends to jail, and pays the arresting officer.
        /// </summary>
        public static void ArrestSmuggler(ExtPlayer officer, ExtPlayer smuggler)
        {
            try
            {
                if (!ActiveMissions.ContainsKey(smuggler))
                {
                    Notify.Send(officer, NotifyType.Info, NotifyPosition.BottomCenter, "This person has no active smuggling mission.", 3000);
                    return;
                }

                var mission = ActiveMissions[smuggler];
                var smugglerData = smuggler.GetCharacterData();
                if (smugglerData == null) return;

                int jailTime = JailTimes[mission.Cargo];
                int officerReward = mission.Payout / 4; // Officer gets 25% of cargo value

                // Jail the smuggler
                smugglerData.WantedLVL = 0;
                Notify.Send(smuggler, NotifyType.Error, NotifyPosition.BottomCenter, 
                    $"ARRESTED! You were caught smuggling {mission.Cargo}!\n" +
                    $"Jail time: {jailTime} minutes. Cargo confiscated.", 10000);

                // Teleport to jail
                NAPI.Entity.SetEntityPosition(smuggler, Fractions.Police.PrisonPosition + new Vector3(0, 0, 1.12));

                // Reward the officer
                MoneySystem.Wallet.Change(officer, officerReward);
                Notify.Send(officer, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Smuggler arrested! {mission.Cargo} cargo confiscated.\n" +
                    $"Arrest bonus: ${officerReward}", 5000);

                // Remove mission
                ActiveMissions.Remove(smuggler);

                // Broadcast to all law enforcement
                BroadcastToLawEnforcement($"[ARREST] {smuggler.Name} arrested for smuggling {mission.Cargo}. " +
                    $"Sentenced to {jailTime} minutes. Arresting officer rewarded ${officerReward}.");
            }
            catch (Exception e)
            {
                Log.Write($"ArrestSmuggler Exception: {e.ToString()}");
            }
        }

        /// <summary>
        /// Checks if a player is currently smuggling (for police to identify)
        /// </summary>
        public static bool IsSmuggling(ExtPlayer player)
        {
            return ActiveMissions.ContainsKey(player) && ActiveMissions[player].PickedUp;
        }

        /// <summary>
        /// Gets the cargo type a smuggler is carrying
        /// </summary>
        public static CargoType? GetCargoType(ExtPlayer player)
        {
            if (ActiveMissions.ContainsKey(player))
                return ActiveMissions[player].Cargo;
            return null;
        }

        private static void BroadcastToLawEnforcement(string message)
        {
            try
            {
                var players = NAPI.Pools.GetAllPlayers();
                foreach (var p in players)
                {
                    var extP = (ExtPlayer)p;
                    if (extP == null) continue;
                    var fracId = extP.GetFractionId();
                    if (fracId == (int)Models.Fractions.POLICE ||
                        fracId == (int)Models.Fractions.SHERIFF ||
                        fracId == (int)Models.Fractions.FIB ||
                        fracId == (int)Models.Fractions.WATERPOLICE ||
                        fracId == (int)Models.Fractions.COASTGUARD ||
                        fracId == (int)Models.Fractions.ARMY)
                    {
                        Notify.Send(extP, NotifyType.Warning, NotifyPosition.BottomCenter, message, 10000);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"BroadcastToLawEnforcement Exception: {e.ToString()}");
            }
        }
    }

    public class SmugglingMission
    {
        public SmugglingRoutes.CargoType Cargo { get; set; }
        public Vector3 PickupPoint { get; set; }
        public Vector3 DropoffPoint { get; set; }
        public int Payout { get; set; }
        public DateTime StartTime { get; set; }
        public bool PickedUp { get; set; }
        public bool Delivered { get; set; }
    }
}
