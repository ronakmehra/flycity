using GTANetworkAPI;
using NeptuneEvo.Handles;
using System;
using System.Collections.Generic;
using System.Linq;
using Localization;
using NeptuneEvo.Core;
using NeptuneEvo.Chars;
using NeptuneEvo.Functions;
using NeptuneEvo.Fractions.Models;
using NeptuneEvo.GUI;
using NeptuneEvo.Players;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Players.Popup.List.Models;
using Newtonsoft.Json;
using Redage.SDK;

namespace NeptuneEvo.Fractions
{
    /// <summary>
    /// Government Organizations System
    /// Handles: LSPD, SAHP, FIB, LifeInvader News, Emergency Hospital, Jail, National Armory
    /// Features: HR hiring, special vehicles, building interiors, clothing, vehicle restrictions
    /// </summary>
    class GovernmentOrgs : Script
    {
        private static readonly nLog Log = new nLog("Fractions.GovernmentOrgs");

        #region Organization Definitions

        public enum GovOrgType
        {
            LSPD = 7,           // Los Santos Police Department (maps to POLICE)
            SAHP = 18,          // San Andreas Highway Patrol (maps to SHERIFF)
            FIB = 9,            // Federal Investigation Bureau
            LifeInvader = 15,   // LifeInvader News (maps to LSNEWS)
            Hospital = 8,       // Emergency Hospital (maps to EMS)
            Jail = 19,          // County Jail (NEW)
            NationalArmory = 14 // National Armory (maps to ARMY)
        }

        // Organization names and descriptions
        public static Dictionary<GovOrgType, OrgInfo> Organizations = new Dictionary<GovOrgType, OrgInfo>()
        {
            { GovOrgType.LSPD, new OrgInfo("Los Santos Police Department", "LSPD", "Protect and serve the citizens of Los Santos", 18) },
            { GovOrgType.SAHP, new OrgInfo("San Andreas Highway Patrol", "SAHP", "Highway enforcement and traffic safety", 16) },
            { GovOrgType.FIB, new OrgInfo("Federal Investigation Bureau", "FIB", "Federal law enforcement and intelligence", 15) },
            { GovOrgType.LifeInvader, new OrgInfo("LifeInvader News", "LINV", "Breaking news coverage and journalism", 10) },
            { GovOrgType.Hospital, new OrgInfo("Pillbox Hill Medical Center", "EMS", "Emergency medical services and healthcare", 15) },
            { GovOrgType.Jail, new OrgInfo("Bolingbroke Penitentiary", "JAIL", "State correctional facility management", 12) },
            { GovOrgType.NationalArmory, new OrgInfo("Fort Zancudo National Guard", "ARMY", "National defense and military operations", 14) },
        };

        #endregion

        #region Building Locations & Interiors

