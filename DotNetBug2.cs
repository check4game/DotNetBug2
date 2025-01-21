using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

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
    public int AddFindNoSimd()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddFindNoSimd(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddFindSimd()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddFindSimd(key);
        }

        return ht.Count;
    }

    [Benchmark]
    public int AddNoSimd()
    {
        foreach (var key in data.AsSpan().Slice(0, (int)(ht.Capacity * Load)))
        {
            ht.AddNoSimd(key);
        }

        return ht.Count;
    }
}

internal class ControlBytes : IDisposable
{
    private nint array = nint.Zero;

    public readonly uint Length;

    private const byte emptyValue = byte.MinValue;

    public ControlBytes(uint length)
    {
        unsafe
        {
            array = (nint)NativeMemory.AlignedAlloc(Length = (length + (uint)OffsetSpan.Length), (nuint)OffsetSpan.Length);
        }

        Clear(); emptyVector = CreateVector(emptyValue);
    }

    public void Clear()
    {
        unsafe
        {
            NativeMemory.Fill(array.ToPointer(), Length, emptyValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty(ulong index)
    {
        return GetValue(index) == emptyValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte FindEmptySimd(ref Vector128<byte> source)
    {
        var emptyMask = Vector128.Equals(source, emptyVector).ExtractMostSignificantBits();

        if (emptyMask != 0) return (byte)BitOperations.TrailingZeroCount(emptyMask);

        return byte.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte FindEmptySimdIF(ref Vector128<byte> source)
    {
        var emptyMask = Vector128.Equals(source, emptyVector).ExtractMostSignificantBits();

        if (emptyMask != 0) return FindEmptyNoSimd(ref source);

        return byte.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte FindEmptyNoSimd(ref Vector128<byte> source)
    {
        unsafe
        {
            return FindEmptyNoSimd((byte*)Unsafe.AsPointer(ref source));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte FindEmptyNoSimd(ulong index)
    {
        unsafe
        {
            return FindEmptyNoSimd((byte*)array.ToPointer() + index);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe byte FindEmptyNoSimd(byte* ptr)
    {
        if (*ptr == emptyValue) return 0;

        var ul = *(ulong*)(ptr);

        //if (((ul >> 56) & 0xFF) == emptyValue) return 0;
        if (((ul >> 48) & 0xFF) == emptyValue) return 1;
        if (((ul >> 40) & 0xFF) == emptyValue) return 2;
        if (((ul >> 32) & 0xFF) == emptyValue) return 3;
        if (((ul >> 24) & 0xFF) == emptyValue) return 4;
        if (((ul >> 16) & 0xFF) == emptyValue) return 5;
        if (((ul >> 8) & 0xFF) == emptyValue) return 6;
        if (((ul) & 0xFF) == emptyValue) return 7;

        ul = *(ulong*)(ptr + 8);

        if (((ul >> 56) & 0xFF) == emptyValue) return 8;
        if (((ul >> 48) & 0xFF) == emptyValue) return 9;
        if (((ul >> 40) & 0xFF) == emptyValue) return 10;
        if (((ul >> 32) & 0xFF) == emptyValue) return 11;
        if (((ul >> 24) & 0xFF) == emptyValue) return 12;
        if (((ul >> 16) & 0xFF) == emptyValue) return 13;
        if (((ul >> 8) & 0xFF) == emptyValue) return 14;
        if (((ul) & 0xFF) == emptyValue) return 15;

        return byte.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ToHashCode(ulong index) => (byte)((index >> 56) | 0x01);

    public const byte vectorSize = 16;

    private readonly Vector128<byte> emptyVector;
    public static ReadOnlySpan<byte> OffsetSpan =>
    [
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Compare(ref Vector128<byte> source, ref Vector128<byte> target)
    {
        return Vector128.Equals(source, target).ExtractMostSignificantBits();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Compare(ulong index, ref Vector128<byte> target)
    {
        return Vector128.Equals(AsVector(index), target).ExtractMostSignificantBits();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector128<byte> CreateVector(byte value) => Vector128.Create(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector128<byte> AsVector(ulong index)
    {
        return Vector128.LoadUnsafe(ref GetReference(index));
        /*
        unsafe
        {
            return Vector128.LoadAligned((byte*)array.ToPointer() + index);
        }
        */
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref byte GetReference(ulong index)
    {
        unsafe
        {
            return ref *((byte*)array.ToPointer() + index);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetValue(ulong index)
    {
        unsafe
        {
            return *((byte*)array.ToPointer() + index);
        }
    }

    public bool IsDisposed => array == IntPtr.Zero;

    private void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            unsafe
            {
                NativeMemory.AlignedFree(array.ToPointer());
            }

            array = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        Dispose(true); GC.SuppressFinalize(this);
    }

    ~ControlBytes() => Dispose(false);
}

class HashTable
{
    public HashTable(uint Capacity = 32 * 1024 * 1024)
    {
        _Capacity = Capacity;

        _lengthMinusOne = (_Capacity - 1);

        _controlBytes = new ControlBytes(_Capacity);

        _entries = GC.AllocateUninitializedArray<uint>((int)_controlBytes.Length);

        //_lengthMinusOne &= ~15u;
    }

    public uint Capacity => _Capacity;

    private uint _Capacity, _lengthMinusOne;
    
    public void Clear()
    {
        Count = 0; _controlBytes.Clear();
    }

    private ControlBytes _controlBytes;

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
    private static ulong ToIndex(uint key) => (uint)key.GetHashCode() * 11400714819323198485UL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFindSimd(uint key)
    {
        var index = ToIndex(key);

        var hashCode = ControlBytes.ToHashCode(index);

        // Create a SIMD vector with the value of 'hashCode' for quick equality checks.
        var target = _controlBytes.CreateVector(hashCode);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0;

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            var source = _controlBytes.AsVector(index);

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            var resultMask = _controlBytes.Compare(ref source, ref target);   

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var offset = (uint)BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (GetReference(_entries, index + offset) == key) return;

                //// Clear the lowest set bit in `mask` to continue with the next matching bit.
                resultMask = ResetLowestSetBit(resultMask);
            }

            {
                var offset = _controlBytes.FindEmptySimd(ref source);

                if (offset != byte.MaxValue)
                {
                    index += offset;

                    _controlBytes.GetReference(index) = hashCode;
                    GetReference(_entries, index) = key;

                    Count++; return;
                }
            }

            jumpDistance += ControlBytes.vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFindNoSimd(uint key)
    {
        var index = ToIndex(key);

        var hashCode = ControlBytes.ToHashCode(index);

        // Create a SIMD vector with the value of 'hashCode' for quick equality checks.
        var target = _controlBytes.CreateVector(hashCode);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0;

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            // Compare `source` and `target` vectors to find any positions with a matching control byte.
            //var resultMask = _controlBytes.Compare(_controlBytes.AsVector(index), target);
            var resultMask = _controlBytes.Compare(index, ref target);

            // Loop over each set bit in `mask` (indicating matching positions).
            while (resultMask != 0)
            {
                // Find the lowest set bit in `mask` (first matching position).
                var offset = (uint)BitOperations.TrailingZeroCount(resultMask);

                // If a matching key is found, update the entry's value and return the old value.
                if (GetReference(_entries, index + offset) == key) return;

                //// Clear the lowest set bit in `mask` to continue with the next matching bit.
                resultMask = ResetLowestSetBit(resultMask);
            }

            {
                var offset = _controlBytes.FindEmptyNoSimd(index);

                if (offset != byte.MaxValue)
                {
                    index += offset;

                    _controlBytes.GetReference(index) = hashCode;
                    GetReference(_entries, index) = key;

                    Count++; return;
                }
            }

            jumpDistance += ControlBytes.vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddNoSimd(uint key)
    {
        var index = ToIndex(key);

        var hashCode = ControlBytes.ToHashCode(index);

        // Initialize the probing jump distance to zero, which will increase with each probe iteration.
        byte jumpDistance = 0;

        while (true)
        {
            index &= _lengthMinusOne; // Use bitwise AND to ensure the index wraps around within the bounds of the map. Thus preventing out of bounds exceptions

            ref var cb = ref _controlBytes.GetReference(index);

            foreach (var offset in ControlBytes.OffsetSpan)
            {
                if (hashCode == cb && key == GetReference(_entries, index + offset)) return;
                cb = Unsafe.AddByteOffset(ref cb, 1);
            }

            {
                var offset = _controlBytes.FindEmptyNoSimd(index);

                if (offset != byte.MaxValue)
                {
                    index += offset;

                    _controlBytes.GetReference(index) = hashCode;
                    GetReference(_entries, index) = key;

                    Count++; return;
                }
            }

            jumpDistance += ControlBytes.vectorSize; // Increase the jump distance by 16 to probe the next cluster.
            index += jumpDistance; // Move the index forward by the jump distance.           
        }
    }
}
