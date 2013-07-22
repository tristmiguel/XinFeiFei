namespace uWDFExtractor
{
	public struct FileEntry
	{
		public int Id { get; private set; }
		public int Offset { get; private set; }
		public int Size { get; private set; }
		public int Space { get; private set; }

		public FileEntry(int id, int offset, int size, int space) : this()
		{
			Id = id;
			Offset = offset;
			Size = size;
			Space = space;
		}

		public static bool operator==(FileEntry a, FileEntry b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(FileEntry a, FileEntry b)
		{
			return !(a == b);
		}
	}
}