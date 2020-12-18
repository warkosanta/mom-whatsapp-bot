using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using MomsWhatsAppBot.Models;
using MomsWhatsAppBot.Services;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Twilio.AspNet.Common;
using TwilioReceive.Controllers;
using Xunit;
using Xunit.Sdk;

namespace BotTests
{
    public class SmsControllerTests
    {
        [Theory]
        [JsonFileData("TestData.json", "ValidSmsRequests")]
        public async Task ValidRequest_CorrectResponse(object request)
        {
            var r = request as SmsRequestDto;
            var ytservice = new Mock<YTVideoService>();
            var settings = new Mock<IUserSettingsService>();

            settings.Setup(s => s.GetAsync("9999"))
                .ReturnsAsync(new List<UserSettings> { r.UserSettings });

            var controller = new SmsController(ytservice.Object, settings.Object);
            var response = await controller.Index(r.SmsRequest);
            var xml = new XmlDocument();
            xml.LoadXml(response.Data);
            Assert.Equal(r.ResponseMessage, xml.GetElementsByTagName("Response").Item(0)?.InnerText);
        }

        [Theory]
        [JsonFileData("TestData.json", "InvalidSmsRequests")]
        public async Task InvalidRequest_CorrectResponse(object request)
        {
            var r = request as SmsRequestDto;
            var ytservice = new Mock<YTVideoService>();
            var settings = new Mock<IUserSettingsService>();

            settings.Setup(s => s.GetAsync("0000"))
                .ReturnsAsync(new List<UserSettings>());

            var controller = new SmsController(ytservice.Object, settings.Object);
            var response = await controller.Index(r.SmsRequest);
            var xml = new XmlDocument();
            xml.LoadXml(response.Data);
            Assert.Equal(r.ResponseMessage, xml.GetElementsByTagName("Response").Item(0)?.InnerText);
        }

    }
    public class JsonFileDataAttribute: DataAttribute
    {
        private readonly string _filePath;
        private readonly string _propertyName;

        /// <summary>
        /// Load data from a JSON file as the data source for a theory
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the JSON file to load</param>
        public JsonFileDataAttribute(string filePath)
            : this(filePath, null) { }

        /// <summary>
        /// Load data from a JSON file as the data source for a theory
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the JSON file to load</param>
        /// <param name="propertyName">The name of the property on the JSON file that contains the data for the test</param>
        public JsonFileDataAttribute(string filePath, string propertyName)
        {
            _filePath = filePath;
            _propertyName = propertyName;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) { throw new ArgumentNullException(nameof(testMethod)); }

            // Get the absolute path to the JSON file
            var path = Path.IsPathRooted(_filePath)
                ? _filePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), _filePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Could not find file at path: {path}");
            }

            // Load the file
            var fileData = File.ReadAllText(path);

            if (string.IsNullOrEmpty(_propertyName))
            {
                //whole file is the data
                return JsonConvert.DeserializeObject<List<object[]>>(fileData);
            }

            // Only use the specified property as the data
            var allData = JObject.Parse(fileData);
            var data = allData[_propertyName];
            return data.ToObject<List<SmsRequestDto[]>>();
        }
    }
    public class SmsRequestDto
    {
        public SmsRequest SmsRequest { get; set; }
        public string ResponseMessage { get; set; }
        public UserSettings UserSettings { get; set; }
    }
}