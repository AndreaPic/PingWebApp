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
            string ret = string.Empty;

            Interlocked.Increment(ref callNumber);
            _logger.LogInformation($"Ping call number: {callNumber}");

            try
            {
                int delay = ConfigRoot.GetValue<int>("DelayMS", 0);

                if (delay > 0)
                {
                    await Task.Delay(delay);
                }
               
                string pongBaseAddress = ConfigRoot.GetValue<string>("PongBaseAddress");
                string endpoint = string.Empty;
                if (pongBaseAddress != null)
                {
                    endpoint = $"{pongBaseAddress}/api/Pong/Pong";
                }

                var httpRequestMessage = new HttpRequestMessage(
                                HttpMethod.Get,
                                endpoint);

                var httpClient = _httpClientFactory.CreateClient();
                int timeout = ConfigRoot.GetValue<int>("TimeoutMS", 0);

                if (timeout > 0)
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                }

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
