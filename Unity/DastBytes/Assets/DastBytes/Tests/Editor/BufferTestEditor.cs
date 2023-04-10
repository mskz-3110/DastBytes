using NUnit.Framework;

namespace DastBytes {
  public class BufferTestEditor {
    static private readonly int PerformanceCount = 1000000;

    [Test]
    public void InitializeTest(){
      var buffer = new Buffer();

      Assert.That(buffer.Length == 0);
      Assert.That(buffer.Capacity == 1024);
    }

    [Test]
    public void BoolTest(){
      var buffer = new Buffer();

      buffer.WriteBool(false);
      Assert.That(!buffer.ReadBool());
      buffer.WriteBool(true);
      Assert.That(buffer.ReadBool());
    }

    [Test]
    public void Int8Test(){
      var buffer = new Buffer();

      buffer.WriteInt8(sbyte.MinValue);
      Assert.That(buffer.ToString() == "80");
      Assert.That(buffer.ReadInt8() == sbyte.MinValue);
      buffer.WriteInt8(sbyte.MaxValue);
      Assert.That(buffer.ToString() == "7F");
      Assert.That(buffer.ReadInt8() == sbyte.MaxValue);

      buffer.WriteUInt8((byte)0x01);
      Assert.That(buffer.ToString() == "01");
      Assert.That(buffer.ReadUInt8() == (byte)0x01);
    }

    [Test]
    public void Int16Test(){
      var buffer = new Buffer();

      buffer.WriteInt16(short.MinValue);
      Assert.That(buffer.ToString() == "0080");
      Assert.That(buffer.ReadInt16() == short.MinValue);
      buffer.WriteInt16(short.MaxValue);
      Assert.That(buffer.ToString() == "FF7F");
      Assert.That(buffer.ReadInt16() == short.MaxValue);

      buffer.WriteUInt16((ushort)0x0123);
      Assert.That(buffer.ToString() == "2301");
      Assert.That(buffer.ReadUInt16() == (ushort)0x0123);
    }

    [Test]
    public void Int32Test(){
      var buffer = new Buffer();

      buffer.WriteInt32(int.MinValue);
      Assert.That(buffer.ToString() == "00000080");
      Assert.That(buffer.ReadInt32() == int.MinValue);
      buffer.WriteInt32(int.MaxValue);
      Assert.That(buffer.ToString() == "FFFFFF7F");
      Assert.That(buffer.ReadInt32() == int.MaxValue);

      buffer.WriteUInt32((uint)0x01234567);
      Assert.That(buffer.ToString() == "67452301");
      Assert.That(buffer.ReadUInt32() == (uint)0x01234567);
    }

    [Test]
    public void Int64Test(){
      var buffer = new Buffer();

      buffer.WriteInt64(long.MinValue);
      Assert.That(buffer.ToString() == "0000000000000080");
      Assert.That(buffer.ReadInt64() == long.MinValue);
      buffer.WriteInt64(long.MaxValue);
      Assert.That(buffer.ToString() == "FFFFFFFFFFFFFF7F");
      Assert.That(buffer.ReadInt64() == long.MaxValue);

      buffer.WriteUInt64((ulong)0x0123456789ABCDEF);
      Assert.That(buffer.ToString() == "EFCDAB8967452301");
      Assert.That(buffer.ReadUInt64() == (ulong)0x0123456789ABCDEF);
    }

    [Test]
    public void VariableLengthTest(){
      var buffer = new Buffer();

      buffer.WriteVariableLength((byte)0x01);
      Assert.That(buffer.Length == 1);
      Assert.That(buffer.ToString() == "01");
      Assert.That(buffer.ReadVariableLength() == (int)0x01);
      buffer.WriteVariableLength((byte)0xFD);
      Assert.That(buffer.Length == 1);
      Assert.That(buffer.ToString() == "FD");
      Assert.That(buffer.ReadVariableLength() == (int)0xFD);

      buffer.WriteVariableLength((byte)0xFE);
      Assert.That(buffer.Length == 3);
      Assert.That(buffer.ToString() == "FEFE00");
      Assert.That(buffer.ReadVariableLength() == (int)0xFE);
      buffer.WriteVariableLength((byte)0xFF);
      Assert.That(buffer.Length == 3);
      Assert.That(buffer.ToString() == "FEFF00");
      Assert.That(buffer.ReadVariableLength() == (int)0xFF);

      buffer.WriteVariableLength(511);
      Assert.That(buffer.Length == 3);
      Assert.That(buffer.ToString() == "FEFF01");
      Assert.That(buffer.ReadVariableLength() == 511);
      buffer.WriteVariableLength(65535);
      Assert.That(buffer.Length == 3);
      Assert.That(buffer.ToString() == "FEFFFF");
      Assert.That(buffer.ReadVariableLength() == 65535);

      buffer.WriteVariableLength(131071);
      Assert.That(buffer.Length == 5);
      Assert.That(buffer.ToString() == "FFFFFF0100");
      Assert.That(buffer.ReadVariableLength() == 131071);
    }

