using System.Collections.Generic;

namespace Universities.Utils
{
    /// <summary>
    /// Frequently used methods for splitting strings into string arrays.
    /// </summary>
    public static class StringSplit
    {
        /// <summary>
        /// Method that takes a string line, separator character and a character used to indicate a separate string within the one provided (usually "").
        /// </summary>
        /// <param name="line">The string to be split.</param>
        /// <param name="separator">Separator character by which the string is split.</param>
        /// <param name="skipChar">Skip character used to exclude a part of the string from being split.</param>
        /// <returns>A string array with removed separator and skip characters and trimmed white spaces.</returns>
        public static string[] SkipStrings(string line, char separator, char skipChar)
        {
            List<string> result = new List<string>();
            int lastSep = -1;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == skipChar) i = line.IndexOf(skipChar, i + 1);
                int nextSepIndex = line.IndexOf(separator, i);
                i = nextSepIndex == -1 ? line.Length : nextSepIndex;
                result.Add(line.Substring(lastSep + 1, i - lastSep - 1).Trim(skipChar).Trim());
                lastSep = i;
            }
            if (line.EndsWith(separator)) result.Add(string.Empty);
            return result.ToArray();
        }
    }
}