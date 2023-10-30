using System.Collections.Generic;
using System.Numerics;
using VoidShips.util.polyfill;

namespace VoidShips.util.mem;

public sealed class BitVector
{
    // Format:
    //
    // Word 0                 Word 1
    // -----------            -----------
    // 0000...0000            0000...0000
    //           ^ element 0            ^ element 64
    //             (LSB, trailing)        (LSB, trailing)
    // ^ element 63           ^ element 127
    //   (MSB, leading)       (MSB, leading)
    private readonly List<ulong> _words = new();

    public void Add(int index)
    {
        var wordIndex = index / 64;
        _words.EnsureMinLength(wordIndex + 1, _ => 0uL);
        _words[wordIndex] |= 1uL << (index % 64);
    }

    public void Remove(int index)
    {
        _words[index / 64] &= ~(1uL << (index % 64));

        // Truncate so no empty words exist at the end.
        for (var i = _words.Count - 1; i >= 0; i--)
            if (_words[i] == 0) _words.Pop();
    }
    
    public int AddSmallest()
    {
        var min = FirstZero();
        Add(min);
        return min;
    }

    public int FirstZero()
    {
        for (var i = 0; i < _words.Count; i++)
        {
            var word = _words[i]; 
            if (word == ulong.MaxValue) continue;
            var trailingOnes = BitOperations.TrailingZeroCount(~word);

            return i * 64 + trailingOnes;
        }

        return _words.Count * 64;
    }

    public int SmallestLength()
    {
        return _words.Count == 0
            ? 0
            // This is the index of the first bit in the word one past the end of the vector.
            : _words.Count * 64
              // We subtract off the number of leading zeroes in the last word. If there are no zeroes, our length is
              // properly defined to be the capacity of our bit list.
              - BitOperations.LeadingZeroCount(_words[^1]);
    }
}
