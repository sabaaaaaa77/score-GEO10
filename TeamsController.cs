using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SCORE
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "f55f3556bb0b4edbb66903a32cec93f4";

        public TeamsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", _apiKey);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchTeam([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("სახელი ცარიელია");

            // ვეძებთ ყველა გუნდს, ფილტრს JS-ში ან აქ გავაკეთებთ უკეთესად
            var response = await _httpClient.GetAsync("https://api.football-data.org/v4/teams?limit=500");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "API Error");

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<FootballDataResponse>(content);

            // ვფილტრავთ იმ გუნდს, რომელიც მომხმარებელმა ჩაწერა
            var team = data.Teams.FirstOrDefault(t =>
                t.Name.ToLower().Contains(name.ToLower()) ||
                (t.ShortName != null && t.ShortName.ToLower().Contains(name.ToLower())));

            if (team == null) return NotFound(new { message = "გუნდი ვერ მოიძებნა" });

            return Ok(team); // ვაბრუნებთ მხოლოდ ერთ კონკრეტულ გუნდს!
        }
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetTeamDetails(int id)
        {
            // API-ს მივმართავთ კონკრეტული ID-ით
            var response = await _httpClient.GetAsync($"https://api.football-data.org/v4/teams/{id}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "გუნდის დეტალები ვერ მოიძებნა");

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    } 


    // დამხმარე კლასები JSON-ისთვის
    public class FootballDataResponse { public List<TeamInfo> Teams { get; set; } }
    public class TeamInfo
    {
        public int Id { get; set; } // დაამატე ეს ხაზი აუცილებლად!
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Crest { get; set; }
        public string Venue { get; set; }
        public int? Founded { get; set; }
        public string Website { get; set; }
        public string ClubColors { get; set; }
    }
}
