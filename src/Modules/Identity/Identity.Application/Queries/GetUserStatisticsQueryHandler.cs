using FluentResults;
using Identity.Application.Dtos;
using Identity.Application.Services;
using Identity.Domain.Entities;
using MediatR;
using TheShop.SharedKernel;

namespace Identity.Application.Queries;

public class GetUserStatisticsQueryHandler : IRequestHandler<GetUserStatisticsQuery, Result<UserStatisticsDto>>
{
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly ISessionService _sessionService;
    private readonly IUserStatisticsCacheService _cacheService;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public GetUserStatisticsQueryHandler(
        IRepository<ApplicationUser> userRepository,
        ISessionService sessionService,
        IUserStatisticsCacheService cacheService)
    {
        _userRepository = userRepository;
        _sessionService = sessionService;
        _cacheService = cacheService;
    }

    public async Task<Result<UserStatisticsDto>> Handle(GetUserStatisticsQuery request, CancellationToken ct)
    {
        var cacheKey = _cacheService.BuildCacheKey(request.FromDate, request.ToDate);
        var cached = await _cacheService.GetAsync(cacheKey, ct);
        if (cached is not null)
        {
            return Result.Ok(cached);
        }

        var now = DateTime.UtcNow;
        var lastDayStart = now.AddDays(-1);
        var lastWeekStart = now.AddDays(-7);
        var lastMonthStart = now.AddMonths(-1);

        var allUsers = await _userRepository.GetAllAsync(ct);

        var fromDate = request.FromDate ?? lastMonthStart;
        var toDate = request.ToDate ?? now;

        var registrationsByDate = allUsers
            .Where(u => u.CreatedAt >= fromDate && u.CreatedAt <= toDate)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new RegistrationsByDateDto(g.Key, g.Count()))
            .OrderBy(x => x.Date)
            .ToList();

        var activeSessionsCount = await _sessionService.CountActiveSessionsAsync(ct);

        var statistics = new UserStatisticsDto(
            allUsers.Count,
            allUsers.Count(u => u.CreatedAt >= lastDayStart),
            allUsers.Count(u => u.CreatedAt >= lastWeekStart),
            allUsers.Count(u => u.CreatedAt >= lastMonthStart),
            activeSessionsCount,
            registrationsByDate);

        await _cacheService.SetAsync(cacheKey, statistics, CacheDuration, ct);

        return Result.Ok(statistics);
    }
}

