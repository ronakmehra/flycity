// Delivery Job - Client Side
// Handles delivery waypoints, checkpoints, timers, and completion

let deliveryBlip = null;
let deliveryMarker = null;
let deliveryCheckpoint = null;
let isExpressDelivery = false;
let expressTimer = null;
let deliveryStartTime = 0;

// Set delivery waypoint
gm.events.add('client.delivery.setWaypoint', (x, y, z, express) => {
    try {
        clearDeliveryMarkers();
        
        isExpressDelivery = express;
        const pos = new mp.Vector3(x, y, z);
        
        // Create blip on map
        deliveryBlip = mp.blips.new(1, pos, {
            name: express ? "EXPRESS Delivery" : "Delivery Point",
            color: express ? 1 : 2,
            shortRange: false,
            scale: 1.2
        });
        
        // Create marker at destination
        deliveryMarker = mp.markers.new(1, new mp.Vector3(x, y, z - 1), 2, {
            color: express ? [255, 50, 0, 200] : [0, 255, 100, 200],
            visible: true,
            dimension: 0
        });
        
        // Create checkpoint
        deliveryCheckpoint = mp.checkpoints.new(4, pos, 3, {
            color: express ? [255, 50, 0, 200] : [0, 255, 100, 200],
            visible: true,
            dimension: 0
        });
        
        // Set GPS waypoint
        mp.game.ui.setNewWaypoint(x, y);
        
        // Start express timer
        if (express) {
            deliveryStartTime = Date.now();
            expressTimer = setInterval(() => {
                let elapsed = Math.floor((Date.now() - deliveryStartTime) / 1000);
                let remaining = 180 - elapsed;
                if (remaining <= 0) {
                    clearInterval(expressTimer);
                    mp.gui.emmit(`window.notifications.push("error", "EXPRESS FAILED", "Time ran out!")`);
                    isExpressDelivery = false;
                }
            }, 1000);
        }
        
        let typeText = express ? "EXPRESS DELIVERY" : "Standard Delivery";
        mp.gui.emmit(`window.notifications.push("info", "${typeText}", "Follow the GPS marker to deliver your package.")`);
        
    } catch(e) {
        mp.events.callRemote("client_trycatch", "vehicle/delivery", "setWaypoint", e.toString());
    }
});

// Check player proximity to delivery point
mp.events.add('render', () => {
    if (!deliveryMarker) return;
    
    const playerPos = mp.players.local.position;
    const markerPos = deliveryMarker.position;
    
    const dist = mp.game.system.vdist(playerPos.x, playerPos.y, playerPos.z, markerPos.x, markerPos.y, markerPos.z);
    
    if (dist < 3.0) {
        completeDelivery(dist);
    }
});

function completeDelivery(distance) {
    if (expressTimer) {
        clearInterval(expressTimer);
        expressTimer = null;
    }
    
    // Notify server
    mp.events.callRemote('server.delivery.complete', isExpressDelivery, distance);
    clearDeliveryMarkers();
}

function clearDeliveryMarkers() {
    if (deliveryBlip) { deliveryBlip.destroy(); deliveryBlip = null; }
    if (deliveryMarker) { deliveryMarker.destroy(); deliveryMarker = null; }
    if (deliveryCheckpoint) { deliveryCheckpoint.destroy(); deliveryCheckpoint = null; }
    if (expressTimer) { clearInterval(expressTimer); expressTimer = null; }
}

// Garbage route waypoints
gm.events.add('client.garbage.setRoute', (x, y, z) => {
    try {
        mp.game.ui.setNewWaypoint(x, y);
        mp.gui.emmit(`window.notifications.push("info", "Garbage Route", "Drive to the next collection point.")`);
    } catch(e) {
        mp.events.callRemote("client_trycatch", "vehicle/delivery", "garbageRoute", e.toString());
    }
});

// Clothing reset
gm.events.add('client.clothing.resetToCivilian', () => {
    try {
        // Reset all clothing to default/stored civilian clothes
        mp.events.callRemote('server.clothing.loadCivilian');
    } catch(e) {
        mp.events.callRemote("client_trycatch", "vehicle/delivery", "clothingReset", e.toString());
    }
});
