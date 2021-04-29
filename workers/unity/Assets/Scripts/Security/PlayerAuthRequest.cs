using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cubism
{
    /*
     * Note: it might make more sense to use Protobuf for this kind of custom serializable data
     */
    public class PlayerAuthRequest
    {
        public string SessionKey;

        public byte[] Serialize()
        {
            using (var memoryStream = new MemoryStream())
            {
                var bw = new BinaryWriter(memoryStream);
                bw.Write(SessionKey);
                bw.Close();
                return memoryStream.ToArray();
            }
        }

        public static PlayerAuthRequest Deserialize(byte[] serialized)
        {
            using (var memoryStream = new MemoryStream(serialized))
            {
                var br = new BinaryReader(memoryStream);
                var deserialized = new PlayerAuthRequest();
                deserialized.SessionKey = br.ReadString();
                return deserialized;
            }
        }
    }
}
