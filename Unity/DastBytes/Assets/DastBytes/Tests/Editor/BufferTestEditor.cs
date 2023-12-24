using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace DastBytes {
  public class BufferTestEditor {
    static private readonly int PerformanceCount = 1000000;

    [Test]
    public void InitializeTest(){
      var buffer = new Buffer();

      Assert.That(buffer.Length == 0);
    }

    [Test]
    public void BoolTest(){
      var buffer = new Buffer();

      buffer.Write(false);
      Assert.That(buffer.ToString() == "00");
      CheckEquals(buffer, new byte[]{0});
      Assert.That(!buffer.Read<bool>());

      buffer.Write(true);
      Assert.That(buffer.ToString() == "01");
      CheckEquals(buffer, new byte[]{1});
      Assert.That(buffer.Read<bool>());
    }

    [Test]
    public void ByteTest(){
      var buffer = new Buffer();

      buffer.Write(sbyte.MinValue);
      Assert.That(buffer.ToString() == "80");
      CheckEquals(buffer, new byte[]{0x80});
      Assert.That(buffer.Read<sbyte>() == sbyte.MinValue);

      buffer.Write(sbyte.MaxValue);
      Assert.That(buffer.ToString() == "7F");
      CheckEquals(buffer, new byte[]{0x7F});
      Assert.That(buffer.Read<sbyte>() == sbyte.MaxValue);

      buffer.Write((byte)0x01);
      Assert.That(buffer.ToString() == "01");
      CheckEquals(buffer, new byte[]{0x01});
      Assert.That(buffer.Read<byte>() == 0x01);
    }

    [Test]
    public void ShortTest(){
      var buffer = new Buffer();

      buffer.Write(short.MinValue);
      CheckEquals(buffer, BitConverter.GetBytes(short.MinValue));
      Assert.That(buffer.ToString() == "0080");
      CheckEquals(buffer, new byte[]{0x00, 0x80});
      Assert.That(buffer.Read<short>() == short.MinValue);

      buffer.Write(short.MaxValue);
      CheckEquals(buffer, BitConverter.GetBytes(short.MaxValue));
      Assert.That(buffer.ToString() == "FF7F");
      CheckEquals(buffer, new byte[]{0xFF, 0x7F});
      Assert.That(buffer.Read<short>() == short.MaxValue);

      buffer.Write((ushort)0x0123);
      CheckEquals(buffer, BitConverter.GetBytes((ushort)0x0123));
      Assert.That(buffer.ToString() == "2301");
      CheckEquals(buffer, new byte[]{0x23, 0x01});
      Assert.That(buffer.Read<ushort>() == 0x0123);
    }

    [Test]
    public void IntTest(){
      var buffer = new Buffer();

      buffer.Write(int.MinValue);
      CheckEquals(buffer, BitConverter.GetBytes(int.MinValue));
      Assert.That(buffer.ToString() == "00000080");
      CheckEquals(buffer, new byte[]{0x00, 0x00, 0x00, 0x80});
      Assert.That(buffer.Read<int>() == int.MinValue);

      buffer.Write(int.MaxValue);
      CheckEquals(buffer, BitConverter.GetBytes(int.MaxValue));
      Assert.That(buffer.ToString() == "FFFFFF7F");
      CheckEquals(buffer, new byte[]{0xFF, 0xFF, 0xFF, 0x7F});
      Assert.That(buffer.Read<int>() == int.MaxValue);

      buffer.Write((uint)0x01234567);
      CheckEquals(buffer, BitConverter.GetBytes((uint)0x01234567));
      Assert.That(buffer.ToString() == "67452301");
      CheckEquals(buffer, new byte[]{0x67, 0x45, 0x23, 0x01});
      Assert.That(buffer.Read<uint>() == 0x01234567);
    }

    [Test]
    public void LongTest(){
      var buffer = new Buffer();

      buffer.Write(long.MinValue);
      CheckEquals(buffer, BitConverter.GetBytes(long.MinValue));
      Assert.That(buffer.ToString() == "0000000000000080");
      CheckEquals(buffer, new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80});
      Assert.That(buffer.Read<long>() == long.MinValue);

      buffer.Write(long.MaxValue);
      CheckEquals(buffer, BitConverter.GetBytes(long.MaxValue));
      Assert.That(buffer.ToString() == "FFFFFFFFFFFFFF7F");
      CheckEquals(buffer, new byte[]{0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F});
      Assert.That(buffer.Read<long>() == long.MaxValue);

      buffer.Write((ulong)0x0123456789ABCDEF);
      CheckEquals(buffer, BitConverter.GetBytes((ulong)0x0123456789ABCDEF));
      Assert.That(buffer.ToString() == "EFCDAB8967452301");
      CheckEquals(buffer, new byte[]{0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01});
      Assert.That(buffer.Read<ulong>() == 0x0123456789ABCDEF);
    }

    [Test]
    public void FloatTest(){
      var buffer = new Buffer();

      buffer.Write((float)2.998817E-38);
      CheckEquals(buffer, BitConverter.GetBytes((float)2.998817E-38));
      Assert.That(buffer.ToString() == "69452301");
      CheckEquals(buffer, new byte[]{0x69, 0x45, 0x23, 0x01});
      Assert.That(buffer.Read<float>() == 2.998817E-38f);
    }

    [Test]
    public void DoubleTest(){
      var buffer = new Buffer();

      buffer.Write(3.5127005640885E-303);
      CheckEquals(buffer, BitConverter.GetBytes(3.5127005640885E-303));
      Assert.That(buffer.ToString() == "E9CDAB8967452301");
      CheckEquals(buffer, new byte[]{0xE9, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01});
      Assert.That(buffer.Read<double>() == 3.5127005640885E-303);
    }

    [Test]
    public void StringTest(){
      var buffer = new Buffer();

      for (var i = 0; i < 256; ++i){
        Assert.That(Buffer.GetHexByteString((byte)i) == $"{i:X2}");
      }

      buffer.Write("data", 4);
      Assert.That(buffer.Length == 4);
      Assert.That(buffer.Read(4) == "data");

      buffer.Write("ア");
      Assert.That(buffer.Read() == "ア");
      buffer.Write("イ");
      Assert.That(buffer.Read() == "イ");
      buffer.Write("ウ");
      Assert.That(buffer.Read() == "ウ");
      buffer.Write("エ");
      Assert.That(buffer.Read() == "エ");
      buffer.Write("オ");
      Assert.That(buffer.Read() == "オ");

      buffer.Write("あいうえお");
      Assert.That(buffer.Length == 19);
      Assert.That(buffer.ToString() == "0F000000E38182E38184E38186E38188E3818A");
      CheckEquals(buffer, new byte[]{0x0F, 0x00, 0x00, 0x00, 0xE3, 0x81, 0x82, 0xE3, 0x81, 0x84, 0xE3, 0x81, 0x86, 0xE3, 0x81, 0x88, 0xE3, 0x81, 0x8A});
      Assert.That(buffer.Read<int>() == 15);
      CheckEquals(buffer, Encoding.UTF8.GetBytes("あいうえお"));
    }

    [Test]
    public void Vector3Test(){
      var buffer = new Buffer();

      buffer.Write(new Vector3(1, 2, 3));
      Assert.That(buffer.Read<Vector3>() == new Vector3(1, 2, 3));
    }

    [Test]
    public void QuaternionTest(){
      var buffer = new Buffer();

      buffer.Write(new Quaternion(1, 2, 3, 4));
      Assert.That(buffer.Read<Quaternion>() == new Quaternion(1, 2, 3, 4));
    }

    [Test]
    public void IntSpanTest(){
      var buffer = new Buffer();

      var writeArray = new int[]{1, 2, 3};
      buffer.Write(writeArray.AsSpan());
      Assert.That(buffer.ToString() == "010000000200000003000000");

      var readSpan = (new int[3]).AsSpan();
      buffer.Read(readSpan);
      for (var i = 0; i < readSpan.Length; ++i){
        Assert.That(readSpan[i] == writeArray[i]);
      }

      writeArray = new int[]{4, 5, 6};
      buffer.Write(new ReadOnlySpan<int>(writeArray));
      Assert.That(buffer.ToString() == "040000000500000006000000");

      buffer.Read(readSpan);
      for (var i = 0; i < readSpan.Length; ++i){
        Assert.That(readSpan[i] == writeArray[i]);
      }

      writeArray = new int[]{7, 8, 9};
      buffer.Write(new ReadOnlySpan<int>(writeArray));
      Assert.That(buffer.ToString() == "070000000800000009000000");
      var readOnlySpan = buffer.AsReadOnlySpan();
      Assert.That(readOnlySpan[0] == 7);
      Assert.That(readOnlySpan[1] == 0);
      Assert.That(readOnlySpan[2] == 0);
      Assert.That(readOnlySpan[3] == 0);
      Assert.That(readOnlySpan[4] == 8);
      Assert.That(readOnlySpan[5] == 0);
      Assert.That(readOnlySpan[6] == 0);
      Assert.That(readOnlySpan[7] == 0);
      Assert.That(readOnlySpan[8] == 9);
      Assert.That(readOnlySpan[9] == 0);
      Assert.That(readOnlySpan[10] == 0);
      Assert.That(readOnlySpan[11] == 0);
    }

    [Test]
    public void BufferTest(){
      var buffer1 = new Buffer();
      buffer1.Write(10);
      CheckEquals(buffer1, new byte[]{0x0A, 0x00, 0x00, 0x00});

      var buffer2 = new Buffer();
      buffer2.Write(true);
      buffer2.Write(buffer1);
      Assert.That(buffer2.Length == 9);
      Assert.That(buffer2.ToString() == "01040000000A000000");
      CheckEquals(buffer2, new byte[]{0x01, 0x04, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00});

      var buffer3 = new Buffer();
      Assert.That(buffer2.Read<bool>());
      buffer2.Read(buffer3);
      Assert.That(buffer2.Length == 0);
      Assert.That(buffer3.Length == 4);
      Assert.That(buffer3.ToString() == "0A000000");
      CheckEquals(buffer3, buffer1.AsBytes());
    }

    [Test]
    public void VariableLengthTest(){
      var buffer = new Buffer();

      buffer.WriteVariableLength(0x01);
      Assert.That(buffer.Length == 1);
      Assert.That(buffer.ToString() == "01");
      Assert.That(buffer.ReadVariableLength() == 0x01);

      buffer.WriteVariableLength(0x0123);
      Assert.That(buffer.Length == 2);
      Assert.That(buffer.ToString() == "4123");
      Assert.That(buffer.ReadVariableLength() == 0x0123);

      buffer.WriteVariableLength(0x01234567);
      Assert.That(buffer.Length == 4);
      Assert.That(buffer.ToString() == "81234567");
      Assert.That(buffer.ReadVariableLength() == 0x01234567);

      buffer.WriteVariableLength(0x0123456789ABCDEF);
      Assert.That(buffer.Length == 8);
      Assert.That(buffer.ToString() == "C123456789ABCDEF");
      Assert.That(buffer.ReadVariableLength() == 0x0123456789ABCDEF);
    }

    [Test]
    public void ArrayTest(){
      var buffer = new Buffer();

      var writeFloats = new float[256];
      for (var i = 0; i < writeFloats.Length; ++i){
        writeFloats[i] = i;
      }
      buffer.Write(writeFloats);

      var readFloats = new float[writeFloats.Length];
      buffer.Read(readFloats);
      for (var i = 0; i < readFloats.Length; ++i){
        Assert.That(readFloats[i] == i);
        Assert.That(writeFloats[i] == readFloats[i]);
      }

      var writeVector3Array = new Vector3[]{new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9)};
      buffer.Write(writeVector3Array);

      var readVector3Array = new Vector3[writeVector3Array.Length];
      buffer.Read(readVector3Array);
      for (var i = 0; i < readVector3Array.Length; ++i){
        Assert.That(readVector3Array[i] == writeVector3Array[i]);
      }
    }

    [Test]
    public void CompoundTest(){
      var buffer = new Buffer();

      Assert.That(buffer.Length == 0);
      Assert.That(buffer.ToString() == "");

      buffer.Write<byte>(1);
      Assert.That(buffer.Length == 1);
      Assert.That(buffer.ToString() == "01");
      Assert.That(buffer.Read<byte>() == 1);

      buffer.Write<ushort>(2);
      Assert.That(buffer.Length == 2);
      Assert.That(buffer.ToString() == "0200");
      Assert.That(buffer.Read<ushort>() == 2);

      buffer.Write<uint>(3);
      Assert.That(buffer.Length == 4);
      Assert.That(buffer.ToString() == "03000000");
      Assert.That(buffer.Read<uint>() == 3);

      buffer.Write<ulong>(4);
      Assert.That(buffer.Length == 8);
      Assert.That(buffer.ToString() == "0400000000000000");
      Assert.That(buffer.Read<ulong>() == 4);
    }

    [Test]
    public void RewriteTest(){
      var buffer = new Buffer();

      buffer.Write<byte>(1);
      buffer.Write(2);
      Assert.That(buffer.Length == 5);
      Assert.That(buffer.ToString() == "0102000000");
      buffer.Rewrite(rewriteBuffer => {
        rewriteBuffer.Write<byte>(0);
      });
      Assert.That(buffer.Length == 5);
      Assert.That(buffer.ToString() == "0002000000");
      buffer.Rewrite(rewriteBuffer => {
        rewriteBuffer.Write<byte>(2);
        rewriteBuffer.Write(3);
        rewriteBuffer.Write<byte>(4);
      });
      Assert.That(buffer.Length == 6);
      Assert.That(buffer.ToString() == "020300000004");
    }

    [Test]
    public void RereadTest(){
      var buffer = new Buffer();

      buffer.Write<byte>(1);
      buffer.Write(2);
      Assert.That(buffer.Length == 5);
      Assert.That(buffer.ToString() == "0102000000");
      buffer.Reread(rereadBuffer => {
        Assert.That(rereadBuffer.Read<byte>() == 1);
      });
      Assert.That(buffer.Length == 5);
      Assert.That(buffer.ToString() == "0102000000");
      buffer.Reread(rereadBuffer => {
        Assert.That(rereadBuffer.Read<byte>() == 1);
        Assert.That(rereadBuffer.Read<int>() == 2);
        rereadBuffer.Write(3);
        Assert.That(rereadBuffer.Read<int>() == 3);
      });
      Assert.That(buffer.Length == 9);
      Assert.That(buffer.ToString() == "010200000003000000");
    }

    [Test]
    public void SBytePerformanceTest(){
      PerformanceTest((sbyte)1);
    }

    [Test]
    public void ShortPerformanceTest(){
      PerformanceTest((short)1);
    }

    [Test]
    public void IntPerformanceTest(){
      PerformanceTest((int)1);
    }

    [Test]
    public void LongPerformanceTest(){
      PerformanceTest((long)1);
    }

    [Test]
    public void FloatPerformanceTest(){
      PerformanceTest((float)1);
    }

    [Test]
    public void DoublePerformanceTest(){
      PerformanceTest((double)1);
    }

    private void PerformanceTest<T>(T value) where T : unmanaged {
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.Write(value);
        if(!buffer.Read<T>().Equals(value)){
          Log(buffer);
          Assert.That(false);
        }
      }
    }

    [Test]
    public void StringPerformanceTest(){
      var buffer = new Buffer();
      var value = "1234";
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.Write(value);
        if (!buffer.Read().Equals(value)){
          Log(buffer);
          Assert.That(false);
        }
      }
    }

    [Test]
    public void BufferPerformanceTest(){
      var writeBuffer = new Buffer();
      var tmpBuffer = new Buffer();
      var readBuffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        writeBuffer.Clear();
        writeBuffer.Write(i);
        tmpBuffer.Write(writeBuffer);
        if(!tmpBuffer.Read(readBuffer).Read<int>().Equals(i)){
          Log("W", writeBuffer);
          Log("T", tmpBuffer);
          Log("R", readBuffer);
          Assert.That(false);
        }
      }
    }

    private void CheckEquals(Buffer buffer, byte[] checkBytes){
//      Log(buffer);
      CheckEquals(buffer.AsBytes(), checkBytes);
    }

    private void CheckEquals(byte[] bytes1, byte[] bytes2){
      var bytes1Length = bytes1.Length;
      var bytes2Length = bytes2.Length;
      if (bytes1Length != bytes2Length){
        Debug.Log($"bytes1.Length({bytes1Length}) != bytes2.Length({bytes2Length})");
        Assert.That(false);
      }

      for (var i = 0; i < bytes1Length; ++i){
        if (bytes1[i] != bytes2[i]){
          Debug.Log($"i={i} ({bytes1[i]} != {bytes2[i]})");
          Assert.That(false);
        }
      }
    }

    private void Log(Buffer buffer){
      Debug.Log($"Length={buffer.Length} ToString={buffer.ToString()}");
    }

    private void Log(string caption, Buffer buffer){
      Debug.Log($"{caption}: Length={buffer.Length} ToString={buffer.ToString()}");
    }
  }
}