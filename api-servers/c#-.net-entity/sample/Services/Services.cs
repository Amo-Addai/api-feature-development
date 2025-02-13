using System;
using System.Text;
using System.Text.Json;
// using System.Collections.Generic;
using System.Linq;
// using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ChatGPTBackend.Data;
using ChatGPTBackend.Models;


namespace ChatGPTBackend.Services
{

    public class DbSeeder
    {
        private readonly IServiceProvider _serviceProvider;

        public DbSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Seed()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (dbContext?.Users != null)
            {
                if (dbContext.Users.Any())
                {
                    Console.WriteLine("Users Table has already been seeded");
                }
                else
                {
                    dbContext.Users.AddRange(
                        new User { Username = "user1", Password = "password1" },
                        new User { Username = "user2", Password = "password2" }
                    );
                    dbContext.SaveChanges();
                }
            }
            else Console.WriteLine("Database has no User collection");

            if (dbContext?.Queries != null)
            {
                if (dbContext.Queries.Any())
                {
                    Console.WriteLine("Queries Table has already been seeded. Clearing existing data and re-seeding ...");

                    // Remove all existing queries from the table
                    dbContext.Queries.RemoveRange(dbContext.Queries);
                }
                dbContext.Queries.AddRange(
                    new Sample { SampleText = "sample1", ResponseText = "response1", UserId = "1" },
                    new Sample { SampleText = "sample2", ResponseText = "response2", UserId = "2" }
                );
                dbContext.SaveChanges();
            }
            else Console.WriteLine("Database has no Sample collection");
        }
    }

    public interface IDataService<TEntity>
    {
        IEnumerable<TEntity> GetAll();
        TEntity? GetById(object id);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete(object id);
    }

    public class DataService<TEntity> : IDataService<TEntity> where TEntity : class
    {
        private readonly AppDbContext _context;

        public DataService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _context.Set<TEntity>();
        }

        public TEntity? GetById(object id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public void Insert(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            _context.SaveChanges();
        }

        public void Delete(object id)
        {
            TEntity? entityToDelete = _context.Set<TEntity>().Find(id);
            if (entityToDelete != null)
            {
                _context.Set<TEntity>().Remove(entityToDelete);
                _context.SaveChanges();
            }
        }
    }

    public interface IUserService
    {
        User? Authenticate(string username, string password);
        User? GetUserById(string id);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext? _context;
        private readonly DataService<User> _dataService;

        public UserService(AppDbContext context)
        {
            _context = context;
            _dataService = new DataService<User>(_context);
        }

        public User? Authenticate(string username, string password)
        {
            return _dataService.GetAll().FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public User? GetUserById(string id)
        {
            return _dataService.GetById(Convert.ToInt32(id));
        }
    }

    public interface ISampleService
    {
        IEnumerable<Sample>? GetUserQueries(string userId);
        void SaveSample(Sample sample);
    }

    public class SampleService : ISampleService
    {
        private readonly AppDbContext? _context;
        private readonly DataService<Sample> _dataService;

        public SampleService(AppDbContext context)
        {
            _context = context;
            _dataService = new DataService<Sample>(_context);
        }

        public IEnumerable<Sample>? GetUserQueries(string userId)
        {
            // return _dataService.GetAll().Where(q => q.UserId == userId) / .Select(q => q);;
            return from q in _dataService.GetAll() ?? Array.Empty<Sample>() where q.UserId == userId select q;
        }

        public void SaveSample(Sample sample)
        {
            _dataService.Insert(sample);
        }
    }

    public interface IRequestService
    {
        Task<string?> GetGptResponse(string sample);
    }

    public class RequestService : IRequestService
    {
        private readonly HttpClient _httpClient;
        
        public RequestService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string?> GetGptResponse(string sample)
        {
            const string apiUrl = "https://api.openai.com/v1";
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "NO_API_KEY";
            // Console.WriteLine(apiKey);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            // Define request payload
            var requestPayload = new
            {
                model = "gpt-3.5-turbo", 
                messages = new[] { new { role = "user", content = sample } },
                temperature = 0.7,
                // todo: deprecated props
                // "text-davinci-003", - deprecated model
                // prompt = sample,
                // max_tokens = 7,
            };
            // Console.WriteLine(JsonSerializer.Serialize(requestPayload));

            var response = await _httpClient.PostAsJsonAsync($"{apiUrl}/chat/completions", requestPayload);

            /* // todo: manual json serialize option

            // Serialize request payload
            var jsonRequest = JsonSerializer.Serialize(requestPayload);

            // Define request content
            var content = new StringContent(jsonRequest, Encoding.UTF8, new MediaTypeHeaderValue("application/json")?.MediaType ?? "application/octet-stream");

            // Send request to ChatGPT API
            var response = await _httpClient.PostAsync($"{apiUrl}/chat/completions", content);

            */

            // Console.WriteLine(response);
            
            // Check if request was successful
            if (response.IsSuccessStatusCode)
            {
                // Read response content
                var jsonResponse = await response.Content.ReadAsStringAsync(); // todo: test both
                // var jsonResponse = await response.Content.ReadAsStringAsync<dynamic>();

                // Console.WriteLine(jsonResponse);

                // Deserialize response
                var responseData = JsonSerializer.Deserialize<ChatGptResponse>(jsonResponse);

                if (responseData != null)
                {
                    // Console.WriteLine(responseData);

                    string? responseText;

                    try
                    {
                        responseText = responseData?.choices?[0]?.message?.content?.Trim();
                        // Console.WriteLine($"{responseText ?? "-"}");

                        return responseText;

                    } // todo: for debugging purposes only; remove when not required anymore
                    catch (Exception e)
                    {
                        Console.WriteLine($"\nChatGPT Sample Failed: {e.Message}\n");
                        Console.WriteLine($"\nStackTrace: {e.StackTrace}\n");
                    }
                }
            }

            return null;

        }
    }

    public class ChatGptResponse
    {
        public List<ChatGptChoice>? choices { get; set; }
        // public ChatGptChoice[]? choices { get; set; }
    }

    public class ChatGptChoice
    {
        public ChatGptMessage? message { get; set; }
    }

    public class ChatGptMessage
    {
        public string? content { get; set; }
    }

    /** Sample ChatGptResponse; 
    
    Sample - Tell me everything you know about C# Entity Server and Flutter

    {
        "id": "chatcmpl-8vkNKDhUNkHub8qJWGqVB2i47OIiY",
        "object": "chat.completion",
        "created": 1708773654,
        "model": "gpt-3.5-turbo-0125",
        "choices": [
            {
            "index": 0,
            "message": {
                "role": "assistant",
                "content": "C# Entity Server is a framework developed by Microsoft for building data-driven applications. It is part of the .NET ecosystem and is used to create web services and APIs that interact with databases. C# Entity Server allows developers to define data models, sample databases, and perform CRUD operations easily.\n\nFlutter, on the other hand, is an open-source UI software development kit created by Google for building natively compiled applications for mobile, web, and desktop from a single codebase. Flutter uses the Dart programming language and provides a rich set of pre-built widgets and tools for building beautiful and fast user interfaces.\n\nIn summary, C# Entity Server is used for creating backend services and APIs, while Flutter is used for building cross-platform user interfaces. Developers can use these two technologies together to create full-stack applications that are both performant and visually appealing."
            },
            "logprobs": null,
            "finish_reason": "stop"
            }
        ],
        "usage": {
            "prompt_tokens": 19,
            "completion_tokens": 168,
            "total_tokens": 187
        },
        "system_fingerprint": "fp_86156a94a0"
    }

    */

}
