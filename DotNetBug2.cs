using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

[SimpleJob(RunStrategy.Monitoring, launchCount: 1, iterationCount: 5, warmupCount: 2)]
public class HashTableBenchmark
{
    [Params(0.5f)]
    public float Load { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rnd = new Random(3);
        var uni = new HashSet<uint>();

        while (uni.Count < HashTable.Capacity)
        {
            uni.Add((uint)rnd.NextInt64());
        }

        uni.ToArray().AsSpan().CopyTo(data);
    }

    private readonly uint[] data = new uint[HashTable.Capacity];

    private readonly HashTable ht = new HashTable();

    [IterationSetup]
    public void IterationSetup()
    {
        ht.Clear();
    }

    [Benchmark(Baseline = true)]
    public int AddOptimal()
    {
        foreach(var key in data.AsSpan()[..(int)(HashTable.Capacity * Load)])
        {
            ht.AddOptimal(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddOptimalFix1()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(HashTable.Capacity * Load)))
        {
            ht.AddOptimalFix1(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddOptimalFix2()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(HashTable.Capacity * Load)))
        {
            ht.AddOptimalFix1(key);
        }

        return ht.Count;
    }
}

class HashTable
{
    public const uint Capacity = 32 * 1024 * 1024, _lengthMinusOne = Capacity - 1;
    public HashTable()
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

    public int Count { get; private set; } = 0;

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


    /// <summary>
    /// slow down on 'OLD' CPU (: but fast on intel 12XXX+ & AMD 9700X
    /// </summary>
    /// <param name="key"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOptimal(uint key)
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
    public void AddOptimalFix1(uint key)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOptimalFix2(uint key)
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
}
