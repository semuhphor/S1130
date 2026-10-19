using System;
using System.IO;

namespace S1130.SystemObjects.Devices.Disks
{
	/// <summary>
	/// A 2315 disk cartridge held entirely in memory, loaded from and saved to a disk image
	/// that is byte-for-byte compatible with the SIMH-based IBM 1130 simulator (ibm1130_disk.c).
	///
	/// Image layout (no header, no padding):
	///   - 203 cylinders x 8 sectors = 1,624 sectors.
	///   - Each sector is 321 words, each word stored as 2 bytes, little-endian.
	///   - Word 0 of a sector holds the sector address (cylinder * 8 + sector); words 1-320 are data.
	///   - A sector starts at byte offset (cylinder * 8 + sector) * 642.
	///   - A full image is therefore exactly 1,042,608 bytes.
	///
	/// The cartridge lives in memory while mounted. Nothing touches the file until <see cref="Flush"/>
	/// is called (the drive does this on unmount), and only if something was written. The save goes
	/// to a temporary file first and is then moved over the original, so a crash cannot leave a
	/// half-written image behind.
	/// </summary>
	public class DiskImageCartridge : MemoryCartridge
	{
		public const int Cylinders = 203;									// cylinders on a 2315 cartridge
		public const int SectorsPerCylinder = 8;							// 4 sectors on each of 2 surfaces
		public const int WordsPerSector = 321;								// sector address word + 320 data words
		public const int SectorCount = Cylinders * SectorsPerCylinder;		// 1,624 sectors on a cartridge
		public const int BytesPerSector = WordsPerSector * 2;				// 642 bytes in the image file
		public const int ImageSize = SectorCount * BytesPerSector;			// 1,042,608 bytes in the image file

		private readonly ushort[] _words = new ushort[SectorCount * WordsPerSector];	// the whole disk, in memory
		private readonly string _path;										// file to save to (null = memory only)
		private bool _mounted;												// true while in a drive
		private bool _dirty;												// true if written since load/last flush
		private int _currentCylinder;										// cylinder the heads are on

