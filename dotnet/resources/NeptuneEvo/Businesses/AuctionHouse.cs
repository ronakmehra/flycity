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
using NeptuneEvo.Players.Popup.List.Models;
using Localization;

namespace NeptuneEvo.Businesses
{
    /// <summary>
    /// Auction House system - Players can bid on vehicles, properties, weapons, 
    /// rare items, and businesses. Auctions run on timers with competitive bidding.
    /// </summary>
    class AuctionHouse : Script
    {
        private static readonly nLog Log = new nLog("Businesses.AuctionHouse");

        // Auction House locations
        public static Vector3 MainAuctionHouse = new Vector3(-596.5f, -894.8f, 25.3f);  // Pillbox Hill
        public static Vector3 LuxuryAuctionHouse = new Vector3(-1387.2f, -478.5f, 33.2f); // Rockford Hills (luxury)
        public static Vector3 UndergroundAuction = new Vector3(481.5f, -1315.8f, 29.2f);  // La Mesa (black market)

        // Active auctions
        public static Dictionary<int, AuctionItem> ActiveAuctions = new Dictionary<int, AuctionItem>();
        private static int NextAuctionId = 1;

        // Auction categories
        public enum AuctionCategory
        {
            Vehicles,
            Properties,
            Weapons,
            RareItems,
            Businesses,
            Jewelry,
            Art,
            Contraband // Black market only
        }

        // Pre-listed auction items (NPC auctions)
        public static List<AuctionTemplate> AuctionTemplates = new List<AuctionTemplate>()
        {
            // Vehicles
            new AuctionTemplate("Sultan RS Classic", AuctionCategory.Vehicles, 150000, "Classic tuner car in pristine condition"),
            new AuctionTemplate("Armored Kuruma", AuctionCategory.Vehicles, 525000, "Bulletproof luxury sedan"),
            new AuctionTemplate("Oppressor Mk I", AuctionCategory.Vehicles, 2800000, "Rocket-powered motorcycle"),
            new AuctionTemplate("Toreador", AuctionCategory.Vehicles, 3660000, "Submersible sports car with boost"),
            new AuctionTemplate("Turismo R", AuctionCategory.Vehicles, 500000, "Italian supercar"),
            new AuctionTemplate("Insurgent Custom", AuctionCategory.Vehicles, 1350000, "Military-grade armored truck"),
            // Properties
            new AuctionTemplate("Vinewood Hills Mansion", AuctionCategory.Properties, 3500000, "4 bed, pool, city views"),
            new AuctionTemplate("Paleto Bay Beach House", AuctionCategory.Properties, 250000, "Quiet beachfront property"),
            new AuctionTemplate("Downtown LS Penthouse", AuctionCategory.Properties, 5000000, "Luxury penthouse with rooftop"),
            new AuctionTemplate("Sandy Shores Trailer", AuctionCategory.Properties, 65000, "Budget desert living"),
            // Rare Items
            new AuctionTemplate("Gold Plated Pistol", AuctionCategory.Weapons, 95000, "Unique gold-finished weapon"),
            new AuctionTemplate("Diamond Encrusted Watch", AuctionCategory.Jewelry, 120000, "Rare luxury timepiece"),
            new AuctionTemplate("Stolen Painting", AuctionCategory.Art, 200000, "Mysterious artwork of unknown origin"),
            new AuctionTemplate("Antique Samurai Sword", AuctionCategory.RareItems, 80000, "Historic Japanese blade"),
            new AuctionTemplate("Rare Baseball Card Collection", AuctionCategory.RareItems, 50000, "Mint condition set"),
            // Black market
            new AuctionTemplate("Military Sniper Rifle", AuctionCategory.Contraband, 75000, "Untraceable military hardware"),
            new AuctionTemplate("Drug Lab Equipment", AuctionCategory.Contraband, 150000, "Complete processing setup"),
            new AuctionTemplate("Counterfeit Money Printer", AuctionCategory.Contraband, 200000, "High-quality counterfeiting machine"),
        };

