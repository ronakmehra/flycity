// Government Buildings Interior Client-Side Handler
// Handles interior navigation, waypoint markers, and building entry/exit

let currentBuilding = null;
let interiorBlips = [];
let interiorMarkers = [];

// Building entry event
gm.events.add('client.govbuilding.showInterior', (jsonData) => {
    try {
        const data = JSON.parse(jsonData);
        currentBuilding = data;
        
        // Create interior navigation blips
        createInteriorNavigation(data);
        
        mp.gui.emmit(`window.notifications.push("info", "Entered: ${data.orgName}", "Use markers to navigate the building.")`);
    } catch(e) {
        mp.events.callRemote("client_trycatch", "fractions/govbuildings", "showInterior", e.toString());
    }
});

function createInteriorNavigation(data) {
    clearInteriorMarkers();
    
    // Reception marker
    if (data.reception) {
        createNavPoint(data.reception, "Reception", 0, 100, 255);
    }
    
    // Locker Room
    if (data.lockerRoom) {
        createNavPoint(data.lockerRoom, "Locker Room", 0, 200, 0);
    }
    
    // Armory
    if (data.armory) {
        createNavPoint(data.armory, "Armory", 255, 50, 0);
    }
    
    // Jail/Cells
    if (data.cells) {
        createNavPoint(data.cells, "Cells", 150, 150, 150);
    }
    
    // HR Office
    if (data.hrOffice) {
        createNavPoint(data.hrOffice, "HR Office", 0, 100, 255);
    }
    
    // Garage
    if (data.garage) {
        createNavPoint(data.garage, "Garage", 255, 200, 0);
    }
}

function createNavPoint(pos, name, r, g, b) {
    const marker = mp.markers.new(1, new mp.Vector3(pos.X, pos.Y, pos.Z - 1), 1, {
        color: [r, g, b, 180],
        visible: true,
        dimension: 0
    });
    interiorMarkers.push(marker);
}

function clearInteriorMarkers() {
    interiorMarkers.forEach(m => {
        if (m && m.destroy) m.destroy();
    });
    interiorMarkers = [];
    interiorBlips.forEach(b => {
        if (b && b.destroy) b.destroy();
    });
    interiorBlips = [];
}

// Cleanup on exit
gm.events.add('client.govbuilding.exit', () => {
    clearInteriorMarkers();
    currentBuilding = null;
});