		/// <summary>
		/// Creates a cartridge from raw image bytes.
		/// </summary>
		/// <param name="image">The image, exactly <see cref="ImageSize"/> bytes.</param>
		/// <param name="path">File that <see cref="Flush"/> saves to, or null for a memory-only cartridge.</param>
		/// <param name="readOnly">If true, writes are rejected (like a write-protected cartridge).</param>
		/// <exception cref="InvalidDataException">The image is not exactly <see cref="ImageSize"/> bytes.</exception>
		public DiskImageCartridge(byte[] image, string path = null, bool readOnly = false)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));
			if (image.Length != ImageSize)
				throw new InvalidDataException(
					$"A 2315 disk image must be {ImageSize} bytes but this one is {image.Length}.");

			for (var i = 0; i < _words.Length; i++)								// convert little-endian bytes to words
			{
				_words[i] = (ushort)(image[i * 2] | image[i * 2 + 1] << 8);
			}
			_path = path;
			ReadOnly = readOnly;
		}

		/// <summary>
		/// Loads a cartridge from an image file. <see cref="Flush"/> saves back to the same file.
		/// </summary>
		public static DiskImageCartridge Load(string path, bool readOnly = false)
		{
			return new DiskImageCartridge(File.ReadAllBytes(path), path, readOnly);
		}

		/// <summary>
		/// Creates a new blank cartridge.
		/// </summary>
		/// <param name="formatted">
		/// If true, every sector's address word is filled in (as a formatted disk would be) and the data is zero.
		/// If false, the whole disk is zero.
		/// </param>
		/// <param name="path">File that <see cref="Flush"/> saves to, or null for a memory-only cartridge.</param>
		public static DiskImageCartridge CreateBlank(bool formatted, string path = null)
		{
			var cartridge = new DiskImageCartridge(new byte[ImageSize], path);
			if (formatted)
			{
				for (var sector = 0; sector < SectorCount; sector++)
				{
					cartridge._words[sector * WordsPerSector] = (ushort)sector;
				}
			}
			cartridge._dirty = path != null;									// a brand-new file still has to be saved
			return cartridge;
		}

		/// <summary>True if writes are rejected (write-protected cartridge).</summary>
		public bool ReadOnly { get; }

		/// <summary>True if a write was attempted on a read-only cartridge since it was mounted.</summary>
		public bool WriteRejected { get; private set; }

		/// <summary>True if the cartridge has changes that have not yet been saved.</summary>
		public bool IsDirty => _dirty;

		public override bool Mounted => _mounted;

		/// <summary>
		/// Cylinder the heads are on (0-202). Set by the drive after each seek.
		/// </summary>
		public override int CurrentCylinder
		{
			get => _currentCylinder;
			set
			{
				if (value < 0 || value >= Cylinders)
					throw new ArgumentOutOfRangeException(nameof(value), "Cylinder must be 0-202.");
				_currentCylinder = value;
			}
		}

		public override void Mount()
		{
			_mounted = true;
			WriteRejected = false;
		}

		public override void UnMount()
		{
			_mounted = false;
		}

		/// <summary>
		/// Reads a sector on the current cylinder.
		/// </summary>
		/// <param name="sectorNumber">Sector 0-7.</param>
		/// <returns>A copy of the 321 words of the sector (word 0 is the sector address).</returns>
		public override ushort[] Read(int sectorNumber)
		{
			var start = StartOfSector(sectorNumber);
			var sector = new ushort[WordsPerSector];
			Array.Copy(_words, start, sector, 0, WordsPerSector);
			return sector;
		}

		/// <summary>
		/// Writes a sector on the current cylinder, exactly as the C simulator does: the words given are stored
		/// as they are (including word 0), and whatever is left of the 321-word sector is filled with zeros.
		/// On a read-only cartridge nothing is stored and <see cref="WriteRejected"/> is set.
		/// </summary>
		/// <param name="sectorNumber">Sector 0-7.</param>
		/// <param name="sector">The words to write, starting with word 0 of the sector.</param>
		/// <param name="wc">How many words of <paramref name="sector"/> to write (limited to 321).</param>
		public override void Write(int sectorNumber, ArraySegment<ushort> sector, int wc)
		{
			var start = StartOfSector(sectorNumber);
			if (ReadOnly)
			{
				WriteRejected = true;
				return;
			}

			var count = Math.Min(Math.Min(wc, sector.Count), WordsPerSector);
			if (count < 0)
				count = 0;
			Array.Copy(sector.Array, sector.Offset, _words, start, count);
			Array.Clear(_words, start + count, WordsPerSector - count);			// rest of the sector gets zeroed
			_dirty = true;
		}

		/// <summary>
		/// Saves the cartridge to its file if anything has changed. Does nothing for a memory-only cartridge.
		/// </summary>
		public override void Flush()
		{
			if (_path == null || !_dirty)
				return;

			var temp = _path + ".tmp";
			File.WriteAllBytes(temp, ToImageBytes());
			File.Move(temp, _path, overwrite: true);								// replace the old image in one step
			_dirty = false;
		}

		/// <summary>
		/// Returns the whole cartridge in the shared image format (see class description).
		/// </summary>
		public byte[] ToImageBytes()
		{
			var image = new byte[ImageSize];
			for (var i = 0; i < _words.Length; i++)
			{
				image[i * 2] = (byte)(_words[i] & 0xFF);
				image[i * 2 + 1] = (byte)(_words[i] >> 8);
			}
			return image;
		}

		private int StartOfSector(int sectorNumber)							// index of word 0 of a sector on this cylinder
		{
			if (sectorNumber < 0 || sectorNumber >= SectorsPerCylinder)
				throw new ArgumentOutOfRangeException(nameof(sectorNumber), "Sector must be 0-7.");
			return (_currentCylinder * SectorsPerCylinder + sectorNumber) * WordsPerSector;
		}
	}
}
