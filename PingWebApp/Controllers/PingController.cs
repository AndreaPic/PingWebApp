using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace PingWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        private static int callNumber = 0;
        private IConfigurationRoot ConfigRoot;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public PingController(IHttpClientFactory httpClientFactory, IConfiguration configRoot, ILogger<PingController> logger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            ConfigRoot = (IConfigurationRoot)configRoot;
        }

        [HttpGet("Ping")]
        public async Task<IActionResult> Ping()
        {
            Interlocked.Increment(ref callNumber);
            _logger.LogInformation($"Ping call number: {callNumber}");

            int delay = ConfigRoot.GetValue<int>("DelayMS", 0);

            if (delay > 0)
            {
                await Task.Delay(delay);
            }

            string ret = string.Empty;

            try
            {
                var httpRequestMessage = new HttpRequestMessage(
                                HttpMethod.Get,
                                "https://localhost:7244/api/Pong/Pong");

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    ret = $"Pong Call Number: {callNumber} - Ping Response: " + await httpResponseMessage.Content.ReadAsStringAsync();
                }
                else
                {
                    ret = $"Call To Ping Return Status Code: {httpResponseMessage.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Ok(ex.Message);
            }

            return Ok(ret);
        }
    }
}
