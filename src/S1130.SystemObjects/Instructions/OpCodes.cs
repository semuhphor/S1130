using System.Collections.Generic;

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
		,ShiftLeft = 0x02
		,ShiftRight = 0x03
		,BranchSkip = 0x09
		,BranchStore = 0x08
		,ModifyIndex = 0x0e
		,Wait = 0x06
		,ExecuteInputOutput = 0x01
    };

	public class OpcodeReference
	{
		public Dictionary<OpCodes, IInstruction> ReferencDictionary 
		{
			get {
				return new Dictionary<OpCodes, IInstruction>()
					{
						{OpCodes.Load, new Load()},
						{OpCodes.LoadDouble, new LoadDouble()},
						{OpCodes.Store, new Store()},
						{OpCodes.StoreDouble, new StoreDouble()},
						{OpCodes.LoadIndex, new LoadIndex()},
						{OpCodes.StoreIndex, new StoreIndex()},
						{OpCodes.LoadStatus, new LoadStatus()},
						{OpCodes.StoreStatus, new StoreStatus()},
						{OpCodes.Add, new Add()},
						{OpCodes.AddDouble, new AddDouble()},
						{OpCodes.Subtract, new Subtract()},
						{OpCodes.SubtractDouble, new SubtractDouble()},
						{OpCodes.And, new And()},
						{OpCodes.Or, new Or()},
						{OpCodes.ExclusiveOr, new ExclusiveOr()},
						{OpCodes.Multiply, new Multiply()},
						{OpCodes.Divide, new Divide()},
						{OpCodes.ShiftLeft, new ShiftLeft()},
						{OpCodes.ShiftRight, new ShiftRight()},
						{OpCodes.BranchSkip, new BranchSkip()},
						{OpCodes.BranchStore, new BranchStore()},
						{OpCodes.ModifyIndex, new ModifyIndex()},
						{OpCodes.Wait, new Wait()},
						{OpCodes.ExecuteInputOutput, new ExecuteInputOutput()}
					};
				}
		}
	}
}