using System;
using System.Collections.Generic;
using System.Text;

namespace FileToVox.Main
{
	public abstract class SimpleSingleton<T> where T : class, new()
	{
		#region ConstStatic

		private static T mInstance;
		public static T Instance
		{
			get
			{
				if (mInstance == null)
				{
					mInstance = new T();
				}

				return mInstance;
			}
		}

		#endregion
	}
}
