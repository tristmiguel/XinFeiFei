using System;

namespace uXinEmu.XinFeiFei.Data_Model
{
	static class PositionExtensions
	{
		public static double DistanceTo(this Position a, Position b)
		{
			return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
		}
	}
}
