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

namespace ShopbridgeEventProcessor
{
    public class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("appsettings.json", false)
            .Build();

            var conString = configuration.GetConnectionString("Shopbridge_Context");
            var sc = new ServiceCollection()
            .AddDbContext<ShopbridgeContext>(options =>
                    options.UseSqlServer(conString));

            var serviceProvider = sc.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<ShopbridgeContext>();
            var repo = new Repository(dbContext);

            var amqpSection = configuration.GetSection("amqp");

            var factory = new ConnectionFactory() { HostName = amqpSection.GetSection("hostname").Value };

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

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}