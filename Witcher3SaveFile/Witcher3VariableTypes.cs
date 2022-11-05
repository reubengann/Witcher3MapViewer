using System.Text;

namespace Witcher3SaveFile
{
    public enum QuestStatusState
    {
        NotFound = 0, Inactive = 1, Active = 2, Failed = 3, Success = 4
    }

    public class Witcher3GenericVariable
    {
        public Witcher3RawType Type;
        public int Size;
        public int Offset;

        public Witcher3GenericVariable()
        {
            Type = Witcher3RawType.Unknown;
        }
    }

    public class Witcher3Collection : Witcher3GenericVariable
    {
        public List<Witcher3GenericVariable> Members;

        public override string ToString()
        {
            return "Set[" + Members.Count.ToString() + "]";
        }

        public Witcher3GenericVariable SearchForByName(string query)
        {
            foreach (Witcher3GenericVariable v in Members)
            {
                string putativename = "";
                switch (v.Type)
                {
                    case Witcher3RawType.BS:
                        putativename = ((Witcher3BS)v).Name;
                        break;
                    case Witcher3RawType.VL:
                        putativename = ((Witcher3VL)v).Name;
                        break;
                    case Witcher3RawType.OP:
                        putativename = ((Witcher3OP)v).Name;
                        break;
                    case Witcher3RawType.BLCK:
                        putativename = ((Witcher3BLCK)v).Name;
                        break;
                    case Witcher3RawType.AVAL:
                        putativename = ((Witcher3AVAL)v).Name;
                        break;
                    case Witcher3RawType.PORP:
                        putativename = ((Witcher3PORP)v).Name;
                        break;
                    default:
                        break;
                }
                if (putativename == query)
                    return v;
            }
            return null;
        }
    }

    public class Witcher3NamedCollection : Witcher3Collection
    {
        public string Name;
    }

