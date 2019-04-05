namespace S1130.SystemObjects
{
	public interface ICard
	{
		ushort[] Columns { get; set; }
		ushort this[int columnIndex] { get; set; }
	}
}
