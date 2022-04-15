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

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public PingController(IHttpClientFactory httpClientFactory, ILogger<PingController> logger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory; 
        }

        [HttpGet("Ping")]
        public async Task<IActionResult> Ping()
        {
            Interlocked.Increment(ref callNumber);
            _logger.LogInformation($"Ping call number: {callNumber}");

            //await Task.Delay(11000);
            await Task.Delay(1000);

            string ret = string.Empty;

            var httpRequestMessage = new HttpRequestMessage(
                                HttpMethod.Get,
                                "https://localhost:7244/api/Pong/Pong");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            try
            {
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
