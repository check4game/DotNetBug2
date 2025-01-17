using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

BenchmarkRunner.Run(typeof(Program).Assembly);

[SimpleJob(RunStrategy.Monitoring, launchCount: 1, iterationCount: 10, warmupCount: 5, invocationCount: 1)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class HashTableBenchmark
{
    [Params(0.50f)]
    public float Load { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var rnd = new Random(3);

        var uni = new HashSet<uint>();

        while (uni.Count < ht.Capacity)
        {
            uni.Add((uint)rnd.NextInt64());
        }

        data = uni.ToArray();
    }

    private uint[] data = null!;

    private readonly HashTable ht = new HashTable();

    [IterationSetup]
    public void IterationSetup()
    {
        ht.Clear();
    }

    [Benchmark(Baseline = true)]
    public int AddOptimal()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddOptimal(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddOptimalFix1()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddOptimalFix1(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddOptimalFix2()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddOptimalFix1(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddOptimalFix3()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddOptimalFix3(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddOptimalCPU()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddOptimalCPU(key);
        }

        return ht.Count;
    }
}

class HashTable
{
    public HashTable(uint Capacity = 32 * 1024 * 1024)
    {
        _Capacity = Capacity;

        _lengthMinusOne = (_Capacity - 1);

        _controlBytes = GC.AllocateUninitializedArray<byte>((int)_Capacity + _vectorSize);

        _entries = GC.AllocateUninitializedArray<uint>((int)_Capacity + _vectorSize);

        Clear();
    }

    public uint Capacity => _Capacity;

    private uint _Capacity, _lengthMinusOne;

    private const byte _vectorSize = 16;

    private const byte _emptyBucket = byte.MinValue;

    private Vector128<byte> _emptyBucketVector = Vector128.Create(_emptyBucket);
    public void Clear()
    {
        Count = 0; _controlBytes.AsSpan().Fill(_emptyBucket);
    }

    private byte[] _controlBytes;

    private uint[] _entries;
    
    public int Count { get; private set; } = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ResetLowestSetBit(uint value) => value & (value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T Find<T>(T[] array, ulong index)
    {
        ref var arr0 = ref MemoryMarshal.GetArrayDataReference(array);
        return ref Unsafe.Add(ref arr0, (nuint)index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Find(byte[] array, ulong index)
    {
        ref var arr0 = ref MemoryMarshal.GetArrayDataReference(array);
        return ref Unsafe.AddByteOffset(ref arr0, (nuint)index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte FindValue(byte[] array, ulong index)
    {
        ref var arr0 = ref MemoryMarshal.GetArrayDataReference(array);
        return Unsafe.AddByteOffset(ref arr0, (nuint)index);
    }

    private const ulong _goldenRatio = 11400714819323198485UL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetHashCode(uint key) => ((uint)key.GetHashCode() * _goldenRatio);

    private static byte H2(ulong hashcode) => (byte)((hashcode >> 55) | 0x01);

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

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
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
                if (_emptyBucket != FindValue(_controlBytes, index))
                {
                    index += (uint)BitOperations.TrailingZeroCount(emptyMask);
                }

                Find(_controlBytes, index) = h2;
                Find(_entries, index) = key;

                Count++;

                return;
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
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
                while (_emptyBucket != FindValue(_controlBytes, index)) index++;

                Find(_controlBytes, index) = h2;
                Find(_entries, index) = key;

                Count++;

                return;
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOptimalFix3(uint key)
    {
        var index = GetHashCode(key);

        var h2 = H2(index);

        // Create a SIMD vector with the value of 'h2' for quick equality checks.
        var target = Vector128.Create(h2);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0;

        ref var controlBytes = ref MemoryMarshal.GetArrayDataReference(_controlBytes);

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            ref var cb = ref Unsafe.AddByteOffset(ref controlBytes, (nuint)index);

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = Vector128.Equals(Vector128.LoadUnsafe(in cb), target).ExtractMostSignificantBits();
            //var resultMask = Vector128.Equals(Unsafe.ReadUnaligned<Vector128<sbyte>>(in Unsafe.As<sbyte,byte>(ref cb)), target).ExtractMostSignificantBits();

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

            for (byte i = 0; i < _vectorSize; i++)
            {
                if (_emptyBucket == cb)
                {
                    cb = h2; Find(_entries, index + i) = key; Count++; return;
                }

                cb = Unsafe.AddByteOffset(ref cb, 1);
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOptimalCPU(uint key)
    {
        var index = GetHashCode(key);

        var h2 = H2(index);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0;

        ref var controlBytes = ref MemoryMarshal.GetArrayDataReference(_controlBytes);

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            ref var cb = ref Unsafe.AddByteOffset(ref controlBytes, (nuint)index);

            for (byte pos = 0; pos < _vectorSize; pos++)
            {
                if (h2 == cb && Find(_entries, index + pos) == key) return;

                cb = Unsafe.AddByteOffset(ref cb, 1);
            }

            cb = Unsafe.AddByteOffset(ref cb, -_vectorSize);

            for (byte pos = 0; pos < _vectorSize; pos++)
            {
                if (_emptyBucket == cb)
                {
                    cb = h2; Find(_entries, index + pos) = key; Count++; return;
                }

                cb = Unsafe.AddByteOffset(ref cb, 1);
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }
}
