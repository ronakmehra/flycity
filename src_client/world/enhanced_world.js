// ============================================================
// ENHANCED WORLD FEATURES - FlyCity Roleplay
// ============================================================
// Features:
// - Enhanced ambient traffic & pedestrians
// - Dynamic city events (construction, accidents, protests)
// - Street vendors & NPCs
// - Ambient sounds & atmosphere
// - Weather effects on gameplay
// - Day/Night cycle events
// - Random encounters
// - City zones with unique properties
// ============================================================

// ====== ENHANCED TRAFFIC SYSTEM ======
const trafficSettings = {
    enabled: true,
    density: 1.0, // 0.0 - 3.0
    pedDensity: 1.0,
    parkedDensity: 1.0,
    boatDensity: 0.5,
    airDensity: 0.3,
};

// Set traffic density based on time of day
gm.events.add(global.renderName["10s"], () => {
    try {
        if (!global.loggedin) return;
        if (!trafficSettings.enabled) return;

        const hours = mp.game.time.getClockHours();
        let multiplier = 1.0;

        // Rush hours have more traffic
        if ((hours >= 7 && hours <= 9) || (hours >= 17 && hours <= 19)) {
            multiplier = 1.5;
        }
        // Night has less traffic
        else if (hours >= 23 || hours <= 5) {
            multiplier = 0.4;
        }
        // Midday normal
        else {
            multiplier = 1.0;
        }

        mp.game.streaming.setVehicleDensityMultiplierThisFrame(trafficSettings.density * multiplier);
        mp.game.streaming.setRandomVehicleDensityMultiplierThisFrame(trafficSettings.density * multiplier);
        mp.game.streaming.setPedDensityMultiplierThisFrame(trafficSettings.pedDensity * multiplier);
        mp.game.streaming.setScenarioPedDensityMultiplierThisFrame(trafficSettings.pedDensity * multiplier, trafficSettings.pedDensity * multiplier);
        mp.game.streaming.setParkedVehicleDensityMultiplierThisFrame(trafficSettings.parkedDensity);
    } catch (e) {}
});

// ====== CITY ZONES WITH UNIQUE PROPERTIES ======
const cityZones = {
    DOWNTOWN: {
        name: "Downtown LS",
        bounds: { x1: -800, y1: -1200, x2: 500, y2: -100 },
        effects: { traffic: 1.5, crime: 0.3, police: 1.5 }
    },
    SOUTH_LS: {
        name: "South Los Santos",
        bounds: { x1: -200, y1: -2000, x2: 800, y2: -1200 },
        effects: { traffic: 0.8, crime: 1.5, police: 0.5 }
    },
    VINEWOOD: {
        name: "Vinewood Hills",
        bounds: { x1: -1500, y1: -200, x2: 800, y2: 800 },
        effects: { traffic: 0.6, crime: 0.1, police: 1.0 }
    },
    SANDY_SHORES: {
        name: "Sandy Shores",
        bounds: { x1: 1500, y1: 3000, x2: 2500, y2: 4000 },
        effects: { traffic: 0.3, crime: 0.8, police: 0.2 }
    },
    PALETO: {
        name: "Paleto Bay",
        bounds: { x1: -500, y1: 5800, x2: 500, y2: 6600 },
        effects: { traffic: 0.2, crime: 0.4, police: 0.3 }
    },
    PORT: {
        name: "Port of LS",
        bounds: { x1: 700, y1: -3200, x2: 1700, y2: -2500 },
        effects: { traffic: 0.5, crime: 1.2, police: 0.4 }
    },
    AIRPORT: {
        name: "LS International",
        bounds: { x1: -1500, y1: -3400, x2: -800, y2: -2400 },
        effects: { traffic: 0.8, crime: 0.1, police: 2.0 }
    },
};

let currentZone = null;

gm.events.add(global.renderName["5s"], () => {
    try {
        if (!global.loggedin) return;
        const pos = global.localplayer.position;
        
        for (let key in cityZones) {
            const zone = cityZones[key];
            if (pos.x >= zone.bounds.x1 && pos.x <= zone.bounds.x2 &&
                pos.y >= zone.bounds.y1 && pos.y <= zone.bounds.y2) {
                if (currentZone !== key) {
                    currentZone = key;
                    mp.gui.emmit(`window.hudStore.setZone && window.hudStore.setZone('${zone.name}')`);
                }
                return;
            }
        }
        if (currentZone !== null) {
            currentZone = null;
            mp.gui.emmit(`window.hudStore.setZone && window.hudStore.setZone('')`);
        }
    } catch (e) {}
});

