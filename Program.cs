using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ShopbridgeEventProcessor.Data.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShopbridgeEventProcessor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace ShopbridgeEventProcessor
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) => { });

        static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .Build();            

            NpgsqlConnectionStringBuilder connBuilder = new NpgsqlConnectionStringBuilder();
            connBuilder.Host = Environment.GetEnvironmentVariable("DB_HOST");
            connBuilder.Database= Environment.GetEnvironmentVariable("DATABASE");
            connBuilder.Username = Environment.GetEnvironmentVariable("USERNAME");            
            connBuilder.Password = Environment.GetEnvironmentVariable("PASSWORD");
            connBuilder.Port = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT"));
            connBuilder.Pooling = true;
                             
            Console.WriteLine(connBuilder.ConnectionString);

            var sc = new ServiceCollection()
            .AddDbContext<ShopbridgeContext>(options =>
                    options.UseNpgsql(connBuilder.ConnectionString));

            var serviceProvider = sc.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<ShopbridgeContext>();
            var repo = new Repository(dbContext);
        
            var factory = new ConnectionFactory() { HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "shopbridgequeue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine(" [x] Received {0}", message);

                    Product product = JsonConvert.DeserializeObject<Product>(message);
                    repo.AddorUpdateProduct(product);

                };
                channel.BasicConsume(queue: "shopbridgequeue", autoAck: true, consumer: consumer);

                builder.Run();
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }            
        }
    }
}