using AurSystem.Framework;
using AurSystem.Framework.Models.Options;
using AurSystem.Framework.Services;
using AurSystem.Framework.Configuration;
using AurSystem.Framework.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using OrderService.Api;
using OrderService.Api.Integrations.Courier.Activities;
using OrderService.Api.Integrations.Futures;
using OrderService.Api.Integrations.ItineraryPlanners;

var builder = WebApplication.CreateBuilder(args);

// configure builder
builder.Services.AddLogging(x => x.AddConsole());
builder.Services.Configure<SupabaseConfig>(builder.Configuration.GetSection("Supabase"));
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddSingleton<MessageMapper>();
builder.Services.AddSingleton<SupabaseClient>();
builder.Services.TryAddScoped<IItineraryPlanner<SubmitOrder>, OrderItineraryPlanner>();

// mass transit
builder.Services.AddMassTransit(x =>
{
    
    x.ApplyCustomMassTransitConfiguration();
    x.AddDelayedMessageScheduler();
    x.AddActivitiesFromNamespaceContaining<OrderActivity>();
    x.AddFuturesFromNamespaceContaining<OrderFuture>();

    x.AddSagaRepository<FutureState>()
        .InMemoryRepository();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.AutoStart = true;
        cfg.ApplyCustomBusConfiguration();
        
        cfg.UseDelayedMessageScheduler();
        cfg.ConfigureEndpoints(context);
    });
});

// swagger config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Service API", Version = "v1" });    
});

// API endpoints
builder.Services.AddEndpointDefinitions(typeof(Program));

// configure app
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API V1");
});

app.UseEndpointDefinitions();

app.Run();


/*



            // order
            //services.AddGenericRequestClient();
            services.AddMassTransit(x =>
            {              
                x.ApplyCustomMassTransitConfiguration();
                x.AddDelayedMessageScheduler();
                        
                x.AddConsumers(Assembly.GetExecutingAssembly());
                x.AddActivities(Assembly.GetExecutingAssembly());
                x.AddRequestClient<TakeProductTransactionMessage>();
                //x.SetKebabCaseEndpointNameFormatter();
                x.AddSagaStateMachine<OrderStateMachine, OrderState>()                
                    .InMemoryRepository();
                x.AddSagaStateMachine<OrderCourierStateMachine, OrderTransactionState>()
                    .InMemoryRepository();
                    
                // x.AddConsumersFromNamespaceContaining<CookOnionRingsConsumer>();
                // x.AddActivitiesFromNamespaceContaining<GrillBurgerActivity>();
                // x.AddFuturesFromNamespaceContaining<OrderFuture>();
                // x.AddSagaRepository<FutureState>()
                //     .EntityFrameworkRepository(r =>
                //     {
                //         r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                //         r.LockStatementProvider = new SqlServerLockStatementProvider();
                //
                //         r.ExistingDbContext<ForkJointSagaDbContext>();
                //     });
                    
                    
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.AutoStart = true;
                    cfg.ApplyCustomBusConfiguration();
                    cfg.UseDelayedMessageScheduler();
                    cfg.ConfigureEndpoints(context);                
                });
            })
            .AddMassTransitHostedService();            
 */
 
