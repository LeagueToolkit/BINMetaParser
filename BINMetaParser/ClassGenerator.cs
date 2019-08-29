using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            { BINValueType.StringHash, "uint" },
            { BINValueType.FlagsBoolean, "bool" }
        };

        public static string GenerateClass(BINClass metaClass, Dictionary<uint, string> classNames, Dictionary<uint, string> fieldNames)
        {
            string classDefinition = "public class ";
            classDefinition += GuessClassName(metaClass.hash, classNames);

            //Check class inheritance
            bool inheritsClass = metaClass.parentClass != 0;
            if (inheritsClass)
            {
                classDefinition += " : " + GuessClassName(metaClass.parentClass, classNames);
            }

            //Check interface implementations
            if (metaClass.implements.Count != 0)
            {
                if (!inheritsClass)
                {
                    classDefinition += " : ";
                }
                else
                {
                    classDefinition += ", ";
                }

                for (int i = 0; i < metaClass.implements.Count; i++)
                {
                    classDefinition += GuessClassName(metaClass.implements[i][0], classNames);

                    if (i + 1 != metaClass.implements.Count)
                    {
                        classDefinition += ", ";
                    }
                }
            }

            classDefinition += "\n{\n";

            //Generate properties
            foreach(BINProperty metaProperty in metaClass.properties)
            {
                string propertyType = GeneratePropertyType(metaProperty, classNames);
                string propertyName = GuessPropertyName(metaProperty.hash, fieldNames);

                classDefinition += "    public " + propertyType + " " + propertyName + " { get; set; }\n";
            }

            classDefinition += "\n}";

            return classDefinition;
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

            if(IsPrimitiveType(metaProperty.type))
            {
                type = TypeToObject[metaProperty.type];
            }
            else if(metaProperty.type == BINValueType.Structure || metaProperty.type == BINValueType.Embedded)
            {
                type = GenerateStructureType(metaProperty, classNames);
            }
            else if(metaProperty.type == BINValueType.Container)
            {
                type = GenerateContainerType(metaProperty, classNames);
            }
            else if(metaProperty.type == BINValueType.Map)
            {
                type = GenerateMapType(metaProperty, classNames);
            }
            else if(metaProperty.type == BINValueType.LinkOffset)
            {
                type = GenerateLinkOffsetType(metaProperty, classNames);
            }
            else if(metaProperty.type == BINValueType.OptionalData)
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
            return GuessClassName(metaProperty.otherClass, classNames);
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

            return type;
        }

        private static bool IsPrimitiveType(BINValueType type)
        {
            return type != BINValueType.Container && type != BINValueType.Embedded && type != BINValueType.Structure &&
                type != BINValueType.LinkOffset && type != BINValueType.Map && type != BINValueType.OptionalData;
        }
    }
}
