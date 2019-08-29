using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BINMetaParser
{
    public static class FNV32
    {
        public static uint Hash(string toHash)
        {
            toHash = toHash.ToLower();
            uint hash = 2166136261;
            for (int i = 0; i < toHash.Length; i++)
            {
                hash = hash ^ toHash[i];
                hash = hash * 16777619;
            }

            return hash;
        }
    }
}
