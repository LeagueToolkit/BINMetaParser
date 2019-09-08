using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BINMetaParser
{
    public static class ClassGenerator
    {
        private static readonly Dictionary<BINValueType, string> TypeToObject = new Dictionary<BINValueType, string>()
        {
            { BINValueType.Boolean, "bool" },
            { BINValueType.SByte, "sbyte" },
            { BINValueType.Byte, "byte" },
            { BINValueType.Int16, "short" },
            { BINValueType.UInt16, "ushort" },
            { BINValueType.Int32, "int" },
            { BINValueType.UInt32, "uint" },
            { BINValueType.Int64, "long" },
            { BINValueType.UInt64, "ulong" },
            { BINValueType.Float, "float" },
            { BINValueType.FloatVector2, "Vector2" },
            { BINValueType.FloatVector3, "Vector3" },
            { BINValueType.FloatVector4, "Vector4" },
            { BINValueType.Matrix44, "R3DMatrix44" },
            { BINValueType.Color, "ColorRGBAVector4Byte" },
            { BINValueType.String, "string" },
            { BINValueType.StringHash, "Hash" },
            { BINValueType.FlagsBoolean, "bool" }
        };

        public static IEnumerable<string> GenerateClasses(List<BINClass> metaClasses, Dictionary<uint, string> classNames, Dictionary<uint, string> fieldNames)
        {
            foreach(BINClass metaClass in metaClasses)
            {
                yield return GenerateClass(metaClass, GetInterfaces(metaClass), classNames, fieldNames);
            }

            List<BINClass> GetInterfaces(BINClass metaClass)
            {
                List<BINClass> interfaces = new List<BINClass>();

                if(metaClass.parentClass != 0)
                {
                    BINClass parent = metaClasses.Find(x => x.hash == metaClass.parentClass);

                    if(parent.isInterface)
                    {
                        interfaces.AddRange(GetInterfaces(parent));
                        interfaces.Add(parent);
                    }
                }

                if(metaClass.implements.Count != 0)
                {
                    foreach(uint[] implement in metaClass.implements)
                    {
                        BINClass metaInterface = metaClasses.Find(x => x.hash == implement[0]);

                        interfaces.AddRange(GetInterfaces(metaInterface));
                        interfaces.Add(metaInterface);
                    }
                }

                return interfaces;
            }
        }

        public static string GenerateClass(BINClass metaClass, List<BINClass> interfaces, Dictionary<uint, string> classNames, Dictionary<uint, string> fieldNames)
        {
            string definition = "";
            string name = GuessClassName(metaClass.hash, classNames);
            List<Tuple<string, string>> properties = new List<Tuple<string, string>>();

            if(!metaClass.isInterface)
            {
                foreach (BINClass interfaceClass in interfaces)
                {
                    properties.AddRange(GenerateProperties(interfaceClass, classNames, fieldNames));
                }
            }
            
            properties.AddRange(GenerateProperties(metaClass, classNames, fieldNames));


            definition += "public ";
            definition += metaClass.isInterface ? "interface " : "class ";
            definition += name;

            //Check class inheritance
            bool inheritsClass = metaClass.parentClass != 0;
            if (inheritsClass)
            {
                definition += " : " + GuessClassName(metaClass.parentClass, classNames);
            }

            //Check interface implementations
            if (metaClass.implements.Count != 0)
            {
                if (!inheritsClass)
                {
                    definition += " : ";
                }
                else
                {
                    definition += ", ";
                }

                for (int i = 0; i < metaClass.implements.Count; i++)
                {
                    definition += GuessClassName(metaClass.implements[i][0], classNames);

                    if (i + 1 != metaClass.implements.Count)
                    {
                        definition += ", ";
                    }
                }
            }

            definition += "\n{\n";

            foreach(Tuple<string, string> property in properties)
            {
                if(metaClass.isInterface)
                {
                    definition += string.Format("    {0} {1}\n", property.Item1, property.Item2);
                }
                else
                {
                    definition += string.Format("    {0} public {1}\n", property.Item1, property.Item2);
                }
            }
            
            if(!metaClass.isInterface)
            {
                definition += "\n";
                definition += "    public " + name + "()" + "\n    {\n" + "\n    }\n";
            }


            definition += "}";

            return definition;
        }

        private static List<Tuple<string, string>> GenerateProperties(BINClass metaClass, Dictionary<uint, string> classNames, Dictionary<uint, string> fieldNames)
        {
            List<Tuple<string, string>> properties = new List<Tuple<string, string>>();

            foreach (BINProperty metaProperty in metaClass.properties)
            {
                string propertyType = GeneratePropertyType(metaProperty, classNames);
                string propertyName = GuessPropertyName(metaProperty.hash, fieldNames);
                string attribute = "";

                if (!Regex.IsMatch(propertyName, @"^m\d+"))
                {
                    attribute = string.Format(@"[BINValue(""{0}"")]", propertyName);

                    if (propertyName[0] == 'm' && char.IsUpper(propertyName[1]))
                    {
                        propertyName = propertyName.Substring(1);
                    }
                    else if (char.IsLower(propertyName[0]))
                    {
                        propertyName = char.ToUpper(propertyName[0]) + propertyName.Substring(1);
                    }
                }
                else
                {
                    attribute = string.Format(@"[BINValue({0})]", metaProperty.hash);
                }

                properties.Add(new Tuple<string, string>(attribute, propertyType + " " + propertyName + " { get; set; }"));
            }

            return properties;
        }

        private static string GuessClassName(uint hash, Dictionary<uint, string> classNames)
        {
            string className;
            if (!classNames.TryGetValue(hash, out className))
            {
                className = "Class_" + hash.ToString();
            }

            return className;
        }

        private static string GuessPropertyName(uint hash, Dictionary<uint, string> propertyNames)
        {
            string propertyName;
            if (!propertyNames.TryGetValue(hash, out propertyName))
            {
                propertyName = "m" + hash.ToString();
            }

            return propertyName;
        }

        private static string GeneratePropertyType(BINProperty metaProperty, Dictionary<uint, string> classNames)
        {
            string type = "";

            if (IsPrimitiveType(metaProperty.type))
            {
                type = TypeToObject[metaProperty.type];
            }
            else if (metaProperty.type == BINValueType.Structure || metaProperty.type == BINValueType.Embedded)
            {
                type = GenerateStructureType(metaProperty, classNames);
            }
            else if (metaProperty.type == BINValueType.Container)
            {
                type = GenerateContainerType(metaProperty, classNames);
            }
            else if (metaProperty.type == BINValueType.Map)
            {
                type = GenerateMapType(metaProperty, classNames);
            }
            else if (metaProperty.type == BINValueType.LinkOffset)
            {
                type = GenerateLinkOffsetType(metaProperty, classNames);
            }
            else if (metaProperty.type == BINValueType.OptionalData)
            {
                type = GenerateOptionalDataType(metaProperty, classNames);
            }

            return type;
        }

        private static string GenerateStructureType(BINProperty metaProperty, Dictionary<uint, string> classNames)
        {
            return GuessClassName(metaProperty.otherClass, classNames); ;
        }

        private static string GenerateContainerType(BINProperty metaProperty, Dictionary<uint, string> classNames)
        {
            string itemType = "";
            if (IsPrimitiveType(metaProperty.containerI.type))
            {
                itemType = TypeToObject[metaProperty.containerI.type];
            }
            else
            {
                itemType = GuessClassName(metaProperty.otherClass, classNames);
            }

            return "List<" + itemType + ">";
        }

        private static string GenerateMapType(BINProperty metaProperty, Dictionary<uint, string> classNames)
        {
            string keyType = TypeToObject[metaProperty.mapI.key];
            string valueType = "";

            if (IsPrimitiveType(metaProperty.mapI.value))
            {
                valueType = TypeToObject[metaProperty.mapI.value];
            }
            else
            {
                valueType = GuessClassName(metaProperty.otherClass, classNames);
            }

            return "Dictionary<" + keyType + ", " + valueType + ">";
        }

        private static string GenerateLinkOffsetType(BINProperty metaProperty, Dictionary<uint, string> classNames)
        {
            return "Link<" + GuessClassName(metaProperty.otherClass, classNames) + ">";
        }

        private static string GenerateOptionalDataType(BINProperty metaProperty, Dictionary<uint, string> classNames)
        {
            string type = "";

            if (IsPrimitiveType(metaProperty.containerI.type))
            {
                type = TypeToObject[metaProperty.containerI.type];
            }
            else
            {
                type = GuessClassName(metaProperty.otherClass, classNames);
            }

            return "Optional<" + type + ">";
        }

        private static bool IsPrimitiveType(BINValueType type)
        {
            return type != BINValueType.Container && type != BINValueType.Embedded && type != BINValueType.Structure &&
                type != BINValueType.LinkOffset && type != BINValueType.Map && type != BINValueType.OptionalData;
        }
    }
}
