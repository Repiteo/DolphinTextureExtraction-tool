﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Hack.io;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static Hack.io.Util.GenericExtensions;

namespace Hack.io.J3D
{
    public static class J3DGraph
    {
        /// <summary>
        /// Padding string
        /// </summary>
        public static readonly string Padding = "Hack.io © Super Hackio Incorporated 2018-2021";

        /// <summary>
        /// Adds Padding to the Current Position in the provided Stream
        /// </summary>
        /// <param name="J3DFile">The Stream to add padding to</param>
        /// <param name="multiple">The byte multiple to pad to</param>
        public static void AddPadding(Stream J3DFile, int multiple)
        {
            int PadCount = 0;
            while (J3DFile.Position % multiple != 0)
                J3DFile.WriteByte((byte)Padding[PadCount++]);
        }

        /// <summary>
        /// Find the start of a sequence in the AllList, if the sequence exists, returns -1
        /// </summary>
        /// <param name="AllList"></param>
        /// <param name="Sequence"></param>
        /// <returns></returns>
        public static int FindSequence<T>(List<T> AllList, List<T> Sequence)
        {
            int matchup = 0, start = -1;

            bool found = false, started = false;

            for (int i = 0; i < AllList.Count; i++)
            {
                if (AllList[i].Equals(Sequence[matchup]))
                {
                    if (!started)
                    {
                        start = i;
                        started = true;
                    }
                    matchup++;
                    if (matchup == Sequence.Count)
                    {
                        found = true;
                        break;
                    }
                }
                else
                {
                    matchup = 0;
                    start = -1;
                    started = false;
                }
            }
            if (!found)
                start = -1;
            return start;
        }

        /// <summary>
        /// Represents a J3D Animation Track
        /// </summary>
        public class J3DKeyFrame
        {
            /// <summary>
            /// The Time in the timeline that this keyframe is assigned to
            /// </summary>
            public ushort Time { get; set; }
            /// <summary>
            /// The Value to set to
            /// </summary>
            public float Value { get; set; }
            /// <summary>
            /// Tangents affect the interpolation between two consecutive keyframes
            /// </summary>
            public float IngoingTangent { get; set; }
            /// <summary>
            /// Tangents affect the interpolation between two consecutive keyframes
            /// </summary>
            public float OutgoingTangent { get; set; }

