using System;
using System.Collections.Generic;
using System.Linq;

namespace MismeAPI.Service.Utils
{
    internal static class Extensions
    {
        private static readonly Lazy<RandomSecureVersion> RandomSecure =
          new Lazy<RandomSecureVersion>(() => new RandomSecureVersion());

        public static IEnumerable<T> ShuffleSecure<T>(this IEnumerable<T> source)
        {
            var sourceArray = source.ToArray();
            for (int counter = 0; counter < sourceArray.Length; counter++)
            {
                int randomIndex = RandomSecure.Value.Next(counter, sourceArray.Length);
                yield return sourceArray[randomIndex];

                sourceArray[randomIndex] = sourceArray[counter];
            }
        }

        public static string ShuffleTextSecure(this string source)
        {
            var shuffeldChars = source.ShuffleSecure().ToArray();
            return new string(shuffeldChars);
        }

        public static DateTimeOffset ToDateTimeOffset(this DateTime dt, TimeZoneInfo tz)
        {
            if (dt.Kind != DateTimeKind.Unspecified)
            {
                // Handle UTC or Local kinds (regular and hidden 4th kind)
                DateTimeOffset dto = new DateTimeOffset(dt.ToUniversalTime(), TimeSpan.Zero);
                return TimeZoneInfo.ConvertTime(dto, tz);
            }

            if (tz.IsAmbiguousTime(dt))
            {
                // Prefer the daylight offset, because it comes first sequentially (1:30 ET becomes
                // 1:30 EDT)
                TimeSpan[] offsets = tz.GetAmbiguousTimeOffsets(dt);
                TimeSpan offset = offsets[0] > offsets[1] ? offsets[0] : offsets[1];
                return new DateTimeOffset(dt, offset);
            }

            if (tz.IsInvalidTime(dt))
            {
                // Advance by the gap, and return with the daylight offset (2:30 ET becomes 3:30 EDT)
                TimeSpan[] offsets = { tz.GetUtcOffset(dt.AddDays(-1)), tz.GetUtcOffset(dt.AddDays(1)) };
                TimeSpan gap = offsets[1] - offsets[0];
                return new DateTimeOffset(dt.Add(gap), offsets[1]);
            }

            // Simple case
            return new DateTimeOffset(dt, tz.GetUtcOffset(dt));
        }
    }
}
