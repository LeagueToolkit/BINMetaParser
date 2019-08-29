using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BINMetaParser
{
    class Program
    {
        static void Main(string[] args)
        {
            RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(File.ReadAllText("meta_9.17.286.7872.json"));;
            Dictionary<uint, string> classNames = new Dictionary<uint, string>();
            Dictionary<uint, string> fieldNames = new Dictionary<uint, string>();

            List<string> classNamesRaw = File.ReadAllLines("hashes.bintypes.txt").ToList();
            foreach(string classNameRaw in classNamesRaw)
            {
                string className = classNameRaw.Split(' ')[1];
                classNames.Add(FNV32.Hash(className.ToLower()), className);
            }

            List<string> fieldNamesRaw = File.ReadAllLines("hashes.binfields.txt").ToList();
            foreach (string fieldNameRaw in fieldNamesRaw)
            {
                string fieldName = fieldNameRaw.Split(' ')[1];
                fieldNames.Add(FNV32.Hash(fieldName.ToLower()), fieldName);
            }

            List<string> classes = new List<string>();
            foreach(BINClass metaClass in rootObject.classes)
            {
                classes.Add(ClassGenerator.GenerateClass(metaClass, classNames, fieldNames));
            }

            File.WriteAllLines("classes.cs", classes);
        }
    }
}
