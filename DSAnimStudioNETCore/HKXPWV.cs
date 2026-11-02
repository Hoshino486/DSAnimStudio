using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    /// <summary>
    /// 在黑暗之魂1（Dark Souls）、黑暗之魂3（Dark Souls III）和血源诅咒（Bloodborne）里发现的某种 ragdoll 相关容器
    /// </summary>
    public class HKXPWV : SoulsFile<HKXPWV>
    {
        /// <summary>
        /// 这个 HKXPWV 对应的游戏。
        /// </summary>
        public enum GameType
        {
            /// <summary>
            /// 黑暗之魂（Dark Souls）
            /// </summary>
            DS1,
            /// <summary>
            /// Bloodborne
            /// </summary>
            BB,
            /// <summary>
            /// 黑暗之魂3（Dark Souls III）
            /// </summary>
            DS3
        }
        /// <summary>
        /// 文件是否为大端序。
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// 这个文件对应哪一款游戏。
        /// </summary>
        public GameType Game { get; set; }

        /// <summary>
        /// 头部里含义未知。可以是 0、1、2 或 5。一般是 0。
        /// </summary>
        public uint UnkC { get; set; }

        /// <summary>
        /// 影响 HKX 中 Bone map 条目的项列表。
        /// </summary>
        public List<BoneMapEntry> BoneMapEntries { get; set; }

        /// <summary>
        /// 影响 Anim Skeleton 中 Bone 的项列表。
        /// </summary>
        public List<AnimBoneEntry> AnimBoneEntries { get; set; }

        /// <summary>
        /// 影响 ragdoll Bone 的项列表。
        /// </summary>
        public List<RagdollBoneEntry> RagdollBoneEntries { get; set; }

        /// <summary>
        /// 创建一个新的空 HKXPWV。
        /// </summary>
        public HKXPWV()
        {
            BoneMapEntries = new List<BoneMapEntry>();
            AnimBoneEntries = new List<AnimBoneEntry>();
            RagdollBoneEntries = new List<RagdollBoneEntry>();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        protected override void Read(BinaryReaderEx br)
        {
            // 在两种端序下这个值都应该是 1
            int versionCheck = br.AssertInt32(1, 0x1000000);
            if (versionCheck == 0x1000000)
            {
                BigEndian = true;
                br.BigEndian = true;
            }

            br.AssertInt16(0);

            ushort boneMapEntryCount = br.ReadUInt16();
            ushort animBoneEntryCount = br.ReadUInt16();
            ushort ragdollBoneEntryCount = br.ReadUInt16();

            if (br.Length == (0x20 + (boneMapEntryCount * 8) + (animBoneEntryCount * 8) + (ragdollBoneEntryCount * 16)))
            {
                Game = GameType.BB;
            }
            else if (br.Length == (0x20 + (boneMapEntryCount * 4) + (animBoneEntryCount * 8) + (ragdollBoneEntryCount * 16)))
            {
                Game = GameType.DS3;
            }
            else if (br.Length == (0x20 + (boneMapEntryCount * 4) + (animBoneEntryCount * 8) + (ragdollBoneEntryCount * 8)))
            {
                Game = GameType.DS1;
            }
            else
            {
                throw new System.IO.InvalidDataException("Invalid struct sizes found in HKXPWV file.");
            }

            UnkC = br.AssertUInt32(0, 1, 2, 5);

            for (int i = 0; i < 4; i++)
                br.AssertInt32(0);

            for (int i = 0; i < boneMapEntryCount; i++)
                BoneMapEntries.Add(new BoneMapEntry(br, Game));

            for (int i = 0; i < animBoneEntryCount; i++)
                AnimBoneEntries.Add(new AnimBoneEntry(br));

            for (int i = 0; i < ragdollBoneEntryCount; i++)
                RagdollBoneEntries.Add(new RagdollBoneEntry(br, Game));
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;

            //Version
            bw.WriteInt32(1);

            bw.WriteInt16(0);

            bw.WriteUInt16((ushort)BoneMapEntries.Count);
            bw.WriteUInt16((ushort)AnimBoneEntries.Count);
            bw.WriteUInt16((ushort)RagdollBoneEntries.Count);

            bw.WriteUInt32(UnkC);

            for (int i = 0; i < 4; i++)
                bw.WriteInt32(0);

            foreach (var boneMapEntry in BoneMapEntries)
                boneMapEntry.Write(bw, Game);

            foreach (var animBoneEntry in AnimBoneEntries)
                animBoneEntry.Write(bw);

            foreach (var ragdollBoneEntry in RagdollBoneEntries)
                ragdollBoneEntry.Write(bw, Game);
        }

        /// <summary>
        /// 包含 HKX 中 Bone map 条目的参数。
        /// </summary>
        public class BoneMapEntry
        {
            /// <summary>
            /// Unknown
            /// </summary>
            public int UnknownValueA { get; set; } = -1;

            /// <summary>
            /// Unknown
            /// </summary>
            public int UnknownValueB { get; set; } = 0;

            /// <summary>
            /// 用默认值创建一个默认的 BoneMapEntry。
            /// </summary>
            public BoneMapEntry()
            {

            }

            internal BoneMapEntry(BinaryReaderEx br, GameType game)
            {
                if (game == GameType.BB)
                {
                    UnknownValueA = br.ReadInt32();
                    UnknownValueB = br.ReadInt32();
                }
                else
                {
                    UnknownValueA = br.ReadInt16();
                    UnknownValueB = br.ReadInt16();
                }
            }

            internal void Write(BinaryWriterEx bw, GameType game)
            {
                if (game == GameType.BB)
                {
                    bw.WriteInt32(UnknownValueA);
                    bw.WriteInt32(UnknownValueB);
                }
                else
                {
                    bw.WriteInt16((short)UnknownValueA);
                    bw.WriteInt16((short)UnknownValueB);
                }
            }
        }

        /// <summary>
        /// 包含 HKX 中 Anim Bone 条目的参数。
        /// </summary>
        public class AnimBoneEntry
        {
            /// <summary>
            /// 这条 Anim Bone 所引用的 SkeletonParam 条目的 ID。
            /// </summary>
            public short SkeletonParamID { get; set; }

            /// <summary>
            /// Unknown. 
            /// Possible values on PC: { 0, 1, 2, 3 }.
            /// Possible values on PS3: { 0, 0x10, 0x20, 0x30 }
            /// </summary>
            public byte Unk2 { get; set; }

            /// <summary>
            /// 用最常见的取值创建一个默认的 AnimBoneEntry
            /// 字段值：SkeletonParamRowID = -1，Unk2 = 0。
            /// </summary>
            public AnimBoneEntry()
            {
                SkeletonParamID = -1;
                Unk2 = 0;
            }

            internal AnimBoneEntry(BinaryReaderEx br)
            {
                SkeletonParamID = br.ReadInt16();
                Unk2 = br.ReadByte();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(SkeletonParamID);
                bw.WriteByte(Unk2);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
            }
        }

        /// <summary>
        /// 包含 HKX 中 ragdoll Bone 条目的参数。
        /// </summary>
        public class RagdollBoneEntry
        {
            /// <summary>
            /// 这根 ragdoll Bone 所引用的 RagdollParam 条目的 ID。
            /// -1 表示不引用任何条目。
            /// </summary>
            public int RagdollParamID { get; set; } = -1;

            /// <summary>
            /// 这根 ragdoll Bone 被破坏时要播放的 Anim ID。
            /// 在 TAE 中播放动画 a99_XXXX。
            /// </summary>
            public short DamageAnimID { get; set; } = -1;

            /// <summary>
            /// 定义此布娃娃骨骼的碰撞体被分配到哪个 NPC 部件组（如果有）。
            /// 用于 EMEVD 里的 NPC 部位事件，或者敌人的弱点。
            /// </summary>
            public byte NPCPartGroupIndex { get; set; } = 0;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unknown { get; set; } = 0;

            /// <summary>
            /// Bloodborne 的 HKXPWV 文件独有的未知值。
            /// </summary>
            public sbyte UnknownBB { get; set; } = -1;

            /// <summary>
            /// 创建一个默认的 RagdollBoneEntry
            /// </summary>
            public RagdollBoneEntry()
            {

            }

            internal RagdollBoneEntry(BinaryReaderEx br, GameType game)
            {
                if (game == GameType.BB)
                {
                    RagdollParamID = br.ReadInt32();
                    DamageAnimID = br.ReadInt16();
                    NPCPartGroupIndex = br.ReadByte();
                    Unknown = br.ReadByte();
                    UnknownBB = br.ReadSByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }
                else
                {
                    RagdollParamID = br.ReadInt16();
                    DamageAnimID = br.ReadInt16();
                    NPCPartGroupIndex = br.ReadByte();
                    Unknown = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);

                    if (game == GameType.DS3)
                    {
                        br.AssertInt64(0);
                    }
                }
                
            }

            internal void Write(BinaryWriterEx bw, GameType game)
            {
                if (game == GameType.BB)
                {
                    bw.WriteInt32(RagdollParamID);
                    bw.WriteInt16(DamageAnimID);
                    bw.WriteByte(NPCPartGroupIndex);
                    bw.WriteByte(Unknown);
                    bw.WriteSByte(UnknownBB);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
                else
                {
                    bw.WriteInt16((short)RagdollParamID);
                    bw.WriteInt16(DamageAnimID);
                    bw.WriteByte(NPCPartGroupIndex);
                    bw.WriteByte(Unknown);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    if (game == GameType.DS3)
                        bw.WriteInt64(0);
                }
            }
        }

    }
}
