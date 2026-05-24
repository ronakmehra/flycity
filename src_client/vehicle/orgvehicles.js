// ============================================================
// ORGANIZATION & GANG VEHICLE SYSTEM - FlyCity Roleplay
// ============================================================
// Features:
// - Organization vehicle garage with exclusive cars
// - Gang-specific vehicles (armored, fast, utility)
// - Vehicle upgrades per org level
// - Gang war vehicles (spawnable during wars)
// - Organization fleet management
// - Premium org vehicles with special abilities
// ============================================================

// ====== ORGANIZATION VEHICLE CATEGORIES ======
const orgVehicleCategories = {
    // Street Gang Vehicles
    GANG_STREET: {
        name: translateText("Уличные"),
        vehicles: [
            { model: "sultan", name: "Sultan", price: 25000, rank: 1, speed: "fast" },
            { model: "kuruma", name: "Kuruma", price: 45000, rank: 2, speed: "fast" },
            { model: "elegy2", name: "Elegy RH8", price: 55000, rank: 2, speed: "super" },
            { model: "buffalo", name: "Buffalo", price: 30000, rank: 1, speed: "fast" },
            { model: "schafter2", name: "Schafter", price: 50000, rank: 3, speed: "fast" },
            { model: "sentinel", name: "Sentinel", price: 35000, rank: 1, speed: "sport" },
            { model: "f620", name: "F620", price: 40000, rank: 2, speed: "sport" },
            { model: "exemplar", name: "Exemplar", price: 60000, rank: 3, speed: "sport" },
            { model: "tailgater", name: "Tailgater", price: 45000, rank: 2, speed: "sport" },
            { model: "schwarzer", name: "Schwarzer", price: 55000, rank: 3, speed: "sport" },
        ]
    },

    // Muscle Cars (Biker Gangs)
    GANG_MUSCLE: {
        name: translateText("Масл кары"),
        vehicles: [
            { model: "dominator", name: "Dominator", price: 35000, rank: 1, speed: "fast" },
            { model: "gauntlet", name: "Gauntlet", price: 40000, rank: 1, speed: "fast" },
            { model: "sabregt", name: "Sabre Turbo", price: 30000, rank: 1, speed: "fast" },
            { model: "virgo", name: "Virgo", price: 25000, rank: 1, speed: "normal" },
            { model: "phoenix", name: "Phoenix", price: 35000, rank: 2, speed: "fast" },
            { model: "ruiner", name: "Ruiner", price: 40000, rank: 2, speed: "fast" },
            { model: "impaler", name: "Impaler", price: 55000, rank: 3, speed: "fast" },
            { model: "imperator", name: "Imperator", price: 80000, rank: 4, speed: "super" },
            { model: "dominator2", name: "Pisswasser Dominator", price: 65000, rank: 3, speed: "fast" },
            { model: "ratloader2", name: "Rat-Truck", price: 20000, rank: 1, speed: "slow" },
        ]
    },

    // Bikes (Biker Gangs)
    GANG_BIKES: {
        name: translateText("Мотоциклы банды"),
        vehicles: [
            { model: "daemon", name: "Daemon", price: 15000, rank: 1, speed: "fast" },
            { model: "hexer", name: "Hexer", price: 18000, rank: 1, speed: "fast" },
            { model: "zombiea", name: "Zombie Chopper", price: 20000, rank: 2, speed: "fast" },
            { model: "wolfsbane", name: "Wolfsbane", price: 22000, rank: 2, speed: "fast" },
            { model: "nightblade", name: "Nightblade", price: 25000, rank: 2, speed: "fast" },
            { model: "ratbike", name: "Rat Bike", price: 12000, rank: 1, speed: "normal" },
            { model: "avarus", name: "Avarus", price: 28000, rank: 3, speed: "fast" },
            { model: "chimera", name: "Chimera", price: 35000, rank: 3, speed: "fast" },
            { model: "sanctus", name: "Sanctus", price: 50000, rank: 4, speed: "fast" },
            { model: "deathbike", name: "Deathbike", price: 75000, rank: 5, speed: "super" },
            { model: "shotaro", name: "Shotaro", price: 100000, rank: 5, speed: "super" },
            { model: "hakuchou2", name: "Hakuchou Drag", price: 85000, rank: 4, speed: "super" },
        ]
    },

    // Armored Vehicles (Mafia)
    GANG_ARMORED: {
        name: translateText("Бронированные"),
        vehicles: [
            { model: "kuruma2", name: "Kuruma Armored", price: 150000, rank: 4, speed: "fast", armored: true },
            { model: "insurgent", name: "Insurgent", price: 200000, rank: 5, speed: "normal", armored: true },
            { model: "insurgent2", name: "Insurgent Pick-Up", price: 250000, rank: 6, speed: "normal", armored: true },
            { model: "nightshark", name: "Nightshark", price: 180000, rank: 5, speed: "fast", armored: true },
            { model: "menacer", name: "Menacer", price: 220000, rank: 5, speed: "fast", armored: true },
            { model: "halftrack", name: "Half-track", price: 300000, rank: 6, speed: "slow", armored: true },
            { model: "apc", name: "APC", price: 500000, rank: 7, speed: "slow", armored: true },
            { model: "khanjali", name: "TM-02 Khanjali", price: 750000, rank: 8, speed: "slow", armored: true },
            { model: "barrage", name: "Barrage", price: 280000, rank: 6, speed: "normal", armored: true },
            { model: "stromberg", name: "Stromberg", price: 400000, rank: 7, speed: "fast", armored: true },
        ]
    },

    // Super Cars (Premium Orgs)
    GANG_SUPER: {
        name: translateText("Суперкары банды"),
        vehicles: [
            { model: "zentorno", name: "Zentorno", price: 200000, rank: 4, speed: "super" },
            { model: "t20", name: "T20", price: 250000, rank: 5, speed: "super" },
            { model: "osiris", name: "Osiris", price: 220000, rank: 4, speed: "super" },
            { model: "reaper", name: "Reaper", price: 230000, rank: 5, speed: "super" },
            { model: "tempesta", name: "Tempesta", price: 260000, rank: 5, speed: "super" },
            { model: "vagner", name: "Vagner", price: 300000, rank: 6, speed: "super" },
            { model: "visione", name: "Visione", price: 350000, rank: 6, speed: "super" },
            { model: "emerus", name: "Emerus", price: 400000, rank: 7, speed: "super" },
            { model: "krieger", name: "Krieger", price: 450000, rank: 7, speed: "super" },
            { model: "s80", name: "S80RR", price: 500000, rank: 8, speed: "super" },
            { model: "deveste", name: "Deveste Eight", price: 550000, rank: 8, speed: "super" },
            { model: "thrax", name: "Thrax", price: 480000, rank: 7, speed: "super" },
            { model: "furia", name: "Furia", price: 520000, rank: 8, speed: "super" },
            { model: "ignus", name: "Ignus", price: 600000, rank: 9, speed: "super" },
            { model: "zeno", name: "Zeno", price: 650000, rank: 9, speed: "super" },
        ]
    },

    // Utility Vehicles (All Orgs)
    GANG_UTILITY: {
        name: translateText("Служебные"),
        vehicles: [
            { model: "baller3", name: "Baller LE", price: 60000, rank: 2, speed: "sport" },
            { model: "xls", name: "XLS", price: 55000, rank: 2, speed: "sport" },
            { model: "granger", name: "Granger", price: 40000, rank: 1, speed: "normal" },
            { model: "rumpo3", name: "Rumpo Custom", price: 30000, rank: 1, speed: "normal" },
            { model: "speedo4", name: "Speedo Custom", price: 35000, rank: 1, speed: "normal" },
            { model: "mule4", name: "Mule Custom", price: 45000, rank: 2, speed: "slow" },
            { model: "pounder2", name: "Pounder Custom", price: 55000, rank: 3, speed: "slow" },
            { model: "brickade", name: "Brickade", price: 50000, rank: 3, speed: "slow" },
            { model: "youga2", name: "Youga Classic", price: 25000, rank: 1, speed: "normal" },
            { model: "burrito3", name: "Burrito", price: 20000, rank: 1, speed: "normal" },
        ]
    },

    // Air Vehicles (High Rank Orgs)
    GANG_AIR: {
        name: translateText("Воздушные"),
        vehicles: [
            { model: "buzzard2", name: "Buzzard", price: 500000, rank: 7, speed: "super" },
            { model: "maverick", name: "Maverick", price: 350000, rank: 6, speed: "fast" },
            { model: "frogger", name: "Frogger", price: 300000, rank: 5, speed: "fast" },
            { model: "swift2", name: "Swift Deluxe", price: 600000, rank: 8, speed: "fast" },
            { model: "volatus", name: "Volatus", price: 700000, rank: 8, speed: "fast" },
            { model: "havok", name: "Havok", price: 250000, rank: 5, speed: "fast" },
            { model: "supervolito2", name: "SuperVolito Carbon", price: 800000, rank: 9, speed: "super" },
            { model: "akula", name: "Akula", price: 1000000, rank: 10, speed: "super" },
            { model: "hunter", name: "Hunter", price: 1200000, rank: 10, speed: "super" },
            { model: "savage", name: "Savage", price: 900000, rank: 9, speed: "super" },
        ]
    },

    // Water Vehicles
    GANG_WATER: {
        name: translateText("Водные"),
        vehicles: [
            { model: "dinghy", name: "Dinghy", price: 30000, rank: 1, speed: "fast" },
            { model: "speeder", name: "Speeder", price: 50000, rank: 2, speed: "fast" },
            { model: "jetmax", name: "Jetmax", price: 80000, rank: 3, speed: "super" },
            { model: "toro", name: "Toro", price: 120000, rank: 4, speed: "fast" },
            { model: "seashark", name: "Seashark", price: 15000, rank: 1, speed: "fast" },
            { model: "marquis", name: "Marquis", price: 100000, rank: 3, speed: "normal" },
        ]
    },
};

