namespace Identity.Application.Dtos;

public record UserStatisticsDto(
    int TotalUsers,
    int NewUsersLastDay,
    int NewUsersLastWeek,
    int NewUsersLastMonth,
    int ActiveSessionsNow,
    List<RegistrationsByDateDto> RegistrationsByDate);

public record RegistrationsByDateDto(DateTime Date, int Count);

