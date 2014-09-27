namespace S1130.SystemObjects
{
	public interface IInputOutputSystem
	{
		void ExecuteIocc(ISystemState state, ushort ioccAddress);
	}
}