            public J3DKeyFrame(ushort time, float value, float ingoing = 0, float? outgoing = null)
            {
                Time = time;
                Value = value;
                IngoingTangent = ingoing;
                OutgoingTangent = outgoing ?? ingoing;
            }
            public J3DKeyFrame(List<float> Data, int i, short Count, short Index, int Tangent)
            {
                TangentMode TM = (TangentMode)Tangent;
                if (Count == 1)
                {
                    Time = 0;
                    Value = Data[Index];
                    IngoingTangent = 0;
                    OutgoingTangent = 0;
                }
                else
                {
                    Time = (ushort)Data[i];
                    Value = Data[i + 1];
                    IngoingTangent = Data[i + 2];
                    OutgoingTangent = TM == TangentMode.DESYNC ? Data[i + 3] : IngoingTangent;
                }
            }
            /// <summary>
            /// Converts the values based on a rotation multiplier
            /// </summary>
            /// <param name="RotationFraction">The byte in the file that determines the rotation fraction</param>
            /// <param name="Revert">Undo the conversion</param>
            public void ConvertRotation(byte RotationFraction, bool Revert = false)
            {
                float RotationMultiplier = (float)(Math.Pow(RotationFraction, 2) * (180.0 / 32768.0));
                Value           = Revert ? Value           / RotationMultiplier : Value           * RotationMultiplier;
                IngoingTangent  = Revert ? IngoingTangent  / RotationMultiplier : IngoingTangent  * RotationMultiplier;
                OutgoingTangent = Revert ? OutgoingTangent / RotationMultiplier : OutgoingTangent * RotationMultiplier;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString() => string.Format("Time: {0}, Value: {1}, Ingoing: {2}, Outgoing: {3}", Time, Value, IngoingTangent, OutgoingTangent);

            public override bool Equals(object obj)
            {
                return obj is J3DKeyFrame frame &&
                        Time == frame.Time &&
                        Value == frame.Value &&
                        IngoingTangent == frame.IngoingTangent &&
                        OutgoingTangent == frame.OutgoingTangent;
            }

            public override int GetHashCode()
            {
                var hashCode = 2107829771;
                hashCode = hashCode * -1521134295 + Time.GetHashCode();
                hashCode = hashCode * -1521134295 + Value.GetHashCode();
                hashCode = hashCode * -1521134295 + IngoingTangent.GetHashCode();
                hashCode = hashCode * -1521134295 + OutgoingTangent.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(J3DKeyFrame frame1, J3DKeyFrame frame2) => EqualityComparer<J3DKeyFrame>.Default.Equals(frame1, frame2);

            public static bool operator !=(J3DKeyFrame frame1, J3DKeyFrame frame2) => !(frame1 == frame2);
        }

        /// <summary>
        /// J3D Looping Modes
        /// </summary>
        public enum LoopMode : byte
        {
            /// <summary>
            /// Play Once then Stop.
            /// </summary>
            ONCE = 0x00,
            /// <summary>
            /// Play Once then Stop and reset to the first frame.
            /// </summary>
            ONCERESET = 0x01,
            /// <summary>
            /// Constantly play the animation.
            /// </summary>
            REPEAT = 0x02,
            /// <summary>
            /// Play the animation to the end. then reverse the animation and play to the start, then Stop.
            /// </summary>
            ONCEANDMIRROR = 0x03,
            /// <summary>
            /// Play the animation to the end. then reverse the animation and play to the start, repeat.
            /// </summary>
            REPEATANDMIRROR = 0x04
        }

        /// <summary>
        /// J3D Tangent Modes
        /// </summary>
        public enum TangentMode : short
        {
            /// <summary>
            /// One tangent value is stored, used for both the incoming and outgoing tangents
            /// </summary>
            SYNC = 0x00,
            /// <summary>
            /// Two tangent values are stored, the incoming and outgoing tangents, respectively
            /// </summary>
            DESYNC = 0x01
        }
    }

    public static class NameTableIO
    {
        public static List<string> ReadStringTable(this Stream reader, int offset)
        {
            List<string> names = new List<string>();

            reader.Position = offset;

            short stringCount = BitConverter.ToInt16(reader.ReadReverse(0, 2), 0);
            reader.Position += 0x02;

            for (int i = 0; i < stringCount; i++)
            {
                reader.Position += 0x02;
                short nameOffset = BitConverter.ToInt16(reader.ReadReverse(0, 2), 0);
                long saveReaderPos = reader.Position;
                reader.Position = offset + nameOffset;

                names.Add(reader.ReadString());

                reader.Position = saveReaderPos;
            }

            return names;
        }

        public static void WriteStringTable(this Stream writer, List<string> names)
        {
            long start = writer.Position;

            writer.WriteReverse(BitConverter.GetBytes((short)names.Count), 0, 2);
            writer.Write(new byte[2] { 0xFF, 0xFF }, 0, 2);

            foreach (string st in names)
            {
                writer.WriteReverse(BitConverter.GetBytes(HashString(st)), 0, 2);
                writer.Write(new byte[2], 0, 2);
            }

            long curOffset = writer.Position;
            for (int i = 0; i < names.Count; i++)
            {
                writer.Seek((int)(start + (6 + i * 4)), SeekOrigin.Begin);
                writer.WriteReverse(BitConverter.GetBytes((short)(curOffset - start)), 0, 2);
                writer.Seek((int)curOffset, SeekOrigin.Begin);

                writer.WriteString(names[i], 0x00);

                curOffset = writer.Position;
            }
        }

        private static ushort HashString(string str)
        {
            ushort hash = 0;

            foreach (char c in str)
            {
                hash *= 3;
                hash += (ushort)c;
            }

            return hash;
        }
    }
}