// ====== GANG WAR SPECIAL VEHICLES ======
const gangWarVehicles = {
    ATTACK: [
        { model: "technical2", name: "Technical Custom", requiredMembers: 4 },
        { model: "insurgent2", name: "Insurgent Pick-Up", requiredMembers: 6 },
        { model: "menacer", name: "Menacer", requiredMembers: 5 },
        { model: "barrage", name: "Barrage", requiredMembers: 8 },
        { model: "halftrack", name: "Half-track", requiredMembers: 8 },
    ],
    TRANSPORT: [
        { model: "brickade", name: "Brickade", requiredMembers: 2 },
        { model: "insurgent", name: "Insurgent", requiredMembers: 4 },
        { model: "nightshark", name: "Nightshark", requiredMembers: 3 },
        { model: "granger", name: "Granger", requiredMembers: 2 },
    ],
    PURSUIT: [
        { model: "kuruma2", name: "Kuruma Armored", requiredMembers: 2 },
        { model: "zentorno", name: "Zentorno", requiredMembers: 3 },
        { model: "vagner", name: "Vagner", requiredMembers: 5 },
        { model: "shotaro", name: "Shotaro", requiredMembers: 4 },
        { model: "hakuchou2", name: "Hakuchou Drag", requiredMembers: 3 },
    ],
};