    public class Witcher3BS : Witcher3NamedCollection
    {
        public Witcher3BS()
        {
            Type = Witcher3RawType.BS;
            Members = new List<Witcher3GenericVariable>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Name + "[" + Members.Count.ToString() + "]");
            foreach (Witcher3GenericVariable m in Members)
            {
                sb.AppendLine(m.ToString());
            }
            return sb.ToString();
        }
    }

    public class Witcher3BLCK : Witcher3NamedCollection
    {
        public Witcher3BLCK()
        {
            Type = Witcher3RawType.BLCK;
            Members = new List<Witcher3GenericVariable>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Name + "[" + Members.Count.ToString() + "]");
            foreach (Witcher3GenericVariable m in Members)
            {
                sb.AppendLine(m.ToString());
            }
            return sb.ToString();
        }
    }

    public class Witcher3VL : Witcher3GenericVariable
    {
        public string Name;
        public Witcher3Value Value;
        public string SpecificType;

        public Witcher3VL()
        {
            Type = Witcher3RawType.VL;
        }

        public override string ToString()
        {
            return SpecificType + " " + Name + " = " + Value.ToString();
        }
    }

    public class Witcher3AVAL : Witcher3GenericVariable
    {
        public string Name;
        public Witcher3Value Value;
        public string SpecificType;

        public Witcher3AVAL()
        {
            Type = Witcher3RawType.AVAL;
        }

        public override string ToString()
        {
            return SpecificType + " " + Name + " = " + Value.ToString();
        }
    }

    public class Witcher3PORP : Witcher3GenericVariable
    {
        public string Name;
        public Witcher3Value Value;
        public string SpecificType;

        public Witcher3PORP()
        {
            Type = Witcher3RawType.PORP;
        }

        public override string ToString()
        {
            return SpecificType + " " + Name + " = " + Value.ToString();
        }
    }

    public class Witcher3OP : Witcher3GenericVariable
    {
        public string Name;
        public Witcher3Value Value;
        public string SpecificType;

        public Witcher3OP()
        {
            Type = Witcher3RawType.OP;
        }

        public override string ToString()
        {
            return SpecificType + " " + Name + " = " + Value.ToString();
        }
    }

    public class Witcher3SXAP : Witcher3Collection
    {

        public bool Hidden;

        public Witcher3SXAP()
        {
            Hidden = false;
            Type = Witcher3RawType.SXAP;
            Members = new List<Witcher3GenericVariable>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Set[" + Members.Count.ToString() + "]");
            foreach (Witcher3GenericVariable m in Members)
            {
                sb.AppendLine(m.ToString());
            }
            return sb.ToString();
        }
    }

    public class Witcher3SBDF : Witcher3GenericVariable
    {
        public Witcher3SBDF()
        {
            Type = Witcher3RawType.SBDF;
        }

        public override string ToString()
        {
            return "Undeciphered SBDF data";
        }
    }

    public class Witcher3ROTS : Witcher3Collection
    {
        public Witcher3ROTS()
        {
            Type = Witcher3RawType.ROTS;
            Members = new List<Witcher3GenericVariable>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Set[" + Members.Count.ToString() + "]");
            foreach (Witcher3GenericVariable m in Members)
            {
                sb.AppendLine(m.ToString());
            }
            return sb.ToString();
        }
    }

    public class Witcher3SS : Witcher3Collection
    {
        public Witcher3SS()
        {
            Type = Witcher3RawType.SS;
            Members = new List<Witcher3GenericVariable>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Set[" + Members.Count.ToString() + "]");
            foreach (Witcher3GenericVariable m in Members)
            {
                sb.AppendLine(m.ToString());
            }
            return sb.ToString();
        }
    }

    public enum Witcher3RawType
    {
        BS, VL, SXAP, SBDF, ROTS, SS, OP, BLCK, AVAL, PORP, Unknown
    }

    public class Witcher3Value
    {
        public object Value;
        public int Size;
        public string Name;
        public string Type;

        public Witcher3Value()
        {
            Name = "";
        }

        public Witcher3Value(object value, int size)
        {
            Value = value;
            Size = size;
        }

        public Witcher3Value(object value, int size, string type)
        {
            Value = value;
            Size = size;
            Type = type;
        }

        public override string ToString()
        {
            if (Value != null)
            {
                Type valueType = Value.GetType();
                if (valueType.IsArray/* && expectedType.IsAssignableFrom(valueType.GetElementType()*/)
                {
                    Array a = Value as Array;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("[");
                    foreach (var v in a)
                    {
                        sb.AppendLine(v.ToString());
                    }
                    sb.AppendLine("]");
                    return sb.ToString();
                }
                else return Value.ToString();
            }
            return "undefined val string (" + Type + ")";
        }
    }

    public class Witcher3HandleData : Witcher3Value
    {
        public int unknowncode1;
        public List<Witcher3Value> HandleInfo;
        public string specificType;

        public Witcher3HandleData()
        {
            HandleInfo = new List<Witcher3Value>();
        }
    }

    public class SQuestThreadSuspensionData : Witcher3Value
    {
        public string scopeBlockGUID;
        public Witcher3Value QuestThread;

        public SQuestThreadSuspensionData()
        {
            scopeBlockGUID = "";
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SQuestThreadSuspensionData");
            sb.AppendLine("scopeBlockGUID = " + scopeBlockGUID);
            sb.AppendLine(QuestThread.ToString());
            return sb.ToString();
        }
    }

    public class SActionPointIdData : Witcher3Value
    {
        public string IdName;
        public string componentGUID;
        public string entityName;
        public string entityGUID;

        public SActionPointIdData()
        {
            IdName = "";
            componentGUID = "";
            entityName = "";
            entityGUID = "";
        }
    }

    public class EntityHandle : Witcher3Value
    {
        public int unknown1;
        public string GUID;

        public override string ToString()
        {
            return GUID;
        }
    }

    public class Witcher3Vector : Witcher3Value
    {
        public float? X, Y, Z, W;

        public override string ToString()
        {
            return "<" + String.Join(", ", new List<float?> { X, Y, Z, W }) + ">";
        }
    }

    public class Witcher3Vector3 : Witcher3Value
    {
        public float? X, Y, Z;
    }

    public class Witcher3Vector2 : Witcher3Value
    {
        public float? X, Y;
    }

    public class Witcher3EulerAngles : Witcher3Value
    {
        public float? Pitch, Yaw, Roll;
    }

    class Witcher3Matrix : Witcher3Value
    {
        public Witcher3Vector X, Y, Z, W;
    }

    class Witcher3SimpleSet : Witcher3Value
    {
        public List<Witcher3Value> SetInfo;

        public Witcher3SimpleSet()
        {
            SetInfo = new List<Witcher3Value>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Name + "[" + SetInfo.Count.ToString() + "]");
            foreach (Witcher3Value m in SetInfo)
            {
                sb.AppendLine(m.ToString());
            }
            return sb.ToString();
        }
    }

    class EngineTime : Witcher3Value
    {
        public uint timeval;

        public override string ToString()
        {
            return "Undeciphered EngineTime";
        }
    }

    class TagList : Witcher3Value
    {
        public List<string> Tags;

        public TagList()
        {
            Tags = new List<string>();
        }
    }


    class EngineTransform : Witcher3Value
    {
        public override string ToString()
        {
            return "Undeciphered enginetransform";
        }
    }
}
