namespace S1130.SystemObjects
{
	public class Card : ICard
	{
		public Card()
		{
			Columns = new ushort[80];
		}
		public ushort[] Columns { get; set; }

		public ushort this[int columnIndex]
		{
			get { return Columns[columnIndex]; }
			set { Columns[columnIndex] = value; }
		}
	}
}