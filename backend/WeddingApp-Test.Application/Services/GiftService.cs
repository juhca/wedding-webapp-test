using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using WeddingApp_Test.Application.DTO.Gift;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Services;

public class GiftService(IGiftRepository giftRepository, IUserRepository userRepository, IMapper mapper, IEmailDispatchService emailDispatch) : IGiftService
{
    public async Task<IEnumerable<GiftDto>> GetAllVisibleAsync(Guid? currentUserId = null)
    {
        var gifts = await giftRepository.GetVisibleAsync();
        var dtos = mapper.Map<IEnumerable<GiftDto>>(gifts).ToList();

        if (currentUserId.HasValue)
        {
            foreach (var dto in dtos)
            {
                var gift = gifts.First(g => g.Id == dto.Id);
                dto.IsReservedByMe = gift.Reservations.Any(r => r.ReservedByUserId == currentUserId.Value);
            }
        }

        return dtos;
    }

    public async Task<GiftDto> GetByIdAsync(Guid id, Guid? currentUserId = null)
    {
        var gift = await giftRepository.GetByIdWithReservationsAsync(id);

        if (gift is null)
        {
            throw new KeyNotFoundException($"Gift with ID {id} not found");
        }
        
        var dto = mapper.Map<GiftDto>(gift);
        
        if (currentUserId.HasValue)
        {
            dto.IsReservedByMe = gift.Reservations.Any(r => r.ReservedByUserId == currentUserId.Value);
        }
        
        return dto;
    }

    public async Task<GiftDto> CreateAsync(CreateGiftDto dto)
    {
        var gift = mapper.Map<Gift>(dto);
        gift.Id = Guid.NewGuid();
        gift.CreatedAt = DateTime.UtcNow;
        
        await giftRepository.AddAsync(gift);
        await giftRepository.SaveChangesAsync();
        
        return mapper.Map<GiftDto>(gift);
    }

    public async Task<GiftDto> UpdateAsync(Guid id, UpdateGiftDto dto)
    {
        var gift = await giftRepository.GetByIdAsync(id);
        
        if (gift is null)
        {
            throw new KeyNotFoundException($"Gift with ID {id} not found");
        }
        
        mapper.Map(dto, gift);
        gift.UpdatedAt = DateTime.UtcNow;
        
        giftRepository.Update(gift);
        await giftRepository.SaveChangesAsync();
        
        return mapper.Map<GiftDto>(gift);
    }

    public async Task DeleteAsync(Guid id)
    {
        var gift = await giftRepository.GetByIdWithReservationsAsync(id);

        if (gift == null)
        {
            throw new KeyNotFoundException($"Gift with ID {id} not found");
        }

        // TODO(MAYBE DELETE THIS IN THE FUTURE ~ or make it optional)
        if (gift.Reservations.Any())
        {
            throw new InvalidOperationException("Cannot delete a gift that has reservations");
        }

        giftRepository.Delete(gift);
        await giftRepository.SaveChangesAsync();
    }

    public async Task<GiftReservationConfirmationDto> ReserveGiftAsync(Guid giftId, Guid userId, ReserveGiftDto dto)
    {
        var gift = await giftRepository.GetByIdWithReservationsAsync(giftId);

        if (gift is null)
        {
            throw new KeyNotFoundException($"Gift with ID {giftId} not found");
        }
        
        // Check 1: is gift fully reserved?
        if (gift.IsFullyReserved)
        {
            throw new InvalidOperationException("This gift is fully reserved");
        }
        
        // Check 2: has user already reserved this gift?
        var existingReservation = await giftRepository.GetUserReservationForGiftAsync(giftId, userId);
        if (existingReservation is not null)
        {
            throw new InvalidOperationException("You have already reserved this gift");
        }
        
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("User not found");
        }

        var reservation = new GiftReservation
        {
            Id = Guid.NewGuid(),
            GiftId = giftId,   
            ReservedByUserId = userId,
            ReservedAt = DateTime.UtcNow,
            Notes = dto.Notes,
            ReminderRequested = dto.WantsReminder,
            ReminderScheduledFor = dto.ReminderDate,
        };
        
        await giftRepository.AddReservationAsync(reservation);
        gift.UpdatedAt = DateTime.UtcNow;
        giftRepository.Update(gift);
        await giftRepository.SaveChangesAsync();
        
        // Reload to get updated counts
        gift = await giftRepository.GetByIdWithReservationsAsync(giftId);

        reservation.Gift = gift!;
        reservation.ReservedBy = user;
        _ = emailDispatch.DispatchEventAsync("GiftReserved", user, new Dictionary<string, object?> {
            ["gift"] = new { name = gift!.Name, price = gift.Price, purchaseLink = gift.PurchaseLink }
        });

