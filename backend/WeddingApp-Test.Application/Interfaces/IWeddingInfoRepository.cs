using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Application.Interfaces;

public interface IWeddingInfoRepository
{
    /// <summary>
    /// Get the wedding info (should only be one record)
    /// </summary>
    Task<WeddingInfo?> GetWeddingInfoAsync();
    
    /// <summary>
    /// Create wedding info (should only be called once during setup)
    /// </summary>
    Task CreateAsync(WeddingInfo weddingInfo);
    
    /// <summary>
    /// Update wedding info
    /// </summary>
    Task UpdateAsync(WeddingInfo weddingInfo);
    
    /// <summary>
    /// Check if wedding info exists
    /// </summary>
    Task<bool> ExistsAsync();
}