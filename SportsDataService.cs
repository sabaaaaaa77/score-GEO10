using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using SCORE.Data;
using SCORE.Models;
using System.Net.Http;

namespace SCORE.Services
{
    public class SportsDataService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private const string ApiToken = "f55f3556bb0b4edbb66903a32cec93f4";

        public SportsDataService(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task UpdateLiveMatches()
        {
            Console.WriteLine("\n[DEBUG] === მონაცემების განახლება Football-Data.org-დან... ===");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://api.football-data.org/v4/matches"),
                Headers = { { "X-Auth-Token", ApiToken } }
            };

            try
            {
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(body);

                // აქ ვიყენებთ ინდექსატორს ["matches"], რომ ერორი არ ამოაგდოს
                var matches = jsonObject["matches"] as JArray;

                if (matches == null || !matches.Any())
                {
                    Console.WriteLine("[DEBUG] დღეისთვის მატჩები არ მოიძებნა.");
                    return;
                }

                foreach (JToken item in matches)
                {
                    try
                    {
                        string homeName = item["homeTeam"]?["name"]?.ToString() ?? "Unknown";
                        string awayName = item["awayTeam"]?["name"]?.ToString() ?? "Unknown";
                        string leagueName = item["competition"]?["name"]?.ToString() ?? "Other League";

                        // ანგარიშის დამუშავება (Null-ების შემოწმებით)
                        var homeScoreVal = item["score"]?["fullTime"]?["home"];
                        var awayScoreVal = item["score"]?["fullTime"]?["away"];

                        string scoreStr = (homeScoreVal?.Type == JTokenType.Null || homeScoreVal == null)
                                          ? "0 - 0"
                                          : $"{homeScoreVal} - {awayScoreVal}";

                        string statusFromApi = item["status"]?.ToString();
                        MatchStatus status = (statusFromApi == "IN_PLAY" || statusFromApi == "LIVE")
                                             ? MatchStatus.Live
                                             : MatchStatus.Scheduled;

                        // 1. ლიგის მართვა
                        var league = await _context.Leagues.FirstOrDefaultAsync(l => l.Name == leagueName);
                        if (league == null)
                        {
                            league = new League
                            {
                                Name = leagueName,
                                Sport = SportType.Football,
                                Country = item["area"]?["name"]?.ToString() ?? "Europe",
                                LogoUrl = item["competition"]?["emblem"]?.ToString() ?? "https://via.placeholder.com/150"
                            };
                            _context.Leagues.Add(league);
                            await _context.SaveChangesAsync();
                        }

                        // 2. გუნდების მართვა
                        var homeTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Name == homeName);
                        if (homeTeam == null)
                        {
                            homeTeam = new Team { Name = homeName, Sport = SportType.Football, LogoUrl = item["homeTeam"]?["crest"]?.ToString() };
                            _context.Teams.Add(homeTeam);
                        }

                        var awayTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Name == awayName);
                        if (awayTeam == null)
                        {
                            awayTeam = new Team { Name = awayName, Sport = SportType.Football, LogoUrl = item["awayTeam"]?["crest"]?.ToString() };
                            _context.Teams.Add(awayTeam);
                        }
                        await _context.SaveChangesAsync();

                        // 3. მატჩის მართვა
                        var existingMatch = await _context.Matches
                            .FirstOrDefaultAsync(m => m.HomeTeamId == homeTeam.Id && m.AwayTeamId == awayTeam.Id);

                        if (existingMatch == null)
                        {
                            _context.Matches.Add(new Match
                            {
                                LeagueId = league.Id,
                                HomeTeamId = homeTeam.Id,
                                AwayTeamId = awayTeam.Id,
                                Score = scoreStr,
                                Status = status,
                                StartTime = item["utcDate"] != null ? (DateTime)item["utcDate"] : DateTime.UtcNow
                            });
                        }
                        else
                        {
                            existingMatch.Score = scoreStr;
                            existingMatch.Status = status;
                        }

                        await _context.SaveChangesAsync();
                        Console.WriteLine($"[SAVED] {homeName} vs {awayName}");
                    }
                    catch (Exception ex) { Console.WriteLine($"[ITEM ERROR] {ex.Message}"); }
                }

                Console.WriteLine("=== ბაზა წარმატებით განახლდა! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] {ex.Message}");
            }
        }
        public async Task<string> GetErovnuliLigaData()
        {
            var client = new HttpClient();
            // შენი Sportmonks ტოკენი და ეროვნული ლიგის ID (462)
            var url = "https://api.sportmonks.com/v3/football/leagues/462?api_token=126AMSut2Jtkv8DtfeH4vOA2YkfLFwhkP3iPOc0lKFNyfbzrJ11EGEWhoNNv";

            var response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }
}