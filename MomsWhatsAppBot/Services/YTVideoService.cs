using AutoMapper;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Options;
using MomsWhatsAppBot.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MomsWhatsAppBot.Services
{
    public class YTVideoService
    {
        private static YouTubeService service;
        private readonly Dictionary<VideoMode,
            Func<WhatsAppCommand, IEnumerable<string>>> videoModes =
            new Dictionary<VideoMode, Func<WhatsAppCommand, IEnumerable<string>>>
            {
               [VideoMode.AllFresh] = GetAllFreshVideos,
               [VideoMode.AllRandom] = GetAllRandomVideos,
               [VideoMode.EachFresh] = GetEachFreshVideos,
               [VideoMode.EachRandom] = GetEachRandomVideos
            };

        public YTVideoService(IOptions<YouTubeSettings> youTubeSettings)
        {
            service = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = youTubeSettings.Value.Key,
                ApplicationName = this.GetType().ToString()
            });
            
        }
        public async Task<IEnumerable<string>> GetVideosAsync(WhatsAppCommand searchParams)
        {
            IsCommandValid(searchParams);

            if (!videoModes.TryGetValue(searchParams.mode, out var f))
            {
                throw new ArgumentException();
            }

            var links = await Task.Run(() => f.Invoke(searchParams));
            return links;
        }

        private bool IsCommandValid(WhatsAppCommand searchParams)
        {
            var results = new List<ValidationResult>();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(searchParams);

            if (Validator.TryValidateObject(searchParams, context, results, true)) return true;
            else throw new ValidationException(
                results.Select(i => i.ErrorMessage).Aggregate((i, j) => i + "\n" + j));
        }

        private static IEnumerable<string> GetAllFreshVideos(WhatsAppCommand searchParams)
        {
            var searchListRequest = service.Search.List("snippet");
            List<SearchResult> results = new List<SearchResult>();

            // для каждого канала 
            foreach (var id in searchParams.chanellsIDs)
            {
                // находим свежайшие видео
                searchListRequest.ChannelId = id;
                searchListRequest.MaxResults = searchParams.quantity;
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
                // добавляем в общую коллекцию
                var result = Task.Run(() => searchListRequest.ExecuteAsync()).Result;
                results.AddRange(result.Items);
            }

            var random = new Random();
            var links = new List<string>();
            HashSet<int> numbers = new HashSet<int>();
            while (numbers.Count < searchParams.quantity)
            {
                numbers.Add(random.Next(results.Count));
            }
            foreach (var number in numbers)
            {
                links.Add(results.ElementAt(number).Id.VideoId);
            }
            return links;
        }

        private static IEnumerable<string> GetAllRandomVideos(WhatsAppCommand searchParams)
        {
            var searchListRequest = service.Search.List("snippet");
            List<SearchResult> results = new List<SearchResult>();
            var random = new Random();

            // для каждого канала 
            foreach (var id in searchParams.chanellsIDs)
            {
                // находим случайные видео
                searchListRequest.ChannelId = id;
                searchListRequest.MaxResults = random.Next(searchParams.quantity, searchParams.quantity + 30);

                // устанавливаем случаные параметры сортировки
                Array values = Enum.GetValues(typeof(SearchResource.ListRequest.OrderEnum));
                var order = (SearchResource.ListRequest.OrderEnum)values.GetValue(random.Next(values.Length));

                searchListRequest.Order = order;
                // добавляем в общую коллекцию
                results.AddRange(Task.Run(() => searchListRequest.ExecuteAsync()).Result.Items);
            }

            var links = new List<string>();
            HashSet<int> numbers = new HashSet<int>(); 
            while (numbers.Count < searchParams.quantity)
            {
                numbers.Add(random.Next(results.Count));
            }
            foreach(var number in numbers)
            {
                links.Add(results.ElementAt(number).Id.VideoId);
            }
            return links;
        }

        private static IEnumerable<string> GetEachFreshVideos(WhatsAppCommand searchParams)
        {
            var searchListRequest = service.Search.List("snippet");
            List<SearchResult> results = new List<SearchResult>();
            var random = new Random();


            // для каждого канала 
            foreach (var id in searchParams.chanellsIDs)
            {
                // находим свежайшие видео
                searchListRequest.ChannelId = id;
                searchListRequest.MaxResults = 5;
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
                // добавляем в общую коллекцию
                var result = Task.Run(() => searchListRequest.ExecuteAsync()).Result;

                var num = random.Next(result.Items.Count);
                results.Add(result.Items.ElementAt(num));
            }

            var links = results.Select(l => l.Id.VideoId);
            return links;
        }
        private static IEnumerable<string> GetEachRandomVideos(WhatsAppCommand searchParams)
        {
            var searchListRequest = service.Search.List("snippet");
            var random = new Random();
            var links = new List<string>();

            HashSet<int> numbers = new HashSet<int>();
            // для каждого канала 
            foreach (var id in searchParams.chanellsIDs)
            {
                var results = new List<SearchResult>();

                // находим случайные видео
                searchListRequest.ChannelId = id;
                searchListRequest.MaxResults = random.Next(0, 30);

                // устанавливаем случаные параметры сортировки
                Array values = Enum.GetValues(typeof(SearchResource.ListRequest.OrderEnum));
                var order = (SearchResource.ListRequest.OrderEnum)values.GetValue(random.Next(values.Length));
                searchListRequest.Order = order;

                // добавляем в общую коллекцию
                results.AddRange(Task.Run(() => searchListRequest.ExecuteAsync()).Result.Items);

                var num = random.Next(results.Count);
                links.Add(results.ElementAt(num).Id.VideoId);
            }

            return links;
        }
    }
}