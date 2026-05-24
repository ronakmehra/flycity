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
using Localization;

namespace NeptuneEvo.Fractions.LSNews
{
    /// <summary>
    /// Newspaper Job for LifeInvader / LS News faction members.
    /// Members can write articles, take photographs, and deliver newspapers
    /// across Los Santos for faction income and personal rewards.
    /// </summary>
    class NewspaperJob : Script
    {
        private static readonly nLog Log = new nLog("Fractions.LSNews.NewspaperJob");

        // LifeInvader Office (LS News HQ) - Newspaper desk
        public static Vector3 NewsroomDesk = new Vector3(-1076.9f, -248.5f, 44.0f);
        
        // Printing press
        public static Vector3 PrintingPress = new Vector3(-1082.5f, -252.3f, 44.0f);
        
        // Newspaper loading dock
        public static Vector3 LoadingDock = new Vector3(-1090.3f, -260.8f, 37.8f);

        // Newspaper stand locations around the city
        public static List<Vector3> NewspaperStands = new List<Vector3>()
        {
            // Downtown
            new Vector3(98.5f, -1068.3f, 29.4f),
            new Vector3(-240.8f, -920.5f, 29.7f),
            new Vector3(178.3f, -1020.8f, 29.3f),
            // Vinewood
            new Vector3(298.5f, 176.8f, 104.2f),
            new Vector3(-415.2f, 12.5f, 46.2f),
            // Del Perro
            new Vector3(-1380.5f, -500.2f, 33.2f),
            new Vector3(-1540.8f, -420.3f, 35.6f),
            // Vespucci
            new Vector3(-1180.2f, -1510.5f, 4.4f),
            // Mirror Park
            new Vector3(1020.5f, -420.8f, 65.1f),
            // Pacific Bluffs  
            new Vector3(-1850.3f, -630.5f, 11.2f),
            // Sandy Shores
            new Vector3(1960.8f, 3820.2f, 32.2f),
            // Paleto Bay
            new Vector3(-375.5f, 6040.8f, 31.5f),
        };

        // Photography hotspot locations
        public static List<PhotoSpot> PhotoSpots = new List<PhotoSpot>()
        {
            new PhotoSpot("Vinewood Sign", new Vector3(726.5f, 1198.3f, 326.0f), 500),
            new PhotoSpot("Del Perro Pier", new Vector3(-1650.2f, -1120.5f, 13.0f), 350),
            new PhotoSpot("Maze Bank Tower", new Vector3(-75.8f, -818.2f, 326.2f), 600),
            new PhotoSpot("LSIA Airport", new Vector3(-1037.5f, -2737.8f, 20.2f), 400),
            new PhotoSpot("Police Department", new Vector3(441.2f, -982.5f, 30.7f), 450),
            new PhotoSpot("Sandy Shores", new Vector3(1967.5f, 3745.8f, 32.3f), 300),
            new PhotoSpot("Mount Chiliad", new Vector3(501.8f, 5604.5f, 797.9f), 800),
            new PhotoSpot("Terminal Docks", new Vector3(1299.8f, -3264.5f, 5.5f), 400),
        };

        // Article types and pay
        public static Dictionary<string, int> ArticleTypes = new Dictionary<string, int>()
        {
            {"Breaking News", 1500},
            {"Crime Report", 1200},
            {"Weather Report", 500},
            {"Celebrity Gossip", 800},
            {"Sports Coverage", 900},
            {"Political Article", 1100},
            {"Business Report", 1000},
            {"Community Story", 600},
            {"Investigative Report", 2000},
            {"Editorial", 700},
        };

        // Pay settings
        public static int PayPerNewspaperDelivery = 150;
        public static int PayPerStandRestock = 300;
        public static int PhotoBonus = 250;
        public static int BreakingNewsBonus = 500;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                // Newsroom desk
                CustomColShape.CreateCylinderColShape(NewsroomDesk, 2, 2, 0, ColShapeEnums.LSNewsNewspaper);
                NAPI.TextLabel.CreateTextLabel("~g~LifeInvader Newsroom\n~w~Write articles & manage newspapers",
                    NewsroomDesk + new Vector3(0, 0, 0.5), 10F, 0.4F, 0, new Color(0, 200, 0));
                NAPI.Marker.CreateMarker(1, NewsroomDesk - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(0, 200, 0, 180));

