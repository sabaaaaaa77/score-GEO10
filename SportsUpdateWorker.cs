using SCORE.Services;

namespace SCORE.Services
{
    public class SportsUpdateWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SportsUpdateWorker> _logger;

        public SportsUpdateWorker(IServiceProvider serviceProvider, ILogger<SportsUpdateWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("სპორტის განახლების რობოტი ჩაირთო...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var sportsDataService = scope.ServiceProvider.GetRequiredService<SportsDataService>();

                    try
                    {
                        _logger.LogInformation("მონაცემების ავტომატური განახლება დაიწყო...");
                        await sportsDataService.UpdateLiveMatches();
                        _logger.LogInformation("მონაცემები წარმატებით განახლდა.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"შეცდომა განახლებისას: {ex.Message}");
                    }
                }

                // დაიცადე 60 წამი შემდეგ განახლებამდე
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}