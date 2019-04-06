using System;

namespace MusicMetaData
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Creates a new array that is a subset of the given array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array containing all items</param>
        /// <param name="index">Start position of the subset</param>
        /// <param name="length">Amount of items that are to be copied</param>
        /// <returns>The subset array</returns>
        public static T[] SubArray<T>(this T[] array, int index, int length)
        {
            var subArray = new T[length];
            Array.Copy(array, index, subArray, 0, length);
            return subArray;
        }

        /// <summary>
        /// Find the First Occourance of a pattern in an array of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="pattern"></param>
        /// <returns>Position of the first occourance of a pattern, or -1 if it couldn't be found</returns>
        public static int SearchPattern<T>(this T[] array, T[] pattern)
        {
            if (pattern.Length > array.Length)
            {
                return -1;
            }
            for(int i = 0; i < array.Length; i++)
            {
                if (IsMatch(array, i, pattern))
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool IsMatch<T>(T[] array, int position, T[] pattern)
        {
            if (array.Length < (pattern.Length - position))
            {
                return false;
            }

            for (int i = 0; i < pattern.Length; i++)
            {
                if (!array[i + position].Equals(pattern[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if a certain bit is set to 1 in a byte
        /// </summary>
        /// <param name="b">The byte</param>
        /// <param name="position">Position of the bit that should be checked</param>
        /// <returns>True when the bit is 1</returns>
        public static bool IsSet(this byte b, int position)
        {
            return (b >> position) != 0;
        }

        public static T[] Append<T>(this T[] array, T[] append)
        {
            T[] newArray = new T[array.Length + append.Length];
            Array.Copy(array, newArray, array.Length);
            Array.Copy(append, 0, newArray, array.Length, append.Length);

            return newArray;
        }
    }
}
