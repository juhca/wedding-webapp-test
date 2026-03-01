using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Application.Interfaces;
using WeddingApp_Test.Domain.Entities;
using WeddingApp_Test.Infrastructure.Persistence;

namespace WeddingApp_Test.Infrastructure.Repositories;

public class WeddingInfoRepository(AppDbContext context) : IWeddingInfoRepository
{
    public async Task<WeddingInfo?> GetWeddingInfoAsync()
    {
        return await context.WeddingInfo
            .Include(w => w.UpdatedBy)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(WeddingInfo weddingInfo)
    {
        weddingInfo.CreatedAt = DateTime.UtcNow;
        await context.WeddingInfo.AddAsync(weddingInfo);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(WeddingInfo weddingInfo)
    {
        weddingInfo.UpdatedAt = DateTime.UtcNow;
        context.WeddingInfo.Update(weddingInfo);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync()
    {
        return await context.WeddingInfo.AnyAsync();
    }
}