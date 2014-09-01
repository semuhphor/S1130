namespace S1130.SystemObjects.Instructions
{
	// Note: Opcodes are values after shift >> 11
    public enum OpCodes
    {
        Load = 0x18
        ,LoadDouble = 0x19
        ,Store = 0x1a
        ,StoreDouble = 0x1b
        ,LoadIndex = 0x0c
        ,StoreIndex = 0x0d
        ,LoadStatus = 0x04
        ,StoreStatus = 0x05
		,Add = 0x10
		,AddDouble = 0x11
		,Subtract = 0x12
		,SubtractDouble = 0x13
		,And = 0x1c
		,Or = 0x1d
		,ExclusiveOr = 0x1e
		,Multiply = 0x14
		,Divide = 0x15
    };
}