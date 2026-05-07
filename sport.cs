using System.ComponentModel.DataAnnotations;

namespace SCORE.Models
{
    // ენუმერაციები - ტიპების განსასაზღვრად
    public enum SportType { Football = 1, Basketball = 2, F1 = 3 }
    public enum MatchStatus { Scheduled, Live, Finished, Postponed }

    // ლიგის მოდელი
    public class League
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = "Global";
        public SportType Sport { get; set; }
        public string? LogoUrl { get; set; } // დავამატე ? რომ NULL შეიძლებოდეს
    }

    // გუნდის მოდელი
    public class Team
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public SportType Sport { get; set; }
        public string? LogoUrl { get; set; } // დავამატე ? რომ NULL შეიძლებოდეს
    }

    // მატჩის მოდელი
    public class Match
    {
        public int Id { get; set; }

        public int LeagueId { get; set; }
        public League? League { get; set; }

        public int? HomeTeamId { get; set; }
        public Team? HomeTeam { get; set; }

        public int? AwayTeamId { get; set; }
        public Team? AwayTeam { get; set; }

        public string Score { get; set; } = "0:0";
        public MatchStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public string? AffiliateLink { get; set; }
    }

    // API-ს მოდელები (თუ გჭირდება ცალკე კლასებად)
    public class FootballApiResponse
    {
        public List<FootballData>? data { get; set; }
    }

    public class FootballData
    {
        public string? league_name { get; set; }
        public string? home_name { get; set; }
        public string? away_name { get; set; }
        public string? score { get; set; }
    }
}