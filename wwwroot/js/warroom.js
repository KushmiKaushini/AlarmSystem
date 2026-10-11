/* ============================================
   WAR ROOM DASHBOARD
============================================ */

document.addEventListener("DOMContentLoaded", function () {

    startClock();

    autoRefresh();

    initializeSearch();

    animateCards();

    animateCounters();

    initializeTooltips();

});

/* ============================================
   LIVE CLOCK
============================================ */

function startClock() {

    function updateClock() {

        const now = new Date();

        const options = {
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
            hour12: true
        };

        const clock = document.getElementById("clock");

        if (clock)
            clock.innerHTML =
                now.toLocaleTimeString([], options);

    }

    updateClock();

    setInterval(updateClock, 1000);

}

/* ============================================
   AUTO REFRESH
============================================ */

function autoRefresh() {

    setInterval(function () {

        showLoading();

        setTimeout(function () {

            location.reload();

        }, 800);

    }, 120000);

}

/* ============================================
   SEARCH TABLE
============================================ */

function initializeSearch() {

    const txt =
        document.getElementById("tableSearch");

    if (!txt)
        return;

    txt.addEventListener("keyup", function () {

        const filter =
            this.value.toLowerCase();

        const rows =
            document.querySelectorAll("#faultTable tbody tr");

        rows.forEach(function (row) {

            const value =
                row.innerText.toLowerCase();

            row.style.display =
                value.indexOf(filter) > -1
                    ? ""
                    : "none";

        });

    });

}

/* ============================================
   LOADING OVERLAY
============================================ */

function showLoading() {

    if (document.getElementById("loadingOverlay"))
        return;

    const overlay =
        document.createElement("div");

    overlay.id = "loadingOverlay";

    overlay.innerHTML =

        '<div class="spinner-border text-light" style="width:4rem;height:4rem"></div>' +

        '<h4 style="margin-top:20px;color:white;">Refreshing Dashboard...</h4>';

    overlay.style.position = "fixed";
    overlay.style.left = "0";
    overlay.style.top = "0";
    overlay.style.width = "100%";
    overlay.style.height = "100%";
    overlay.style.background = "rgba(0,0,0,.65)";
    overlay.style.zIndex = "99999";
    overlay.style.display = "flex";
    overlay.style.flexDirection = "column";
    overlay.style.justifyContent = "center";
    overlay.style.alignItems = "center";

    document.body.appendChild(overlay);

}

/* ============================================
   CARD HOVER EFFECT
============================================ */

function animateCards() {

    const cards =
        document.querySelectorAll(".dashboard-card,.operator-card");

    cards.forEach(function (card) {

        card.addEventListener("mouseenter", function () {

            this.style.transform =
                "translateY(-8px) scale(1.02)";

        });

        card.addEventListener("mouseleave", function () {

            this.style.transform =
                "translateY(0px) scale(1)";

        });

    });

}

/* ============================================
   COUNTER ANIMATION
============================================ */

function animateCounters() {

    const counters =
        document.querySelectorAll(".dashboard-card h2,.operator-card h1");

    counters.forEach(function (counter) {

        let target =
            parseInt(counter.innerText);

        if (isNaN(target))
            return;

        let value = 0;

        const speed = Math.max(1, Math.floor(target / 50));

        const timer = setInterval(function () {

            value += speed;

            if (value >= target) {

                value = target;

                clearInterval(timer);

            }

            counter.innerText = value;

        }, 20);

    });

}

/* ============================================
   TABLE ROW HIGHLIGHT
============================================ */

document.addEventListener("mouseover", function (e) {

    const row = e.target.closest("tbody tr");

    if (!row)
        return;

    row.style.background =
        "#E3F2FD";

});

document.addEventListener("mouseout", function (e) {

    const row = e.target.closest("tbody tr");

    if (!row)
        return;

    row.style.background = "";

});

/* ============================================
   BUTTON RIPPLE EFFECT
============================================ */

document.querySelectorAll(".btn").forEach(function (button) {

    button.addEventListener("click", function () {

        this.style.transform =
            "scale(.95)";

        setTimeout(() => {

            this.style.transform =
                "scale(1)";

        }, 150);

    });

});

/* ============================================
   TOOLTIPS
============================================ */

function initializeTooltips() {

    if (typeof bootstrap === "undefined")
        return;

    document
        .querySelectorAll('[data-bs-toggle="tooltip"]')
        .forEach(function (element) {

            new bootstrap.Tooltip(element);

        });

}

/* ============================================
   SCROLL TO TOP
============================================ */

const scrollButton =
    document.createElement("button");

scrollButton.innerHTML =
    '<i class="fa fa-arrow-up"></i>';

scrollButton.className =
    "btn btn-primary";

scrollButton.style.position = "fixed";
scrollButton.style.bottom = "30px";
scrollButton.style.right = "30px";
scrollButton.style.display = "none";
scrollButton.style.zIndex = "999";

document.body.appendChild(scrollButton);

window.addEventListener("scroll", function () {

    scrollButton.style.display =
        window.scrollY > 300
            ? "block"
            : "none";

});

scrollButton.onclick = function () {

    window.scrollTo({

        top: 0,

        behavior: "smooth"

    });

};

/* ============================================
   REFRESH BUTTON
============================================ */

function refreshDashboard() {

    showLoading();

    setTimeout(function () {

        location.reload();

    }, 600);

}

/* ============================================
   EXPORT SUCCESS MESSAGE
============================================ */

function exportStarted() {

    console.log("Export Started");

}

/* ============================================
   PAGE LOAD ANIMATION
============================================ */

window.onload = function () {

    document.body.style.opacity = 0;

    document.body.style.transition =
        ".4s";

    setTimeout(function () {

        document.body.style.opacity = 1;

    }, 50);

};