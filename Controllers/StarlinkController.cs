using AlarmSystem.Models;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlarmSystem.Controllers
{
    [ApiController]
    [Route("api/starlink")]
    public class StarlinkController : ControllerBase
    {
        private readonly StarlinkApiService _apiService;
        private readonly ILogger<StarlinkController> _logger;

        public StarlinkController(StarlinkApiService apiService, ILogger<StarlinkController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test([FromBody] ApiTestRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Endpoint))
                return BadRequest(new { message = "Endpoint is required." });

            try
            {
                var result = await _apiService.CallEndpointAsync(req.Endpoint, req.Method ?? "GET", req.BearerToken, req.RequestBody);
                return Ok(new
                {
                    status = result.Status,
                    statusText = result.StatusText,
                    responseTime = result.ResponseTime,
                    timestamp = result.Timestamp,
                    data = result.Data,
                    rawBody = result.RawBody
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for {Endpoint}", req.Endpoint);
                return StatusCode(502, new { message = "Upstream request failed", detail = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Starlink auth configuration missing for {Endpoint}", req.Endpoint);
                return BadRequest(new
                {
                    message = "Starlink authentication is not configured.",
                    detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Starlink test failed for {Endpoint}", req.Endpoint);
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}
