using AutoMapper;
using WeddingApp_Test.Application.DTO.Reminder;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class ReminderService(IReminderRepository reminderRepository, IGiftRepository giftRepository, IRsvpRepository rsvpRepository, IWeddingInfoRepository weddingInfoRepository, IMapper mapper) : IReminderService
{
    private const int MaxRemindersPerTarget = 10;

    public async Task<ReminderDto> AddGiftReminderAsync(Guid giftId, Guid userId, AddReminderDto dto)
    {
        var reservation = await giftRepository.GetUserReservationForGiftAsync(giftId, userId)
            ?? throw new InvalidOperationException("You do not have a reservation for this gift.");

        var scheduledFor = await CalculateAndValidateScheduledFor(dto);

        var count = await reminderRepository.CountByTargetAsync(ReminderType.Gift, reservation.Id);
        if (count >= MaxRemindersPerTarget)
        {
            throw new InvalidOperationException($"You can set at most {MaxRemindersPerTarget} reminders per gift reservation.");
        }

        var reminder = BuildReminder(ReminderType.Gift, reservation.Id, dto, scheduledFor);
        await reminderRepository.AddAsync(reminder);
        await reminderRepository.SaveChangesAsync();

        return mapper.Map<ReminderDto>(reminder);
    }

    public async Task<IEnumerable<ReminderDto>> GetGiftRemindersAsync(Guid giftId, Guid userId)
    {
        var reservation = await giftRepository.GetUserReservationForGiftAsync(giftId, userId)
            ?? throw new InvalidOperationException("You do not have a reservation for this gift.");

        var reminders = await reminderRepository.GetByTargetAsync(ReminderType.Gift, reservation.Id);
        
        return mapper.Map<IEnumerable<ReminderDto>>(reminders);
    }

    public async Task<ReminderDto> AddRsvpReminderAsync(Guid userId, AddReminderDto dto)
    {
        var rsvp = await rsvpRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("You have not submitted an RSVP yet.");

        var scheduledFor = await CalculateAndValidateScheduledFor(dto);

        var count = await reminderRepository.CountByTargetAsync(ReminderType.Rsvp, rsvp.Id);
        if (count >= MaxRemindersPerTarget)
        {
            throw new InvalidOperationException($"You can set at most {MaxRemindersPerTarget} reminders per RSVP.");
        }

        var reminder = BuildReminder(ReminderType.Rsvp, rsvp.Id, dto, scheduledFor);
        await reminderRepository.AddAsync(reminder);
        await reminderRepository.SaveChangesAsync();

        return mapper.Map<ReminderDto>(reminder);
    }

    public async Task<IEnumerable<ReminderDto>> GetRsvpRemindersAsync(Guid userId)
    {
        var rsvp = await rsvpRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("You have not submitted an RSVP yet.");

        var reminders = await reminderRepository.GetByTargetAsync(ReminderType.Rsvp, rsvp.Id);
        
        return mapper.Map<IEnumerable<ReminderDto>>(reminders);
    }

    public async Task DeleteReminderAsync(Guid reminderId, Guid userId)
    {
        var reminder = await reminderRepository.GetByIdAsync(reminderId)
            ?? throw new KeyNotFoundException("Reminder not found.");

        await VerifyOwnership(reminder, userId);

        reminderRepository.Delete(reminder);
        
        await reminderRepository.SaveChangesAsync();
    }

    private async Task<DateTime> CalculateAndValidateScheduledFor(AddReminderDto dto)
    {
        var weddingInfo = await weddingInfoRepository.GetWeddingInfoAsync();
        if (weddingInfo?.WeddingDate is null)
        {
            throw new InvalidOperationException("Wedding date has not been set yet.");
        }

        var weddingDate = weddingInfo.WeddingDate.Value;

        var scheduledFor = dto.Unit switch
        {
            ReminderUnit.Days   => weddingDate.AddDays(-dto.Value),
            ReminderUnit.Weeks  => weddingDate.AddDays(-dto.Value * 7),
            ReminderUnit.Months => weddingDate.AddMonths(-dto.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(dto.Unit))
        };

        if (scheduledFor.Date <= DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("The reminder date must be in the future.");
        }

        return scheduledFor;
    }

    private static Reminder BuildReminder(ReminderType type, Guid targetId, AddReminderDto dto, DateTime scheduledFor)
    {
        return new Reminder
        {
            Id = Guid.NewGuid(),
            Type = type,
            TargetId = targetId,
            Value = dto.Value,
            Unit = dto.Unit,
            Note = dto.Note?.Trim(),
            ScheduledFor = scheduledFor,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task VerifyOwnership(Reminder reminder, Guid userId)
    {
        switch (reminder.Type)
        {
            case ReminderType.Gift:
                var reservation = await giftRepository.GetReservationAsync(reminder.TargetId);
                if (reservation is null || reservation.ReservedByUserId != userId)
                {
                    throw new UnauthorizedAccessException("You do not own this reminder.");
                }
                break;

            case ReminderType.Rsvp:
                var rsvp = await rsvpRepository.GetByUserIdAsync(userId);
                if (rsvp is null || rsvp.Id != reminder.TargetId)
                {
                    throw new UnauthorizedAccessException("You do not own this reminder.");
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown reminder type: {reminder.Type}");
        }
    }
}
