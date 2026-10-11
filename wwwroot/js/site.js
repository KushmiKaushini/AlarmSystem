// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
	const storageKey = "alarm.sidebarCollapsed";
	const viewDataCachePrefix = "alarm.viewdata.cache.v1";
	const viewDataCacheTtlMs = 2 * 60 * 1000;
	const viewDataPrefetchInFlight = new Map();

	function buildViewDataCacheKey(sectionKey, search = "", top = "200") {
		const normalizedSection = String(sectionKey ?? "").trim().toLowerCase();
		const normalizedSearch = String(search ?? "").trim().toLowerCase();
		const normalizedTop = String(top ?? "200").trim() || "200";
		return `${viewDataCachePrefix}:${normalizedSection}:${normalizedSearch}:${normalizedTop}`;
	}

	function readViewDataCache(sectionKey, search = "", top = "200") {
		try {
			const key = buildViewDataCacheKey(sectionKey, search, top);
			const raw = sessionStorage.getItem(key);
			if (!raw) return null;

			const parsed = JSON.parse(raw);
			if (!parsed || typeof parsed !== "object") return null;
			if (Date.now() - Number(parsed.cachedAt ?? 0) > viewDataCacheTtlMs) return null;

			return parsed.payload ?? null;
		} catch {
			return null;
		}
	}

	function writeViewDataCache(sectionKey, payload, search = "", top = "200") {
		try {
			const key = buildViewDataCacheKey(sectionKey, search, top);
			sessionStorage.setItem(key, JSON.stringify({
				cachedAt: Date.now(),
				payload
			}));
		} catch {
			// Ignore storage quota/private mode errors.
		}
	}

	function getApiRootBaseUrl() {
		const root = typeof window.appApiBase === "string" ? window.appApiBase : "/api";
		return root.replace(/\/+$/, "");
	}

	async function prefetchViewDataSection(sectionKey, search = "", top = "200") {
		const normalizedSection = String(sectionKey ?? "").trim().toLowerCase();
		if (!normalizedSection) return;

		const existing = readViewDataCache(normalizedSection, search, top);
		if (existing) return;

		const requestKey = `${normalizedSection}|${String(search ?? "").trim().toLowerCase()}|${String(top ?? "200").trim() || "200"}`;
		if (viewDataPrefetchInFlight.has(requestKey)) return;

		const params = new URLSearchParams();
		if (search) params.set("search", search);
		if (top) params.set("top", top);
		const baseUrl = `${getApiRootBaseUrl()}/viewdata/section/${encodeURIComponent(normalizedSection)}`;
		console.log("API BASE:", baseUrl);

		const query = params.toString();
		const url = query ? `${baseUrl}?${query}` : baseUrl;

		const inFlight = (async () => {
			try {
				const res = await fetch(url, {
					cache: "no-store",
					headers: { Accept: "application/json" }
				});
				if (!res.ok) return;

				const payload = await res.json();
				writeViewDataCache(normalizedSection, payload, search, top);
			} catch {
				// Prefetch failures are non-blocking.
			}
		})();

		viewDataPrefetchInFlight.set(requestKey, inFlight);
		try {
			await inFlight;
		} finally {
			viewDataPrefetchInFlight.delete(requestKey);
		}
	}

	document.addEventListener("DOMContentLoaded", () => {
		const toggleButton = document.getElementById("sideMenuToggle");
		const sideMenu = document.getElementById("mainSideMenu");
		const body = document.body;
		const menuToggleButtons = Array.from(document.querySelectorAll("[data-menu-toggle]"));
		const viewDataPrefetchLinks = Array.from(document.querySelectorAll("[data-section-key], [data-viewdata-section-key]"));

		function initializeViewDataPrefetch() {
			const discoveredKeys = new Set();

			viewDataPrefetchLinks.forEach((link) => {
				const sectionKey = link.getAttribute("data-section-key") || link.getAttribute("data-viewdata-section-key");
				if (!sectionKey) return;
				discoveredKeys.add(sectionKey);

				const triggerPrefetch = () => {
					void prefetchViewDataSection(sectionKey, "", "200");
				};

				link.addEventListener("pointerenter", triggerPrefetch, { passive: true });
				link.addEventListener("focusin", triggerPrefetch, { passive: true });
				link.addEventListener("mousedown", triggerPrefetch, { passive: true });
			});

			let delayMs = 80;
			discoveredKeys.forEach((key) => {
				window.setTimeout(() => {
					void prefetchViewDataSection(key, "", "200");
				}, delayMs);
				delayMs += 120;
			});
		}

		initializeViewDataPrefetch();

		function initializeSubmenus() {
			menuToggleButtons.forEach((button) => {
				const targetId = button.getAttribute("data-menu-toggle");
				if (!targetId) return;

				const submenu = document.getElementById(targetId);
				if (!submenu) return;

				const targetStorageKey = `alarm.submenu.${targetId}`;
				const group = button.closest(".side-link-group");
				const hasActiveLink = Boolean(group?.querySelector(".side-link.active, .side-sublink.active"));

				function applyExpandedState(expanded, persist = true) {
					submenu.classList.toggle("open", expanded);
					button.classList.toggle("expanded", expanded);
					button.setAttribute("aria-expanded", String(expanded));

					if (!persist) return;
					try {
						localStorage.setItem(targetStorageKey, expanded ? "1" : "0");
					} catch {
						// Ignore storage access errors.
					}
				}

				let expanded = submenu.classList.contains("open") || hasActiveLink;
				try {
					const stored = localStorage.getItem(targetStorageKey);
					if (stored !== null) expanded = stored === "1" || hasActiveLink;
				} catch {
					expanded = submenu.classList.contains("open") || hasActiveLink;
				}

				applyExpandedState(expanded, false);

				button.addEventListener("click", () => {
					const nextExpanded = !submenu.classList.contains("open");
					applyExpandedState(nextExpanded, true);
				});
			});
		}

		if (body.classList.contains("has-sidebar")) {
			initializeSubmenus();
		}

		if (!toggleButton || !sideMenu || !body.classList.contains("has-sidebar")) return;

		const desktopQuery = window.matchMedia("(min-width: 993px)");

		function setToggleUi(collapsed) {
			const label = collapsed ? "Show menu" : "Hide menu";
			toggleButton.setAttribute("aria-label", label);
			toggleButton.setAttribute("title", label);
			toggleButton.classList.toggle("is-collapsed", collapsed);
			toggleButton.setAttribute("aria-expanded", String(!collapsed));
		}

		function applyState(collapsed, persist = true) {
			const shouldCollapse = desktopQuery.matches && collapsed;
			body.classList.toggle("sidebar-collapsed", shouldCollapse);
			setToggleUi(shouldCollapse);

			if (!persist) return;
			try {
				localStorage.setItem(storageKey, shouldCollapse ? "1" : "0");
			} catch {
				// Ignore storage access errors (private mode, blocked storage, etc).
			}
		}

		let savedCollapsed = true;
		try {
			const stored = localStorage.getItem(storageKey);
			savedCollapsed = stored === null ? true : stored === "1";
		} catch {
			savedCollapsed = true;
		}

		applyState(savedCollapsed, false);

		toggleButton.addEventListener("click", () => {
			const nextCollapsed = !body.classList.contains("sidebar-collapsed");
			applyState(nextCollapsed, true);
		});

		desktopQuery.addEventListener("change", () => {
			if (!desktopQuery.matches) {
				body.classList.remove("sidebar-collapsed");
				setToggleUi(false);
				return;
			}

			let collapsed = false;
			try {
				collapsed = localStorage.getItem(storageKey) === "1";
			} catch {
				collapsed = false;
			}

			applyState(collapsed, false);
		});
	});
})();
