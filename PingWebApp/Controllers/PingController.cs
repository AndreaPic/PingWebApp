using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text;
using SPS.DistributedLoopDetector.Extensions;

namespace PingWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {

        private string MemoryAllocation(int quantity)
        {
            string s = null;
            if (quantity > 0)
            {
                s = String.Empty;
                s = s.PadRight(10);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < quantity*1000; i++)
                {
                    sb.Append(s);
                }
                s = sb.ToString();
            }
            return s;
        }

        private static int callNumber = 0;
        private IConfigurationRoot ConfigRoot;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        IHttpContextAccessor httpContextAccessor;

        public PingController(IHttpClientFactory httpClientFactory, IConfiguration configRoot, ILogger<PingController> logger, IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            ConfigRoot = (IConfigurationRoot)configRoot;
            httpContextAccessor = contextAccessor;
        }

        [HttpGet("Ping")]
        public async Task<IActionResult> Ping()
        {
            string ret = string.Empty;

            Interlocked.Increment(ref callNumber);
            _logger.LogInformation($"Ping call number: {callNumber}");

            try
            {

                int mallocQty = ConfigRoot.GetValue<int>("MemoryAllocation", 0);
                var s = MemoryAllocation(mallocQty);

                int delay = ConfigRoot.GetValue<int>("DelayMS", 0);

                if (delay > 0)
                {
                    await Task.Delay(delay);
                }
               
                string pongBaseAddress = ConfigRoot.GetValue<string>("PongBaseAddress");
                string endpoint = string.Empty;
                if (pongBaseAddress != null)
                {
                    endpoint = $"{pongBaseAddress}api/Pong/Pong";
                }

                var httpRequestMessage = new HttpRequestMessage(
                                HttpMethod.Get,
                                endpoint);

                var httpClient = _httpClientFactory.CreateClient();
                int timeout = ConfigRoot.GetValue<int>("TimeoutMS", 0);

                if (timeout > 0)
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                }
                    
                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    ret = $"Ping Call Number: {callNumber} - Pong Response: " + await httpResponseMessage.Content.ReadAsStringAsync();
                }
                else
                {
                    ret = $"Call To Pong Return Status Code: {httpResponseMessage.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Ok(ex.Message);
            }

            return Ok(ret);
        }

        [HttpGet("Fibonacci/{reqValue?}")]
        public IActionResult Fibonacci(int? reqValue)
        {
            string ret = "???";

            _logger.LogInformation($"Fibonacci call");

            try
            {
                if (reqValue.HasValue)
                {
                    var result = CalculateFibonacciSeries(reqValue.Value);
                    ret = result.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Ok(ex.Message);
            }

            return Ok(ret);
        }

        static long CalculateFibonacciSeries(int n)
        {
            long first = 0;
            long second = 1;
            long result = 0;

            if (n == 0) return 0; 
            if (n == 1) return 1; 

            for (int i = 2; i <= n; i++)
            {
                result = first + second;
                first = second;
                second = result;
            }

            return result;
        }

        [HttpGet("Pang")]
        public async Task<IActionResult> Pang()
        {
            string ret = string.Empty;

            Interlocked.Increment(ref callNumber);
            _logger.LogInformation($"Pang call number: {callNumber}");

            try
            {

                int mallocQty = ConfigRoot.GetValue<int>("MemoryAllocation", 0);
                var s = MemoryAllocation(mallocQty);

                int delay = ConfigRoot.GetValue<int>("DelayMS", 0);

                if (delay > 0)
                {
                    await Task.Delay(delay);
                }

                string pongBaseAddress = ConfigRoot.GetValue<string>("PongBaseAddress");
                string endpoint = string.Empty;
                if (pongBaseAddress != null)
                {
                    endpoint = $"{pongBaseAddress}api/Pong/Peng";
                }

                var httpRequestMessage = new HttpRequestMessage(
                                HttpMethod.Get,
                                endpoint);

                //var httpClient = _httpClientFactory.CreateClient();
                HttpClient httpClient = new HttpClient(); //this is an alternative without HttpClientFactory

                int timeout = ConfigRoot.GetValue<int>("TimeoutMS", 0);

                if (timeout > 0)
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                }

                //var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                var httpResponseMessage = await httpClient.SendDLoopDAsync(httpRequestMessage, httpContextAccessor); //this is an alternative without HttpClientFactory

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    ret = $"Pang Call Number: {callNumber} - Peng Response: " + await httpResponseMessage.Content.ReadAsStringAsync();
                }
                else
                {
                    ret = $"Call To Peng Return Status Code: {httpResponseMessage.StatusCode}";
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