// ====== ORGANIZATION VEHICLE BONUSES ======
const orgVehicleBonuses = {
    // Level-based upgrades for org vehicles
    1: { speedBonus: 0, armorBonus: 0, nosCapacity: 50 },
    2: { speedBonus: 5, armorBonus: 10, nosCapacity: 60 },
    3: { speedBonus: 8, armorBonus: 15, nosCapacity: 70 },
    4: { speedBonus: 12, armorBonus: 20, nosCapacity: 80 },
    5: { speedBonus: 15, armorBonus: 30, nosCapacity: 90 },
    6: { speedBonus: 18, armorBonus: 35, nosCapacity: 95 },
    7: { speedBonus: 20, armorBonus: 40, nosCapacity: 100 },
    8: { speedBonus: 22, armorBonus: 45, nosCapacity: 100 },
    9: { speedBonus: 25, armorBonus: 50, nosCapacity: 100 },
    10: { speedBonus: 30, armorBonus: 60, nosCapacity: 100 },
};

// ====== CLIENT EVENTS ======
let orgGarageOpen = false;
let orgGarageData = null;
let orgGarageEntity = null;

// Open org vehicle garage
gm.events.add('client.orgvehicle.open', async (category, orgLevel, orgBalance) => {
    try {
        if (global.menuCheck()) return;
        global.menuOpen();
        orgGarageOpen = true;

        const categoryData = orgVehicleCategories[category];
        if (!categoryData) return;

        // Filter vehicles by org level (rank)
        const availableVehicles = categoryData.vehicles.filter(v => v.rank <= orgLevel);
        const bonuses = orgVehicleBonuses[Math.min(orgLevel, 10)];

        const data = {
            category: categoryData.name,
            vehicles: availableVehicles,
            bonuses: bonuses,
            orgBalance: orgBalance,
            orgLevel: orgLevel,
        };

        mp.gui.emmit(`window.router.setView("OrgVehicleGarage", '${JSON.stringify(data)}');`);
        gm.discord(translateText("В гараже организации"));
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "openGarage", e.toString());
    }
});

// Buy org vehicle
gm.events.add('client.orgvehicle.buy', (modelName, colorId) => {
    try {
        if (new Date().getTime() - global.lastCheck < 1000) return;
        global.lastCheck = new Date().getTime();
        mp.events.callRemote('server.orgvehicle.buy', modelName, colorId);
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "buy", e.toString());
    }
});

// Spawn org vehicle
gm.events.add('client.orgvehicle.spawn', (vehicleId) => {
    try {
        if (new Date().getTime() - global.lastCheck < 1000) return;
        global.lastCheck = new Date().getTime();
        mp.events.callRemote('server.orgvehicle.spawn', vehicleId);
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "spawn", e.toString());
    }
});

// Despawn org vehicle
gm.events.add('client.orgvehicle.despawn', (vehicleId) => {
    try {
        mp.events.callRemote('server.orgvehicle.despawn', vehicleId);
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "despawn", e.toString());
    }
});

