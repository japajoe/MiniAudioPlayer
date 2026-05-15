using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MiniAudioPlayer.Utilities
{
    public class NaturalFileInfoComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if(string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
                return 0;

            // Extract just the file name for sorting, ignoring the full directory path
            string nameX = System.IO.Path.GetFileName(x);
            string nameY = System.IO.Path.GetFileName(y);

            return ArrayAlphaNumPrefixCompare(nameX, nameY);
        }

        private int ArrayAlphaNumPrefixCompare(string x, string y)
        {
            var chunksX = Regex.Split(x, "([0-9]+)");
            var chunksY = Regex.Split(y, "([0-9]+)");

            for (int i = 0; i < Math.Min(chunksX.Length, chunksY.Length); i++)
            {
                if (chunksX[i] != chunksY[i])
                {
                    if (int.TryParse(chunksX[i], out int nX) && int.TryParse(chunksY[i], out int nY))
                    {
                        return nX.CompareTo(nY);
                    }
                    return chunksX[i].CompareTo(chunksY[i]);
                }
            }

            return chunksX.Length.CompareTo(chunksY.Length);
        }
    }
}