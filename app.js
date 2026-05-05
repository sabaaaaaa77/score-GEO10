const MATCHES_API = 'http://score-geo.runasp.net/api/Sports/live';

const STANDINGS_API = 'http://score-geo.runasp.net/api/Standings/ucl-live';

const TEAMS_API = 'http://score-geo.runasp.net/api/Teams/search';

const TEAM_DETAILS_BASE = 'http://score-geo.runasp.net/api/Teams/details';



let allMatches = [];

let visibleCount = 9;



// ================= HEADER / BURGER =================

function setupHeader() {

    const burger = document.getElementById('burger');

    const overlay = document.getElementById('mobileOverlay');



    if (!burger || !overlay) return;



    burger.addEventListener('click', () => {

        burger.classList.toggle('open');

        overlay.classList.toggle('active');

        document.body.style.overflow = overlay.classList.contains('active') ? 'hidden' : 'auto';

    });



    document.querySelectorAll('.mobile-nav-item').forEach(link => {

        link.addEventListener('click', () => {

            burger.classList.remove('open');

            overlay.classList.remove('active');

            document.body.style.overflow = 'auto';

        });

    });



    window.addEventListener('scroll', () => {

        const header = document.querySelector('.glass-header');

        if (!header) return;

        if (window.scrollY > 50) {

            header.style.padding = '5px 0';

            header.style.background = 'rgba(0,0,0,0.95)';

        } else {

            header.style.padding = '10px 0';

            header.style.background = 'rgba(10,10,10,0.8)';

        }

    });

}



// ================= SEARCH (MODIFIED) =================

async function handleSearch() {

    // ვიღებთ მნიშვნელობას mainSearch ინფუთიდან

    const searchInput = document.getElementById('mainSearch');

    if (!searchInput) return;

   

    const query = searchInput.value.trim();

    if (!query) return;



    // გვერდების გადართვა

    const footballPage = document.getElementById('football-page');

    const detailsPage = document.getElementById('details-page');

    const container = document.getElementById('details-container');



    if (footballPage) footballPage.style.display = 'none';

    if (detailsPage) detailsPage.style.display = 'block';

   

    container.innerHTML = `<p style="color:#00ff85; text-align:center; padding:20px;">მიმდინარეობს ძებნა: ${query}...</p>`;



    try {

        const response = await fetch(`${TEAMS_API}?name=${encodeURIComponent(query)}`);

        if (!response.ok) throw new Error('გუნდი ვერ მოიძებნა');

       

        const team = await response.json();

        const teamId = team.id || team.Id;



        const geoTime = new Date().toLocaleTimeString('ka-GE', {

            hour: '2-digit',

            minute: '2-digit',

            timeZone: 'Asia/Tbilisi'

        });



        container.innerHTML = `

            <div class="team-profile" style="text-align:center; color:white; padding:20px;">

                <img src="${team.crest || team.logoUrl || 'https://img.icons8.com/ios-filled/100/ffffff/football2.png'}"

                     width="150"

                     onerror="this.src='https://img.icons8.com/ios-filled/100/ffffff/football2.png'">

                <h1 style="margin: 15px 0;">${team.name}</h1>

                <div class="team-info" style="background:rgba(255,255,255,0.05); padding:20px; border-radius:15px; display:inline-block; text-align:left; min-width:280px;">

                    <p style="margin:10px 0;"><strong>🏟️ სტადიონი:</strong> ${team.venue || 'N/A'}</p>

                    <p style="margin:10px 0;"><strong>📅 დაარსდა:</strong> ${team.founded || 'N/A'}</p>

                    <p style="margin:10px 0;"><strong>🎨 ფერები:</strong> ${team.clubColors || 'N/A'}</p>

                    <p style="margin:10px 0;"><strong>🕒 დრო (GEO):</strong> ${geoTime}</p>

                    <p style="margin:10px 0;"><strong>🌐 საიტი:</strong> <a href="${team.website}" target="_blank" style="color:#00ff85;">გახსნა</a></p>

                </div>

                <div style="margin-top: 25px;">

                    <button onclick="goToFullDetails(${teamId})" class="see-more-style">სრული სტატისტიკა</button>

                    <button onclick="closeDetails()" class="see-more-style" style="background:transparent; border:1px solid #444; margin-left:10px;">დახურვა</button>

                </div>

            </div>`;

    } catch (error) {

        container.innerHTML = `

            <div style="text-align:center; padding:40px;">

                <p style="color:#ff4d4d; font-size:1.2rem;">❌ ${error.message}</p>

                <button onclick="closeDetails()" class="see-more-style" style="margin-top:20px;">უკან დაბრუნება</button>

            </div>`;

    }

}



