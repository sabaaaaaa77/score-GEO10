// ================= CONFIG =================
const MATCHES_API = 'http://score-geo.runasp.net/api/Sports/live';
const STANDINGS_API = 'http://score-geo.runasp.net/api/Standings/ucl-live'; // UCL-ის ცხრილისთვის ეს გამოვიყენოთ
const SCORERS_API = 'http://score-geo.runasp.net/api/Standings/Scorers';

let allMatches = [];
let liveVisibleCount = 6;
let finishedVisibleCount = 6;

// ================= LOAD MATCHES =================
async function loadMatches() {
    try {
        const response = await fetch(MATCHES_API + '?nocache=' + Date.now(), {
            cache: "no-store"
        });
        allMatches = await response.json();
        renderFootballPage();
    } catch (e) {
        console.error("Matches Load Error:", e);
    }
}

// ================= RENDER =================
function renderFootballPage() {
    const liveList = document.getElementById('fb-live-list');
    const schedList = document.getElementById('fb-schedule-list');
    const finishedList = document.getElementById('fb-finished-list');

    if (!liveList || !schedList) return;

    const now = new Date();

    // --- LIVE ---
    const liveMatches = allMatches.filter(m => {
        const raw = m.utcDate || m.date || m.dateTime || m.startTime;
        if (!raw) return false;

        const matchTime = new Date(raw.endsWith('Z') ? raw : raw + 'Z');
        const diff = (now - matchTime) / 60000;

        return m.status === 1 && diff >= -15 && diff <= 130;
    });

    // --- SCHEDULE ---
    const scheduledMatches = allMatches.filter(m => {
        const raw = m.utcDate || m.date || m.dateTime || m.startTime;
        if (!raw) return false;

        const matchTime = new Date(raw.endsWith('Z') ? raw : raw + 'Z');
        return m.status === 0 && matchTime > now;
    });

    // --- FINISHED ---
    const finishedMatches = allMatches.filter(m => {
        const raw = m.utcDate || m.date || m.dateTime || m.startTime;
        if (!raw) return false;

        const matchTime = new Date(raw.endsWith('Z') ? raw : raw + 'Z');
        const diff = (now - matchTime) / 60000;

        return (m.status !== 1 && m.status !== 0) || diff > 130;
    });

    // --- RENDER LIVE ---
    const visibleLive = liveMatches.slice(0, liveVisibleCount);
    liveList.innerHTML = visibleLive.length
        ? visibleLive.map(m => createMatchHTML(m, true)).join('')
        : `<div style="text-align:center;color:#888;padding:20px;">ლაივი არ არის</div>`;

    handleSeeMoreButton('live-btn-wrapper', liveMatches.length, liveVisibleCount, () => {
        liveVisibleCount += 6;
        renderFootballPage();
    });

    // --- RENDER SCHEDULE ---
    schedList.innerHTML = scheduledMatches.length
        ? scheduledMatches.slice(0, 15).map(m => createMatchHTML(m)).join('')
        : `<div style="text-align:center;color:#888;padding:20px;">მატჩები არ არის</div>`;

    // --- RENDER FINISHED ---
    if (finishedList) {
        const visibleFinished = finishedMatches.slice(0, finishedVisibleCount);

        finishedList.innerHTML = visibleFinished.length
            ? visibleFinished.map(m => createMatchHTML(m, false, true)).join('')
            : `<div style="text-align:center;color:#888;padding:20px;">შედეგები არ არის</div>`;

        handleSeeMoreButton('finished-btn-wrapper', finishedMatches.length, finishedVisibleCount, () => {
            finishedVisibleCount += 6;
            renderFootballPage();
        });
    }
}

// ================= MATCH CARD =================
function createMatchHTML(match, isLive = false, isFinished = false) {
    const placeholder = 'https://img.icons8.com/ios-filled/100/ffffff/football2.png';

    const raw = match.utcDate || match.date || match.dateTime || match.startTime;
    const date = new Date(raw?.endsWith('Z') ? raw : raw + 'Z');

    let statusText = '';

    if (isLive) {
        const min = match.minute || match.elapsed || '';
        statusText = `<span class="live-tag">🔴 ${min ? min + "'" : 'LIVE'}</span>`;
    } else if (isFinished) {
        statusText = `<span class="finished-tag">🏁 FT</span>`;
    } else {
        const time = isNaN(date)
            ? '--:--'
            : date.toLocaleTimeString('ka-GE', {
                  hour: '2-digit',
                  minute: '2-digit',
                  hour12: false,
                  timeZone: 'Asia/Tbilisi'
              });

        statusText = `<span>🕒 ${time}</span>`;
    }

    // ✅ ძლიერი score parsing
    let hScore = match.homeScore ?? match.score?.home ?? 0;
    let aScore = match.awayScore ?? match.score?.away ?? 0;

    if (typeof match.score === 'string' && match.score.includes('-')) {
        const [h, a] = match.score.split('-');
        hScore = h.trim();
        aScore = a.trim();
    }

    return `
    <div class="fb-match-card ${isLive ? 'is-live' : ''}">
        <div class="status-bar">
            <span>${match.league?.name || 'League'}</span>
            ${statusText}
        </div>

        <div class="team-row">
            <div class="team-info">
                <img src="${match.homeTeam?.logoUrl || placeholder}" onerror="this.src='${placeholder}'">
                <span>${match.homeTeam?.name || 'Team 1'}</span>
            </div>
            <div class="score-box">${hScore}</div>
        </div>

        <div class="team-row">
            <div class="team-info">
                <img src="${match.awayTeam?.logoUrl || placeholder}" onerror="this.src='${placeholder}'">
                <span>${match.awayTeam?.name || 'Team 2'}</span>
            </div>
            <div class="score-box">${aScore}</div>
        </div>
    </div>`;
}

// ================= SEE MORE =================
function handleSeeMoreButton(id, total, current, action) {
    const wrapper = document.getElementById(id);
    if (!wrapper) return;

    wrapper.innerHTML = '';

    if (total > current) {
        const btn = document.createElement('button');
        btn.innerText = 'SEE MORE';
        btn.className = 'see-more-style';
        btn.onclick = action;
        wrapper.appendChild(btn);
    }
}

// ================= STANDINGS =================
async function loadStandings(code) {
    const table = document.getElementById('standings-body');
    if (!table) return;

    try {
        const res = await fetch(`${STANDINGS_API}/${code}`);
        const data = await res.json();

        table.innerHTML = data.map(t => `
        <tr>
            <td>${t.position}</td>
            <td>${t.teamName}</td>
            <td>${t.played}</td>
            <td>${t.won}</td>
            <td>${t.draw}</td>
            <td>${t.lost}</td>
            <td>${t.points}</td>
        </tr>`).join('');

    } catch {
        table.innerHTML = `<tr><td colspan="7">No data</td></tr>`;
    }
}

// ================= SCORERS =================
async function loadTopScorers(code) {
    const list = document.getElementById('scorers-list');
    if (!list) return;

    try {
        const res = await fetch(`${SCORERS_API}/${code}`);
        const data = await res.json();

        list.innerHTML = data.slice(0, 5).map((p, i) => `
        <div class="scorer-item">
            <span>${i + 1}. ${p.playerName}</span>
            <strong>${p.goals} ⚽</strong>
        </div>`).join('');

    } catch (e) {
        console.error(e);
    }
}

// ================= INIT =================
document.addEventListener('DOMContentLoaded', () => {
    loadMatches();
    loadStandings('PL');
    loadTopScorers('PL');

    // ✅ მხოლოდ ერთი ინტერვალი
    setInterval(loadMatches, 3000);
});