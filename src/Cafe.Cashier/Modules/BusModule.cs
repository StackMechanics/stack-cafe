using System;
using Autofac;
using Cafe.Messages;
using Nimbus.Configuration;
using Nimbus.Infrastructure;
using Nimbus.InfrastructureContracts;
using Nimbus.Logger.Serilog.Configuration;
using Nimbus.Serializers.Json.Configuration;
using Nimbus.Transports.Redis;

namespace Cashier.Modules
{
    public class BusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            
            var handlerTypesProvider = new AssemblyScanningTypeProvider(typeof(BusModule).Assembly, typeof(PlaceOrderCommand).Assembly);

            
            builder.RegisterNimbus(handlerTypesProvider);
            builder.Register(componentContext => new BusBuilder()
                                                 .Configure()
                                                 .WithTransport(
                                                     new RedisTransportConfiguration().WithConnectionString("localhost")
                                                     )
                                                 .WithNames("Cashier", Environment.MachineName)
                                                 .WithTypesFrom(handlerTypesProvider)
                                                 .WithAutofacDefaults(componentContext)
                                                 .WithSerilogLogger()
                                                 .WithJsonSerializer()
//                                                 .WithGlobalInboundInterceptorTypes(typeof(CorrelationIdInterceptor))
                                                 .Build())
                   .As<IBus>()
                   .AutoActivate()
                   .OnActivated(c => c.Instance.Start())
                   .OnRelease(bus =>
                              {
                                  bus.Stop().Wait();
                                  bus.Dispose();
                              })
                   .SingleInstance();
        }
    }
}