using Glink.Demo;
using Glink.Demo.Sdk.Grpc;
using Glink.Runtime.Application.Contracts;
using Glink.Runtime.Application.Contracts.CalculationManager;
using Glink.Runtime.Application.Contracts.DataCenter;
using Glink.Runtime.Application.Contracts.MessageCenter;
using Glink.Runtime.Application.Contracts.Pipline;
using Glink.Runtime.Application.Contracts.PlaybackCenter;
using Glink.Runtime.CalculationManager;
using Glink.Runtime.DataCenter;
using Glink.Runtime.DataCenter.MetaData;
using Glink.Runtime.MessageCenter;
using Glink.Runtime.PlaybackCenter;
using Glink.WebApi.BackgroundServices;
using Grpc.Core;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var path = Path.Combine(Environment.CurrentDirectory, "Glink.WebApi.xml");
    c.IncludeXmlComments(path, true);
});
builder.Services.AddSingleton<MetaDataCenter>();
builder.Services.AddSingleton<RawDataCenter>();
builder.Services.AddSingleton<IMessageCenter<ICalculationPipline>, MessageCenter<ICalculationPipline>>();
builder.Services.AddSingleton<IMessagePublisher<ICalculationPipline>, MessagePublisher<ICalculationPipline>>();
builder.Services.AddSingleton<IMessageSubscriber<ICalculationPipline>, MessageSubscriber<ICalculationPipline>>();
builder.Services.AddSingleton<ICalculationPiplineManager, CalculationPiplineManager>();
builder.Services.AddSingleton<IDataProducer<MetaDataCenter>, DataProducer<MetaDataCenter>>();
builder.Services.AddSingleton<IDataConsumer<MetaDataCenter>, DataConsumer<MetaDataCenter>>();
builder.Services.AddSingleton<IDataProducer<RawDataCenter>, DataProducer<RawDataCenter>>();
builder.Services.AddSingleton<IDataConsumer<RawDataCenter>, DataConsumer<RawDataCenter>>();
builder.Services.AddSingleton<IPlaybackCenter, PlaybackCenter>();
builder.Services.AddGrpcClient<Greeter.GreeterClient>(nameof(Greeter.GreeterClient), o =>
{
    o.Address = new Uri("http://localhost:5100");
});
builder.Services.AddSingleton(t => Channel.CreateUnbounded<(string, byte[])>());

builder.Services.AddHostedService<DemoMetaDataPushBackgroundService>();
builder.Services.AddHostedService<RawDataConsumerBackgroundService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
