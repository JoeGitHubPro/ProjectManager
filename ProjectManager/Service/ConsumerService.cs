using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Constants;
using ProjectManager.Data;
using ProjectManager.Helpers;
using ProjectManager.Models;

namespace ProjectManager.Service
{
    public class ConsumerService : IConsumerService
    {
        private readonly AppDbContext _context;

        public ConsumerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Consumer>> GetAllConsumersAsync()
        {
            return await _context.Consumers
                .Include(c => c.Location)
                .Include(c => c.ResponsibleSeller)
                .ToListAsync();
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return await _context.Set<Location>().ToListAsync();
        }

        public async Task<List<IdentityUser>> GetAllSellersAsync()
        {
            return await _context.Set<IdentityUser>().ToListAsync();
        }

        public async Task<Consumer> CreateConsumerAsync(Consumer consumer)
        {
            // Validate uniqueness
            if (!string.IsNullOrWhiteSpace(consumer.Phone) &&
                await _context.Consumers.AnyAsync(c => c.Phone == consumer.Phone))
            {
                throw new InvalidOperationException("A consumer with this phone number already exists.");
            }

            if (!string.IsNullOrWhiteSpace(consumer.CommercialRegisterNo) &&
                await _context.Consumers.AnyAsync(c => c.CommercialRegisterNo == consumer.CommercialRegisterNo))
            {
                throw new InvalidOperationException("A consumer with this commercial register number already exists.");
            }

            await _context.Consumers.AddAsync(consumer);
            await _context.SaveChangesAsync();

            return await _context.Consumers
                .Include(c => c.Location)
                .Include(c => c.ResponsibleSeller)
                .FirstOrDefaultAsync(c => c.Id == consumer.Id);
        }

        public async Task<Consumer> UpdateConsumerAsync(Consumer consumer)
        {
            var existingConsumer = await _context.Consumers.FindAsync(consumer.Id);
            if (existingConsumer == null)
            {
                throw new KeyNotFoundException("Consumer not found");
            }

            // Update properties
            existingConsumer.Phone = consumer.Phone;
            existingConsumer.CommercialRegisterNo = consumer.CommercialRegisterNo;
            existingConsumer.Name = consumer.Name;
            existingConsumer.Description = consumer.Description;
            existingConsumer.LocationId = consumer.LocationId;
            existingConsumer.LocationMap = consumer.LocationMap;
            existingConsumer.Status = consumer.Status;
            existingConsumer.ResponsibleSellerId = consumer.ResponsibleSellerId;
            existingConsumer.Whatsapp = consumer.Whatsapp;
            existingConsumer.Facebook = consumer.Facebook;
            existingConsumer.TikTok = consumer.TikTok;
            existingConsumer.X = consumer.X;
            existingConsumer.Instagram = consumer.Instagram;
            existingConsumer.Site = consumer.Site;
            existingConsumer.Notes = consumer.Notes;

            _context.Consumers.Update(existingConsumer);
            await _context.SaveChangesAsync();

            return await _context.Consumers
                .Include(c => c.Location)
                .Include(c => c.ResponsibleSeller)
                .FirstOrDefaultAsync(c => c.Id == consumer.Id);
        }

