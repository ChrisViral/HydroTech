using System;
using System.Collections.Generic;
#pragma warning disable 618

namespace HydroTech.Utils
{
    /// <summary>
    /// Unused. Anything in here should ONLY ever be called through EnumUtils.
    /// </summary>
    /// <typeparam name="TEnum">Type should be Enum. Really, it's gonna throw if you don't.</typeparam>
    [Obsolete("This class is only a helper class, do not use this, use EnumUtils instead")]
    public abstract class EnumConstraint<TEnum> where TEnum : class
    {
        /// <summary>
        /// Generic enum conversion utility class
        /// </summary>
        private class EnumConverter
        {
            #region Fields
            /// <summary>
            /// Stores the string -> enum conversion
            /// </summary>
            private readonly Dictionary<string, TEnum> values = new Dictionary<string, TEnum>();

            /// <summary>
            /// Stores the enum -> string conversion
            /// </summary>
            private readonly Dictionary<TEnum, string> names = new Dictionary<TEnum, string>();

            /// <summary>
            /// The name of the enum values correctly ordered for index search
            /// </summary>
            public readonly string[] orderedNames;

            /// <summary>
            /// The values of the Enum correctly ordered for index search
            /// </summary>
            public readonly TEnum[] orderedValues;
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new EnumConvertor from the given type
            /// </summary>
            /// <param name="enumType">Type of converter. Must be an enum type.</param>
            public EnumConverter(Type enumType)
            {
                if (enumType == null) { throw new ArgumentNullException(nameof(enumType), "Enum conversion type cannot be null"); }
                Array values = Enum.GetValues(enumType);
                this.orderedNames = new string[values.Length];
                this.orderedValues = new TEnum[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    TEnum value = (TEnum)values.GetValue(i);
                    string name = Enum.GetName(enumType, value);
                    this.orderedNames[i] = name;
                    this.orderedValues[i] = value;
                    this.values.Add(name, value);
                    this.names.Add(value, name);
                }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Tries to parse the given Enum member and stores the result in the out parameter. Returns false if it fails.
            /// </summary>
            /// <param name="name">String to parse</param>
            /// <param name="value">Value to store the result into</param>
            public void TryGetValue<T>(string name, out T value) where T : struct, TEnum
            {
                TEnum result;
                this.values.TryGetValue(name, out result);
                value = (T)result;
            }

            /// <summary>
            /// Tries to get the string name of the Enum value and stores it in the out parameter. Returns false if it fails.
            /// </summary>
            /// <param name="value">Enum to get the string for</param>
            /// <param name="name">Value to store the result into</param>
            public void TryGetName<T>(T value, out string name) where T : struct, TEnum
            {
                this.names.TryGetValue(value, out name);
            }
            #endregion
        }

        #region Fields
        /// <summary>
        /// Holds all the known enum converters
        /// </summary>
        private static readonly Dictionary<Type, EnumConverter> converters = new Dictionary<Type, EnumConverter>();
        #endregion

        #region Methods
        /// <summary>
        /// Returns the converter of the given type or creates one if there are none
        /// </summary>
        /// <typeparam name="T">Type of the enum conversion</typeparam>
        private static EnumConverter GetConverter<T>() where T : struct, TEnum
        {
            EnumConverter converter;
            Type enumType = typeof(T);
            if (!converters.TryGetValue(enumType, out converter))
            {
                converter = new EnumConverter(enumType);
                converters.Add(enumType, converter);
            }
            return converter;
        }

        /// <summary>
        /// Returns the string value of an Enum
        /// </summary>
        /// <param name="value">Enum value to convert to string</param>
        public static string GetName<T>(T value) where T : struct, TEnum
        {
            string result;
            GetConverter<T>().TryGetName(value, out result);
            return result;
        }

        /// <summary>
        /// Parses the given string to the given Enum type 
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="name">String to parse</param>
        public static T GetValue<T>(string name) where T : struct, TEnum
        {
            T result;
            GetConverter<T>().TryGetValue(name, out result);
            return result;
        }

        /// <summary>
        /// Gets the enum value at the given index
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="index">Index of the element to get</param>
        public static T GetValueAt<T>(int index) where T : struct, TEnum
        {
            EnumConverter converter = GetConverter<T>();
            if (index < 0 || index >= converter.orderedNames.Length) { return default(T); }
            T result;
            converter.TryGetValue(converter.orderedNames[index], out result);
            return result;
        }

        /// <summary>
        /// Finds the string name of the enum value at the given index
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="index">Index of the name to find</param>
        public static string GetNameAt<T>(int index) where T : struct, TEnum
        {
            EnumConverter converter = GetConverter<T>();
            if (index < 0 || index >= converter.orderedNames.Length) { return null; }
            return converter.orderedNames[index];
        }

        /// <summary>
        /// Returns the string representation of each enum member in order
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        public static string[] GetNames<T>() where T : struct, TEnum
        {
            return GetConverter<T>().orderedNames;
        }

        /// <summary>
        /// Gets an array of all the values of the Enum
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        public static T[] GetValues<T>() where T : struct, TEnum
        {
            return Array.ConvertAll(GetConverter<T>().orderedValues, v => (T)v);
        }

        /// <summary>
        /// Returns the index of the Enum value of the given name
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="name">Name of the element to find</param>
        public static int IndexOf<T>(string name) where T : struct, TEnum
        {
            return GetNames<T>().IndexOf(name);
        }

        /// <summary>
        /// Returns the index of the Enum member of the given value
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="value">Value to find the index of</param>
        public static int IndexOf<T>(T value) where T : struct, TEnum
        {
            return GetValues<T>().IndexOf(value);
        }
        #endregion
    }

    /// <summary>
    /// Enum utility class, containing various enum parsing/tostring methods.
    /// </summary>
    public sealed class EnumUtils : EnumConstraint<Enum>
    {
        /* Nothing to see here, this is just a dummy class to force T to be an Enum.
         * The actual implementation is in EnumConstraint */

        #region Constructor
        /// <summary>
        /// Prevents object instantiation, this should act as a static class
        /// </summary>
        private EnumUtils() { }
        #endregion
    }
}