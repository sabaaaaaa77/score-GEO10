using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCORE.Data;
using SCORE.Models;
using SCORE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCORE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StandingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IStandingsService _service;

        public StandingsController(AppDbContext context, IStandingsService service)
        {
            _context = context;
            _service = service;
        }

        // --- 1. ფეხბურთის ლიგების ცხრილი (Standings) ---
        // GET: api/Standings/PL
        [HttpGet("{code}")]
        public async Task<IActionResult> GetStandings(string code)
        {
            try
            {
                int leagueId = GetLeagueId(code);
                if (leagueId == 0) return BadRequest("არასწორი ლიგის კოდი");

                // ვეძებთ მონაცემებს ჩვენს ბაზაში
                var standings = await _context.Standings
                    .Where(s => s.LeagueId == leagueId)
                    .OrderBy(s => s.Position)
                    .ToListAsync();

                // თუ ბაზა ცარიელია, ავტომატურად ვანახლებთ სერვისიდან
                if (standings == null || !standings.Any())
                {
                    await _service.UpdateStandingsAsync(code);

                    // განახლების შემდეგ ხელახლა ვკითხულობთ ბაზიდან
                    standings = await _context.Standings
                        .Where(s => s.LeagueId == leagueId)
                        .OrderBy(s => s.Position)
                        .ToListAsync();
                }

                return Ok(standings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "შეცდომა ცხრილის წამოღებისას", details = ex.Message });
            }
        }

        // --- 2. ბომბარდირები (Scorers) ---
        // GET: api/Standings/Scorers/PL
        [HttpGet("Scorers/{code}")]
        public async Task<IActionResult> GetScorers(string code)
        {
            try
            {
                // ბომბარდირებს პირდაპირ სერვისიდან ვიღებთ (როგორც შენს წინა კოდში იყო)
                var scorers = await _service.GetTopScorersAsync(code);
                if (scorers == null) return NotFound("ბომბარდირების მონაცემები ვერ მოიძებნა");

                return Ok(scorers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "შეცდომა ბომბარდირების წამოღებისას", details = ex.Message });
            }
        }

        // --- 3. ჩემპიონთა ლიგა ---
        [HttpGet("ucl")]
        public async Task<IActionResult> GetUclStandings()
        {
            try
            {
                var result = await _service.GetUclStandingsAsync();
                if (result == null) return BadRequest("მონაცემები ცარიელია ან API ლიმიტი ამოიწურა");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 4. კალათბურთი ---
        [HttpGet("basketball")]
        public async Task<IActionResult> GetBasketball([FromQuery] string date = null)
        {
            string queryDate = string.IsNullOrEmpty(date) ? DateTime.Now.ToString("dd/MM/yyyy") : date;

            try
            {
                var data = await _service.GetBasketballEventsAsync(queryDate);
                if (string.IsNullOrEmpty(data)) return NotFound("მონაცემები ვერ მოიძებნა");
                return Content(data, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // --- 5. მონაცემების იძულებითი განახლება ბაზაში ---
        [HttpPost("update/{code}")]
        public async Task<IActionResult> UpdateLeague(string code)
        {
            try
            {
                await _service.UpdateStandingsAsync(code);
                return Ok(new { message = $"{code} წარმატებით განახლდა" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- დამხმარე მეთოდი ლიგის ID-ებისთვის ---
        private int GetLeagueId(string code)
        {
            return code.ToUpper() switch
            {
                "PL" => 2021,
                "PD" => 2014,
                "SA" => 2019,
                "BL1" => 2002,
                "FL1" => 2015,
                "CL" => 2001,
                _ => 0
            };
        }

        // --- 6. საქართველოს ეროვნული ლიგა ---
        [HttpGet("erovnuli-liga")]
        public async Task<IActionResult> GetErovnuliLiga()
        {
            try
            {
                var data = await _service.GetErovnuliLigaStandingsAsync();
                if (string.IsNullOrEmpty(data)) return BadRequest("მონაცემები ვერ წამოვიდა");
                return Content(data, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}