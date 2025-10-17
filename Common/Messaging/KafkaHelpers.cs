using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.Messaging
{
    public static class KafkaSer
    {

        static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        { 
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        };

        public static string Serialize<T>(T value)=>JsonSerializer.Serialize(value, JsonOpts);  
        public static T? Deserialize<T>(ReadOnlySpan<byte> data) => JsonSerializer.Deserialize<T>(data, JsonOpts);

    }

    public sealed class KafkaProducer<T> : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaProducer(string bootstrapServers, string topic, string? clientId = null)
        {
            var cfg = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                LingerMs = 5,
                BatchSize = 64_000,
                MessageTimeoutMs = 5000,        // 5-second timeout
                SocketTimeoutMs = 5000,
                MessageSendMaxRetries = 5,
                CompressionType = CompressionType.Snappy,
                ClientId = clientId ?? $"producer-{typeof(T).Name.ToLower()}"

            };
            _producer=new ProducerBuilder<string, string>(cfg).Build(); 
            _topic=topic;   
        }


        public async Task ProduceAsync(string key, T value, CancellationToken ct)
        {
            var payload = KafkaSer.Serialize(value);
            var msg = new Message<string, string> { Key = key, Value = payload };
            var res = await _producer.ProduceAsync(_topic, msg, ct);
        }
        public void Dispose() =>_producer.Flush(TimeSpan.FromSeconds(5));
    }

    public sealed class KafkaConsumer<T>:IDisposable
    {
        private readonly IConsumer<string, string> _consumer;

        public KafkaConsumer(string bootstrapServers,string groupId)
        {
            var cfg = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                PartitionAssignmentStrategy=PartitionAssignmentStrategy.CooperativeSticky
            };

            _consumer=new ConsumerBuilder<string, string>(cfg).Build();


        }

        public void Subscribe(params string[] topics) => _consumer.Subscribe(topics);   

        public (ConsumeResult<string, string> cr,T? value) Consume(CancellationToken ct)
        {
            var cr=_consumer.Consume(ct);
            var value = KafkaSer.Deserialize<T>(Encoding.UTF8.GetBytes(cr.Message.Value));
           return (cr, value);
        }


        public void Commit(ConsumeResult<string,string> cr)=> _consumer.Commit(cr);

        public void close() => _consumer.Close();
        public void Dispose() => _consumer.Dispose();
    }


}
