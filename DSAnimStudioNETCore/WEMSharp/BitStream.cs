using System.IO;

namespace WEMSharp
{
    /// <summary>
    /// 包在 Stream 外面、支持按位读取的 wrapper
    /// </summary>
    internal class BitStream
    {
        private Stream _stream;
        private byte _bitBuffer;
        private int _bitsLeft;
        internal ulong TotalBitsRead { get; private set; }

        internal BitStream(Stream stream)
        {
            this._stream = stream;
        }

        /// <summary>
        /// 从 Stream 里读一位
        /// </summary>
        internal byte GetBit()
        {
            if (this._bitsLeft == 0)
            {
                this._bitBuffer = (byte)this._stream.ReadByte();
                this._bitsLeft = 8;
            }

            this.TotalBitsRead++;
            this._bitsLeft--;

            return (this._bitBuffer & (0x80 >> this._bitsLeft)) != 0 ? (byte)1 : (byte)0;
        }

        /// <summary>
        /// 从 stream 里读任意数量的位
        /// </summary>
        /// <param name="bitCount">Amount of bits to read</param>
        internal uint Read(int bitCount)
        {
            uint result = 0;

            for (int i = 0; i < bitCount; i++)
            {
                if (GetBit() == 1)
                {
                    result |= (1U << i);
                }
            }

            return result;
        }
    }
}
