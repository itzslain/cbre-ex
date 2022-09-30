using System;
using System.ComponentModel;

namespace CBRE.Editor.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            System.Reflection.FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static T FromDescription<T>(string description) where T : struct, IConvertible
        {
            Type type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            foreach (T item in Enum.GetValues(type))
            {
                System.Reflection.FieldInfo fi = type.GetField(item.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                string desc = attributes.Length > 0 ? attributes[0].Description : item.ToString();
                if (desc == description) return item;
            }
            return default(T);
        }
    }
}
