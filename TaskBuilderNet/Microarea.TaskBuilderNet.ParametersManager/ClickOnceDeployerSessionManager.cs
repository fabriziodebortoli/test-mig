using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Microarea.TaskBuilderNet.ParametersManager
{
    public class ClickOnceDeployerSessionManager
    {
        IParametersManager parametersManager = new ParametersManagerV1();

        public string Country { get; set; }

        //---------------------------------------------------------------------------
        public string PackData(ClickOnceDeployerEnvelope envelope, bool isOn, Guid sessionId)
        {
            byte[] buffer = null;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, envelope);
                buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
            }
            string inputString = Convert.ToBase64String(buffer);

            return parametersManager.SetParameter(
                isOn,
                inputString,
                sessionId.ToString(),
                this.Country
                );
        }

        //---------------------------------------------------------------------------
        public ClickOnceDeployerEnvelope UnpackData(string dataFile, bool isOn, Guid sessionId)
        {
            string outputString = parametersManager.GetParameter(
                 isOn,
                 dataFile,
                 sessionId.ToString(),
                 this.Country
                 );

            var buffer = Convert.FromBase64String(outputString);

            var bf = new BinaryFormatter();
            var envelope = (ClickOnceDeployerEnvelope)bf.Deserialize(new MemoryStream(buffer));

            return envelope;
        }
    }
}