        return new GiftReservationConfirmationDto
        {
            ReservationId = reservation.Id,
            GiftId = gift!.Id,
            GiftName = gift.Name,
            PurchaseLink = gift.PurchaseLink,
            Message = "Gift reserved successfully! Check your email for details.",
            ReminderScheduled = dto.WantsReminder,
            ReminderDate = dto.ReminderDate,
            RemainingReservations = gift.RemainingReservations ?? 0,
            GiftFullyReserved = gift.IsFullyReserved
        };
    }

    public async Task UnreserveGiftAsync(Guid giftId, Guid userId)
    {
        var reservation = await giftRepository.GetUserReservationForGiftAsync(giftId, userId);
        if (reservation is null)
        {
            throw new KeyNotFoundException($"Reservation not found");
        }
        
        giftRepository.DeleteReservation(reservation);
        var gift =  await giftRepository.GetByIdAsync(giftId);
        if (gift is not null)
        {
            gift.UpdatedAt = DateTime.UtcNow;
            giftRepository.Update(gift);
        }
        
        await giftRepository.SaveChangesAsync();

        if (gift is not null)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user is not null)
                _ = emailDispatch.DispatchEventAsync("GiftUnreserved", user, new Dictionary<string, object?> {
                    ["gift"] = new { name = gift.Name }
                });
        }
    }

    public async Task<IEnumerable<GiftDto>> GetMyReservedGiftsAsync(Guid userId)
    {
        var gifts = await giftRepository.GetReservedByUserAsync(userId);
        var dtos = mapper.Map<IEnumerable<GiftDto>>(gifts).ToList();

        foreach (var dto in dtos)
        {
            dto.IsReservedByMe = true;
        }

        return dtos;
    }

    public async Task<ImportGiftsResultDto> ImportGiftsAsync(IEnumerable<CreateGiftDto> dtos)
    {
        var result = new ImportGiftsResultDto();
        var row = 0;

        foreach (var dto in dtos)
        {
            row++;
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(dto, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                result.FailureCount++;
                result.Errors.Add(new ImportGiftErrorDto { Row = row, GiftName = dto.Name, Error = errors });
                continue;
            }

            try
            {
                var gift = await CreateAsync(dto);
                result.ImportedGifts.Add(gift);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(new ImportGiftErrorDto { Row = row, GiftName = dto.Name, Error = ex.Message });
            }
        }

        return result;
    }

    public Task<ImportGiftsResultDto> ImportGiftsCsvAsync(IFormFile file)
    {
        var csvGift = ParseCsvFile(file);
        var importGift = ImportGiftsAsync(csvGift);
        
        return importGift;
    }

    private List<CreateGiftDto> ParseCsvFile(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());

        var headerLine = reader.ReadLine()
            ?? throw new InvalidOperationException("CSV file is missing a header row");

        var headers = SplitCsvLine(headerLine)
            .Select(h => h.ToLowerInvariant())
            .ToArray();

        int Col(params string[] names) =>
            names.Select(n => Array.IndexOf(headers, n)).FirstOrDefault(i => i >= 0, -1);

        var nameIdx    = Col("name");
        if (nameIdx < 0) throw new InvalidOperationException("CSV must contain a 'Name' column");

        var descIdx    = Col("description");
        var priceIdx   = Col("price");
        var imageIdx   = Col("imageurl", "image_url");
        var linkIdx    = Col("purchaselink", "purchase_link");
        var maxResIdx  = Col("maxreservations", "max_reservations");
        var orderIdx   = Col("displayorder", "display_order");
        var visibleIdx = Col("isvisible", "is_visible");

        var dtos = new List<CreateGiftDto>();
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = SplitCsvLine(line);
            string? Get(int idx) => idx >= 0 && idx < cols.Length ? cols[idx] : null;

            dtos.Add(new CreateGiftDto
            {
                Name             = Get(nameIdx) ?? string.Empty,
                Description      = Get(descIdx),
                Price            = decimal.TryParse(Get(priceIdx), out var price) ? price : null,
                ImageUrl         = Get(imageIdx),
                PurchaseLink     = Get(linkIdx),
                MaxReservations  = int.TryParse(Get(maxResIdx), out var maxRes) ? maxRes : 1,
                DisplayOrder     = int.TryParse(Get(orderIdx), out var order) ? order : 0,
                IsVisible        = !bool.TryParse(Get(visibleIdx), out var vis) || vis,
            });
        }

        return dtos;
    }

    private string[] SplitCsvLine(string line)
    {
        var result   = new List<string>();
        var current  = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString().Trim());
        return result.ToArray();
    }
}