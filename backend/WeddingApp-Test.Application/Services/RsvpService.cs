using WeddingApp_Test.Application.DTO.Rsvp;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Application.Interfaces.Email;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Domain.Enums;

namespace WeddingApp_Test.Application.Services;

public class RsvpService : IRsvpService
{
    private readonly IRsvpRepository _rsvpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailDispatchService _emailDispatchService;

    public RsvpService(IRsvpRepository rsvpRepository, IUserRepository userRepository,  IEmailDispatchService emailDispatchService)
    {
        _rsvpRepository = rsvpRepository;
        _userRepository = userRepository;
        _emailDispatchService = emailDispatchService;
    }
    
    public async Task<RsvpDto?> GetUserRsvpAsync(Guid userId)
    {
        var rsvp = await _rsvpRepository.GetByUserIdAsync(userId);
        
        return rsvp != null ? RsvpDto.FromEntity(rsvp) : null;
    }

    public async Task<RsvpDto> CreateOrUpdateRsvpAsync(Guid userId, CreateRsvpDto dto, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found");
        }
        
        // Validate companion limit
        var maxCompanions = user.MaxCompanions ?? 0;
        if (dto.Companions.Count > maxCompanions)
        {
            throw new InvalidOperationException(
                $"You can bring maximum {maxCompanions} companion(s). You provided {dto.Companions.Count}.");
        }
        
        var existingRsvp = await _rsvpRepository.GetByUserIdAsync(userId);
        
        Rsvp rsvp;

        if (existingRsvp is null)
        {
            // Create new RSVP
            rsvp = new Rsvp
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsAttending = dto.IsAttending,
                DietaryRestrictions = dto.DietaryRestrictions,
                Notes = dto.Notes,
                RespondedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Companions = dto.Companions.Select(c => new GuestCompanion
                {
                    Id = Guid.NewGuid(),
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Age = c.Age,
                    DietaryRestrictions = c.DietaryRestrictions,
                    Notes = c.Notes,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };
            
            await _rsvpRepository.AddAsync(rsvp);
        }
        else
        {
            // Update existing RSVP
            existingRsvp.IsAttending = dto.IsAttending;
            existingRsvp.Notes = dto.Notes;
            existingRsvp.RespondedAt = DateTime.UtcNow;
            existingRsvp.UpdatedAt = DateTime.UtcNow;
            existingRsvp.DietaryRestrictions = dto.DietaryRestrictions;
            
            existingRsvp.Companions.Clear(); // Remove old companions then add new
            existingRsvp.Companions = dto.Companions.Select(c => new GuestCompanion
            {
                Id = Guid.NewGuid(),
                RsvpId = existingRsvp.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Age = c.Age,
                DietaryRestrictions = c.DietaryRestrictions,
                Notes = c.Notes,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            rsvp = existingRsvp;
            _rsvpRepository.Update(rsvp);
        }
        await _rsvpRepository.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var isNew = existingRsvp is null; // determine before overwrite if needed
            var eventName = isNew ? "rsvp.submited" :  "rsvp.updated";
            var context = new Dictionary<string, object?>
            {
                ["Rsvp"] = rsvp,
                ["RelatedEntityId"] = rsvp.Id
            };
            
            await _emailDispatchService.DispatchEventAsync(eventName, user, context, ct);
        }
        
        
        var result = RsvpDto.FromEntity(rsvp);
        result.MaxCompanionsAllowed = maxCompanions;
        
        return result;
    }

    public async Task<RsvpSummaryDto> GetSummaryAsync()
    {
        var allUsers = (await _userRepository.GetAllAsync()).ToList();
        var totalInvited = allUsers.Count(u => u.Role != UserRole.Admin);
        
        var allRsvps = (await _rsvpRepository.GetAllWithUsersAsync()).ToList();
        var attending = (await _rsvpRepository.GetAttendingAsync()).ToList();
        var notAttending = await _rsvpRepository.GetNotAttendingAsync();
        
        // Calculate total people (guest + companions)
        var totalPeople = attending.Sum(r => r.TotalGuests);
        var totalCompanions = attending.Sum(r => r.Companions.Count);

        var respondedUserIds = allRsvps.Select(r => r.UserId).ToHashSet();
        var pendingUsers = allUsers
            .Where(u => u.Role != UserRole.Admin && !respondedUserIds.Contains(u.Id))
            .ToList();
        
        return new RsvpSummaryDto
        {
            TotalInvited = totalInvited,
            TotalResponded = allRsvps.Count,
            TotalAttending = attending.Count,
            TotalNotAttending = notAttending.Count(),
            TotalPeople = totalPeople,
            TotalCompanions = totalCompanions,
            PendingResponses = pendingUsers.Count,
            AttendingGuests = attending.Select(RsvpWithUserDto.FromEntity).ToList(),
            NotAttendingGuests = notAttending.Select(RsvpWithUserDto.FromEntity).ToList(),
            PendingGuests = pendingUsers.Select(u => new RsvpWithUserDto
            {
                UserId = u.Id,
                UserFirstName = u.FirstName,
                UserLastName = u.LastName,
                UserEmail = u.Email,
                IsAttending = false,
                RespondedAt = null,
                MaxCompanionsAllowed = u.MaxCompanions ?? 0
            }).ToList()
        };
    }

    public async Task<IEnumerable<RsvpWithUserDto>> GetAllWithUsersAsync()
    {
        var rsvps = await _rsvpRepository.GetAllWithUsersAsync();
        
        return rsvps.Select(RsvpWithUserDto.FromEntity);
    }

    public async Task<IEnumerable<CateringExportDto>> ExportForCateringAsync()
    {
        var attending = await _rsvpRepository.GetAttendingAsync();
        var result = new List<CateringExportDto>();

        foreach (var rsvp in attending)
        {
            // Main guest
            result.Add(new CateringExportDto
            {
                GuestType = "Main",
                FirstName = rsvp.User.FirstName,
                LastName = rsvp.User.LastName,
                Age = null, // Main guest age not tracked
                DietaryRestrictions = rsvp.DietaryRestrictions,
                Notes = rsvp.Notes,
                MainGuestEmail = rsvp.User.Email
            });
            
            // Companions
            foreach (var companion in rsvp.Companions)
            {
                result.Add(new CateringExportDto
                {
                    GuestType = "Companion",
                    FirstName = companion.FirstName,
                    LastName = companion.LastName,
                    Age = companion.Age,
                    DietaryRestrictions = companion.DietaryRestrictions,
                    Notes = companion.Notes,
                    MainGuestEmail = rsvp.User.Email
                });
            }
        }
        
        return result;
    }
}