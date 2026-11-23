using Microsoft.AspNetCore.Identity;
using ProjectManager.Models;

namespace ProjectManager.Service
{
    public interface IConsumerService
    {
        Task<List<Consumer>> GetAllConsumersAsync();
        Task<List<Location>> GetAllLocationsAsync();
        Task<List<IdentityUser>> GetAllSellersAsync();
        Task<Consumer> CreateConsumerAsync(Consumer consumer);
        Task<Consumer> UpdateConsumerAsync(Consumer consumer);
        Task DeleteConsumerAsync(int id);
        Task<List<Consumer>> ImportConsumersFromExcelAsync(string filePath);
        Task AssignConsumersAsync(List<int> consumerIds, string sellerId);
    }

}
