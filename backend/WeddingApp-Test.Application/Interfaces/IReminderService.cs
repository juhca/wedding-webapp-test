using WeddingApp_Test.Application.DTO.Reminder;

namespace WeddingApp_Test.Application.Interfaces;

public interface IReminderService
{
    Task<ReminderDto> AddGiftReminderAsync(Guid giftId, Guid userId, AddReminderDto dto);
    Task<IEnumerable<ReminderDto>> GetGiftRemindersAsync(Guid giftId, Guid userId);
    Task<ReminderDto> AddRsvpReminderAsync(Guid userId, AddReminderDto dto);
    Task<IEnumerable<ReminderDto>> GetRsvpRemindersAsync(Guid userId);
    Task DeleteReminderAsync(Guid reminderId, Guid userId);
}
