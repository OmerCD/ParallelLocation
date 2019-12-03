using System;
using System.Collections.Generic;
using MessageObjectRouter;
using Microsoft.Extensions.DependencyInjection;
using Parallel.Location;

namespace Parallel.Main.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddLocationCalculatorRouter(this IServiceCollection services)
        {
            services.AddLocationCalculatorRouter(builder =>
            {
                builder.AddLocationCalculator<int>(new GradientDescent());
            });
            return services;
        }

        public static IServiceCollection AddElectorParser(this IServiceCollection services)
        {
            services.AddKeyExtractor<IKeyExtractor<byte[], byte[]>, ByteKeyExtractor, byte, int>(builder =>
            {
                builder.AddKeyInfo(98, 2);
                builder.AddKeyInfo(4, 1);
            });

            services.AddRouteParser<ByteParseRouter<byte[]>, byte[], byte[]>(builder =>
            {
                builder.UseKeyExtractor(services.BuildServiceProvider()
                    .GetRequiredService<IKeyExtractor<byte[], byte[]>>());
                builder.AddType(new byte[] {98, 9}, typeof(int));
            });
            return services;
        }
    }
}