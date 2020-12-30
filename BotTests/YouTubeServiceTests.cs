using Microsoft.Extensions.Options;
using MomsWhatsAppBot.Models;
using MomsWhatsAppBot.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace BotTests
{
    public class YouTubeServiceTests
    {

        [Fact]
        public void GetVideo_CorrectQuantity_ReturnsSameQuantity()
        {
            // Arrange
            var options = Options.Create(new YouTubeSettings() 
            { Key = "AIzaSyC6bknsEUudzRcx0Yl5cCkpa3guk3Khu1I" });
            var videoService = new YTVideoService(options);

            var command = new WhatsAppCommand()
            {
                chanellsIDs = new string[] { "UC-lHJZR3Gqxm24_Vd_AJ5Yw" },
                quantity = 5,
                mode = VideoMode.AllRandom
            };

            // Act
            var result = videoService.GetVideosAsync(command).GetAwaiter().GetResult();

            // Assert
            Assert.Equal(result.Count(), command.quantity);
        }

        [Fact]
        public void GetVideo_InvalidModel_ThrowsValidationException()
        {
            // Arrange
            var options = Options.Create(new YouTubeSettings()
            { Key = "AIzaSyC6bknsEUudzRcx0Yl5cCkpa3guk3Khu1I" });
            var videoService = new YTVideoService(options);

            var invalidCommand = new WhatsAppCommand();

            // Act
            Action actual = () => 
                videoService.GetVideosAsync(invalidCommand).GetAwaiter().GetResult();

            //Assert
            Assert.Throws<ValidationException>(actual);

        }
    }
}