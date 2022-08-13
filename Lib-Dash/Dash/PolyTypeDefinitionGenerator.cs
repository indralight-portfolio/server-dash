using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using Common.Reflection;
using Common.Utility;

namespace Dash
{
    public static class PolyTypeDefinitionGenerator
    {
        private const string NewLine = "\n"; // os 상관없이 통일
        public static void Generate(string path)
        {
            var targetAssemblies = new[]
            {
                typeof(Common.StaticInfo.ISpecificInfo).Assembly, // common
                typeof(Dash.StaticInfo.StaticInfo).Assembly
            };

            var allTypes = (from assembly in targetAssemblies
                from eachType in assembly.GetTypes()
                select eachType).ToList();

            var polyTypeDefinitionTypes =
                from eachType in allTypes
                where eachType.GetCustomAttribute<PolyTypeDefinitionAttribute>(false) != null
                where eachType.IsAbstract == true || eachType.IsInterface == true
                select eachType;

            StringBuilder sb = new StringBuilder();
            TextWriter textWriter = new StringWriter(sb);
            textWriter.Write($"// this code is generated by Assets/Scripts/Tool/CodeGeneration. Do not edit." +
                             NewLine);
            textWriter.Write("using System;" + NewLine);
            textWriter.Write("using System.Collections.Generic;" + NewLine);
            textWriter.Write("using Common.Reflection;" + NewLine);
            textWriter.Write("namespace Dash" + NewLine);
            textWriter.Write("{" + NewLine);
            textWriter.Write("    public class PolyTypeDefinitions : IPolyTypeDefinitions" + NewLine);
            textWriter.Write("    {" + NewLine);
            textWriter.Write("        public IReadOnlyDictionary<Type, PolyTypeDefinition> Dic => _dic;" + NewLine);
            textWriter.Write("        private static Dictionary<Type, PolyTypeDefinition> _dic = new Dictionary<Type, PolyTypeDefinition> {" + NewLine);

            foreach (Type polyTypeDefinitionType in polyTypeDefinitionTypes)
            {
                try
                {
                    var targetTypes = from eachType in allTypes
                        where polyTypeDefinitionType.IsAssignableFrom(eachType) && eachType.IsAbstract == false
                        // && (eachType.GenericTypeArguments == null || eachType.GenericTypeArguments.Length == 0)
                        select eachType;

                    textWriter.Write($"            {{" + NewLine);
                    textWriter.Write($"                typeof({TypeUtility.PrettyName(polyTypeDefinitionType)}), new PolyTypeDefinition(typeof({TypeUtility.PrettyName(polyTypeDefinitionType)}), new[]" +
                                     NewLine);
                    textWriter.Write($"                    {{" + NewLine);
                    textWriter.Write($"{string.Join("", targetTypes.Select(t => $"                        typeof({TypeUtility.PrettyName(t)}),{NewLine}"))}");
                    textWriter.Write($"                    }})" + NewLine);
                    textWriter.Write("            }," + NewLine);
                }
                catch (Exception e)
                {
                    Common.Log.Logger.Fatal(e);
                    throw new Exception($"PolyTypeDefinition {polyTypeDefinitionType} failed!");
                }
            }

            textWriter.Write("        };" + NewLine);
            textWriter.Write("    }" + NewLine);
            textWriter.Write("}" + NewLine);
            File.WriteAllText(path, sb.ToString());
        }
    }
}