using System;
using System.Collections.Generic;
using System.Text;

namespace Geojam.Api.Tests
{
    public class BridgesServiceData
    {
        public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { 1, new int[] { 3 }, 8 },
            new object[] { 2, new int[] { 1, 3 }, 22 },
            new object[] { 3, new int[] { 1, 2, 3 }, 133 },
            new object[] { 4, null, 133 },
        };
    }
}
