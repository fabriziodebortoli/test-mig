using System;
using System.Text;
using RabbitMQ.Client;


namespace Microarea.TaskBuilderNet.Core.RabbitMQManager
{
    //===================================================================================
    public static class RabbitMQManager
    {
        public static bool PublishMessage(string hostname, int port, string queue, string message)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = hostname, Port = port };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var body = Encoding.UTF8.GetBytes(message);
                    var properties = channel.CreateBasicProperties();
                    channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: properties, body: body);
                }
            }
            catch (Exception exx)
            {
                string error = exx.Message;
                return false;
            }

            return true;
        }
    }
}
