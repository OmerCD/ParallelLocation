using System;
using System.Collections.Generic;
using System.Linq;
using MessageObjectRouter;
using Microsoft.Extensions.DependencyInjection;
using Parallel.Location;
using Parallel.Shared.DataTransferObjects;
using TeltonikaParser;

namespace Parallel.Main.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddLocationCalculatorRouter(this IServiceCollection services)
        {
            var locationBuilder = new LocationCalculatorBuilder<GradientDescent, Anchor>();
            GradientDescent gradientDescent = locationBuilder.WithAnchors(
                    new Anchor(6, 12, 0, 201),
                    new Anchor(6, 0, 0, 211),
                    new Anchor(8, 0, 0, 102),
                    new Anchor(4, 12, 0, 103),
                    new Anchor(0, 2, 0, 601))
                .Build();
            services.AddLocationCalculatorRouter(builder =>
            {
                builder.AddLocationCalculator<MessageType4>(gradientDescent);
            });
            return services;
        }

        public static IServiceCollection AddElectorParser(this IServiceCollection services)
        {
            services.AddKeyExtractor<IKeyExtractor<byte[], byte[]>, ByteKeyExtractor, byte, int>(builder =>
            {
                builder.AddKeyInfo(4, 1);
                builder.AddKeyInfo(98, 2);
                builder.AddKeyInfo(113, 1);
                builder.AddKeyInfo(0, 4);
            });

            services.AddRouteParser<ByteParseRouter<byte[]>, byte[], byte[]>(builder =>
            {
                builder.UseKeyExtractor(services.BuildServiceProvider()
                    .GetRequiredService<IKeyExtractor<byte[], byte[]>>());
                builder.AddType(new byte[] {4}, typeof(MessageType4));
                builder.AddType(new byte[] {98, 9}, typeof(GenericPacketSubtype9));
                builder.AddType(new byte[] {113}, typeof(CanBusInfo));

                builder.UseComparer(new ByteArrayComparer());
            });
            return services;
        }

        public static IServiceCollection AddElectorTeltonikaParser(this IServiceCollection services)
        {
            services.AddKeyExtractor<IKeyExtractor<byte[], byte[]>, ByteKeyExtractor, byte, int>(builder =>
            {
                builder.AddKeyInfo(0, 4);
            });
            services.AddRouteParser<ByteParseRouter<byte[]>, byte[], byte[]>(builder =>
            {
                builder.UseKeyExtractor(services.BuildServiceProvider()
                    .GetRequiredService<IKeyExtractor<byte[], byte[]>>());
                builder.AddType(new byte[] {0, 0, 0, 0}, typeof(List<Position>));

                builder.UseComparer(new ByteArrayComparer());
            });
            return services;
        }

        public class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                if (left == null || right == null)
                {
                    return left == right;
                }

                if (left.Length != right.Length)
                {
                    return false;
                }

                for (int i = 0; i < left.Length; i++)
                {
                    if (left[i] != right[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                return key.Aggregate(0, (current, cur) => current + cur);
            }
        }
    }
}