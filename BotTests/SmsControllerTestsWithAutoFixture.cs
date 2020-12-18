using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using MomsWhatsAppBot.Models;
using MomsWhatsAppBot.Services;
using Moq;
using TwilioReceive.Controllers;
using Xunit;

namespace BotTests
{
    public class SmsControllerTestsWithAutoFixture
    {
        [Theory, AutoMoqData]
        public async Task CanSaveEntityAsync([Frozen]Mock<IYTVideoService> ytservice,
            [Frozen]Mock<IUserSettingsService> settings, SmsRequestDto dto)
        {
            // Arrange
            settings.Setup(s =>
                    s.GetAsync(Regex.Replace(dto.SmsRequest.From, "[^0-9.-]", "")))
                .ReturnsAsync(new List<UserSettings> { dto.UserSettings });
            ytservice.Setup(s =>
                    s.GetVideosAsync(dto.UserSettings.buttons.First().Value))
                .ReturnsAsync(new List<string> { dto.ResponseMessage });

            // Act
            var controller = new SmsController(ytservice.Object, settings.Object);

            // Assert
            var response = await controller.Index(dto.SmsRequest);
            var xml = new XmlDocument();
            xml.LoadXml(response.Data);
            Assert.Equal(dto.UserSettings.buttons.Count + 1, 
                xml.GetElementsByTagName("Response").Item(0)?.ChildNodes.Count);
        }
    }
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(new Fixture()
                .Customize(new AutoMoqCustomization()))
        {
        }
    }
}