        // Minimum bid increment
        public static int MinBidIncrement = 1000;
        public static int AuctionDurationMinutes = 30;
        public static int AuctionHouseFeePercent = 5; // 5% commission

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                // Main Auction House
                CustomColShape.CreateCylinderColShape(MainAuctionHouse, 3, 2, 0, ColShapeEnums.AuctionHouse, 0);
                NAPI.TextLabel.CreateTextLabel("~g~Los Santos Auction House\n~w~Buy & Sell Rare Items",
                    MainAuctionHouse + new Vector3(0, 0, 0.5), 15F, 0.5F, 0, new Color(0, 200, 0));
                NAPI.Marker.CreateMarker(1, MainAuctionHouse - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 2.0f, new Color(255, 215, 0, 220));
                Main.CreateBlip(new Main.BlipData(480, "Auction House", MainAuctionHouse, 46, true, 1.0f));

                // Luxury Auction House
                CustomColShape.CreateCylinderColShape(LuxuryAuctionHouse, 3, 2, 0, ColShapeEnums.AuctionHouse, 1);
                NAPI.TextLabel.CreateTextLabel("~y~Premium Auction Gallery\n~w~Exclusive Luxury Auctions",
                    LuxuryAuctionHouse + new Vector3(0, 0, 0.5), 15F, 0.5F, 0, new Color(255, 215, 0));
                NAPI.Marker.CreateMarker(1, LuxuryAuctionHouse - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 2.0f, new Color(255, 215, 0, 220));
                Main.CreateBlip(new Main.BlipData(480, "Premium Auction", LuxuryAuctionHouse, 5, true, 0.8f));

                // Underground/Black Market Auction
                CustomColShape.CreateCylinderColShape(UndergroundAuction, 3, 2, 0, ColShapeEnums.AuctionHouseBlackMarket, 0);
                NAPI.Marker.CreateMarker(1, UndergroundAuction - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1.5f, new Color(128, 0, 0, 150));
                // No blip - players discover this through word of mouth

                // Initialize some NPC auctions
                InitializeNPCAuctions();

