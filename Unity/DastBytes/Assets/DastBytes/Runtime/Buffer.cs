using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DastBytes {
  public class Buffer {
    static private UTF8Encoding UTF8 = new UTF8Encoding(false);

    static private string[] HexByteStrings = new string[256];
    static public string GetHexByteString(byte index) => HexByteStrings[index];

    static Buffer(){
      for (int i = 0; i < HexByteStrings.Length; ++i){
        HexByteStrings[i] = $"{i:X2}";
      }
    }

    public class VariableLength {
      public const long MaxInt6 = 0x3F;
      public const long MaxInt14 = 0x3FFF;
      public const long MaxInt30 = 0x3FFFFFFF;
      public const long MaxInt62 = 0x3FFFFFFFFFFFFFFF;

      public class TypeBitFlag {
        public const byte Int6 = 0x00;
        public const byte Int14 = 0x40;
        public const byte Int30 = 0x80;
        public const byte Int62 = 0xC0;
        public const byte All = 0xC0;
      }

      public delegate byte ReadEvent();

      public delegate void ToBytesEvent(ReadOnlySpan<byte> bytes);

      [StructLayout(LayoutKind.Explicit)]
      private struct Int64 {
        [FieldOffset(0)] public long Value;
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(5)] public byte Byte5;
        [FieldOffset(6)] public byte Byte6;
        [FieldOffset(7)] public byte Byte7;

        public Int64(long value) : this(){
          Value = value;
        }
      }

      static public long ReadFromBytes(ReadEvent onRead){
        byte msbyte = onRead();
        switch (msbyte & TypeBitFlag.All){
          case TypeBitFlag.Int6: return msbyte;

          case TypeBitFlag.Int14:{
            Int64 int64 = new Int64();
            int64.Byte1 = (byte)(msbyte & ~TypeBitFlag.All);
            int64.Byte0 = onRead();
            return int64.Value;
          }

          case TypeBitFlag.Int30:{
            Int64 int64 = new Int64();
            int64.Byte3 = (byte)(msbyte & ~TypeBitFlag.All);
            int64.Byte2 = onRead();
            int64.Byte1 = onRead();
            int64.Byte0 = onRead();
            return int64.Value;
          }

          default:{
            Int64 int64 = new Int64();
            int64.Byte7 = (byte)(msbyte & ~TypeBitFlag.All);
            int64.Byte6 = onRead();
            int64.Byte5 = onRead();
            int64.Byte4 = onRead();
            int64.Byte3 = onRead();
            int64.Byte2 = onRead();
            int64.Byte1 = onRead();
            int64.Byte0 = onRead();
            return int64.Value;
          }
        }
      }

      static public void ToBytes(long length, ToBytesEvent onToBytes){
        Int64 int64 = new Int64(length);
        if (length <= MaxInt6){
          onToBytes(stackalloc byte[]{int64.Byte0});
        }else if (length <= MaxInt14){
          onToBytes(stackalloc byte[]{(byte)(TypeBitFlag.Int14 | int64.Byte1), int64.Byte0});
        }else if (length <= MaxInt30){
          onToBytes(stackalloc byte[]{(byte)(TypeBitFlag.Int30 | int64.Byte3), int64.Byte2, int64.Byte1, int64.Byte0});
        }else{
          onToBytes(stackalloc byte[]{(byte)(TypeBitFlag.Int62 | int64.Byte7), int64.Byte6, int64.Byte5, int64.Byte4, int64.Byte3, int64.Byte2, int64.Byte1, int64.Byte0});
        }
      }
    }

    public delegate void BufferEvent(Buffer buffer);

    private byte[] m_Bytes = new byte[0];

    private byte[] m_AsBytes = new byte[0];

    public byte[] AsBytes(){
      int length = Length;
      if (m_AsBytes.Length != length) Array.Resize(ref m_AsBytes, length);
      System.Buffer.BlockCopy(m_Bytes, m_ReadIndex, m_AsBytes, 0, length);
      return m_AsBytes;
    }

    public ReadOnlySpan<byte> AsReadOnlySpan() => new ReadOnlySpan<byte>(m_Bytes, m_ReadIndex, Length);

    private int m_ReadIndex;

    private int m_WriteIndex;

    public int Length => m_WriteIndex - m_ReadIndex;

    private StringBuilder m_StringBuilder = new StringBuilder();

    private const int MinCapacity = 4;

    public Buffer() : this(MinCapacity){}

    public Buffer(int capacity){
      m_Bytes = new byte[Math.Max(MinCapacity, capacity)];
    }

    public void Clear(){
      m_ReadIndex = m_WriteIndex = 0;
    }

    private int CalcResizeLength(int needLength){
      int resizeLength = m_Bytes.Length * 2;
      while (resizeLength < needLength){
        resizeLength *= 2;
      }
      return resizeLength;
    }

    private void CheckWriteBytes(int length){
      int needLength = m_WriteIndex + length;
      if (m_Bytes.Length < needLength) Array.Resize(ref m_Bytes, CalcResizeLength(needLength));
    }

    unsafe public void Write<T>(T value) where T : unmanaged {
      int byteCount = sizeof(T);
      CheckWriteBytes(byteCount);
      fixed (void* bytesPointer = &m_Bytes[m_WriteIndex]){
        System.Buffer.MemoryCopy(&value, bytesPointer, byteCount, byteCount);
      }
      m_WriteIndex += byteCount;
    }

    public void Write(string value, int byteCount){
      CheckWriteBytes(byteCount);
      UTF8.GetBytes(value, 0, value.Length, m_Bytes, m_WriteIndex);
      m_WriteIndex += byteCount;
    }

    public void Write(string value){
      int byteCount = UTF8.GetByteCount(value);
      Write(byteCount);
      Write(value, byteCount);
    }

    public void Write(Buffer value){
      Write(value.Length);
      Write(value.AsReadOnlySpan());
    }

    public void WriteVariableLength(long value){
      VariableLength.ToBytes(value, (bytes) => Write(bytes));
    }

    unsafe public void Write<T>(Span<T> array) where T : unmanaged {
      int byteCount = array.Length * sizeof(T);
      CheckWriteBytes(byteCount);
      MemoryMarshal.AsBytes(array).CopyTo(m_Bytes.AsSpan(m_WriteIndex, byteCount));
      m_WriteIndex += byteCount;
    }

    unsafe public void Write<T>(ReadOnlySpan<T> array) where T : unmanaged {
      int byteCount = array.Length * sizeof(T);
      CheckWriteBytes(byteCount);
      MemoryMarshal.AsBytes(array).CopyTo(m_Bytes.AsSpan(m_WriteIndex, byteCount));
      m_WriteIndex += byteCount;
    }

    unsafe public void Write<T>(T[] array) where T : unmanaged {
      Write(array.AsSpan());
    }

    public void Rewrite(BufferEvent onBuffer){
      int writeIndex = m_WriteIndex;
      m_WriteIndex = m_ReadIndex;
      onBuffer(this);
      m_WriteIndex = Math.Max(writeIndex, m_WriteIndex);
    }

    unsafe public T Read<T>() where T : unmanaged {
      T value = default;
      int byteCount = sizeof(T);
      fixed (void* bytesPointer = &m_Bytes[m_ReadIndex]){
        System.Buffer.MemoryCopy(bytesPointer, &value, byteCount, byteCount);
      }
      m_ReadIndex += byteCount;
      return value;
    }

    public string Read(int byteCount){
      int readIndex = m_ReadIndex;
      m_ReadIndex += byteCount;
      return UTF8.GetString(m_Bytes, readIndex, byteCount);
    }

    public string Read(){
      return Read(Read<int>());
    }

    public Buffer Read(Buffer buffer){
      int length = Read<int>();
      buffer.Write(m_Bytes.AsSpan(m_ReadIndex, length));
      m_ReadIndex += length;
      return buffer;
    }

    public long ReadVariableLength(){
      return VariableLength.ReadFromBytes(Read<byte>);
    }

    unsafe public void Read<T>(Span<T> array) where T : unmanaged {
      int byteCount = array.Length * sizeof(T);
      MemoryMarshal.Cast<byte, T>(m_Bytes.AsSpan(m_ReadIndex, byteCount)).CopyTo(array);
      m_ReadIndex += byteCount;
    }

    unsafe public void Read<T>(T[] array) where T : unmanaged {
      Read(array.AsSpan());
    }

    public void Reread(BufferEvent onBuffer){
      int readIndex = m_ReadIndex;
      m_ReadIndex = 0;
      onBuffer(this);
      m_ReadIndex = readIndex;
    }

    public override string ToString(){
      m_StringBuilder.Clear();
      Span<byte> bytes = m_Bytes.AsSpan(m_ReadIndex, Length);
      for (int i = 0; i < bytes.Length; ++i){
        m_StringBuilder.Append(HexByteStrings[bytes[i]]);
      }
      return m_StringBuilder.ToString();
    }
  }
}