// ====== RANDOM CITY EVENTS ======
const randomEvents = {
    events: [
        { type: "car_accident", chance: 0.02, duration: 120000 },
        { type: "street_fight", chance: 0.015, duration: 60000 },
        { type: "police_chase", chance: 0.01, duration: 90000 },
        { type: "construction", chance: 0.005, duration: 300000 },
        { type: "street_race_npc", chance: 0.008, duration: 45000 },
        { type: "ambulance_call", chance: 0.02, duration: 60000 },
        { type: "fire_truck", chance: 0.01, duration: 120000 },
        { type: "armored_truck", chance: 0.005, duration: 180000 },
    ],
    activeEvents: [],
};

// ====== AMBIENT ATMOSPHERE ======
gm.events.add(global.renderName["2s"], () => {
    try {
        if (!global.loggedin) return;

        // Disable some annoying default behaviors
        mp.game.gameplay.setCreateRandomCops(true);
        mp.game.gameplay.setCreateRandomCopsNotOnScenarios(true);
        
        // Enhanced vehicle variety
        mp.game.vehicle.setRandomTrains(true);
        
    } catch (e) {}
});

// ====== WEATHER EFFECTS ON GAMEPLAY ======
gm.events.add('client.weather.effects', (weatherType) => {
    try {
        switch(weatherType) {
            case "RAIN":
            case "THUNDER":
                // Reduce vehicle traction in rain
                if (global.localplayer.isInAnyVehicle(false)) {
                    const vehicle = global.localplayer.vehicle;
                    if (vehicle && mp.vehicles.exists(vehicle)) {
                        // Wet roads - reduced grip notification
                        mp.events.call('notify', 3, 9, translateText("Скользкая дорога! Будьте осторожны 🌧️"), 5000);
                    }
                }
                break;
            case "SNOW":
            case "BLIZZARD":
                mp.events.call('notify', 3, 9, translateText("Снежная буря! Видимость снижена ❄️"), 5000);
                break;
            case "FOGGY":
                mp.events.call('notify', 3, 9, translateText("Густой туман! Видимость ограничена 🌫️"), 5000);
                break;
        }
    } catch (e) {}
});

// ====== DAY/NIGHT CYCLE EVENTS ======
let lastTimeEvent = -1;

gm.events.add(global.renderName["10s"], () => {
    try {
        if (!global.loggedin) return;
        const hours = mp.game.time.getClockHours();
        
        if (hours !== lastTimeEvent) {
            lastTimeEvent = hours;
            
            // Notify special times
            switch(hours) {
                case 6:
                    mp.gui.emmit(`window.hudStore.dayNightEvent && window.hudStore.dayNightEvent('sunrise')`);
                    break;
                case 20:
                    mp.gui.emmit(`window.hudStore.dayNightEvent && window.hudStore.dayNightEvent('sunset')`);
                    break;
                case 0:
                    mp.gui.emmit(`window.hudStore.dayNightEvent && window.hudStore.dayNightEvent('midnight')`);
                    break;
                case 12:
                    mp.gui.emmit(`window.hudStore.dayNightEvent && window.hudStore.dayNightEvent('noon')`);
                    break;
            }
        }
    } catch (e) {}
});

// ====== ENHANCED STREET LIGHTING ======
gm.events.add("render", () => {
    try {
        if (!global.loggedin) return;
        
        const hours = mp.game.time.getClockHours();
        
        // Enable artificial lights at night
        if (hours >= 20 || hours <= 6) {
            mp.game.graphics.setArtificialLightsState(false);
        }
    } catch (e) {}
});

// ====== VEHICLE POPULATION CONTROL ======
gm.events.add('client.world.setTraffic', (density, pedDensity) => {
    trafficSettings.density = Number(density);
    trafficSettings.pedDensity = Number(pedDensity);
});

gm.events.add('client.world.toggleTraffic', (enabled) => {
    trafficSettings.enabled = Boolean(enabled);
});

// ====== GARBAGE TRUCK ROUTES (Ambient) ======
const ambientRoutes = {
    garbageTruck: true,
    deliveryTrucks: true,
    taxiService: true,
    busRoutes: true,
};

// ====== STREET VENDORS & AMBIENT NPCS ======
const streetVendorLocations = [
    { x: -1195.4, y: -1508.8, z: 4.4, heading: 125.0, type: "hotdog" },
    { x: 286.3, y: -1062.2, z: 29.3, heading: 65.0, type: "fruit" },
    { x: -1220.5, y: -286.8, z: 37.7, heading: 205.0, type: "newspaper" },
    { x: 159.5, y: -1033.5, z: 29.3, heading: 340.0, type: "coffee" },
    { x: -706.1, y: -913.5, z: 19.2, heading: 85.0, type: "taco" },
    { x: 1213.4, y: -471.8, z: 66.2, heading: 55.0, type: "icecream" },
    { x: -1489.5, y: -378.2, z: 40.1, heading: 130.0, type: "flower" },
    { x: -310.6, y: -693.3, z: 33.1, heading: 250.0, type: "electronics" },
];

