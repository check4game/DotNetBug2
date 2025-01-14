using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

var rnd = new Random(3);
var uni = new HashSet<uint>();

while (uni.Count < HT.Capacity)
{
    uni.Add((uint)rnd.NextInt64());
}

var data = uni.ToArray();

var ht = new HT();

int NT = 5, NI = (int)(HT.Capacity / 2);

var sw = new Stopwatch[3];

for (int ii = 0; ii < sw.Length; ii++)
{
    sw[ii] = Stopwatch.StartNew();

    for (int i = 0; i < NT; i++)
    {
        ht.Clear();

        switch (ii)
        {
            case 0:
                foreach (var key in data.AsSpan().Slice(0, NI))
                {
                    ht.AddModernCPUSuperFastOldCPUSlow(key);
                }
                break;
            case 1:
                foreach (var key in data.AsSpan().Slice(0, NI))
                {
                    ht.AddOldCPUFast(key);
                }
                break;
            case 2:
                foreach (var key in data.AsSpan().Slice(0, NI))
                {
                    ht.AddOldCPUSuperFast(key);
                }
                break;
        }
    }

    sw[ii].Stop();

    switch (ii)
    {
        case 0:
            Console.Write("AddModernCPUSuperFastOldCPUSlow");
            break;
        case 1:
            Console.Write("AddOldCPUFast                  ");
            break;
        case 2:
            Console.Write("AddOldCPUSuperFast             ");
            break;

    }

    Console.WriteLine(" => time={0}, avg={1} ms", sw[ii].Elapsed, (long)(sw[ii].Elapsed.TotalMilliseconds / NT));
}

class HT
{
    public const uint Capacity = 32 * 1024 * 1024, _lengthMinusOne = Capacity - 1;
    public HT()
    {
        Clear();
    }

    public void Clear()
    {
        Count = 0; _controlBytes.AsSpan().Fill(_emptyBucket);
    }

    private sbyte[] _controlBytes = GC.AllocateUninitializedArray<sbyte>((int)Capacity + 16);

    private uint[] _entries = GC.AllocateUninitializedArray<uint>((int)Capacity + 16);

    private const sbyte _emptyBucket = -127;

    private static Vector128<sbyte> _emptyBucketVector = Vector128.Create(_emptyBucket);

    public uint Count { get; private set; } = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ResetLowestSetBit(uint value) => value & (value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T Find<T>(T[] array, ulong index)
    {
        ref var arr0 = ref MemoryMarshal.GetArrayDataReference(array);
        return ref Unsafe.Add(ref arr0, Unsafe.As<ulong, nuint>(ref index));
    }

    private const ulong _goldenRatio = 11400714819323198485;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetHashCode(uint key) => ((uint)key.GetHashCode() * _goldenRatio);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static sbyte H2(ulong hashcode) => (sbyte)(hashcode >> 57);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddModernCPUSuperFastOldCPUSlow(uint key)
    {
        var index = GetHashCode(key);

        var h2 = H2(index);

        // Create a SIMD vector with the value of 'h2' for quick equality checks.
        var target = Vector128.Create(h2);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0;

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            var source = Vector128.LoadUnsafe(ref Find(_controlBytes, index));

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = Vector128.Equals(source, target).ExtractMostSignificantBits();

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var bitPos = BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (Find(_entries, index + (uint)bitPos) == key)
                {
                    return;
                }

                //// Clear the lowest set bit in `mask` to continue with the next matching bit.
                resultMask = ResetLowestSetBit(resultMask);
            }

            var emptyMask = Vector128.Equals(source, _emptyBucketVector).ExtractMostSignificantBits();
            // Check for empty buckets in the current vector.
            if (emptyMask != 0)
            {
                // slow down on OLD_CPU (: but fast on intel 12XXX+ & AMD 9700X
                index += (uint)BitOperations.TrailingZeroCount(emptyMask);

                Find(_controlBytes, index) = h2;
                Find(_entries, index) = key;

                Count++;

                return;
            }

            jumpDistance += 16; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOldCPUSuperFast(uint key)
    {
        var index = GetHashCode(key);

        var h2 = H2(index);

        // Create a SIMD vector with the value of 'h2' for quick equality checks.
        var target = Vector128.Create(h2);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0;

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            var source = Vector128.LoadUnsafe(ref Find(_controlBytes, index));

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = Vector128.Equals(source, target).ExtractMostSignificantBits();

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var bitPos = BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (Find(_entries, index + (uint)bitPos) == key)
                {
                    return;
                }

                //// Clear the lowest set bit in `mask` to continue with the next matching bit.
                resultMask = ResetLowestSetBit(resultMask);
            }

            var emptyMask = Vector128.Equals(source, _emptyBucketVector).ExtractMostSignificantBits();
            // Check for empty buckets in the current vector.
            if (emptyMask != 0)
            {
                while (_emptyBucket != Find(_controlBytes, index)) index++;

                Find(_controlBytes, index) = h2;

                Find(_entries, index) = key;

                Count++;

                return;
            }

            jumpDistance += 16; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOldCPUFast(uint key)
    {
        var index = GetHashCode(key);

        var h2 = H2(index);

        // Create a SIMD vector with the value of 'h2' for quick equality checks.
        var target = Vector128.Create(h2);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0;

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            var source = Vector128.LoadUnsafe(ref Find(_controlBytes, index));

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = Vector128.Equals(source, target).ExtractMostSignificantBits();

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var bitPos = BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (Find(_entries, index + (uint)bitPos) == key)
                {
                    return;
                }

                //// Clear the lowest set bit in `mask` to continue with the next matching bit.
                resultMask = ResetLowestSetBit(resultMask);
            }

            var emptyMask = Vector128.Equals(source, _emptyBucketVector).ExtractMostSignificantBits();
            // Check for empty buckets in the current vector.
            if (emptyMask != 0)
            {
                if (_emptyBucket != Find(_controlBytes, index))
                {
                    index += (uint)BitOperations.TrailingZeroCount(emptyMask);
                }

                Find(_controlBytes, index) = h2;
                Find(_entries, index) = key;

                Count++;

                return;
            }

            jumpDistance += 16; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }
}
