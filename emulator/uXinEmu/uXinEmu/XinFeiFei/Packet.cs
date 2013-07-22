using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using uXinEmu.XinFeiFei.Data_Model;

// ReSharper disable RedundantArgumentName

namespace uXinEmu.XinFeiFei
{
	internal class Packet
	{
		private static readonly Encoding Encoding = Encoding.GetEncoding("gb2312");

		protected readonly BinaryReader Reader;
		protected readonly BinaryWriter Writer;
		private int _size;

		public Packet(ushort opcode)
		{
			Opcode = (Opcode) opcode;

			Writer = new BinaryWriter(new MemoryStream(), Encoding);
			WriteUInt32(0); //Size placeholder
			WriteUInt16(opcode);
		}

		public Packet(byte[] buffer, int offset, int size)
		{
			Size = (ushort) size;

			Reader = new BinaryReader(new MemoryStream(buffer, offset, size), Encoding);
			var sizePacket = ReadUInt32();
			Opcode = (Opcode) Reader.ReadUInt16();

			Debug.Assert(size == sizePacket, "Size doesn't match!");
		}

		public Packet(Opcode opcode)
			: this((ushort) opcode)
		{
		}

		public Opcode Opcode { get; private set; }
		public int Size
		{
			get
			{
				if (Writer != null)
					_size = (int) Writer.BaseStream.Position;

				return _size;
			}
			private set
			{
				if (Writer == null)
					_size = value;
			}
		}

		public virtual byte[] GetBuffer()
		{
			Debug.Assert(Writer != null || Reader != null);

			if (Writer != null)
			{
				var size = Size;
				Writer.BaseStream.Seek(0, SeekOrigin.Begin);
				Writer.Write(size);
				Writer.BaseStream.Seek(size, SeekOrigin.Begin);

				return ((MemoryStream) Writer.BaseStream).GetBuffer();
			}
			
			if (Reader != null)
			{
				Reader.BaseStream.Seek(0, SeekOrigin.Begin);

				return ReadToEnd();
			}

			return null;
		}

		public ushort ReadUInt16()
		{
			return Reader.ReadUInt16();
		}

		public string ReadString()
		{
			return new string(Reader.ReadChars(count: ReadUInt16()));
		}

		public byte[] ReadBuffer()
		{
			//TODO: Find out how this works.

			return null;
		}

		public byte[] ReadToEnd()
		{
			return Reader.ReadBytes((int) (Size - Reader.BaseStream.Position));
		}

		public uint ReadUInt32()
		{
			return Reader.ReadUInt32();
		}

		public byte ReadByte()
		{
			return Reader.ReadByte();
		}

		public byte[] ReadBytes(int count)
		{
			return Reader.ReadBytes(count);
		}

		public void WriteUInt32(uint value)
		{
			Writer.Write(value);
		}

		public void WriteBoolean(bool value)
		{
			WriteByte((byte) (value ? 200 : 0));
		}

		public void WriteByte(byte value)
		{
			Writer.Write(value);
		}

		public void WriteInt32(int value)
		{
			Writer.Write(value);
		}

		public int ReadInt32()
		{
			return Reader.ReadInt32();
		}

		public void WriteString(string value)
		{
			WriteUInt16((ushort) value.Length);

			Writer.Write(Encoding.ASCII.GetBytes(value));
		}

		public void WriteUInt16(ushort value)
		{
			Writer.Write(value);
		}

		public void WriteInt16(short value)
		{
			Writer.Write(value);
		}

		public void WritePosition(Position value)
		{
			WriteFloat(value.X);
			WriteFloat(value.Y);
			WriteFloat(value.Z);
		}

		public void WriteFloat(float value)
		{
			Writer.Write(value);
		}

		public void WriteBytes(byte[] value)
		{
			Writer.Write(value);
		}

		public void WriteSByte(sbyte value)
		{
			Writer.Write(value);
		}

		public void WriteX(int value)
		{
			if (value == 0)
			{
				WriteUInt16(1);

				return;
			}

			BitArray bits;

			var temp = new BitArray(new[] { value });

			temp = BitWriter.TrimZero(temp, false);

			if (temp.Count <= 13)
			{
				bits = BitWriter.TrimZero(BitWriter.AddFront(temp, 2), true);

				bits[0] = true;
			}
			else
			{
				bits = BitWriter.TrimZero(BitWriter.AddFront(temp, 8), true);

				bits[1] = true;
			}

			var bytes = BitWriter.GetBytes(BitWriter.TrimZero(bits, true));

			if (bytes.Length == 4)
				bytes = BitWriter.GetBytes(BitWriter.AddBack(BitWriter.TrimZero(bits, true), 8));

			WriteBytes(bytes);
		}

		public void WriteFormat(string format, params object[] values)
		{
			var formatArray = format.ToLowerInvariant().ToCharArray();

			if (formatArray.Length != values.Length)
				throw new ArgumentException("Format length doesn't match the number of values.");

			for (var i = 0; i < formatArray.Length; i++)
				WriteFormat(formatArray[i], values[i]);
		}

		public void WriteFormat(char format, object value)
		{
			switch (format)
			{
				case 'b':
					WriteByte((byte) value);
					break;
				case 'c':
					WriteSByte((sbyte) value);
					break;
				case 'f':
					WriteFloat((float) value);
					break;
				case 'i':
					WriteInt32((int) value);
					break;
				case 's':
					WriteString((string) value);
					break;
				case 'u':
					WriteUInt32((uint) value);
					break;
				case 'w':
					WriteUInt16((ushort) value);
					break;
				case 'x':
					WriteX((int) value);
					break;
			}
		}
	}
}