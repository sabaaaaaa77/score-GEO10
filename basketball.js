async function fetchBasketball() {
    const tbody = document.getElementById('nba-body');
    const header = document.getElementById('league-header');

    try {
        const response = await fetch('https://localhost:7053/api/Standings/nba');
        const data = await response.json();

        // NBA-ს მონაცემებიც standings[0].rows-ში ზის
        if (data.standings && data.standings[0]) {
            const league = data.standings[0];
            header.innerHTML = `<h1>${league.name}</h1>`;

            const rows = league.rows;
            tbody.innerHTML = '';

            rows.forEach(row => {
                tbody.innerHTML += `
                    <tr>
                        <td>${row.position}</td>
                        <td style="text-align: left;">
                            <strong>${row.team.name}</strong>
                        </td>
                        <td>${row.matches}</td>
                        <td style="color: #27ae60;">${row.wins}</td>
                        <td style="color: #e74c3c;">${row.losses}</td>
                        <td>${row.scoreDiffFormatted}</td>
                        <td><strong>${row.points}</strong></td>
                    </tr>
                `;
            });
        }
    } catch (error) {
        console.error("შეცდომა:", error);
    }
}