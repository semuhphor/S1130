using System;

namespace S1130.SystemObjects
{
	public class Card : ICard
	{
		public Card() : this(new ushort[80])
		{
		}

		public Card(string columns)
		{
			
		}

		public Card(ushort[] columns)
		{
			if (columns.Length != 80)
			{
				throw new ArgumentException("A card must have 80 columns");
			}
			Columns = columns;
		}

		public ushort[] Columns { get; set; }

		public ushort this[int columnIndex]
		{
			get { return Columns[columnIndex]; }
			set { Columns[columnIndex] = value; }
		}
	}
}