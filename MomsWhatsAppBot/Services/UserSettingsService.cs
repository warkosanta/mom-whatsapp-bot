using Microsoft.Extensions.Options;
using MomsWhatsAppBot.Helpers;
using MomsWhatsAppBot.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MomsWhatsAppBot.Services
{
    public class UserSettingsService
    {
        private readonly IMongoCollection<UserSettings> userSettings;

        public UserSettingsService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);

            userSettings = database.GetCollection<UserSettings>(settings.Value.UserSettingsCollectionName);
        }

        public async Task<List<UserSettings>> GetAsync(string mobilePhone)
        {
            var filter = new BsonDocument("userNumber", mobilePhone);
            var result = await userSettings.FindAsync(filter);
            return await result.ToListAsync();
        }
        public async Task PostAsync(UserSettings settings)
        {
            await userSettings.InsertOneAsync(settings);
        }
    }
}