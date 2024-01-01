# DastBytes
Reading and writing data structures in byte arrays

## Support endianness
Little endian only

## Unity
`https://github.com/mskz-3110/DastBytes.git?path=/Unity/DastBytes/Assets/DastBytes/Runtime#v1.0.1`

|Version|Summary|
|:--|:--|
|1.0.1|Update String and VariableLength|
|1.0.0|Initial version|

## Usage
```cs
using DastBytes;

var buffer = new Buffer();
buffer.Write("data", 4);
buffer.Write(4);
buffer.Write(1.5f);
string stringValue = buffer.Read(4); // data
int intValue = bufer.Read<int>(); // 4
float floatValue = buffer.Read<float>(); // 1.5
```