                // Printing press
                CustomColShape.CreateCylinderColShape(PrintingPress, 2, 2, 0, ColShapeEnums.LSNewsPrintingPress);
                NAPI.TextLabel.CreateTextLabel("~y~Printing Press\n~w~Print today's newspaper edition",
                    PrintingPress + new Vector3(0, 0, 0.5), 5F, 0.3F, 0, new Color(200, 200, 0));

                // Loading dock
                CustomColShape.CreateCylinderColShape(LoadingDock, 3, 2, 0, ColShapeEnums.LSNewsLoadingDock);
                NAPI.TextLabel.CreateTextLabel("~y~Loading Dock\n~w~Load newspapers for delivery",
                    LoadingDock + new Vector3(0, 0, 0.5), 10F, 0.4F, 0, new Color(200, 200, 0));
                NAPI.Marker.CreateMarker(1, LoadingDock - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 2.0f, new Color(200, 200, 0, 180));

                // Newspaper stands
                foreach (var stand in NewspaperStands)
                {
                    CustomColShape.CreateCylinderColShape(stand, 2, 2, 0, ColShapeEnums.NewspaperStand);
                }

                Log.Write("LSNews NewspaperJob initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.LSNewsNewspaper)]
        public static void OnNewsroom(ExtPlayer player)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.LSNEWS)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "You don't work for LifeInvader/LS News.", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = "LifeInvader Newsroom";
                frameList.Callback = callback_newsroom;

