namespace S1130.SystemObjects
{
	public interface IInputOutputSystem
	{
		void ExecuteIocc(ICpu cpu, ushort ioccAddress);
	}
}