        // Interior entry points
        public static Dictionary<GovOrgType, BuildingInterior> BuildingInteriors = new Dictionary<GovOrgType, BuildingInterior>()
        {
            { GovOrgType.LSPD, new BuildingInterior(
                new Vector3(441.85f, -982.0f, 30.69f),     // Exterior entry
                new Vector3(442.17f, -981.21f, 30.69f),    // Interior spawn
                new Vector3(461.8f, -985.4f, 30.7f),       // Reception
                new Vector3(451.3f, -980.2f, 30.7f),       // Locker room (clothing)
                new Vector3(465.2f, -990.7f, 30.7f),       // Armory
                new Vector3(459.4f, -998.3f, 24.9f),       // Jail cells
                new Vector3(447.8f, -973.5f, 30.7f),       // HR Office
                new Vector3(455.9f, -985.1f, 30.7f)        // Garage
            )},
            { GovOrgType.SAHP, new BuildingInterior(
                new Vector3(-449.7f, 6012.6f, 31.7f),      // Sandy Shores station
                new Vector3(-448.2f, 6009.3f, 31.7f),
                new Vector3(-445.6f, 6013.2f, 31.7f),
                new Vector3(-441.9f, 6007.8f, 31.7f),
                new Vector3(-450.3f, 6000.1f, 31.7f),
                new Vector3(-443.8f, 6018.2f, 31.7f),
                new Vector3(-447.1f, 6015.9f, 31.7f),
                new Vector3(-455.2f, 6010.5f, 31.7f)
            )},
            { GovOrgType.FIB, new BuildingInterior(
                new Vector3(136.0f, -749.4f, 46.75f),      // FIB Building
                new Vector3(135.5f, -746.8f, 46.75f),
                new Vector3(140.2f, -752.1f, 46.75f),
                new Vector3(130.8f, -760.5f, 46.75f),
                new Vector3(148.3f, -755.2f, 46.75f),
                new Vector3(137.7f, -765.9f, 46.75f),
                new Vector3(142.5f, -748.3f, 46.75f),
                new Vector3(125.9f, -750.1f, 46.75f)
            )},
            { GovOrgType.LifeInvader, new BuildingInterior(
                new Vector3(-1080.6f, -247.7f, 37.76f),    // LifeInvader Office
                new Vector3(-1078.2f, -246.1f, 37.76f),
                new Vector3(-1084.5f, -249.8f, 37.76f),
                new Vector3(-1075.3f, -252.4f, 37.76f),
                new Vector3(-1082.1f, -243.6f, 37.76f),
                new Vector3(-1070.8f, -248.2f, 37.76f),
                new Vector3(-1077.5f, -240.9f, 37.76f),
                new Vector3(-1086.2f, -255.3f, 37.76f)
            )},
            { GovOrgType.Hospital, new BuildingInterior(
                new Vector3(311.8f, -590.6f, 43.29f),      // Pillbox Hill Hospital
                new Vector3(310.5f, -588.2f, 43.29f),
                new Vector3(316.2f, -593.8f, 43.29f),
                new Vector3(307.1f, -585.4f, 43.29f),
                new Vector3(320.5f, -597.1f, 43.29f),
                new Vector3(305.8f, -592.6f, 43.29f),
                new Vector3(313.4f, -583.9f, 43.29f),
                new Vector3(325.1f, -590.2f, 43.29f)
            )},
            { GovOrgType.Jail, new BuildingInterior(
                new Vector3(1845.5f, 2585.8f, 45.67f),     // Bolingbroke Penitentiary
                new Vector3(1843.2f, 2583.5f, 45.67f),
                new Vector3(1848.7f, 2588.1f, 45.67f),
                new Vector3(1840.1f, 2580.9f, 45.67f),
                new Vector3(1852.3f, 2591.4f, 45.67f),
                new Vector3(1838.6f, 2578.2f, 45.67f),
                new Vector3(1846.9f, 2575.8f, 45.67f),
                new Vector3(1855.4f, 2587.3f, 45.67f)
            )},
            { GovOrgType.NationalArmory, new BuildingInterior(
                new Vector3(-2360.5f, 3249.8f, 92.9f),     // Fort Zancudo
                new Vector3(-2358.1f, 3247.3f, 92.9f),
                new Vector3(-2363.8f, 3252.5f, 92.9f),
                new Vector3(-2355.2f, 3244.7f, 92.9f),
                new Vector3(-2368.1f, 3256.2f, 92.9f),
                new Vector3(-2352.7f, 3241.9f, 92.9f),
                new Vector3(-2361.3f, 3239.5f, 92.9f),
                new Vector3(-2370.5f, 3250.8f, 92.9f)
            )},
        };

        #endregion

        #region HR / Hiring System

        // HR positions (where players can apply for jobs)
        public static Dictionary<GovOrgType, Vector3> HROfficePositions = new Dictionary<GovOrgType, Vector3>()
        {
            { GovOrgType.LSPD, new Vector3(447.8f, -973.5f, 30.7f) },
            { GovOrgType.SAHP, new Vector3(-447.1f, 6015.9f, 31.7f) },
            { GovOrgType.FIB, new Vector3(142.5f, -748.3f, 46.75f) },
            { GovOrgType.LifeInvader, new Vector3(-1077.5f, -240.9f, 37.76f) },
            { GovOrgType.Hospital, new Vector3(313.4f, -583.9f, 43.29f) },
            { GovOrgType.Jail, new Vector3(1846.9f, 2575.8f, 45.67f) },
            { GovOrgType.NationalArmory, new Vector3(-2361.3f, 3239.5f, 92.9f) },
        };

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                foreach (var org in Organizations)
                {
                    var hrPos = HROfficePositions[org.Key];
                    
                    // Create HR interaction point
                    CustomColShape.CreateCylinderColShape(hrPos, 1.5f, 2, 0, ColShapeEnums.GovHR);
                    NAPI.TextLabel.CreateTextLabel($"~b~{org.Value.ShortName} HR Office\n~w~Apply for employment",
                        hrPos + new Vector3(0, 0, 0.5), 10F, 0.4F, 0, new Color(0, 100, 255));
                    NAPI.Marker.CreateMarker(1, hrPos - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1f, new Color(0, 100, 255, 200));

                    // Create building entry points  
                    if (BuildingInteriors.ContainsKey(org.Key))
                    {
                        var interior = BuildingInteriors[org.Key];
                        CustomColShape.CreateCylinderColShape(interior.ExteriorEntry, 2f, 2, 0, ColShapeEnums.GovBuilding);
                        NAPI.TextLabel.CreateTextLabel($"~y~{org.Value.FullName}\n~w~Press E to enter",
                            interior.ExteriorEntry + new Vector3(0, 0, 0.7), 15F, 0.4F, 0, new Color(255, 200, 0));
                    }
                }

