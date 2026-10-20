"use strict";

const $ = id => document.getElementById(id);
const REGISTERS = [["IAR", "iar"], ["ACC", "acc"], ["EXT", "ext"], ["XR1", "xr1"], ["XR2", "xr2"], ["XR3", "xr3"]];
const FLAGS = [["RUN", s => s.running && !s.wait], ["WAIT", s => s.wait], ["CARRY", s => s.carry], ["OVFL", s => s.overflow], ["DISK", s => s.diskMounted]];

const lamps = {};
let printed = "";          // raw printer text received so far
let printNext = 0;         // position to ask the server for next

function lamp() {
	const el = document.createElement("span");
	el.className = "lamp";
	return el;
}

function indicator(label) {
	const wrap = document.createElement("span");
	wrap.className = "indicator";
	const l = lamp();
	wrap.append(l, document.createTextNode(label));
	return [wrap, l];
}

function buildPanel() {
	for (const [label, key] of REGISTERS) {
		const row = document.createElement("div");
		row.className = "register";
		const name = document.createElement("span");
		name.className = "name";
		name.textContent = label;
		const bits = document.createElement("span");
		bits.className = "lamps";
		const list = [];
		for (let i = 0; i < 16; i++) { const l = lamp(); bits.append(l); list.push(l); }
		const hex = document.createElement("span");
		hex.className = "hex";
		row.append(name, bits, hex);
		$("registers").append(row);
		lamps[key] = { bits: list, hex };
	}
	lamps.levels = [];
	for (let i = 0; i < 6; i++) {
		const [el, l] = indicator(`L${i}`);
		$("levels").append(el);
		lamps.levels.push(l);
	}
	lamps.flags = FLAGS.map(([label, test]) => {
		const [el, l] = indicator(label);
		$("flags").append(el);
		return { l, test };
	});
}

function showSnapshot(s) {
	for (const [, key] of REGISTERS) {
		const value = s[key];
		const { bits, hex } = lamps[key];
		for (let i = 0; i < 16; i++) bits[i].classList.toggle("on", (value & (0x8000 >> i)) !== 0);
		hex.textContent = value.toString(16).toUpperCase().padStart(4, "0");
	}
	lamps.levels.forEach((l, i) => l.classList.toggle("on", s.activeLevels.includes(i)));
	lamps.flags.forEach(({ l, test }) => l.classList.toggle("on", !!test(s)));

	$("status").textContent = !s.poweredOn ? "Powered off" : s.running ? (s.wait ? "Waiting" : "Running") : "Stopped";
	$("hopper").textContent = s.hopperCount;
	$("count").textContent = s.instructionCount.toLocaleString();
	$("start").disabled = !s.poweredOn;
	$("stop").disabled = !s.poweredOn;
	$("reset").disabled = !s.poweredOn;
	$("error").hidden = !s.error;
	$("error").textContent = s.error || "";
}

// 1403 text uses LF between lines, FF for a new page and CR between overprinted rows.
function renderPaper(text) {
	const paper = $("paper");
	const atBottom = paper.scrollTop + paper.clientHeight >= paper.scrollHeight - 4;
	paper.replaceChildren();
	text.split("\f").forEach((page, i) => {
		if (i > 0) {
			const rule = document.createElement("span");
			rule.className = "page";
			paper.append(rule);
		}
		paper.append(document.createTextNode(page.split("\n").map(overprint).join("\n")));
	});
	if (atBottom) paper.scrollTop = paper.scrollHeight;
}

function overprint(line) {
	if (!line.includes("\r")) return line;
	const out = [];
	for (const row of line.split("\r")) {
		[...row].forEach((c, i) => { if (c !== " " || out[i] === undefined) out[i] = c; });
	}
	return out.join("");
}

async function call(method, url, body) {
	const response = await fetch(url, { method, body, headers: body ? { "Content-Type": "text/plain" } : {} });
	if (!response.ok) {
		const problem = await response.json().catch(() => ({}));
		throw new Error(problem.detail || problem.title || `${response.status} ${response.statusText}`);
	}
	return response.status === 204 ? null : response.json();
}

function report(error) {
	$("error").hidden = false;
	$("error").textContent = error.message;
}

async function pollMachine() {
	try { showSnapshot(await call("GET", "/api/machine")); } catch { /* server unreachable: try again */ }
	setTimeout(pollMachine, 200);
}

async function pollPrinter() {
	try {
		const chunk = await call("GET", `/api/machine/printout?since=${printNext}`);
		if (chunk.start !== printNext) printed = "";   // torn off elsewhere
		if (chunk.text || chunk.start !== printNext) {
			printed += chunk.text;
			renderPaper(printed);
		}
		printNext = chunk.next;
	} catch { /* try again */ }
	setTimeout(pollPrinter, 500);
}

function key(id, method, url, body) {
	$(id).addEventListener("click", async () => {
		try {
			const result = await call(method, url, typeof body === "function" ? body() : body);
			if (result && "poweredOn" in result) showSnapshot(result);
		} catch (e) { report(e); }
	});
}

buildPanel();
key("boot", "POST", "/api/machine/boot");
key("start", "POST", "/api/machine/start");
key("stop", "POST", "/api/machine/stop");
key("reset", "POST", "/api/machine/reset");
key("load", "POST", "/api/machine/deck", () => $("deck").value);

$("deckfile").addEventListener("change", async e => {
	const file = e.target.files[0];
	if (file) $("deck").value = await file.text();
	e.target.value = "";
});

$("tearoff").addEventListener("click", async () => {
	try {
		await call("DELETE", "/api/machine/printout");
		printed = "";
		renderPaper("");
	} catch (e) { report(e); }
});

$("download").addEventListener("click", () => {
	const link = document.createElement("a");
	link.href = URL.createObjectURL(new Blob([printed], { type: "text/plain" }));
	link.download = "printout.txt";
	link.click();
	URL.revokeObjectURL(link.href);
});

pollMachine();
pollPrinter();
