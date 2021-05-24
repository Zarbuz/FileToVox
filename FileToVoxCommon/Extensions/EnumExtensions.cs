using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace FileToVoxCommon.Extensions
{
	public static class EnumExtensions
	{
		public static DisplayAttribute GetDisplayAttributesFrom(this Enum enumValue, Type enumType)
		{
			return enumType.GetMember(enumValue.ToString())
				.First()
				.GetCustomAttribute<DisplayAttribute>();
		}

	}
}
