using System;

namespace S1130.SystemObjects.Devices
{
	/***********************************************************************
	 * struct to calc cylinder number with floor of zero and ceiling of 202
	 * 
	 * Movement is limited to 202 cylinders +/-;
	 ***********************************************************************/

	public class Cylinder
	{
		public int Current { get; private set; }

		public static Cylinder operator +(Cylinder cyl, int numberOfCylinders)
		{
			cyl.Current += (numberOfCylinders > 202) ? 202 : (numberOfCylinders < -202) ? -202 : numberOfCylinders;
			if (cyl.Current < 0)
			{
				cyl.Current = 0;
			}
			if (cyl.Current > 202)
			{
				cyl.Current = 202;
			}
			return cyl;
		}

		public static Cylinder operator -(Cylinder cyl, int numberOfCylinders)
		{
			return cyl += (short) -numberOfCylinders;
		}
	}
}