        public async Task DeleteConsumerAsync(int id)
        {
            var consumer = await _context.Consumers.FindAsync(id);
            if (consumer != null)
            {
                _context.Consumers.Remove(consumer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Consumer>> ImportConsumersFromExcelAsync(string filePath)
        {
            var rawConsumers = await Task.Run(() => ReaderHelper.ReadConsumersFromExcelAsync(filePath));
            var validConsumers = ValidateExcelData(rawConsumers);
            return await FilterNewConsumersAgainstDatabaseAsync(validConsumers);
        }

        public async Task AssignConsumersAsync(List<int> consumerIds, string sellerId)
        {
            var consumers = await _context.Consumers
                .Where(c => consumerIds.Contains(c.Id))
                .ToListAsync();

            foreach (var consumer in consumers)
            {
                consumer.ResponsibleSellerId = sellerId;
            }

            _context.Consumers.UpdateRange(consumers);
            await _context.SaveChangesAsync();
        }

        private List<Consumer> ValidateExcelData(List<Consumer> consumers)
        {
            // Same validation logic as before
            if (consumers == null || consumers.Count == 0)
                return new List<Consumer>();

            // Basic validation - remove empty names
            var validNameConsumers = consumers
                .Where(c => !string.IsNullOrWhiteSpace(c?.Name))
                .ToList();

            // Ensure unique names (case insensitive)
            validNameConsumers = validNameConsumers
                .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            // Split into phone-based groups
            var withPhones = validNameConsumers
                .Where(c => !string.IsNullOrWhiteSpace(c.Phone))
                .ToList();

            var withoutPhones = validNameConsumers
                .Where(c => string.IsNullOrWhiteSpace(c.Phone))
                .ToList();

            // Ensure unique phones
            var uniquePhones = withPhones
                .GroupBy(c => c.Phone.Trim())
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            // Split into CRN-based groups
            var withCRNs = uniquePhones
                .Where(c => !string.IsNullOrWhiteSpace(c.CommercialRegisterNo))
                .ToList();

            var withoutCRNs = uniquePhones
                .Where(c => string.IsNullOrWhiteSpace(c.CommercialRegisterNo))
                .ToList();

            // Ensure unique CRNs
            var uniqueCRNs = withCRNs
                .GroupBy(c => c.CommercialRegisterNo.Trim())
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            // Set default location and status if needed
            foreach (var consumer in uniqueCRNs.Concat(withoutCRNs).Concat(withoutPhones))
            {
                // Set default location if not specified
                //if (consumer.LocationId == 0 && Locations.Any())
                //{
                //    consumer.LocationId = Locations.First().Id;
                //}

                // Set default status
                consumer.Status = ConsumerStatus.Default;
            }

            // Combine all valid entries
            return uniqueCRNs
                .Concat(withoutCRNs)
                .Concat(withoutPhones)
                .Distinct()
                .ToList();
        }

        private async Task<List<Consumer>> FilterNewConsumersAgainstDatabaseAsync(List<Consumer> validatedConsumers)
        {
            if (validatedConsumers == null || validatedConsumers.Count == 0)
                return new List<Consumer>();

            // Fetch existing values from DB
            var existingData = await _context.Consumers
                .Select(c => new
                {
                    Name = c.Name.Trim(),
                    Phone = c.Phone != null ? c.Phone.Trim() : null,
                    CRN = c.CommercialRegisterNo != null ? c.CommercialRegisterNo.Trim() : null
                })
                .ToListAsync();

            var existingNames = existingData.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingPhones = existingData.Where(x => x.Phone != null).Select(x => x.Phone).ToHashSet();
            var existingCRNs = existingData.Where(x => x.CRN != null).Select(x => x.CRN).ToHashSet();

            // Filter consumers that don't exist in database
            return validatedConsumers
                .Where(c =>
                {
                    var name = c.Name?.Trim();
                    var phone = c.Phone?.Trim();
                    var crn = c.CommercialRegisterNo?.Trim();

                    // Name must be unique (case insensitive)
                    bool isNameNew = !string.IsNullOrWhiteSpace(name) &&
                                    !existingNames.Contains(name);

                    // Phone must be either null or unique
                    bool isPhoneNew = string.IsNullOrWhiteSpace(phone) ||
                                    !existingPhones.Contains(phone);

                    // CRN must be either null or unique
                    bool isCrnNew = string.IsNullOrWhiteSpace(crn) ||
                                   !existingCRNs.Contains(crn);

                    return isNameNew && isPhoneNew && isCrnNew;
                })
                .ToList();
        }
    }
}
