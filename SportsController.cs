using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCORE.Data; // დარწმუნდი რომ ეს სახელი სწორია (შენი AppDbContext-ის ფოლდერი)
using SCORE.Services;
using SCORE.Models;

namespace SCORE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SportsController : ControllerBase
    {
        private readonly SportsDataService _sportsDataService;
        private readonly AppDbContext _context; // აქ 'object'-ის ნაცვლად უნდა იყოს AppDbContext

        // Constructor-ში ორივე სერვისი უნდა შემოვიყვანოთ
        public SportsController(SportsDataService sportsDataService, AppDbContext context)
        {
            _sportsDataService = sportsDataService;
            _context = context;
        }

        // მისამართი: /api/sports/update
        [HttpGet("update")]
        public async Task<IActionResult> UpdateScores()
        {
            try
            {
                await _sportsDataService.UpdateLiveMatches();
                return Ok("მონაცემები წარმატებით განახლდა ბაზაში!");
            }
            catch (Exception ex)
            {
                return BadRequest($"შეცდომა: {ex.Message}");
            }
        }

        // მისამართი: /api/sports/live
        [HttpGet("live")]
        public async Task<IActionResult> GetLiveMatches()
        {
            try
            {
                // თუ შენს AppDbContext-ში ცხრილს Match ჰქვია, ქვემოთ Matches შეცვალე Match-ით
                var matches = await _context.Matches
                    .Include(m => m.HomeTeam)
                    .Include(m => m.AwayTeam)
                    .Include(m => m.League)
                    .OrderByDescending(m => m.StartTime)
                    .ToListAsync();

                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest($"ბაზიდან წაკითხვის შეცდომა: {ex.Message}");
            }
        }
        

    }
}