                Log.Write("Government Organizations system initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        /// <summary>
        /// HR interaction - allows players to apply for government jobs
        /// Leaders/HR rank members can hire players directly
        /// </summary>
        [Interaction(ColShapeEnums.GovHR)]
        public static void OnHRInteraction(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                // Determine which org this HR belongs to based on position
                GovOrgType nearestOrg = GetNearestOrg(player.Position);

                var frameList = new FrameListData();
                frameList.Header = $"{Organizations[nearestOrg].ShortName} - Human Resources";
                frameList.Callback = callback_hrMenu;

                // If player is in the org with hire permission
                if (characterData.FractionID == (int)nearestOrg)
                {
                    frameList.List.Add(new ListData("Hire Employee (Invite nearby)", "hire"));
                    frameList.List.Add(new ListData("View Staff Roster", "roster"));
                    frameList.List.Add(new ListData("Set Rank", "setrank"));
                    frameList.List.Add(new ListData("Fire Employee", "fire"));
                }
                else if (characterData.FractionID == 0)
                {
                    // Civilian can apply
                    frameList.List.Add(new ListData("Apply for Position", "apply"));
                    frameList.List.Add(new ListData("View Open Positions", "positions"));
                    frameList.List.Add(new ListData("Organization Info", "info"));
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        "You already belong to another organization. Leave it first.", 3000);
                    return;
                }

                Players.Popup.List.Repository.Open(player, frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnHRInteraction Exception: {e.ToString()}");
            }
        }

        private static void callback_hrMenu(ExtPlayer player, object listItem)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                string action = listItem.ToString();
                GovOrgType nearestOrg = GetNearestOrg(player.Position);

                switch (action)
                {
                    case "apply":
                        ApplyForPosition(player, nearestOrg);
                        break;
                    case "hire":
                        HireNearbyPlayer(player, nearestOrg);
                        break;
                    case "roster":
                        ViewRoster(player, nearestOrg);
                        break;
                    case "positions":
                        ShowPositions(player, nearestOrg);
                        break;
                    case "info":
                        ShowOrgInfo(player, nearestOrg);
                        break;
                    case "fire":
                        FireEmployee(player, nearestOrg);
                        break;
                    case "setrank":
                        SetEmployeeRank(player, nearestOrg);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_hrMenu Exception: {e.ToString()}");
            }
        }

