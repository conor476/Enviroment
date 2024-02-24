using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Enviroment.Services;

public class EmailBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var emailCheckerService = scope.ServiceProvider.GetRequiredService<EmailCheckerService>();
                await emailCheckerService.CheckAndCreateTicketsFromEmailAsync();
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
