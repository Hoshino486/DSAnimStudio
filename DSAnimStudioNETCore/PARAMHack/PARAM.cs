using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// 整个系列里到处都在用的通用配置文件。
    /// </summary>
    public partial class PARAM_Hack : SoulsFile<PARAM_Hack>
    {
        /// <summary>
        /// 文件是否为大端序；PS3 / Xbox 360 文件为 true，其余为 false。
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// 表示文件格式的 flag。
        /// </summary>
        public FormatFlags1 Format2D { get; set; }

        /// <summary>
        /// 更多表示文件格式的 flag。
        /// </summary>
        public FormatFlags2 Format2E { get; set; }

        /// <summary>
        /// 原本是与版本 101 的 paramdef 对得上的，但后来一直是 0 或 0xFF。
        /// </summary>
        public byte ParamdefFormatVersion { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public short Unk06 { get; set; }

        /// <summary>
        /// 表示行数据结构的修订版本。
        /// </summary>
        public short ParamdefDataVersion { get; set; }

        /// <summary>
        /// 用来对应 param 和 paramdef。
        /// </summary>
        public string ParamType { get; set; }

        /// <summary>
        /// 根据各行偏移量的间距自动推导；若该 param 没有任何行，则为 -1。
        /// </summary>
        public long DetectedSize { get; private set; }

        /// <summary>
        /// 该 param 的所有行；必须先调用 PARAM_Hack.ApplyParamdef() 载入，才能访问单元格。
        /// </summary>
        public List<Row> Rows { get; set; }

        /// <summary>
        /// 当前应用的 PARAMDEF。
        /// </summary>
        public PARAMDEF AppliedParamdef { get; private set; }

        /// <summary>
        /// 为传入的行返回一个 reader，用于自定义的行读取逻辑
        /// </summary>
        public BinaryReaderEx GetRowReader(Row row)
        {
            RowReader.Position = row.DataOffset;

            return RowReader;
        }
		
        private BinaryReaderEx RowReader;

        public void DisposeRowReader()
        {
            if (RowReader?.Stream != null)
            {
                RowReader.Stream?.Dispose();
                RowReader = null;
            }
        }

        /// <summary>
        /// 从 stream 反序列化文件数据。
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.Position = 0x2C;
            br.BigEndian = BigEndian = br.AssertByte(0, 0xFF) == 0xFF;
            Format2D = (FormatFlags1)br.ReadByte();
            Format2E = (FormatFlags2)br.ReadByte();
            ParamdefFormatVersion = br.ReadByte();
            br.Position = 0;

            // 把文件另存一份私有的，之后好从里面读行数据
            byte[] copy = br.GetBytes(0, (int)br.Stream.Length);
            RowReader = new BinaryReaderEx(BigEndian, copy);

            // 头部里的字符串偏移量非常不可靠，只当作最后手段使用
            long actualStringsOffset = 0;
            long stringsOffset = br.ReadUInt32();
            if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset) || Format2D.HasFlag(FormatFlags1.LongDataOffset))
            {
                br.AssertInt16(0);
            }
            else
            {
                br.ReadUInt16(); // Data start
            }
            Unk06 = br.ReadInt16();
            ParamdefDataVersion = br.ReadInt16();
            ushort rowCount = br.ReadUInt16();
            if (Format2D.HasFlag(FormatFlags1.OffsetParamType))
            {
                br.AssertInt32(0);
                long paramTypeOffset = br.ReadInt64();
                br.AssertPattern(0x14, 0x00);
                if (paramTypeOffset < br.Length)
                    ParamType = br.GetASCII(paramTypeOffset);
                else
                    ParamType = "";
                actualStringsOffset = paramTypeOffset;
            }
            else
            {
                ParamType = br.ReadFixStr(0x20);
            }
            br.Skip(4); // Format
            if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset))
            {
                br.ReadInt32(); // Data start
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
            else if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
            {
                br.ReadInt64(); // Data start
                br.AssertInt64(0);
            }

            Rows = new List<Row>(rowCount);
            for (int i = 0; i < rowCount; i++)
                Rows.Add(new Row(br, this, ref actualStringsOffset));

            if (Rows.Count > 1)
                DetectedSize = Rows[1].DataOffset - Rows[0].DataOffset;
            else if (Rows.Count == 1)
                DetectedSize = (actualStringsOffset == 0 ? stringsOffset : actualStringsOffset) - Rows[0].DataOffset;
            else
                DetectedSize = -1;
        }

        public void Hack_ReadGhettoRowData(int byteCount)
        {
            foreach (var r in Rows)
            {
                var reader = GetRowReader(r);
                r.GhettoDataStorage = reader.ReadBytes(byteCount);
            }
        }

        /// <summary>
        /// 把文件数据序列化到 stream。
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            //if (AppliedParamdef == null)
            //    throw new InvalidOperationException("Params cannot be written without applying a paramdef.");

            bw.BigEndian = BigEndian;

            bw.ReserveUInt32("StringsOffset");
            if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset) || Format2D.HasFlag(FormatFlags1.LongDataOffset))
            {
                bw.WriteInt16(0);
            }
            else
            {
                bw.ReserveUInt16("DataStart");
            }
            bw.WriteInt16(Unk06);
            bw.WriteInt16(ParamdefDataVersion);
            bw.WriteUInt16((ushort)Rows.Count);
            if (Format2D.HasFlag(FormatFlags1.OffsetParamType))
            {
                bw.WriteInt32(0);
                bw.ReserveInt64("ParamTypeOffset");
                bw.WritePattern(0x14, 0x00);
            }
            else
            {
                // 这个 padding 的启发式判断并不完全准确，不过反正也无所谓
                bw.WriteFixStr(ParamType, 0x20, (byte)(Format2D.HasFlag(FormatFlags1.Flag01) ? 0x20 : 0x00));
            }
            bw.WriteByte((byte)(BigEndian ? 0xFF : 0x00));
            bw.WriteByte((byte)Format2D);
            bw.WriteByte((byte)Format2E);
            bw.WriteByte(ParamdefFormatVersion);
            if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset))
            {
                bw.ReserveUInt32("DataStart");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
            else if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
            {
                bw.ReserveInt64("DataStart");
                bw.WriteInt64(0);
            }

            for (int i = 0; i < Rows.Count; i++)
                Rows[i].WriteHeader(bw, this, i);

            // 这大概挺蠢的
            if (Format2D == FormatFlags1.Flag01)
                bw.WritePattern(0x20, 0x00);

            if (Format2D.HasFlag(FormatFlags1.Flag01) && Format2D.HasFlag(FormatFlags1.IntDataOffset))
                bw.FillUInt32("DataStart", (uint)bw.Position);
            else if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
                bw.FillInt64("DataStart", bw.Position);
            else
                bw.FillUInt16("DataStart", (ushort)bw.Position);

            for (int i = 0; i < Rows.Count; i++)
            {
                if (Format2D.HasFlag(FormatFlags1.LongDataOffset))
                    bw.FillInt64($"RowOffset{i}", bw.Position);
                else
                    bw.FillUInt32($"RowOffset{i}", (uint)bw.Position);
                bw.WriteBytes(Rows[i].GhettoDataStorage);
            }

            bw.FillUInt32("StringsOffset", (uint)bw.Position);

            if (Format2D.HasFlag(FormatFlags1.OffsetParamType))
            {
                bw.FillInt64("ParamTypeOffset", bw.Position);
                bw.WriteASCII(ParamType, true);
            }

            for (int i = 0; i < Rows.Count; i++)
                Rows[i].WriteName(bw, this, i);
            // DeS 和 BB 有时（但并非总是）会在这里包含一些无用的填充数据
        }

        /// <summary>
        /// 返回给定 ID 的第一行，找不到就返回 null。
        /// </summary>
        public Row this[int id] => Rows.Find(row => row.ID == id);

        /// <summary>
        /// First set of flags indicating file format; highly speculative.
        /// </summary>
        [Flags]
        public enum FormatFlags1 : byte
        {
            /// <summary>
            /// 没有设置任何 flag。
            /// </summary>
            None = 0,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag01 = 0b0000_0001,

            /// <summary>
            /// 带 32 位数据偏移的扩展 header。
            /// </summary>
            IntDataOffset = 0b0000_0010,

            /// <summary>
            /// 带 64 位数据偏移的扩展 header。
            /// </summary>
            LongDataOffset = 0b0000_0100,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag08 = 0b0000_1000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag10 = 0b0001_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag20 = 0b0010_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag40 = 0b0100_0000,

            /// <summary>
            /// Param 类型字符串是单独写的，而不是在 header 里占固定宽度。
            /// </summary>
            OffsetParamType = 0b1000_0000,
        }

        /// <summary>
        /// Second set of flags indicating file format; highly speculative.
        /// </summary>
        [Flags]
        public enum FormatFlags2 : byte
        {
            /// <summary>
            /// 没有设置任何 flag。
            /// </summary>
            None = 0,

            /// <summary>
            /// 行名字按 UTF-16 写入。
            /// </summary>
            UnicodeRowNames = 0b0000_0001,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag02 = 0b0000_0010,

            /// <summary>
            /// Unknown.
            /// </summary>
            Flag04 = 0b0000_0100,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag08 = 0b0000_1000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag10 = 0b0001_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag20 = 0b0010_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag40 = 0b0100_0000,

            /// <summary>
            /// Unused?
            /// </summary>
            Flag80 = 0b1000_0000,
        }
    }
}
