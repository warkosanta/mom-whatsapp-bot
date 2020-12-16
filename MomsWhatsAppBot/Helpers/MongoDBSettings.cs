using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MomsWhatsAppBot.Helpers
{
    public class MongoDBSettings
    {
        public string UserSettingsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
