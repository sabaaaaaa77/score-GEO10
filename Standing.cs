namespace SCORE.Models
{
    public class Standing
    {
        public int Id { get; set; }
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public int Position { get; set; }
        public string TeamName { get; set; }
        public string TeamLogo { get; set; }
        public int Played { get; set; }

        // აი ესენი გაკლდა და იმიტომ გეჩხუბება:
        public int Won { get; set; }
        public int Draw { get; set; }
        public int Lost { get; set; }

        public int Points { get; set; }
    }
}
