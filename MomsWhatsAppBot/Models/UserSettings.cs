using Google.Apis.YouTube.v3;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MomsWhatsAppBot.Models
{
    public class UserSettings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id {get; set;}

        public string userNumber { get; set; } // actual user mobile phone
        public string greatingMessage { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<string, WhatsAppCommand> buttons { get; set; }
    }

    public class WhatsAppCommand
    {
        public string description { get; set; }

        [Required]
        [MinLength(1)]
        public string[] chanellsIDs { get; set; }

        [Required]
        [Range(1,10)]
        public int quantity { get; set; }

        [BsonDefaultValue(1)]
        public VideoMode mode { get; set; }
    }

    public enum VideoMode
    {
        AllFresh = 0, 
        AllRandom = 1, 
        EachFresh = 2, 
        EachRandom = 3, 
    }
}