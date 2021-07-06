using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dotnet_chat_server
{
    public class Message
    {
        private IDictionary<string, object> payloadMap;

        private Message(IDictionary<string, object> payloadMap)
        {
            this.payloadMap = payloadMap;
        }

        private static void WriteString(BinaryWriter binaryWriter, String value)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(value);

            binaryWriter.Write(bytes.Length);
            binaryWriter.Write(bytes);
        }

        private static String ReadString(BinaryReader binaryReader)
        {
            int stringLength = binaryReader.ReadInt32();

            byte[] inMessage = new byte[stringLength];

            return new UTF8Encoding().GetString(inMessage, 0, binaryReader.Read(inMessage, 0, stringLength));
        }

        public int GetInt32(string propertyName)
        {
            return (int)this.payloadMap[propertyName];
        }

        public string GetString(string propertyName)
        {
            return (string)this.payloadMap[propertyName];
        }

        public int[] GetSingleDimArrayInt32(string propertyName)
        {
            return (int[])this.payloadMap[propertyName];
        }

        public string[] GetSingleDimArrayString(string propertyName)
        {
            return (string[])this.payloadMap[propertyName];
        }

        public int[][] GetMultiDimArrayInt32(string propertyName)
        {
            return (int[][])this.payloadMap[propertyName];
        }

        public string[][] GetMultiDimArrayString(string propertyName)
        {
            return (string[][])this.payloadMap[propertyName];
        }

        public static byte[] Serialize(dynamic payload)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryWriter binaryWriter = new BinaryWriter(memoryStream);

                PropertyInfo[] payloadProperties = payload.GetType().GetProperties() as PropertyInfo[];

                if (payloadProperties != null)
                {
                    binaryWriter.Write(payloadProperties.Length);

                    foreach (PropertyInfo payloadProperty in payloadProperties)
                    {
                        //binaryWriter.Write(payloadProperty.PropertyType.Name.Length);
                        WriteString(binaryWriter, payloadProperty.PropertyType.Name);

                        //binaryWriter.Write(payloadProperty.Name.Length);
                        WriteString(binaryWriter, payloadProperty.Name);

                        switch (payloadProperty.PropertyType.Name)
                        {
                            case "Int32":
                                binaryWriter.Write(payloadProperty.GetValue(payload));
                                break;
                            case "String":
                                //binaryWriter.Write(((String)payloadProperty.GetValue(payload)).Length);
                                WriteString(binaryWriter, (String)payloadProperty.GetValue(payload));
                                break;
                            case "Int32[]":
                                binaryWriter.Write(JsonConvert.SerializeObject(payloadProperty.GetValue(payload)));
                                break;
                            case "String[]":
                                binaryWriter.Write(JsonConvert.SerializeObject(payloadProperty.GetValue(payload)));
                                break;
                            case "Int32[][]":
                                binaryWriter.Write(JsonConvert.SerializeObject(payloadProperty.GetValue(payload)));
                                break;
                            case "String[][]":
                                binaryWriter.Write(JsonConvert.SerializeObject(payloadProperty.GetValue(payload)));
                                break;
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public static Message Deserialize(NetworkStream networkStream)
        {
            BinaryReader binaryReader = new BinaryReader(networkStream);

            int propertiesLength = binaryReader.ReadInt32();

            string propertyType;
            string propertyName;

            var payload = new ExpandoObject() as IDictionary<string, object>;

            for (int i = 0; i < propertiesLength; i++)
            {
                propertyType = ReadString(binaryReader);
                propertyName = ReadString(binaryReader);

                switch (propertyType)
                {
                    case "Int32":
                        payload.Add(propertyName, binaryReader.ReadInt32());
                        break;
                    case "String":
                        payload.Add(propertyName, ReadString(binaryReader));
                        break;
                    case "Int32[]":
                        payload.Add(propertyName, ((JArray)JsonConvert.DeserializeObject(ReadString(binaryReader))).ToObject<int[]>());
                        break;
                    case "String[]":
                        payload.Add(propertyName, ((JArray)JsonConvert.DeserializeObject(ReadString(binaryReader))).ToObject<string[]>());
                        break;
                    case "Int32[][]":
                        payload.Add(propertyName, ((JArray)JsonConvert.DeserializeObject(ReadString(binaryReader))).ToObject<int[][]>());
                        break;
                    case "String[][]":
                        payload.Add(propertyName, ((JArray)JsonConvert.DeserializeObject(ReadString(binaryReader))).ToObject<string[][]>());
                        break;
                }
            }

            return new Message(payload);
        }
    }
}
