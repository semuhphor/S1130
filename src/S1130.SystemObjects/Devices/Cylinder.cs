namespace S1130.SystemObjects.Devices
{
	/***********************************************************************
	 * struct to calc cylinder number with floor of zero and ceiling of 202
	 * 
	 * Current limited to 0 - 202
	 ***********************************************************************/

	public class Cylinder
	{
		public int Current { get; private set; }
		public bool Home { get { return Current == 0; } }

		public static Cylinder operator +(Cylinder cyl, int numberOfCylinders)
		{
			var newCyl = cyl.Current + numberOfCylinders;
			if (newCyl < 0 || newCyl > 202)
			{
				cyl.Current = numberOfCylinders > 0 ? 202 : 0;
			}
			else
			{
				cyl.Current = newCyl;
			}
			return cyl;
		}

		public static Cylinder operator -(Cylinder cyl, int numberOfCylinders)
		{
			return cyl += (short) -numberOfCylinders;
		}
	}
}