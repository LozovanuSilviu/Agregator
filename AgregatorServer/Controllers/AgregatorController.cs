using AgregatorServer.Controllers.Models;
using AgregatorServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgregatorServer.Controllers
{
    [ApiController]
    [Route("api")]

    public class ConsumerController : Controller
    {
        private readonly ILogger<ConsumerController> _logger;
        private readonly AgregatorService _service;

        public ConsumerController(ILogger<ConsumerController> logger, AgregatorService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost("send/to/aggregator")]
        public IActionResult SendToConsumer(News news)
        {
            _logger.LogInformation($"Breaking news for your attention '{news.Message}'");
            _service.Enqueue(news);
            return Ok();
        }  
        
        [HttpPost("send/back/to/aggregator")]
        public IActionResult SendBackToAggregator(ProcessedNews news)
        {
            _logger.LogInformation($"Breaking news for your attention '{news.message} with index {news.index} was received back'");
            _service.EnqueueResponse(news);
            return Ok();
        }
    }
}