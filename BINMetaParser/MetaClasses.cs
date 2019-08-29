using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BINMetaParser
{
    public class RootObject
    {
        public int @base { get; set; }
        public List<BINClass> classes { get; set; }
        public int offset { get; set; }
    }

    public class BINClass
    {
        public int classSize { get; set; }
        public int constructor { get; set; }
        public int destructor { get; set; }
        public uint hash { get; set; }
        public List<uint[]> implementedBy { get; set; }
        public List<uint[]> implements { get; set; }
        public int initfunction { get; set; }
        public int inplaceconstructor { get; set; }
        public int inplacedestructor { get; set; }
        public bool isInterface { get; set; }
        public bool isUnk0 { get; set; }
        public bool isUnk3 { get; set; }
        public bool isValue { get; set; }
        public uint parentClass { get; set; }
        public List<BINProperty> properties { get; set; }
        public int unkSize { get; set; }
        public int vtable { get; set; }
    }

    public class BINProperty
    {
        public int bitmask { get; set; }
        public ContainerI containerI { get; set; }
        public uint hash { get; set; }
        public MapI mapI { get; set; }
        public int offset { get; set; }
        public uint otherClass { get; set; }
        public BINValueType type { get; set; }
    }

    public class MapI
    {
        public BINValueType key { get; set; }
        public BINValueType value { get; set; }
        public int vtable { get; set; }
    }

    public class ContainerI
    {
        public int elemSize { get; set; }
        public BINValueType type { get; set; }
        public int vtable { get; set; }
    }

    public enum BINValueType : uint
    {
        None = 0,
        Boolean = 1,
        SByte = 2,
        Byte = 3,
        Int16 = 4,
        UInt16 = 5,
        Int32 = 6,
        UInt32 = 7,
        Int64 = 8,
        UInt64 = 9,
        Float = 10,
        FloatVector2 = 11,
        FloatVector3 = 12,
        FloatVector4 = 13,
        Matrix44 = 14,
        Color = 15,
        String = 16,
        StringHash = 17,
        Container = 18,
        Structure = 19,
        Embedded = 20,
        LinkOffset = 21,
        OptionalData = 22,
        Map = 23,
        FlagsBoolean = 24
    }
}
