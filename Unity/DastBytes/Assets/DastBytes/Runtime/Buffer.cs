using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DastBytes {
  public class Buffer {
    static private UTF8Encoding UTF8 = new UTF8Encoding(false);

    static private string[] HexByteStrings;

    static Buffer(){
      HexByteStrings = new string[256];
      for (int i = 0; i < HexByteStrings.Length; ++i){
        HexByteStrings[i] = $"{i:X2}";
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

    public void Write(string value){
      int byteCount = UTF8.GetByteCount(value);
      Write(byteCount);
      CheckWriteBytes(byteCount);
      UTF8.GetBytes(value, 0, value.Length, m_Bytes, m_WriteIndex);
      m_WriteIndex += byteCount;
    }

    public void Write(Buffer value){
      Write(value.Length);
      Write(value.AsReadOnlySpan());
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

    public string Read(){
      int byteCount = Read<int>();
      int readIndex = m_ReadIndex;
      m_ReadIndex += byteCount;
      return UTF8.GetString(m_Bytes, readIndex, byteCount);
    }

    public Buffer Read(Buffer buffer){
      int length = Read<int>();
      buffer.Write(m_Bytes.AsSpan(m_ReadIndex, length));
      m_ReadIndex += length;
      return buffer;
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