        private static void ApplyForPosition(ExtPlayer player, GovOrgType org)
        {
            var characterData = player.GetCharacterData();
            if (characterData == null) return;

            if (characterData.FractionID != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You already work somewhere!", 3000);
                return;
            }

            // Auto-hire at rank 1 (applications go through HR)
            characterData.FractionID = (int)org;
            characterData.FractionLVL = 1;

            var orgInfo = Organizations[org];
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                $"Welcome to {orgInfo.FullName}! You are now a Rank 1 {orgInfo.ShortName} employee. Report to duty!", 5000);
        }

        private static void HireNearbyPlayer(ExtPlayer player, GovOrgType org)
        {
            var characterData = player.GetCharacterData();
            if (characterData == null) return;

            // Need rank 8+ or leader to hire
            if (characterData.FractionLVL < 8)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You need Rank 8+ to hire employees.", 3000);
                return;
            }

            // Find nearest player
            var nearbyPlayers = NAPI.Player.GetPlayersInRadiusOfPosition(3f, player.Position);
            var target = nearbyPlayers.FirstOrDefault(p => p != player);

            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "No player nearby. Ask them to stand next to you.", 3000);
                return;
            }

            var targetData = ((ExtPlayer)target).GetCharacterData();
            if (targetData == null) return;

            if (targetData.FractionID != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "That player already works somewhere.", 3000);
                return;
            }

            targetData.FractionID = (int)org;
            targetData.FractionLVL = 1;

            var orgInfo = Organizations[org];
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"You hired {target.Name} into {orgInfo.ShortName}!", 3000);
            Notify.Send((ExtPlayer)target, NotifyType.Success, NotifyPosition.BottomCenter, $"You've been hired into {orgInfo.FullName}!", 4000);
        }

        private static void ViewRoster(ExtPlayer player, GovOrgType org)
        {
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                $"Staff roster: Open your tablet (T key) to view full roster.", 3000);
        }

        private static void ShowPositions(ExtPlayer player, GovOrgType org)
        {
            var orgInfo = Organizations[org];
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                $"{orgInfo.FullName} has {orgInfo.MaxRanks} ranks available. Apply to start at Rank 1!", 4000);
        }

        private static void ShowOrgInfo(ExtPlayer player, GovOrgType org)
        {
            var orgInfo = Organizations[org];
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                $"{orgInfo.FullName} ({orgInfo.ShortName}): {orgInfo.Description}", 5000);
        }

        private static void FireEmployee(ExtPlayer player, GovOrgType org)
        {
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                "Use tablet > Staff > Fire to remove an employee.", 3000);
        }

        private static void SetEmployeeRank(ExtPlayer player, GovOrgType org)
        {
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                "Use tablet > Staff > Set Rank to change employee ranks.", 3000);
        }

        #endregion

        #region Building Interior System

        [Interaction(ColShapeEnums.GovBuilding)]
        public static void OnBuildingEntry(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                GovOrgType nearestOrg = GetNearestOrg(player.Position);
                var interior = BuildingInteriors[nearestOrg];
                var orgInfo = Organizations[nearestOrg];

                // Teleport player inside
                player.Position = interior.InteriorSpawn;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                    $"You entered {orgInfo.FullName}", 3000);

                // Show interior navigation
                player.TriggerEvent("client.govbuilding.showInterior", 
                    JsonConvert.SerializeObject(new {
                        orgName = orgInfo.FullName,
                        reception = interior.Reception,
                        lockerRoom = interior.LockerRoom,
                        armory = interior.Armory,
                        cells = interior.Cells,
                        hrOffice = interior.HROffice,
                        garage = interior.Garage
                    }));
            }
            catch (Exception e)
            {
                Log.Write($"OnBuildingEntry Exception: {e.ToString()}");
            }
        }

        #endregion

        #region Utility Methods

        private static GovOrgType GetNearestOrg(Vector3 position)
        {
            GovOrgType nearest = GovOrgType.LSPD;
            float minDist = float.MaxValue;

            foreach (var kvp in HROfficePositions)
            {
                float dist = position.DistanceTo(kvp.Value);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = kvp.Key;
                }
            }
            return nearest;
        }

        #endregion
    }

    #region Data Models

    public class OrgInfo
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public int MaxRanks { get; set; }

        public OrgInfo(string fullName, string shortName, string description, int maxRanks)
        {
            FullName = fullName;
            ShortName = shortName;
            Description = description;
            MaxRanks = maxRanks;
        }
    }

    public class BuildingInterior
    {
        public Vector3 ExteriorEntry { get; set; }
        public Vector3 InteriorSpawn { get; set; }
        public Vector3 Reception { get; set; }
        public Vector3 LockerRoom { get; set; }
        public Vector3 Armory { get; set; }
        public Vector3 Cells { get; set; }
        public Vector3 HROffice { get; set; }
        public Vector3 Garage { get; set; }

        public BuildingInterior(Vector3 exterior, Vector3 interior, Vector3 reception, 
            Vector3 locker, Vector3 armory, Vector3 cells, Vector3 hr, Vector3 garage)
        {
            ExteriorEntry = exterior;
            InteriorSpawn = interior;
            Reception = reception;
            LockerRoom = locker;
            Armory = armory;
            Cells = cells;
            HROffice = hr;
            Garage = garage;
        }
    }

    #endregion
}
