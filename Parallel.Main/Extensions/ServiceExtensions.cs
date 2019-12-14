using System;
using System.Collections.Generic;
using System.Linq;
using MessageObjectRouter;
using Microsoft.Extensions.DependencyInjection;
using Parallel.Location;
using Parallel.Shared.DataTransferObjects;

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
                builder.AddKeyInfo(4, 1);
                builder.AddKeyInfo(98, 2);
            });

            services.AddRouteParser<ByteParseRouter<byte[]>, byte[], byte[]>(builder =>
            {
                builder.UseKeyExtractor(services.BuildServiceProvider()
                    .GetRequiredService<IKeyExtractor<byte[], byte[]>>());
                builder.AddType(new byte[] {4}, typeof(MessageType4));
                builder.AddType(new byte[] {98, 9}, typeof(GenericPacketSubtype9));

                builder.UseComparer(new ByteArrayComparer());
            });
            return services;
        }
        public class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right) {
                if ( left == null || right == null ) {
                    return left == right;
                }
                if ( left.Length != right.Length ) {
                    return false;
                }
                for ( int i= 0; i < left.Length; i++) {
                    if ( left[i] != right[i] ) {
                        return false;
                    }
                }
                return true;
            }
            public int GetHashCode(byte[] key) {
                if (key == null)
                    throw new ArgumentNullException("key");
                int sum = 0;
                foreach ( byte cur in key ) {
                    sum += cur;
                }
                return sum;
            }
        }
    }
}