// ====== SPEED CAMERAS ======
const speedCameras = [
    { x: -543.0, y: -672.0, z: 33.0, speedLimit: 80, direction: 180 },
    { x: -820.0, y: -1230.0, z: 7.0, speedLimit: 60, direction: 90 },
    { x: 115.0, y: -1722.0, z: 29.0, speedLimit: 80, direction: 0 },
    { x: -247.0, y: -989.0, z: 29.0, speedLimit: 50, direction: 270 },
    { x: 445.0, y: -988.0, z: 25.0, speedLimit: 60, direction: 180 },
    { x: -1055.0, y: -2728.0, z: 13.0, speedLimit: 100, direction: 90 },
    { x: 812.0, y: -1888.0, z: 29.0, speedLimit: 80, direction: 0 },
    { x: -2065.0, y: -332.0, z: 13.0, speedLimit: 60, direction: 45 },
    { x: 1262.0, y: -1710.0, z: 54.0, speedLimit: 100, direction: 135 },
    { x: -1520.0, y: -891.0, z: 10.0, speedLimit: 60, direction: 315 },
];

let lastSpeedCameraWarning = 0;

gm.events.add(global.renderName["1s"], () => {
    try {
        if (!global.loggedin) return;
        if (!global.localplayer.isInAnyVehicle(false)) return;
        
        const vehicle = global.localplayer.vehicle;
        if (!vehicle || !mp.vehicles.exists(vehicle)) return;
        if (vehicle.getPedInSeat(-1) !== global.localplayer.handle) return;

        const pos = global.localplayer.position;
        const speed = vehicle.getSpeed() * 3.6;
        const now = new Date().getTime();

        if (now - lastSpeedCameraWarning < 10000) return;

        for (let cam of speedCameras) {
            const dist = mp.game.system.vdist(pos.x, pos.y, pos.z, cam.x, cam.y, cam.z);
            if (dist < 50 && speed > cam.speedLimit) {
                lastSpeedCameraWarning = now;
                mp.events.call('notify', 4, 9, translateText(`Камера! Превышение: ${Math.round(speed)} / ${cam.speedLimit} км/ч 📸`), 3000);
                mp.events.callRemote('server.world.speedcamera', Math.round(speed), cam.speedLimit);
                break;
            }
        }
    } catch (e) {}
});

// ====== VEHICLE RADIO STATIONS (Enhanced) ======
const customRadioStations = [
    { name: "FlyCity FM", genre: "Hip-Hop/Rap", frequency: "98.7" },
    { name: "Underground Radio", genre: "Electronic", frequency: "101.3" },
    { name: "LS Classics", genre: "Classic Rock", frequency: "105.5" },
    { name: "Ghetto Radio", genre: "Gangsta Rap", frequency: "88.1" },
    { name: "Drift FM", genre: "Eurobeat", frequency: "92.4" },
    { name: "Mafia Radio", genre: "Italian/Jazz", frequency: "96.8" },
];

// ====== CAR MEETS / GATHERINGS ======
gm.events.add('client.world.carmeet', (x, y, z, name) => {
    try {
        mp.events.call("createBlip", "carmeet", translateText("Тусовка") + ": " + name, 524, new mp.Vector3(x, y, z), 1.5, 5);
        mp.events.call('notify', 1, 9, translateText(`Тусовка "${name}" началась! Проверьте карту 🏎️`), 8000);
    } catch (e) {}
});

gm.events.add('client.world.carmeet.end', () => {
    mp.events.call("deleteBlip", "carmeet");
});

// ====== POPULATION DENSITY BASED ON EVENTS ======
gm.events.add('client.world.event.start', (eventType, x, y, z) => {
    try {
        switch(eventType) {
            case "parade":
                mp.events.call('notify', 1, 9, translateText("В городе проходит парад! 🎉"), 8000);
                break;
            case "concert":
                mp.events.call('notify', 1, 9, translateText("Концерт начался! 🎵"), 8000);
                break;
            case "protest":
                mp.events.call('notify', 1, 9, translateText("Протест в центре города! ⚠️"), 8000);
                break;
            case "market":
                mp.events.call('notify', 1, 9, translateText("Черный рынок открыт! 🏴"), 8000);
                break;
            case "race_illegal":
                mp.events.call('notify', 1, 9, translateText("Нелегальные гонки начались! 🏁"), 8000);
                break;
        }
        
        if (x && y && z) {
            mp.events.call("createBlip", "worldevent", translateText("Событие"), 487, new mp.Vector3(x, y, z), 1.5, 1);
        }
    } catch (e) {}
});

gm.events.add('client.world.event.end', () => {
    mp.events.call("deleteBlip", "worldevent");
});