// Upgrade org vehicle
gm.events.add('client.orgvehicle.upgrade', (vehicleId, upgradeType) => {
    try {
        mp.events.callRemote('server.orgvehicle.upgrade', vehicleId, upgradeType);
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "upgrade", e.toString());
    }
});

// Close org garage
gm.events.add('client.orgvehicle.close', () => {
    try {
        orgGarageOpen = false;
        mp.gui.emmit(`window.router.setHud();`);
        global.menuClose();
        
        if (orgGarageEntity && mp.vehicles.exists(orgGarageEntity)) {
            orgGarageEntity.destroy();
            orgGarageEntity = null;
        }
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "close", e.toString());
    }
});

// Preview org vehicle
gm.events.add('client.orgvehicle.preview', async (modelName) => {
    try {
        if (orgGarageEntity && mp.vehicles.exists(orgGarageEntity)) {
            orgGarageEntity.destroy();
        }

        await global.loadModel(modelName);
        
        const pos = global.localplayer.position;
        orgGarageEntity = mp.vehicles.new(mp.game.joaat(modelName), new mp.Vector3(pos.x + 3, pos.y + 3, pos.z), {
            heading: global.localplayer.getHeading(),
            numberPlate: 'ORG VEH',
            alpha: 255,
            locked: true,
            engine: false,
            dimension: global.localplayer.dimension
        });
        
        if (orgGarageEntity) {
            orgGarageEntity.setInvincible(true);
            orgGarageEntity.freezePosition(true);
        }
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "preview", e.toString());
    }
});

// ====== GANG WAR VEHICLE EVENTS ======
gm.events.add('client.gangwar.vehiclemenu', (availableVehicles, teamSize) => {
    try {
        if (global.menuCheck()) return;
        global.menuOpen();

        const data = {
            attack: gangWarVehicles.ATTACK.filter(v => v.requiredMembers <= teamSize),
            transport: gangWarVehicles.TRANSPORT.filter(v => v.requiredMembers <= teamSize),
            pursuit: gangWarVehicles.PURSUIT.filter(v => v.requiredMembers <= teamSize),
            teamSize: teamSize,
        };

        mp.gui.emmit(`window.router.setView("GangWarVehicles", '${JSON.stringify(data)}');`);
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "gangwarMenu", e.toString());
    }
});

gm.events.add('client.gangwar.spawnvehicle', (category, index) => {
    try {
        mp.events.callRemote('server.gangwar.spawnvehicle', category, index);
        mp.gui.emmit(`window.router.setHud();`);
        global.menuClose();
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "gangwarSpawn", e.toString());
    }
});

// ====== ORG VEHICLE STREAM HANDLER - Apply bonuses ======
gm.events.add("vehicleStreamIn", (entity) => {
    try {
        if (!entity || !mp.vehicles.exists(entity)) return;
        
        const orgVehicle = entity.getVariable('ORG_VEHICLE');
        if (!orgVehicle) return;

        const orgLevel = entity.getVariable('ORG_LEVEL') || 1;
        const bonuses = orgVehicleBonuses[Math.min(orgLevel, 10)];

        // Apply speed bonus
        if (bonuses.speedBonus > 0) {
            const maxSpeed = mp.game.vehicle.getVehicleModelMaxSpeed(entity.model);
            entity.setMaxSpeed(maxSpeed + bonuses.speedBonus / 3.6);
        }

        // Visual indicator for org vehicles
        if (entity.getVariable('ORG_VEHICLE_PREMIUM')) {
            // Premium vehicles get neon underglow
            global.setNeonLight && global.setNeonLight(entity, [true, true, true, true]);
        }
    } catch (e) {}
});

// ====== ORG FLEET MANAGEMENT ======
gm.events.add('client.orgfleet.open', (fleetData) => {
    try {
        if (global.menuCheck()) return;
        global.menuOpen();
        
        const data = JSON.parse(fleetData);
        mp.gui.emmit(`window.router.setView("OrgFleetManager", '${JSON.stringify(data)}');`);
        gm.discord(translateText("Управляет автопарком организации"));
    } catch (e) {
        mp.events.callRemote("client_trycatch", "vehicle/orgvehicles", "fleetOpen", e.toString());
    }
});

gm.events.add('client.orgfleet.repair', (vehicleId) => {
    mp.events.callRemote('server.orgfleet.repair', vehicleId);
});

gm.events.add('client.orgfleet.sell', (vehicleId) => {
    mp.events.callRemote('server.orgfleet.sell', vehicleId);
});

gm.events.add('client.orgfleet.transfer', (vehicleId, targetName) => {
    mp.events.callRemote('server.orgfleet.transfer', vehicleId, targetName);
});

gm.events.add('client.orgfleet.close', () => {
    mp.gui.emmit(`window.router.setHud();`);
    global.menuClose();
});
