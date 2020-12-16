using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MomsWhatsAppBot.Models;
using MomsWhatsAppBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace TwilioReceive.Controllers
{
    public class SmsController : TwilioController
    {
        private readonly YTVideoService videoService;
        private readonly UserSettingsService userSettings;

        public SmsController(YTVideoService videoService, UserSettingsService userSettings)
        {
            this.videoService = videoService;
            this.userSettings = userSettings;
        }

        public async Task<TwiMLResult> Index(SmsRequest request)
        {
            var messagingResponse = new MessagingResponse();

            if (request.Body is null && request.From is null)  return TwiML(messagingResponse.Message("Body or sender of request is unknown."));
            var command = request.Body.ToLower();

            var results = await userSettings.GetAsync(Regex.Replace(request.From, "[^0-9.-]", ""));

            if (!results.Any())
            {
                return TwiML(messagingResponse.Message("Ваш номер отсутсвует в базе данных."));
            }

            var settings = results.First();
            if (!settings.buttons.ContainsKey(command))
            {
                messagingResponse.Message(settings.greatingMessage);
                foreach (var b in settings.buttons)
                {
                    messagingResponse.Message($"Send '{b.Key}' to {b.Value.description}");
                }
                return TwiML(messagingResponse);
            }

            var videos = await videoService.GetVideosAsync(settings.buttons[command]);

            foreach (var video in videos)
            {
                messagingResponse.Message(FormProperLink(video));
            }

            return TwiML(messagingResponse);
        }

        private string FormProperLink(string link) =>
             string.Format("https://www.youtube.com/watch?v={0}", link);
    }
}