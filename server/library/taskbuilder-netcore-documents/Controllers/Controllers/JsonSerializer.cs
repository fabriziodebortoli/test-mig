using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("JsonSerializer"), Description("It serialize/unserialize document using Json.")]
    public class JsonSerializer : Controller, IJsonSerializer
    {
        public JsonSerializer()
        {

        }

        public void Serialize(IDocument bo)
        {
        }
        public void Deserialize(IDocument bo)
        {
        }
    }
}
