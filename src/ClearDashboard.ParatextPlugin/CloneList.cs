using System.IO;
using System.Runtime.Serialization;

namespace ClearDashboard.ParatextPlugin
{
    public static class CloneList
    {
        //public static async Task<T> DeepCloneAsync<T>(this T a)
        //{
        //    await using (MemoryStream stream = new MemoryStream())
        //    {
        //        DataContractSerializer dcs = new DataContractSerializer(typeof(T));
        //        dcs.WriteObject(stream, a);
        //        stream.Position = 0;
        //        return (T)dcs.ReadObject(stream);
        //    }
        //}

        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                dcs.WriteObject(stream, a);
                stream.Position = 0;
                return (T)dcs.ReadObject(stream);
            }
        }

    }
}
