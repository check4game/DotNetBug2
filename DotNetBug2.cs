using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

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
    public int AddVectorCPUIndexOf()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddVectorCPUIndexOf(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddVector()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddVector(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddVectorIF()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddVectorIF(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddVectorWhile()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddVectorWhile(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddVectorCPUFor()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddVectorCPUFor(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddCPU()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddCPU(key);
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

        _controlBytes = GC.AllocateArray<byte>((int)_Capacity + _vectorSize);

        _entries = GC.AllocateUninitializedArray<uint>((int)_Capacity + _vectorSize);
    }

    public uint Capacity => _Capacity;

    private uint _Capacity, _lengthMinusOne;

    private const byte _vectorSize = 16;

    private const byte _emptyBucket = byte.MinValue;

    private Vector128<byte> _emptyBucketVector = Vector128.Create(_emptyBucket);
    public void Clear()
    {
        Count = 0; _controlBytes.AsSpan().Clear();
    }

    private byte[] _controlBytes;

    private uint[] _entries;
    
    public int Count { get; private set; } = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ResetLowestSetBit(uint value) => value & (value - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T GetReference<T>(T[] array, ulong index)
    {
        ref var arr0 = ref MemoryMarshal.GetArrayDataReference(array);
        return ref Unsafe.Add(ref arr0, (nuint)index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte GetReference(byte[] array, ulong index)
    {
        ref var arr0 = ref MemoryMarshal.GetArrayDataReference(array);
        return ref Unsafe.AddByteOffset(ref arr0, (nuint)index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetValue(byte[] array, ulong index)
    {
        ref var arr0 = ref MemoryMarshal.GetArrayDataReference(array);
        return Unsafe.AddByteOffset(ref arr0, (nuint)index);
    }

    private const ulong _goldenRatio = 11400714819323198485UL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong GetHashCode(uint key) => ((uint)key.GetHashCode() * _goldenRatio);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte H2(ulong hashcode) => (byte)((hashcode >> 56) | 0x01);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddVector(uint key)
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

            var source = Vector128.LoadUnsafe(ref GetReference(_controlBytes, index));

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = Vector128.Equals(source, target).ExtractMostSignificantBits();

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var bitPos = BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (GetReference(_entries, index + (uint)bitPos) == key)
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

                GetReference(_controlBytes, index) = h2;
                GetReference(_entries, index) = key;

                Count++;

                return;
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddVectorIF(uint key)
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

            var source = Vector128.LoadUnsafe(ref GetReference(_controlBytes, index));

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = Vector128.Equals(source, target).ExtractMostSignificantBits();

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var bitPos = BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (GetReference(_entries, index + (uint)bitPos) == key)
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
                if (_emptyBucket != GetValue(_controlBytes, index))
                {
                    index += (uint)BitOperations.TrailingZeroCount(emptyMask);
                }

                GetReference(_controlBytes, index) = h2;
                GetReference(_entries, index) = key;

                Count++;

                return;
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddVectorWhile(uint key)
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

            var source = Vector128.LoadUnsafe(ref GetReference(_controlBytes, index));

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = Vector128.Equals(source, target).ExtractMostSignificantBits();

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var bitPos = BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (GetReference(_entries, index + (uint)bitPos) == key)
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
                while (_emptyBucket != GetValue(_controlBytes, index)) index++;

                GetReference(_controlBytes, index) = h2;
                GetReference(_entries, index) = key;

                Count++;

                return;
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddVectorCPUFor(uint key)
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
                if (GetReference(_entries, index + (uint)bitPos) == key)
                {
                    return;
                }

                //// Clear the lowest set bit in `mask` to continue with the next matching bit.
                resultMask = ResetLowestSetBit(resultMask);
            }

            for (byte pos = 0; pos < _vectorSize; pos++)
            {
                if (_emptyBucket == cb)
                {
                    cb = h2; GetReference(_entries, index + pos) = key; Count++; return;
                }

                cb = Unsafe.AddByteOffset(ref cb, 1);
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddVectorCPUIndexOf(uint key)
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

            var source = Vector128.LoadUnsafe(ref Unsafe.AddByteOffset(ref controlBytes, (nuint)index));

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = Vector128.Equals(source, target).ExtractMostSignificantBits();

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var bitPos = BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (GetReference(_entries, index + (uint)bitPos) == key)
                {
                    return;
                }

                //// Clear the lowest set bit in `mask` to continue with the next matching bit.
                resultMask = ResetLowestSetBit(resultMask);
            }

            var pos = IndexOf(ref Unsafe.AddByteOffset(ref controlBytes, (nuint)index));

            if (pos != byte.MaxValue)
            {
                Unsafe.AddByteOffset(ref controlBytes, (nuint)index + pos) = h2;
                GetReference(_entries, index + pos) = key; Count++; return;
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte IndexOf(ref byte cb)
    {
        if (cb == _emptyBucket) return 0;

        var ul = Unsafe.As<byte, ulong>(ref cb);

        //if (((ul >> 56) & 0xFF) == _emptyBucket) return 0;
        if (((ul >> 48) & 0xFF) == _emptyBucket) return 1;
        if (((ul >> 40) & 0xFF) == _emptyBucket) return 2;
        if (((ul >> 32) & 0xFF) == _emptyBucket) return 3;
        if (((ul >> 24) & 0xFF) == _emptyBucket) return 4;
        if (((ul >> 16) & 0xFF) == _emptyBucket) return 5;
        if (((ul >>  8) & 0xFF) == _emptyBucket) return 6;
        if (((ul      ) & 0xFF) == _emptyBucket) return 7;

        ul = Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref cb, 8));

        if (((ul >> 56) & 0xFF) == _emptyBucket) return 8;
        if (((ul >> 48) & 0xFF) == _emptyBucket) return 9;
        if (((ul >> 40) & 0xFF) == _emptyBucket) return 10;
        if (((ul >> 32) & 0xFF) == _emptyBucket) return 11;
        if (((ul >> 24) & 0xFF) == _emptyBucket) return 12;
        if (((ul >> 16) & 0xFF) == _emptyBucket) return 13;
        if (((ul >>  8) & 0xFF) == _emptyBucket) return 14;
        if (((ul      ) & 0xFF) == _emptyBucket) return 15;

        return byte.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCPU(uint key)
    {
        var index = GetHashCode(key);

        var h2 = H2(index);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0, pos;

        ref var controlBytes = ref MemoryMarshal.GetArrayDataReference(_controlBytes);

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            ref var cb = ref Unsafe.AddByteOffset(ref controlBytes, (nuint)index);

            for (pos = 0; pos < _vectorSize; pos++)
            {
                if (h2 == cb && GetReference(_entries, index + pos) == key) return;

                cb = Unsafe.AddByteOffset(ref cb, 1);
            }

            pos = IndexOf(ref Unsafe.AddByteOffset(ref controlBytes, (nuint)index));

            if (pos != byte.MaxValue)
            {
                Unsafe.AddByteOffset(ref controlBytes, (nuint)index + pos) = h2;
                GetReference(_entries, index + pos) = key; Count++; return;
            }

            jumpDistance += _vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }
}