                Log.Write("AuctionHouse system initialized.");
            }
            catch (Exception e)
            {
                Log.Write($"onResourceStart Exception: {e.ToString()}");
            }
        }

        private static void InitializeNPCAuctions()
        {
            try
            {
                Random rnd = new Random();
                // Start with 5 random auctions
                for (int i = 0; i < 5; i++)
                {
                    int idx = rnd.Next(0, AuctionTemplates.Count);
                    var template = AuctionTemplates[idx];
                    
                    var auction = new AuctionItem
                    {
                        Id = NextAuctionId++,
                        Name = template.Name,
                        Category = template.Category,
                        Description = template.Description,
                        StartingPrice = template.StartingPrice,
                        CurrentBid = template.StartingPrice,
                        HighestBidder = null,
                        HighestBidderName = "No bids yet",
                        SellerName = "NPC Auctioneer",
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now.AddMinutes(AuctionDurationMinutes + rnd.Next(0, 60)),
                        IsActive = true,
                        IsBlackMarket = template.Category == AuctionCategory.Contraband,
                    };

                    ActiveAuctions[auction.Id] = auction;
                }
            }
            catch (Exception e)
            {
                Log.Write($"InitializeNPCAuctions Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.AuctionHouse)]
        public static void OnAuctionHouse(ExtPlayer player, int index)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                string houseName = index == 0 ? "Los Santos Auction House" : "Premium Auction Gallery";
                
                var frameList = new FrameListData();
                frameList.Header = houseName;
                frameList.Callback = callback_auctionMenu;

                frameList.List.Add(new ListData("Browse Active Auctions", "browse"));
                frameList.List.Add(new ListData("Browse Vehicles", "cat_vehicles"));
                frameList.List.Add(new ListData("Browse Properties", "cat_properties"));
                frameList.List.Add(new ListData("Browse Weapons", "cat_weapons"));
                frameList.List.Add(new ListData("Browse Rare Items", "cat_rare"));
                frameList.List.Add(new ListData("Browse Jewelry", "cat_jewelry"));
                frameList.List.Add(new ListData("Browse Art", "cat_art"));
                frameList.List.Add(new ListData("List Your Item for Auction", "sell"));
                frameList.List.Add(new ListData("My Bids", "mybids"));
                frameList.List.Add(new ListData("My Listings", "mylistings"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnAuctionHouse Exception: {e.ToString()}");
            }
        }

        [Interaction(ColShapeEnums.AuctionHouseBlackMarket)]
        public static void OnBlackMarketAuction(ExtPlayer player, int index)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var frameList = new FrameListData();
                frameList.Header = "Underground Auction";
                frameList.Callback = callback_blackMarketMenu;

                frameList.List.Add(new ListData("Browse Black Market Items", "browse_bm"));
                frameList.List.Add(new ListData("Browse Weapons (Illegal)", "bm_weapons"));
                frameList.List.Add(new ListData("Browse Contraband", "bm_contraband"));
                frameList.List.Add(new ListData("List Stolen Goods", "bm_sell"));
                frameList.List.Add(new ListData("My Black Market Bids", "bm_mybids"));

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"OnBlackMarketAuction Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_auctionMenu")]
        public static void callback_auctionMenu(ExtPlayer player, string value)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                switch (value)
                {
                    case "browse":
                        ShowAuctions(player, null, false);
                        break;
                    case "cat_vehicles":
                        ShowAuctions(player, AuctionCategory.Vehicles, false);
                        break;
                    case "cat_properties":
                        ShowAuctions(player, AuctionCategory.Properties, false);
                        break;
                    case "cat_weapons":
                        ShowAuctions(player, AuctionCategory.Weapons, false);
                        break;
                    case "cat_rare":
                        ShowAuctions(player, AuctionCategory.RareItems, false);
                        break;
                    case "cat_jewelry":
                        ShowAuctions(player, AuctionCategory.Jewelry, false);
                        break;
                    case "cat_art":
                        ShowAuctions(player, AuctionCategory.Art, false);
                        break;
                    case "sell":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "To list an item: Use /auction sell [name] [starting_price] [description]\n" +
                            $"Auction house takes {AuctionHouseFeePercent}% commission on successful sales.", 8000);
                        break;
                    case "mybids":
                        ShowMyBids(player);
                        break;
                    case "mylistings":
                        ShowMyListings(player);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_auctionMenu Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_blackMarketMenu")]
        public static void callback_blackMarketMenu(ExtPlayer player, string value)
        {
            try
            {
                switch (value)
                {
                    case "browse_bm":
                        ShowAuctions(player, null, true);
                        break;
                    case "bm_weapons":
                        ShowAuctions(player, AuctionCategory.Weapons, true);
                        break;
                    case "bm_contraband":
                        ShowAuctions(player, AuctionCategory.Contraband, true);
                        break;
                    case "bm_sell":
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                            "To list on black market: Use /blackauction sell [name] [price] [description]\n" +
                            "No paper trail. 8% commission.", 8000);
                        break;
                    case "bm_mybids":
                        ShowMyBids(player);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"callback_blackMarketMenu Exception: {e.ToString()}");
            }
        }

        private static void ShowAuctions(ExtPlayer player, AuctionCategory? category, bool blackMarketOnly)
        {
            try
            {
                var auctions = ActiveAuctions.Values
                    .Where(a => a.IsActive && a.EndTime > DateTime.Now)
                    .Where(a => category == null || a.Category == category)
                    .Where(a => blackMarketOnly ? a.IsBlackMarket : !a.IsBlackMarket)
                    .OrderBy(a => a.EndTime)
                    .Take(10)
                    .ToList();

                if (auctions.Count == 0)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                        "No active auctions in this category right now. Check back later!", 3000);
                    return;
                }

                var frameList = new FrameListData();
                frameList.Header = blackMarketOnly ? "Black Market Auctions" : "Active Auctions";
                frameList.Callback = callback_bidMenu;

                foreach (var auction in auctions)
                {
                    TimeSpan remaining = auction.EndTime - DateTime.Now;
                    string timeLeft = remaining.TotalMinutes > 60 
                        ? $"{remaining.Hours}h {remaining.Minutes}m" 
                        : $"{remaining.Minutes}m";
                    
                    frameList.List.Add(new ListData(
                        $"[{auction.Category}] {auction.Name} - Current: ${auction.CurrentBid:N0} | Ends: {timeLeft}",
                        $"bid_{auction.Id}"));
                }

                Trigger.ClientEvent(player, "client.framelist.show", frameList);
            }
            catch (Exception e)
            {
                Log.Write($"ShowAuctions Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("server.framelist.callback.callback_bidMenu")]
        public static void callback_bidMenu(ExtPlayer player, string value)
        {
            try
            {
                if (!value.StartsWith("bid_")) return;
                int auctionId = int.Parse(value.Replace("bid_", ""));

                if (!ActiveAuctions.ContainsKey(auctionId))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This auction no longer exists.", 3000);
                    return;
                }

                var auction = ActiveAuctions[auctionId];

                if (auction.EndTime <= DateTime.Now)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This auction has ended.", 3000);
                    return;
                }

                // Show auction details and prompt for bid
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                    $"--- {auction.Name} ---\n" +
                    $"Category: {auction.Category}\n" +
                    $"Description: {auction.Description}\n" +
                    $"Starting Price: ${auction.StartingPrice:N0}\n" +
                    $"Current Bid: ${auction.CurrentBid:N0} by {auction.HighestBidderName}\n" +
                    $"Min Next Bid: ${auction.CurrentBid + MinBidIncrement:N0}\n" +
                    $"Use /bid {auctionId} [amount] to place your bid!", 12000);
            }
            catch (Exception e)
            {
                Log.Write($"callback_bidMenu Exception: {e.ToString()}");
            }
        }

        public static void PlaceBid(ExtPlayer player, int auctionId, int bidAmount)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                if (!ActiveAuctions.ContainsKey(auctionId))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Auction not found.", 3000);
                    return;
                }

                var auction = ActiveAuctions[auctionId];

                if (auction.EndTime <= DateTime.Now)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "This auction has ended.", 3000);
                    return;
                }

                if (bidAmount < auction.CurrentBid + MinBidIncrement)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"Bid too low! Minimum bid is ${auction.CurrentBid + MinBidIncrement:N0}.", 3000);
                    return;
                }

                if (characterData.Money < bidAmount)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, 
                        $"You don't have enough money. You have ${characterData.Money:N0}, need ${bidAmount:N0}.", 3000);
                    return;
                }

                // Return money to previous highest bidder
                if (auction.HighestBidder != null)
                {
                    MoneySystem.Wallet.Change(auction.HighestBidder, auction.CurrentBid);
                    Notify.Send(auction.HighestBidder, NotifyType.Warning, NotifyPosition.BottomCenter, 
                        $"You've been outbid on \"{auction.Name}\"! New bid: ${bidAmount:N0} by {player.Name}.", 5000);
                }

                // Take money from new bidder
                MoneySystem.Wallet.Change(player, -bidAmount);

                auction.CurrentBid = bidAmount;
                auction.HighestBidder = player;
                auction.HighestBidderName = player.Name;

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Bid placed! You are now the highest bidder on \"{auction.Name}\" at ${bidAmount:N0}.", 5000);

                // Extend auction if bid within last 2 minutes
                TimeSpan remaining = auction.EndTime - DateTime.Now;
                if (remaining.TotalMinutes < 2)
                {
                    auction.EndTime = auction.EndTime.AddMinutes(2);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                        "Last-minute bid! Auction extended by 2 minutes.", 3000);
                }
            }
            catch (Exception e)
            {
                Log.Write($"PlaceBid Exception: {e.ToString()}");
            }
        }

        public static void ListItem(ExtPlayer player, string name, int startingPrice, string description, bool isBlackMarket = false)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var auction = new AuctionItem
                {
                    Id = NextAuctionId++,
                    Name = name,
                    Category = isBlackMarket ? AuctionCategory.Contraband : AuctionCategory.RareItems,
                    Description = description,
                    StartingPrice = startingPrice,
                    CurrentBid = startingPrice,
                    HighestBidder = null,
                    HighestBidderName = "No bids yet",
                    SellerName = player.Name,
                    Seller = player,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(AuctionDurationMinutes),
                    IsActive = true,
                    IsBlackMarket = isBlackMarket,
                };

                ActiveAuctions[auction.Id] = auction;

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, 
                    $"Item listed for auction! \"{name}\" starting at ${startingPrice:N0}.\n" +
                    $"Auction runs for {AuctionDurationMinutes} minutes.", 5000);
            }
            catch (Exception e)
            {
                Log.Write($"ListItem Exception: {e.ToString()}");
            }
        }

        private static void ShowMyBids(ExtPlayer player)
        {
            try
            {
                var myBids = ActiveAuctions.Values
                    .Where(a => a.HighestBidder == player && a.IsActive)
                    .ToList();

                if (myBids.Count == 0)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "You have no active bids.", 3000);
                    return;
                }

                foreach (var bid in myBids)
                {
                    TimeSpan remaining = bid.EndTime - DateTime.Now;
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                        $"{bid.Name}: Your bid ${bid.CurrentBid:N0} | Time left: {remaining.Minutes}m", 5000);
                }
            }
            catch (Exception e)
            {
                Log.Write($"ShowMyBids Exception: {e.ToString()}");
            }
        }

        private static void ShowMyListings(ExtPlayer player)
        {
            try
            {
                var myListings = ActiveAuctions.Values
                    .Where(a => a.Seller == player && a.IsActive)
                    .ToList();

                if (myListings.Count == 0)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "You have no active listings.", 3000);
                    return;
                }

                foreach (var listing in myListings)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, 
                        $"{listing.Name}: Current bid ${listing.CurrentBid:N0} by {listing.HighestBidderName}", 5000);
                }
            }
            catch (Exception e)
            {
                Log.Write($"ShowMyListings Exception: {e.ToString()}");
            }
        }

        /// <summary>
        /// Called periodically to close expired auctions and pay out sellers
        /// </summary>
        public static void ProcessExpiredAuctions()
        {
            try
            {
                var expired = ActiveAuctions.Values
                    .Where(a => a.IsActive && a.EndTime <= DateTime.Now)
                    .ToList();

                foreach (var auction in expired)
                {
                    auction.IsActive = false;

                    if (auction.HighestBidder != null)
                    {
                        // Auction won
                        int commission = (auction.CurrentBid * AuctionHouseFeePercent) / 100;
                        int sellerPayout = auction.CurrentBid - commission;

                        // Pay seller
                        if (auction.Seller != null)
                        {
                            MoneySystem.Wallet.Change(auction.Seller, sellerPayout);
                            Notify.Send(auction.Seller, NotifyType.Success, NotifyPosition.BottomCenter, 
                                $"Your auction \"{auction.Name}\" sold for ${auction.CurrentBid:N0}!\n" +
                                $"Commission: ${commission:N0} | Your payout: ${sellerPayout:N0}", 8000);
                        }

                        // Notify winner
                        Notify.Send(auction.HighestBidder, NotifyType.Success, NotifyPosition.BottomCenter, 
                            $"Congratulations! You won the auction for \"{auction.Name}\" at ${auction.CurrentBid:N0}!", 8000);
                    }

                    ActiveAuctions.Remove(auction.Id);
                }
            }
            catch (Exception e)
            {
                Log.Write($"ProcessExpiredAuctions Exception: {e.ToString()}");
            }
        }
    }

    public class AuctionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AuctionHouse.AuctionCategory Category { get; set; }
        public string Description { get; set; }
        public int StartingPrice { get; set; }
        public int CurrentBid { get; set; }
        public ExtPlayer HighestBidder { get; set; }
        public string HighestBidderName { get; set; }
        public ExtPlayer Seller { get; set; }
        public string SellerName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlackMarket { get; set; }
    }

    public class AuctionTemplate
    {
        public string Name { get; set; }
        public AuctionHouse.AuctionCategory Category { get; set; }
        public int StartingPrice { get; set; }
        public string Description { get; set; }

        public AuctionTemplate(string name, AuctionHouse.AuctionCategory category, int startingPrice, string description)
        {
            Name = name;
            Category = category;
            StartingPrice = startingPrice;
            Description = description;
        }
    }
}
