using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SCORE.Data;
using SCORE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SCORE.Services
{
    // ინტერფეისი
    public interface IStandingsService
    {
        Task<UclResponse> GetUclStandingsAsync();
        Task<string> GetBasketballEventsAsync(string date);
        Task<object> GetF1StandingsAsync();
        Task<object> GetUfcFighterStatsAsync(string playerName);
        Task<List<object>> GetTopScorersAsync(string leagueCode);
        Task UpdateStandingsAsync(string leagueCode);
        Task<string?> GetErovnuliLigaStandingsAsync();
    }

    // DTO
    public class UclResponse
    {
        public CompetitionInfo Competition { get; set; }
        public SeasonInfo Season { get; set; }
        public List<UclStandingGroup> Standings { get; set; }
    }

    public class CompetitionInfo
    {
        public string Name { get; set; }
        public string Emblem { get; set; }
    }

    public class SeasonInfo
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UclStandingGroup
    {
        public List<UclTableItem> Table { get; set; }
    }

    public class UclTableItem
    {
        public int Position { get; set; }
        public UclTeam Team { get; set; }
        public int PlayedGames { get; set; }
        public int Won { get; set; }
        public int Draw { get; set; }
        public int Lost { get; set; }
        public int Points { get; set; }
    }

    public class UclTeam
    {
        public string Name { get; set; }
        public string Crest { get; set; }
    }

    // SERVICE
    public class StandingsService : IStandingsService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string FootballToken = "f55f3556bb0b4edbb66903a32cec93f4";
        private const string RapidApiKey = "67cdb46072msh0270b20e95e0e05p170669jsnad6ab9317fda";

        public StandingsService(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // ჩემპიონთა ლიგის standings
        public async Task<UclResponse> GetUclStandingsAsync()
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("X-Auth-Token", FootballToken);

            var response = await client.GetAsync(
                "https://api.football-data.org/v4/competitions/CL/standings"
            );

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<UclResponse>(jsonString);
            }

            return null;
        }

        // კალათბურთი
        public async Task<string> GetBasketballEventsAsync(string date)
        {
            var client = _httpClientFactory.CreateClient();

            string formattedDate;

            if (DateTime.TryParseExact(
                date,
                new[] { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime parsedDate))
            {
                formattedDate = parsedDate.ToString("yyyy-MM-dd");
            }
            else
            {
                formattedDate = DateTime.Now.ToString("yyyy-MM-dd");
            }

            var url = $"https://api.balldontlie.io/v1/games?dates[]={formattedDate}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add(
                "Authorization",
                "6cf096d7-daf3-4482-922d-015fa58355cd"
            );

            try
            {
                var response = await client.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return content;
                }

                return $"{{\"error\": \"API Error\", \"details\": {content}}}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        // F1
        public async Task<object> GetF1StandingsAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    "https://f1-data.p.rapidapi.com/standings/drivers"
                ),
                Headers =
                {
                    { "x-rapidapi-key", RapidApiKey },
                    { "x-rapidapi-host", "f1-data.p.rapidapi.com" },
                }
            };

            using var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject(body);
        }

        // UFC
        public async Task<object> GetUfcFighterStatsAsync(string playerName)
        {
            var client = _httpClientFactory.CreateClient();

            var today = DateTime.Now;

            var url =
                $"https://mmaapi.p.rapidapi.com/api/mma/unique-tournament/19906/schedules/{today.Day}/{today.Month}/{today.Year}";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Headers =
                {
                    { "x-rapidapi-key", RapidApiKey },
                    { "x-rapidapi-host", "mmaapi.p.rapidapi.com" },
                }
            };

            using var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject(body);
        }

        // ბომბარდირები
        public async Task<List<object>> GetTopScorersAsync(string leagueCode)
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("X-Auth-Token", FootballToken);

            var response = await client.GetAsync(
                $"https://api.football-data.org/v4/competitions/{leagueCode}/scorers"
            );

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var data = JObject.Parse(json);

                var scorersList = new List<object>();

                if (data["scorers"] != null)
                {
                    foreach (var item in data["scorers"])
                    {
                        scorersList.Add(new
                        {
                            PlayerName = (string)item["player"]["name"],
                            TeamName = (string)item["team"]["name"],
                            TeamLogo = (string)item["team"]["crest"],
                            Goals = (int)item["goals"]
                        });
                    }
                }

                return scorersList;
            }

            return new List<object>();
        }

        // ბაზის განახლება
        public async Task UpdateStandingsAsync(string leagueCode)
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("X-Auth-Token", FootballToken);

            var response = await client.GetAsync(
                $"https://api.football-data.org/v4/competitions/{leagueCode}/standings"
            );

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var data = JObject.Parse(json);

                int leagueId = (int)data["competition"]["id"];
                string leagueName = (string)data["competition"]["name"];

                var oldData = _context.Standings.Where(s => s.LeagueId == leagueId);

                _context.Standings.RemoveRange(oldData);

                var table = data["standings"]?[0]?["table"];

                if (table != null)
                {
                    foreach (var row in table)
                    {
                        _context.Standings.Add(new Standing
                        {
                            LeagueId = leagueId,
                            LeagueName = leagueName,
                            Position = (int)row["position"],
                            TeamName = (string)row["team"]["name"],
                            TeamLogo = (string)row["team"]["crest"],
                            Played = (int)row["playedGames"],
                            Won = (int)row["won"],
                            Draw = (int)row["draw"],
                            Lost = (int)row["lost"],
                            Points = (int)row["points"]
                        });
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }

        // ეროვნული ლიგა
        public async Task<string?> GetErovnuliLigaStandingsAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    "https://api-football-v1.p.rapidapi.com/v3/standings?league=335&season=2024"
                ),
                Headers =
                {
                    { "x-rapidapi-key", RapidApiKey },
                    { "x-rapidapi-host", "api-football-v1.p.rapidapi.com" },
                }
            };

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return $"{{\"error\":\"{response.StatusCode}\"}}";
        }
    }
}