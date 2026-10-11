// =======================================================
// SLT NOC Dashboard
// alarm-dashboard.js
// Part 1
// =======================================================

let map;
let markerLayer;
let markers = [];

// -------------------------------------------------------
// Dashboard Start
// -------------------------------------------------------

document.addEventListener("DOMContentLoaded", () => {

    startClock();

    createMap();

    if (window.mapNodes) {

        updateDashboard();
        buildTopNodes();

        if (window.mapNodes.length > 0) {
            loadMarkers();
        }

    }

    updateRefreshTime();

});

// -------------------------------------------------------
// Live Clock
// -------------------------------------------------------

function startClock() {

    updateClock();

    setInterval(updateClock, 1000);

}

function updateClock() {

    const clock = document.getElementById("liveClock");

    if (!clock) return;

    const now = new Date();

    clock.innerHTML =
        now.toLocaleDateString() +
        " " +
        now.toLocaleTimeString();

}

// -------------------------------------------------------
// Refresh Time
// -------------------------------------------------------

function updateRefreshTime() {

    const refresh = document.getElementById("lastRefresh");

    if (!refresh) return;

    refresh.innerHTML =
        new Date().toLocaleTimeString();

}

// -------------------------------------------------------
// Create Map
// -------------------------------------------------------

function createMap() {

    map = L.map("map", {

        zoomControl: true

    });

    map.setView([7.8731, 80.7718], 8);

   L.tileLayer(

    "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",

    {

        attribution: "&copy; OpenStreetMap contributors",
        maxZoom: 19

    }

).addTo(map);

    markerLayer = L.featureGroup().addTo(map);

}

// -------------------------------------------------------
// Load Markers
// -------------------------------------------------------

function loadMarkers() {

    markerLayer.clearLayers();

    markers = [];

    const bounds = [];

    window.mapNodes.forEach(node => {

        if (!node.Latitude || !node.Longitude)
            return;

        let color = "#ff4444";
        let nodeType = "OTHER";

    if (node.RPB && node.RPB.trim() !== "") {

    color = "#ffc107";
    nodeType = "RPB";

}
else if (node.CEANode && node.CEANode.toLowerCase() === "y") {

    color = "#4caf50";
    nodeType = "CEA";

}
else if (node.MSAN && node.MSAN.toLowerCase() === "y") {

    color = "#2196f3";
    nodeType = "MSAN";

}

        const marker = L.circleMarker(

            [node.Latitude, node.Longitude],

            {

                radius: 8,
                color: color,
                fillColor: color,
                fillOpacity: 0.9,
                weight: 2

            }

        );

                marker.bindPopup(`

            <div style="min-width:230px">

                <h4>${node.Location}</h4>

                <hr>

                <b>Node Type :</b> ${nodeType}<br>

                <b>Status :</b> ${node.Status === 2 ? "Critical" : node.Status === 1 ? "Healthy" : "Major"}<br>

                <b>Progress :</b> ${node.Progress ?? "-"}<br>

                <b>Engineer :</b> ${node.NWENG ?? "-"}

            </div>

        `);

        marker.addTo(markerLayer);

        markers.push(marker);

        bounds.push([node.Latitude, node.Longitude]);

    });

    if (bounds.length > 0) {

        map.fitBounds(bounds, {

            padding: [40, 40]

        });

    }

}

// -------------------------------------------------------
// Dashboard Statistics
// -------------------------------------------------------

function updateDashboard() {

    let msan = 0;
    let cea = 0;
    let rpb = 0;
    let other = 0;

    window.mapNodes.forEach(node => {

   if (node.RPB && node.RPB.trim() !== "") {

    rpb++;

}
else if (node.CEANode && node.CEANode.toLowerCase() === "y") {

    cea++;

}
else if (node.MSAN && node.MSAN.toLowerCase() === "y") {

    msan++;

}
else {

    other++;

}

    });

    setText("criticalCount", msan);
    setText("majorCount", cea);
    setText("healthyCount", rpb);
    setText("totalCount", other);

    setText("criticalStatus", msan);
    setText("majorStatus", cea);
    setText("healthyStatus", rpb);

}

// -------------------------------------------------------
// Helper
// -------------------------------------------------------

function setText(id, value) {

    const element = document.getElementById(id);

    if (element) {

        element.textContent = value;

    }

}

// -------------------------------------------------------
// Top Network Nodes
// -------------------------------------------------------

function buildTopNodes() {

    const container = document.getElementById("topNodes");

    if (!container)
        return;

    container.innerHTML = "";

    if (!window.mapNodes || window.mapNodes.length === 0) {

        container.innerHTML =
            "<div class='text-center'>No Nodes Available</div>";

        return;

    }

    const nodes = [...window.mapNodes]
        .sort((a, b) => b.Status - a.Status)
        .slice(0, 8);

    nodes.forEach(node => {

        let statusClass = "major";
        let statusText = "Major";

        if (node.Status === 2) {

            statusClass = "critical";
            statusText = "Critical";

        }
        else if (node.Status === 1) {

            statusClass = "healthy";
            statusText = "Healthy";

        }

        container.innerHTML += `

            <div class="node-item">

                <div>

                    <div class="node-name">

                        ${node.Location}

                    </div>

                    <small>

                        ${node.NWENG ?? "-"}

                    </small>

                </div>

                <span class="node-status ${statusClass}">

                    ${statusText}

                </span>

            </div>

        `;

    });

}

// -------------------------------------------------------
// Refresh Dashboard
// -------------------------------------------------------

function refreshDashboard() {

    loadMarkers();

    updateDashboard();

    buildTopNodes();

    updateRefreshTime();

}

// -------------------------------------------------------
// Window Resize
// -------------------------------------------------------

window.addEventListener("resize", () => {

    if (map) {

        setTimeout(() => {

            map.invalidateSize();

        }, 300);

    }

});

// -------------------------------------------------------
// Auto Refresh
// -------------------------------------------------------

setInterval(() => {

    refreshDashboard();

}, 300000);

// -------------------------------------------------------
// Leaflet Ready
// -------------------------------------------------------

setTimeout(() => {

    if (map) {

        map.invalidateSize();

    }

}, 500);

// -------------------------------------------------------
// Status Helper
// -------------------------------------------------------

function getStatusName(status) {

    switch (status) {

        case 2:
            return "Critical";

        case 1:
            return "Healthy";

        default:
            return "Major";

    }

}

function getStatusColor(status) {

    switch (status) {

        case 2:
            return "#ff1744";

        case 1:
            return "#00e676";

        default:
            return "#ff9800";

    }

}

// -------------------------------------------------------
// Console
// -------------------------------------------------------

console.log("Map Nodes:", window.mapNodes);

console.log(window.mapNodes);

console.log(
    `Alarm Dashboard Loaded (${window.mapNodes ? window.mapNodes.length : 0} Nodes)`
);