function goToFullDetails(teamId) {

    window.location.href = `team-details.html?id=${teamId}`;

}



function closeDetails() {

    const detailsPage = document.getElementById('details-page');

    const footballPage = document.getElementById('football-page');

    if (detailsPage) detailsPage.style.display = 'none';

    if (footballPage) footballPage.style.display = 'block';

}



// ================= დანარჩენი კოდი (ხელუხლებელი) =================



async function loadMatches() {

    try {

        const response = await fetch(`${MATCHES_API}?t=${new Date().getTime()}`);

        allMatches = await response.json();

        renderMatches();

    } catch (error) {

        console.error("Load Matches Error:", error);

    }

}



function renderMatches() {

    const container = document.getElementById('matches-container');

    if (!container) return;

    container.innerHTML = '';



    const placeholder = 'https://img.icons8.com/ios-filled/100/00e676/football2.png';

    const now = new Date();



    const filteredMatches = allMatches.filter(match => {

        const matchRawDate = match.utcDate || match.date || match.dateTime || match.startTime;

        if (!matchRawDate) return false;

        const matchTime = new Date(matchRawDate.endsWith('Z') ? matchRawDate : matchRawDate + 'Z');

        const diffInMinutes = (now - matchTime) / (1000 * 60);

        if (match.status === 1) return diffInMinutes >= -15 && diffInMinutes <= 130;

        return match.status === 0 && matchTime > now;

    });



    filteredMatches.slice(0, visibleCount).forEach(match => {

        const isLive = match.status === 1;

        const matchRawDate = match.utcDate || match.date || match.dateTime || match.startTime;

        const dateObj = new Date(matchRawDate.endsWith('Z') ? matchRawDate : matchRawDate + 'Z');

        const timeStr = dateObj.toLocaleTimeString('ka-GE', {

            hour: '2-digit', minute: '2-digit', timeZone: 'Asia/Tbilisi', hour12: false

        });



        let displayScore = "0 - 0";

        if (match.score && String(match.score).includes('-')) displayScore = match.score;



        container.innerHTML += `

            <div class="match-card">

                <div class="league-name">${match.league?.name || 'სხვა ლიგა'}</div>

                <div class="teams-area">

                    <div class="team">

                        <img src="${match.homeTeam?.logoUrl || placeholder}" class="team-logo" onerror="this.src='${placeholder}'">

                        <div class="team-name">${match.homeTeam?.name || 'Home'}</div>

                    </div>

                    <div class="score-area">

                        <div class="score-main">${displayScore}</div>

                        <div class="status-badge ${isLive ? 'bg-live' : 'bg-scheduled'}">

                            ${isLive ? '🔴 LIVE' : '🕒 ' + timeStr}

                        </div>

                    </div>

                    <div class="team">

                        <img src="${match.awayTeam?.logoUrl || placeholder}" class="team-logo" onerror="this.src='${placeholder}'">

                        <div class="team-name">${match.awayTeam?.name || 'Away'}</div>

                    </div>

                </div>

            </div>`;

    });

    updateMatchesSeeMoreBtn(filteredMatches.length);

}



function updateMatchesSeeMoreBtn(totalItems) {

    let btn = document.getElementById('see-more-btn');

    const btnWrapper = document.getElementById('btn-wrapper');

    if (!btnWrapper) return;

    if (totalItems > visibleCount) {

        if (!btn) {

            btn = document.createElement('button');

            btn.id = 'see-more-btn';

            btn.innerText = 'მეტის ნახვა';

            btn.className = 'see-more-style';

            btn.onclick = () => { visibleCount += 9; renderMatches(); };

            btnWrapper.appendChild(btn);

        }

    } else { if (btn) btn.remove(); }

}