                frameList.List.Add(new ListData("Write Breaking News ($1,500)", "write_breaking"));
                frameList.List.Add(new ListData("Write Crime Report ($1,200)", "write_crime"));
                frameList.List.Add(new ListData("Write Investigative Report ($2,000)", "write_investigate"));
                frameList.List.Add(new ListData("Write Celebrity Gossip ($800)", "write_gossip"));
                frameList.List.Add(new ListData("Write Sports Coverage ($900)", "write_sports"));
                frameList.List.Add(new ListData("Write Political Article ($1,100)", "write_politics"));
                frameList.List.Add(new ListData("Write Business Report ($1,000)", "write_business"));
                frameList.List.Add(new ListData("Write Weather Report ($500)", "write_weather"));
                frameList.List.Add(new ListData("Write Editorial ($700)", "write_editorial"));
                frameList.List.Add(new ListData("Write Community Story ($600)", "write_community"));
                frameList.List.Add(new ListData("Photography Assignment", "photo"));
                frameList.List.Add(new ListData("Start Newspaper Delivery Route", "deliver"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnNewsroom Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_newsroom")]
        public static void callback_newsroom(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                if (value.StartsWith("write_"))
                {
                    string articleType = "";
                    int pay = 0;

                    switch (value)
                    {
                        case "write_breaking": articleType = "Breaking News"; pay = 1500; break;
                        case "write_crime": articleType = "Crime Report"; pay = 1200; break;
                        case "write_investigate": articleType = "Investigative Report"; pay = 2000; break;
                        case "write_gossip": articleType = "Celebrity Gossip"; pay = 800; break;
                        case "write_sports": articleType = "Sports Coverage"; pay = 900; break;
                        case "write_politics": articleType = "Political Article"; pay = 1100; break;
                        case "write_business": articleType = "Business Report"; pay = 1000; break;
                        case "write_weather": articleType = "Weather Report"; pay = 500; break;
                        case "write_editorial": articleType = "Editorial"; pay = 700; break;
                        case "write_community": articleType = "Community Story"; pay = 600; break;
                        default: return;
                    }

                    MoneySystem.Wallet.Change(player, pay);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                        $"Article written: \"{articleType}\"! You earned ${pay}. Article submitted to editor.", 5000);

                    // Notify all LS News members
                    var players = NAPI.Pools.GetAllPlayers();
                    foreach (var p in players)
                    {
                        var extP = (ExtPlayer)p;
                        if (extP == null) continue;
                        if (extP.GetFractionId() == (int)Models.Fractions.LSNEWS)
                        {
                            Notify.Send(extP, NotifyType.Info, NotifyPosition.BottomCenter, 
                                $"[LS NEWS] New article submitted: \"{articleType}\" by {player.Name}", 5000);
                        }
                    }
                }
                else if (value == "photo")
                {
                    // Assign random photography mission
                    Random rnd = new Random();
                    int idx = rnd.Next(0, PhotoSpots.Count);
                    var spot = PhotoSpots[idx];

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                        $"Photography Assignment: Go to {spot.Name} and take a photo.\n" +
                        $"Bonus: ${spot.Bonus}. GPS has been set.", 5000);
                    Trigger.ClientEvent(player, "createWaypoint", spot.Position.X, spot.Position.Y);
                }
                else if (value == "deliver")
                {
                    sessionData.WorkData.OnWork = true;
                    sessionData.WorkData.ProgressCount = 0;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                        "Newspaper delivery route started! Pick up papers at the loading dock, then deliver to stands across the city.", 5000);
                    Trigger.ClientEvent(player, "createWaypoint", LoadingDock.X, LoadingDock.Y);
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_newsroom Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.LSNewsPrintingPress)]
        public static void OnPrintingPress(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.LSNEWS)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "LS News members only.", 3000);
                    return;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    "Today's edition printed! 500 copies ready for distribution.\n" +
                    "Head to the loading dock to pick them up for delivery.", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"OnPrintingPress Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.LSNewsLoadingDock)]
        public static void OnLoadingDock(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.LSNEWS)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "LS News members only.", 3000);
                    return;
                }

                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                if (!sessionData.WorkData.OnWork)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Start a delivery route first in the newsroom!", 3000);
                    return;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    "Newspapers loaded into your vehicle! Deliver them to newspaper stands across the city.\n" +
                    "Each stand restocked: +$300", 5000);

                // Set first stand
                Random rnd = new Random();
                int idx = rnd.Next(0, NewspaperStands.Count);
                Trigger.ClientEvent(player, "createWaypoint", NewspaperStands[idx].X, NewspaperStands[idx].Y);
            }
            catch (Exception e)
            {
                Log.Write($"OnLoadingDock Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.NewspaperStand)]
        public static void OnNewspaperStand(ExtPlayer player)
        {
            try
            {
                var fracId = player.GetFractionId();
                if (fracId != (int)Models.Fractions.LSNEWS)
                {
                    // Regular citizens can buy a newspaper
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                        "Today's LS Daily Gazette available here. $5 per copy.", 3000);
                    return;
                }

                var sessionData = player.GetSessionData();
                if (sessionData == null) return;

                if (!sessionData.WorkData.OnWork)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Start a delivery route to restock stands.", 3000);
                    return;
                }

                // Restock the stand
                sessionData.WorkData.ProgressCount++;
                MoneySystem.Wallet.Change(player, PayPerStandRestock);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Stand restocked! +${PayPerStandRestock} | Stands restocked: {sessionData.WorkData.ProgressCount}/{NewspaperStands.Count}", 3000);

                if (sessionData.WorkData.ProgressCount >= NewspaperStands.Count)
                {
                    int totalBonus = PayPerStandRestock * NewspaperStands.Count;
                    int completionBonus = 2000;
                    MoneySystem.Wallet.Change(player, completionBonus);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                        $"All stands restocked! Completion bonus: ${completionBonus}. Return to the newsroom.", 5000);
                    sessionData.WorkData.OnWork = false;
                    sessionData.WorkData.ProgressCount = 0;
                    Trigger.ClientEvent(player, "createWaypoint", NewsroomDesk.X, NewsroomDesk.Y);
                }
                else
                {
                    // Navigate to next stand
                    Random rnd = new Random();
                    int idx = rnd.Next(0, NewspaperStands.Count);
                    Trigger.ClientEvent(player, "createWaypoint", NewspaperStands[idx].X, NewspaperStands[idx].Y);
                }
            }
            catch (Exception e)
            {
                Log.Write($"OnNewspaperStand Exception: {e.ToString()}");
            }
        }
    }

    public class PhotoSpot
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public int Bonus { get; set; }

        public PhotoSpot(string name, Vector3 position, int bonus)
        {
            Name = name;
            Position = position;
            Bonus = bonus;
        }
    }
}
