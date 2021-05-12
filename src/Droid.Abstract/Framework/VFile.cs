using Droid.Core;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Droid.Framework
{
    // mode parm for Seek
    public enum FS_SEEK
    {
        CUR,
        END,
        SET
    }

    public class VFile
    {
        // Get the name of the file.
        public virtual string GetName() => throw new NotImplementedException();
        // Get the full file path.
        public virtual string GetFullPath() => throw new NotImplementedException();
        // Read data from the file to the buffer.
        public virtual int Read(byte[] buffer, int len) => throw new NotImplementedException();
        // Write data from the buffer to the file.
        public virtual int Write(byte[] buffer, int len) => throw new NotImplementedException();
        // Returns the length of the file.
        public virtual int Length() => throw new NotImplementedException();
        // Return a time value for reload operations.
        public virtual DateTime Timestamp() => throw new NotImplementedException();
        // Returns offset in file.
        public virtual int Tell() => throw new NotImplementedException();
        // Forces flush on files being writting to.
        public virtual void ForceFlush() => throw new NotImplementedException();
        // Causes any buffered data to be written to the file.
        public virtual void Flush() => throw new NotImplementedException();
        // Seek on a file.
        public virtual int Seek(long offset, FS_SEEK origin) => throw new NotImplementedException();
        // Go back to the beginning of the file.
        public virtual void Rewind() => throw new NotImplementedException();
        // Like fprintf.
        public virtual int Printf(string fmt, params object[] args) => throw new NotImplementedException();
        // Like fprintf but with argument pointer
        public virtual int VPrintf(string fmt, object[] args) => throw new NotImplementedException();
        // Write a string with high precision floating point numbers to the file.
        public virtual int WriteFloatString(string fmt, params object[] args) => throw new NotImplementedException();

        // Endian portable alternatives to Read(...)
        public int ReadInt(out int value) => throw new NotImplementedException();
        public int ReadUnsignedInt(out uint value) => throw new NotImplementedException();
        public int ReadShort(out short value) => throw new NotImplementedException();
        public int ReadUnsignedShort(out ushort value) => throw new NotImplementedException();
        public int ReadChar(out char value) => throw new NotImplementedException();
        public int ReadUnsignedChar(out byte value) => throw new NotImplementedException();
        public int ReadFloat(out float value) => throw new NotImplementedException();
        public int ReadBool(out bool value) => throw new NotImplementedException();
        public int ReadString(out string s) => throw new NotImplementedException();
        public int ReadVec2(out Vector2 vec) => throw new NotImplementedException();
        public int ReadVec3(out Vector3 vec) => throw new NotImplementedException();
        public int ReadVec4(out Vector4 vec) => throw new NotImplementedException();
        public int ReadVec6(out Vector6 vec) => throw new NotImplementedException();
        public int ReadMat3(out Matrix3x3 mat) => throw new NotImplementedException();

        // Endian portable alternatives to Write(...)
        public int WriteInt(int value) => throw new NotImplementedException();
        public int WriteUnsignedInt(uint value) => throw new NotImplementedException();
        public int WriteShort(short value) => throw new NotImplementedException();
        public int WriteUnsignedShort(ushort value) => throw new NotImplementedException();
        public int WriteChar(char value) => throw new NotImplementedException();
        public int WriteUnsignedChar(byte value) => throw new NotImplementedException();
        public int WriteFloat(float value) => throw new NotImplementedException();
        public int WriteBool(bool value) => throw new NotImplementedException();
        public int WriteString(string s) => throw new NotImplementedException();
        public int WriteVec2(Vector2 vec) => throw new NotImplementedException();
        public int WriteVec3(Vector3 vec) => throw new NotImplementedException();
        public int WriteVec4(Vector4 vec) => throw new NotImplementedException();
        public int WriteVec6(Vector6 vec) => throw new NotImplementedException();
        public int WriteMat3(Matrix3x3 mat) => throw new NotImplementedException();
    }

    public class VFile_Memory : VFile
    {
        public VFile_Memory() => throw new NotImplementedException();   // file for writing without name
        public VFile_Memory(string name) => throw new NotImplementedException();   // file for writing
        public VFile_Memory(string name, byte[] data, int length) => throw new NotImplementedException();   // file for writing
        //public VFile_Memory(string name, byte[] data, int length); // file for reading

        public override string GetName() => name;
        public override string GetFullPath() => name;
        public override int Read(byte[] buffer, int len) => throw new NotImplementedException();
        public override int Write(byte[] buffer, int len) => throw new NotImplementedException();
        public override int Length() => throw new NotImplementedException();
        public override DateTime Timestamp() => throw new NotImplementedException();
        public override int Tell() => throw new NotImplementedException();
        public override void ForceFlush() => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();
        public override int Seek(long offset, FS_SEEK origin) => throw new NotImplementedException();

        // changes memory file to read only
        public void MakeReadOnly() => throw new NotImplementedException();
        // clear the file
        public void Clear(bool freeMemory = true) => throw new NotImplementedException();
        // set data for reading
        public void SetData(byte[] data, int length) => throw new NotImplementedException();
        // returns const pointer to the memory buffer
        public byte[] GetDataPtr() => filePtr;
        // set the file granularity
        public void SetGranularity(int g) { Debug.Assert(g > 0); granularity = g; }

        string name;         // name of the file
        int mode;           // open mode
        int maxSize;        // maximum size of file
        int fileSize;       // size of the file
        int allocated;      // allocated size
        int granularity;    // file granularity
        byte[] filePtr;      // buffer holding the file data
        byte[] curPtr;           // current read/write pointer
    }

    public class VFile_BitMsg : VFile
    {
        public VFile_BitMsg(BitMsg msg) => throw new NotImplementedException();
        //public VFile_BitMsg(BitMsg msg) => throw new NotImplementedException();
        //virtual ~VFile_BitMsg( );

        public override string GetName() => name;
        public override string GetFullPath() => name;
        public override int Read(byte[] buffer, int len) => throw new NotImplementedException();
        public override int Write(byte[] buffer, int len) => throw new NotImplementedException();
        public override int Length() => throw new NotImplementedException();
        public override DateTime Timestamp() => throw new NotImplementedException();
        public override int Tell() => throw new NotImplementedException();
        public override void ForceFlush() => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();
        public override int Seek(long offset, FS_SEEK origin) => throw new NotImplementedException();

        string name;         // name of the file
        int mode;           // open mode
        BitMsg msg;
    }

    public class VFile_Permanent : VFile
    {
        public VFile_Permanent() => throw new NotImplementedException();
        //virtual ~VFile_Permanent( );

        public override string GetName() => name;
        public override string GetFullPath() => fullPath;
        public override int Read(byte[] buffer, int len) => throw new NotImplementedException();
        public override int Write(byte[] buffer, int len) => throw new NotImplementedException();
        public override int Length() => throw new NotImplementedException();
        public override DateTime Timestamp() => throw new NotImplementedException();
        public override int Tell() => throw new NotImplementedException();
        public override void ForceFlush() => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();
        public override int Seek(long offset, FS_SEEK origin) => throw new NotImplementedException();

        // returns file pointer
        public IntPtr GetFilePtr() => o;

        string name;            // relative path of the file - relative path
        string fullPath;        // full file path - OS path
        int mode;               // open mode
        int fileSize;           // size of the file
        IntPtr o;               // file handle
        bool handleSync;	    // true if written data is immediately flushed
    }

    public class VFile_InZip : VFile
    {
        public VFile_InZip() => throw new NotImplementedException();
        //virtual ~idFile_InZip( );

        public override string GetName() => name;
        public override string GetFullPath() => fullPath;
        public override int Read(byte[] buffer, int len) => throw new NotImplementedException();
        public override int Write(byte[] buffer, int len) => throw new NotImplementedException();
        public override int Length() => throw new NotImplementedException();
        public override DateTime Timestamp() => throw new NotImplementedException();
        public override int Tell() => throw new NotImplementedException();
        public override void ForceFlush() => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();
        public override int Seek(long offset, FS_SEEK rigin) => throw new NotImplementedException();

        string name;            // name of the file in the pak
        string fullPath;        // full file path including pak file name
        object zipFilePos;      // zip file info position in pak
        int fileSize;           // size of the file
        object z;				// unzip info
    }
}