async function loadStandings(leagueCode = 'PL') {

    const tableBody = document.getElementById('standings-body');

    if (!tableBody) return;



    tableBody.innerHTML = '<tr><td colspan="7">იტვირთება...</td></tr>';



    try {

        // მთავარია აქ: ucl-live უნდა ამოაკლო!

        const response = await fetch(`http://score-geo.runasp.net/api/Standings/${leagueCode}`);

       

        if (!response.ok) throw new Error('ლიგა ვერ მოიძებნა');

       

        const data = await response.json();

       

        // სერვერი ზოგჯერ აბრუნებს პირდაპირ მასივს, ზოგჯერ ობიექტს

        const standings = data.standings ? data.standings[0].table : (Array.isArray(data) ? data : []);



        tableBody.innerHTML = standings.map(item => `

            <tr>

                <td>${item.position || item.rank}</td>

                <td class="team-info">

                    <img src="${item.teamLogo || item.team?.crest}" width="20">

                    <span>${item.teamName || item.team?.name}</span>

                </td>

                <td>${item.played || item.playedGames}</td>

                <td>${item.won}</td>

                <td>${item.draw}</td>

                <td>${item.lost}</td>

                <td><strong>${item.points}</strong></td>

            </tr>`).join('');

    } catch (error) {

        console.error("Standings Error:", error);

        tableBody.innerHTML = '<tr><td colspan="7">მონაცემები არ არის ხელმისაწვდომი</td></tr>';

    }

}

function changeLeague(code) {

    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));

    if (event && event.target) event.target.classList.add('active');

    loadStandings(code);

}



function setupTableUI() {

    const btn = document.getElementById('seeMoreBtn');

    const wrapper = document.getElementById('tableWrapper');

    if (!btn || !wrapper) return;

    btn.addEventListener('click', () => {

        wrapper.classList.toggle('expanded');

        btn.textContent = wrapper.classList.contains('expanded') ? 'SHOW LESS' : 'SEE MORE';

    });

}



const facts = ['F1-ის პილოტი რბოლისას 3-4 კგ-ს იკლებს.', 'პირველი კალათბურთი ატმის კალათებით ჩატარდა.', 'ჩოგბურთის ყველაზე გრძელი მატჩი 11 საათი გაგრძელდა.'];

let factIndex = 0;

function updateFacts() {

    const factDisplay = document.getElementById('fact-display');

    if (factDisplay) {

        factDisplay.innerText = facts[factIndex];

        factIndex = (factIndex + 1) % facts.length;

    }

}



async function getHighlights() {

    const grid = document.getElementById('highlights-grid');

    if (!grid) return;

    try {

        const response = await fetch('https://www.scorebat.com/video-api/v3/');

        const data = await response.json();

        grid.innerHTML = data.response.slice(0, 12).map(match => `

            <div class="video-card">

                <div class="thumbnail-wrapper" onclick="window.open('${match.matchviewUrl}','_blank')">

                    <img src="${match.thumbnail}" alt="${match.title}">

                    <div class="play-button">▶</div>

                </div>

                <div class="video-info">

                    <h3>${match.title}</h3>

                    <p>🏆 ${match.competition}</p>

                    <a href="${match.matchviewUrl}" target="_blank" class="watch-link">სრულად ნახვა →</a>

                </div>

            </div>`).join('');

    } catch (error) { grid.innerHTML = `<div class="loading">ვიდეოები ვერ ჩაიტვირთა 😕</div>`; }

}



document.addEventListener('DOMContentLoaded', () => {

    setupHeader();

    setupTableUI();

    loadMatches();

    loadStandings('PL');

    updateFacts();

    getHighlights();

    setInterval(updateFacts, 10000);

    setInterval(loadMatches, 30000);

});