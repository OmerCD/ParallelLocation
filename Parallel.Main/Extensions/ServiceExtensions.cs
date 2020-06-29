using System;
using System.Collections.Generic;
using System.Linq;
using Fingerprinting;
using MessageObjectRouter;
using Microsoft.Extensions.DependencyInjection;
using Parallel.Location;
using Parallel.Location.ParticleAreaFilter;
using Parallel.Location.ParticleFilter;
using Parallel.Shared.DataTransferObjects;
using TeltonikaParser;

namespace Parallel.Main.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddLocationCalculatorRouter(this IServiceCollection services)
        {
            var locationBuilder = new LocationCalculatorBuilder<GradientDescent, Anchor>();
            var anchors = new IAnchor[]
            {
                new Anchor(9, 60, 27, 512, 0),
                new Anchor(9, 7, 27, 513, 0),
                new Anchor(35, 96, 27, 514,0),
                new Anchor(99, 89, 27, 515,0),
                new Anchor(145, 66, 27, 516,0),
                new Anchor(160, 9, 27, 517,0),
                new Anchor(97, 8, 27, 518,0),
                new Anchor(56, 9, 27, 519,0),
            };
            // GradientDescent gradientDescent = locationBuilder.WithAnchors(
            //         new Anchor(53.8, 73.4, 92, 208),
            //         new Anchor(905.8, 0, 1.5, 211),
            //         new Anchor(0, 458.5, 76.4, 204),
            //         new Anchor(925.3, 398.3, 92, 206))
            //     .Build();
            GradientDescent gradientDescent = locationBuilder.WithAnchors(anchors).Build();
            
            var particleFilterBuilder = new LocationCalculatorBuilder<Location.ParticleFilter.ParticleFilter, Anchor>(new Location.ParticleFilter.ParticleFilter(3500, 30));
            var particleFilter = particleFilterBuilder.WithAnchors(anchors).Build();
            
            var particleFilter3DBuilder = new LocationCalculatorBuilder<ParticleFilter3D, Anchor>(new ParticleFilter3D(3500, 30));
            var particleFilter3D = particleFilter3DBuilder.WithAnchors(anchors).Build();
            
            var particleAreaFilterBuilder = new LocationCalculatorBuilder<ParticleAreaFilter, Anchor>(new ParticleAreaFilter(3500, 30));
            var particleAreaFilter = particleAreaFilterBuilder.WithAnchors(anchors).Build();
            
            var comexBuilder = new LocationCalculatorBuilder<ComexCalculator, Anchor>();
            var comex = comexBuilder.WithAnchors(anchors).Build();

            services.AddLocationCalculatorRouter(builder =>
            {
                builder.AddLocationCalculator<MessageType4>(particleFilter);
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
            });

            services.AddRouteParser<ByteParseRouter<byte[]>, byte[], byte[]>(builder =>
            {
                builder.UseKeyExtractor(services.BuildServiceProvider()
                    .GetRequiredService<IKeyExtractor<byte[], byte[]>>());
                builder.AddType(new byte[] {4}, typeof(MessageType4));
                builder.AddType(new byte[] {98, 9}, typeof(GenericPacketSubtype9));
                builder.AddType(new byte[] {113}, typeof(CanBusInfo));
                builder.AddType(new byte[] {98, 55}, typeof(SRfGenericPacket));
                builder.AddType(new byte[] {0}, typeof(SMobileConfig));
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