    [Test]
    public void FloatTest(){
      var buffer = new Buffer();

      buffer.WriteFloat((float)2.998817E-38);
      Assert.That(buffer.ToString() == "69452301");
      Assert.That(buffer.ReadFloat() == (float)2.998817E-38);
    }

    [Test]
    public void DoubleTest(){
      var buffer = new Buffer();

      buffer.WriteDouble(3.5127005640885E-303);
      Assert.That(buffer.ToString() == "E9CDAB8967452301");
      Assert.That(buffer.ReadDouble() == 3.5127005640885E-303);
    }

    [Test]
    public void StringTest(){
      var buffer = new Buffer();

      buffer.WriteString("ア");
      Assert.That(buffer.ReadString() == "ア");
      buffer.WriteString("イ");
      Assert.That(buffer.ReadString() == "イ");
      buffer.WriteString("ウ");
      Assert.That(buffer.ReadString() == "ウ");
      buffer.WriteString("エ");
      Assert.That(buffer.ReadString() == "エ");
      buffer.WriteString("オ");
      Assert.That(buffer.ReadString() == "オ");
    }

    [Test]
    public void ToStringTest(){
      var buffer = new Buffer();

      buffer.WriteString("あいうえお");
      Assert.That(buffer.Length == 16);
      Assert.That(buffer.ToString() == "0FE38182E38184E38186E38188E3818A");

      var stringBytes = new byte[0];
      Assert.That(buffer.ReadStringBytes(ref stringBytes, 0) == 15);
      Assert.That(stringBytes[0] == 0xE3);
      Assert.That(stringBytes[1] == 0x81);
      Assert.That(stringBytes[2] == 0x82);
      Assert.That(stringBytes[3] == 0xE3);
      Assert.That(stringBytes[4] == 0x81);
      Assert.That(stringBytes[5] == 0x84);
      Assert.That(stringBytes[6] == 0xE3);
      Assert.That(stringBytes[7] == 0x81);
      Assert.That(stringBytes[8] == 0x86);
      Assert.That(stringBytes[9] == 0xE3);
      Assert.That(stringBytes[10] == 0x81);
      Assert.That(stringBytes[11] == 0x88);
      Assert.That(stringBytes[12] == 0xE3);
      Assert.That(stringBytes[13] == 0x81);
      Assert.That(stringBytes[14] == 0x8A);
    }

    [Test]
    public void BufferTest(){
      var buffer1 = new Buffer();
      buffer1.WriteInt32(10);

      var buffer2 = new Buffer();
      buffer2.WriteBool(true);
      buffer2.AddBuffer(buffer1);
      Assert.That(buffer2.Length == 5);
      Assert.That(buffer2.ToString() == "010A000000");
    }

    [Test]
    public void CopyTest(){
      var buffer = new Buffer(1);

      buffer.WriteUInt8(1);
      buffer.WriteUInt16(2);
      buffer.WriteUInt32(3);
      buffer.WriteUInt64(4);
      Assert.That(buffer.Capacity == 16);
      Assert.That(buffer.Length == 15);
      Assert.That(buffer.ToString() == "010200030000000400000000000000");

      buffer.ResizeAndCopyBytes(ref buffer.WorkingBytes);
      buffer.WorkingBytes[0] = 0x00;
      Assert.That(buffer.Capacity == 16);
      Assert.That(buffer.Length == 15);
      Assert.That(buffer.ToString() == "010200030000000400000000000000");
      buffer.Clear();
      buffer.WorkingBytes[0] = 0xFF;
      buffer.WriteArray<byte>(buffer.WorkingBytes);
      Assert.That(buffer.Capacity == 16);
      Assert.That(buffer.Length == 15);
      Assert.That(buffer.ToString() == "FF0200030000000400000000000000");
    }

    [Test]
    public void ArrayTest(){
      var buffer = new Buffer();

      var writeFloats = new float[255];
      for (var i = 0; i < writeFloats.Length; ++i){
        writeFloats[i] = i;
      }
      buffer.WriteArray<float>(writeFloats);

      var readFloats = new float[writeFloats.Length];
      buffer.ReadArray<float>(readFloats, 0, readFloats.Length);
      for (var i = 0; i < readFloats.Length; ++i){
        Assert.That(writeFloats[i] == readFloats[i]);
      }
    }

