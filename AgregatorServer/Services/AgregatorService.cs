using AgregatorServer.Controllers.Models;
using AgregatorServer.Helpers;
using Newtonsoft.Json;
using RestSharp;

namespace AgregatorServer.Services;

public class AgregatorService
{
    private readonly Queue<News> _queue;
    private readonly Queue<ProcessedNews> _processedQueue;
    private readonly ILogger<AgregatorService> _logger;
    public Mutex mutex { get; set; }
    public Mutex mutex1 { get; set; }

    public AgregatorService(ILogger<AgregatorService> logger)
    {
        _logger = logger;
        _queue = new Queue<News>();
        mutex = new Mutex();
        mutex1 = new Mutex();
        Run();
    }

    public void  Run()
    {
        for (int i = 0; i < 5; i++)
        {
            Task.Run(SendFurther);
        }
        for (int i = 0; i < 5; i++)
        {
            Task.Run(SendBackToProducer);
        }
    }

    public async Task SendFurther()
    {
        while (true)
        {
            mutex.WaitOne();
            if (_queue.Count !=0)
            {
                _queue.TryDequeue(out News letter);
                var proccessed = new ProcessedNews()
                {
                    message = letter.Message,
                    index = Enumerator.Next()
                };
                var client = new RestClient("http://localhost:5168");
                var serializedLetter = JsonConvert.SerializeObject(proccessed);
                var request = new RestRequest("/api/send/to/consumer",Method.Post);
                request.AddJsonBody(serializedLetter);
                var result = client.ExecuteAsync(request);
            }
            mutex.ReleaseMutex();
        }
    }

    public  Task SendBackToProducer()
    {
        while (true)
        {
            mutex.WaitOne();
            if (_processedQueue.Count !=0)
            {
                _processedQueue.TryDequeue(out ProcessedNews news);
                var client = new RestClient("http://localhost:5000");
                var serializedLetter = JsonConvert.SerializeObject(news);
                var request = new RestRequest("/api/send/to/producer",Method.Post);
                request.AddJsonBody(serializedLetter); 
                client.ExecuteAsync(request);
            }
            mutex.ReleaseMutex();
        }
    }

    public void Enqueue(News news)
    {
        mutex.WaitOne();
        _queue.Enqueue(news);
        mutex.ReleaseMutex();
        
    }

    public void EnqueueResponse(ProcessedNews news)
    {
        mutex1.WaitOne();
        if (news is not null)
        {
            _processedQueue.Enqueue(news);
        }
        mutex1.ReleaseMutex();
    }
}