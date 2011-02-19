using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using PluginInterface;

namespace StructBuilder
{
    [Export(typeof(IPlugin))]
    public class StructBuilder : IPlugin
    {
        [Import("PluginFinished")]
        public Func<int, int> Finished { get; set; }

        public void Run(DataTable data)
        {
            string name = Path.GetFileNameWithoutExtension(data.TableName);
            using (StreamWriter writer = new StreamWriter(name + ".h"))
            {
                writer.Write("const char* {0}fmt[] = \"", name);

                List<string> pks = new List<string>();
                foreach (var col in data.PrimaryKey)
                    pks.Add(col.ColumnName);

                for (var i = 0; i < data.Columns.Count; ++i)
                {
                    string type = data.Columns[i].DataType.Name;
                    if (pks.Contains(type))
                    {
                        writer.Write('n');
                        continue;
                    }

                    if (type.IndexOf("Int32") != -1)
                        writer.Write('i');
                    else if (type.IndexOf("Int64") != -1)
                    {
                        writer.Write('i');
                        writer.Write('i');
                    }
                    else if (type.IndexOf("Int16") != -1)
                    {
                        writer.Write('i');
                        ++i;
                    }
                    else if (type.IndexOf("Byte") != -1)
                        writer.Write('b');
                    else if (type.IndexOf("Single") != -1)
                        writer.Write('f');
                    else if (type.IndexOf("String") != -1)
                        writer.Write('s');
                    else
                        throw new Exception("Unknown data type: " + type);
                }

                writer.WriteLine("\";");
                writer.WriteLine();

                writer.WriteLine("struct {0}Entry", name);
                writer.WriteLine("{");

                for (var i = 0; i < data.Columns.Count; ++i)
                {
                    writer.Write("    ");
                    string colname = data.Columns[i].ColumnName;
                    switch (data.Columns[i].DataType.Name)
                    {
                        case "Int64":
                            writer.WriteLine("int64 {0};", colname);
                            break;
                        case "UInt64":
                            writer.WriteLine("uint64 {0};", colname);
                            break;
                        case "Int32":
                            writer.WriteLine("int32 {0};", colname);
                            break;
                        case "UInt32":
                            writer.WriteLine("uint32 {0};", colname);
                            break;
                        case "Int16":
                            writer.WriteLine("int16 {0};", colname);
                            break;
                        case "UInt16":
                            writer.WriteLine("uint16 {0};", colname);
                            break;
                        case "SByte":
                            writer.WriteLine("int8 {0};", colname);
                            break;
                        case "Byte":
                            writer.WriteLine("uint8 {0};", colname);
                            break;
                        case "Single":
                            writer.WriteLine("float {0};", colname);
                            break;
                        //case "Double":
                        //    writer.WriteLine("double {0};", colname);
                        //    break;
                        case "String":
                            writer.WriteLine("const char* {0};", colname);
                            break;
                        default:
                            throw new Exception(String.Format("Unknown field type {0}!", data.Columns[i].DataType.Name));
                    }
                }

                writer.WriteLine("};");
            }

            Finished(data.Columns.Count);
        }
    }
}
