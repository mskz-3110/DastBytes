using System.Collections.Generic;
using System.Text;

namespace DastBytes {
  public class Buffer {
    static private UTF8Encoding UTF8 = new UTF8Encoding(false);

    static private List<byte[]> HexBytes;

    static Buffer(){
      HexBytes = new List<byte[]>();
      for (int i = 0; i < 256; ++i){
        HexBytes.Add(UTF8.GetBytes($"{i:X2}"));
      }
    }

    private enum VariableLengthType {
      UInt16 = 0xFE,
      UInt32 = 0xFF
    }

    private byte[] m_BufferBytes = new byte[0];

    private int m_Capacity;
    public int Capacity {
      get {return m_Capacity;}
    }

    public int ReadIndex;

    public int WriteIndex;

    public int Length => WriteIndex - ReadIndex;

    public byte[] WorkingBytes = new byte[0];

    private byte[] m_StringBytes = new byte[0];

    public Buffer(int capacity = 1024){
      m_Capacity = capacity;
      m_BufferBytes = new byte[m_Capacity];
    }

    public void Clear(){
      ReadIndex = WriteIndex = 0;
    }

    public void Resize(int length){
      m_Capacity = length;
      ByteArray.Resize<byte>(ref m_BufferBytes, length);
      if (length < ReadIndex) ReadIndex = length;
      if (length < WriteIndex) WriteIndex = length;
    }

    private void CheckWriteBytes(int length){
      int needLength = WriteIndex + length;
      if (needLength <= m_Capacity) return;

      while (m_Capacity < needLength){
        m_Capacity *= 2;
      }
      ByteArray.Resize<byte>(ref m_BufferBytes, m_Capacity);
    }

    unsafe public void Write<T>(T value) where T : unmanaged {
      int byteCount = sizeof(T);
      CheckWriteBytes(byteCount);
      unsafe {
        ByteArray.Copy(m_BufferBytes, WriteIndex, &value, byteCount);
      }
      WriteIndex += byteCount;
    }

    public void WriteUInt8(byte value){
      CheckWriteBytes(1);
      m_BufferBytes[WriteIndex++] = value;
    }

    public void WriteInt8(sbyte value){
      WriteUInt8((byte)value);
    }

    public void WriteBool(bool value){
      WriteUInt8((byte)(value ? 1 : 0));
    }

    public void WriteUInt16(ushort value){
      Write<ushort>(value);
    }

    public void WriteInt16(short value){
      Write<short>(value);
    }

    public void WriteUInt32(uint value){
      Write<uint>(value);
    }

    public void WriteInt32(int value){
      Write<int>(value);
    }

    public void WriteUInt64(ulong value){
      Write<ulong>(value);
    }

    public void WriteInt64(long value){
      Write<long>(value);
    }

    public void WriteVariableLength(int value){
      if (value < (int)VariableLengthType.UInt16){
        WriteUInt8((byte)value);
        return;
      }

      if (value <= ushort.MaxValue){
        WriteUInt8((byte)VariableLengthType.UInt16);
        WriteUInt16((ushort)value);
        return;
      }

      WriteUInt8((byte)VariableLengthType.UInt32);
      WriteUInt32((uint)value);
    }

    public void WriteFloat(float value){
      Write<float>(value);
    }

    public void WriteDouble(double value){
      Write<double>(value);
    }

    public void WriteString(string value){
      int byteCount = UTF8.GetByteCount(value);
      WriteVariableLength(byteCount);
      CheckWriteBytes(byteCount);
      UTF8.GetBytes(value, 0, value.Length, m_BufferBytes, WriteIndex);
      WriteIndex += byteCount;
    }

    unsafe public void WriteArray<T>(T[] array, int index, int length) where T : unmanaged {
      int byteCount = length * sizeof(T);
      CheckWriteBytes(byteCount);
      ByteArray.Copy(m_BufferBytes, WriteIndex, array, index, byteCount);
      WriteIndex += byteCount;
    }

    public void WriteArray<T>(T[] array) where T : unmanaged {
      WriteArray(array, 0, array.Length);
    }

    public void AddBuffer(Buffer buffer){
      WriteArray<byte>(buffer.m_BufferBytes, buffer.ReadIndex, buffer.Length);
    }

    unsafe public T Read<T>() where T : unmanaged {
      int byteCount = sizeof(T);
      T value;
      unsafe {
        ByteArray.Copy(&value, m_BufferBytes, ReadIndex, byteCount);
      }
      ReadIndex += byteCount;
      return value;
    }

    public byte ReadUInt8(){
      return m_BufferBytes[ReadIndex++];
    }

    public sbyte ReadInt8(){
      return (sbyte)ReadUInt8();
    }

    public bool ReadBool(){
      return ReadUInt8() != 0;
    }

    public ushort ReadUInt16(){
      return Read<ushort>();
    }

    public short ReadInt16(){
      return Read<short>();
    }

    public uint ReadUInt32(){
      return Read<uint>();
    }

    public int ReadInt32(){
      return Read<int>();
    }

    public ulong ReadUInt64(){
      return Read<ulong>();
    }

    public long ReadInt64(){
      return Read<long>();
    }

    public int ReadVariableLength(){
      byte value = ReadUInt8();
      if (value < (byte)VariableLengthType.UInt16) return (int)value;
      return (value == (byte)VariableLengthType.UInt16) ? (int)ReadUInt16() : ReadInt32();
    }

    public float ReadFloat(){
      return Read<float>();
    }

    public double ReadDouble(){
      return Read<double>();
    }

    public string ReadString(){
      int byteCount = ReadVariableLength();
      string value = UTF8.GetString(m_BufferBytes, ReadIndex, byteCount);
      ReadIndex += byteCount;
      return value;
    }

    public int ReadStringBytes(ref byte[] bytes, int index){
      int byteCount = ReadVariableLength();
      ByteArray.ResizeIfOversized<byte>(ref bytes, byteCount);
      ReadArray<byte>(bytes, index, byteCount);
      return byteCount;
    }

    unsafe public void ReadArray<T>(T[] array, int index, int length) where T : unmanaged {
      int byteCount = length * sizeof(T);
      ByteArray.Copy(array, index, m_BufferBytes, ReadIndex, byteCount);
      ReadIndex += byteCount;
    }

    unsafe public void ReadArray<T>(T[] array) where T : unmanaged {
      ReadArray<T>(array, 0, array.Length);
    }

    public void ResizeAndCopyBytes(ref byte[] bytes){
      int length = Length;
      ByteArray.ResizeIfNotSamesized<byte>(ref bytes, length);
      ByteArray.Copy(bytes, 0, m_BufferBytes, ReadIndex, length);
    }

    public override string ToString(){
      int length = Length;
      int byteCount = length * 2;
      ByteArray.ResizeIfOversized<byte>(ref m_StringBytes, byteCount);
      for (int i = 0; i < length; ++i){
        ByteArray.Copy(m_StringBytes, i * 2, HexBytes[(int)m_BufferBytes[ReadIndex + i]], 0, 2);
      }
      return UTF8.GetString(m_StringBytes, 0, byteCount);
    }
  }
}