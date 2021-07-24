using System;
using System.IO;

namespace CBRE.Editor.Compiling {
    public static class ExportWriterExtensions {
        public static void WriteByteString(this BinaryWriter writer, string str) {
            writer.Write((byte)str.Length);
            for (int i = 0; i < str.Length; i++) {
                writer.Write((byte)str[i]);
            }
        }

        public static void WriteB3DString(this BinaryWriter writer, string str) {
            writer.Write((UInt32)str.Length);
            for (int i = 0; i < str.Length; i++) {
                writer.Write((byte)str[i]);
            }
        }

        public static void WriteNullTerminatedString(this BinaryWriter writer, string str) {
            for (int i = 0; i < str.Length; i++) {
                writer.Write((byte)str[i]);
            }
            writer.Write((byte)0);
        }
    }
}