    [Test]
    public void ResizeTest(){
      var buffer = new Buffer();

      buffer.WriteInt32(1);
      Assert.That(buffer.Capacity == 1024);
      Assert.That(buffer.ReadIndex == 0);
      Assert.That(buffer.WriteIndex == 4);
      Assert.That(buffer.ToString() == "01000000");

      buffer.Resize(1);
      Assert.That(buffer.Capacity == 1);
      Assert.That(buffer.ReadIndex == 0);
      Assert.That(buffer.WriteIndex == 1);
      Assert.That(buffer.ToString() == "01");

      buffer.WriteInt32(2);
      Assert.That(buffer.Capacity == 8);
      Assert.That(buffer.ReadIndex == 0);
      Assert.That(buffer.WriteIndex == 5);
      Assert.That(buffer.ToString() == "0102000000");
    }

    [Test]
    public void CompoundTest(){
      var buffer = new Buffer(1);

      Assert.That(buffer.Capacity == 1);
      Assert.That(buffer.Length == 0);
      Assert.That(buffer.ToString() == "");

      buffer.WriteUInt8(1);
      Assert.That(buffer.Capacity == 1);
      Assert.That(buffer.Length == 1);
      Assert.That(buffer.ToString() == "01");
      Assert.That(buffer.ReadUInt8() == 1);

      buffer.WriteUInt16(2);
      Assert.That(buffer.Capacity == 4);
      Assert.That(buffer.Length == 2);
      Assert.That(buffer.ToString() == "0200");
      Assert.That(buffer.ReadUInt16() == 2);

      buffer.WriteUInt32(3);
      Assert.That(buffer.Capacity == 8);
      Assert.That(buffer.Length == 4);
      Assert.That(buffer.ToString() == "03000000");
      Assert.That(buffer.ReadUInt32() == 3);

      buffer.WriteUInt64(4);
      Assert.That(buffer.Capacity == 16);
      Assert.That(buffer.Length == 8);
      Assert.That(buffer.ToString() == "0400000000000000");
      Assert.That(buffer.ReadUInt64() == 4);
    }

    [Test]
    public void Int8PerformanceTest(){
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.WriteInt8((sbyte)i);
        PerformanceCheckLogIfNotEqual<sbyte>(buffer.ReadInt8(), (sbyte)i);
      }
    }

    [Test]
    public void Int16PerformanceTest(){
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.WriteInt16((short)i);
        PerformanceCheckLogIfNotEqual<short>(buffer.ReadInt16(), (short)i);
      }
    }

    [Test]
    public void Int32PerformanceTest(){
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.WriteInt32(i);
        PerformanceCheckLogIfNotEqual<int>(buffer.ReadInt32(), i);
      }
    }

    [Test]
    public void Int64PerformanceTest(){
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.WriteInt64((long)i);
        PerformanceCheckLogIfNotEqual<long>(buffer.ReadInt64(), (long)i);
      }
    }

    [Test]
    public void VariableLengthPerformanceTest(){
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.WriteVariableLength(i);
        PerformanceCheckLogIfNotEqual<int>(buffer.ReadVariableLength(), i);
      }
    }

    [Test]
    public void FloatPerformanceTest(){
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.WriteFloat((float)i);
        PerformanceCheckLogIfNotEqual<float>(buffer.ReadFloat(), (float)i);
      }
    }

    [Test]
    public void DoublePerformanceTest(){
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.WriteDouble((double)i);
        PerformanceCheckLogIfNotEqual<double>(buffer.ReadDouble(), (double)i);
      }
    }

    [Test]
    public void StringPerformanceTest(){
      var buffer = new Buffer();
      for (var i = 0; i < PerformanceCount; ++i){
        buffer.WriteString("1234");
        PerformanceCheckLogIfNotEqual<string>(buffer.ReadString(), "1234");
      }
    }

    private void PerformanceCheckLogIfNotEqual<T>(T value1, T value2){
//      LogIfNotEqual<T>(value1, value2);
    }

    private void LogIfNotEqual<T>(T value1, T value2){
      if (!value1.Equals(value2)){
        UnityEngine.Debug.LogError($"{value1} != {value2}");
      }
    }

    private void Log(Buffer buffer){
      UnityEngine.Debug.Log($"Capacity={buffer.Capacity} Length={buffer.Length} ReadIndex={buffer.ReadIndex} WriteIndex={buffer.WriteIndex} ToString={buffer.ToString()}");
    }
  }
}