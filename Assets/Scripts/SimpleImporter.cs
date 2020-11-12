using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine;

using System.Runtime.InteropServices;/// <summary>
using System.Runtime.Serialization.Json;
//using System.Windows.Data.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Web.Script.Serialization;
using System.Web;
using UnityEngine.UIElements;
using System.Data.Common;
//using Newtonsoft.Json.Linq;

namespace PointCloudExporter
{
	public class JsonTools
	{
		// 从一个对象信息生成Json串
		public static string ObjectToJson(object obj)
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
			MemoryStream stream = new MemoryStream();
			serializer.WriteObject(stream, obj);
			byte[] dataBytes = new byte[stream.Length];
			stream.Position = 0;
			stream.Read(dataBytes, 0, (int)stream.Length);
			return Encoding.UTF8.GetString(dataBytes);
		}
		// 从一个Json串生成对象信息
		public static object JsonToObject(string jsonString, object obj)
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
			MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
			return serializer.ReadObject(mStream);
		}
	}
	public class SimpleImporter
	{
		// Singleton
		private static SimpleImporter instance;
		private SimpleImporter () {}
		public static SimpleImporter Instance {
			get {
				if (instance == null) {
					instance = new SimpleImporter();
				}
				return instance;
			}
		}

		[DllImport("C:\\Users\\admin\\poly2tri\\out\\build\\x64-Debug\\pcdReader.dll", EntryPoint = "readPCD", CharSet = CharSet.Ansi)]
		public static extern int readPCD(IntPtr p, int seglen, ref IntPtr index);

		public MeshInfos Load (string filePath, int maximumVertex = 65000)
		{
			//System.Diagnostics.Process.Start("CMD.exe", "/C del / q / f *.txt");

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = "/C del / q / f *.txt";
			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();

			{
				TextWriter sw = new StreamWriter("into Load.txt");
				sw.Close();
			}
			char[] filec = filePath.ToCharArray();
			int flen = filePath.Length;
			IntPtr buffer = Marshal.AllocHGlobal(flen * sizeof(char));
			Marshal.Copy(filec, 0, buffer, flen);

			IntPtr ansp = new IntPtr(0);
			{
				TextWriter sw = new StreamWriter("before readPCD.txt");
				sw.Close();
			}
			int anslen = readPCD(buffer, filePath.Length, ref ansp);
			{
				TextWriter sw = new StreamWriter("after readPCD.txt");
				sw.Close();
			}
			char[] ans = new char[anslen];
			Marshal.Copy(ansp, ans, 0, anslen);


			string input = new string(ans);

			{
				TextWriter sw = new StreamWriter("before_JavaScriptSerializer.txt");
				sw.Close();
			}
			var jss = new JavaScriptSerializer();
			{
				TextWriter sw = new StreamWriter("after_JavaScriptSerializer.txt");
				sw.Close();
			}

			/*
            if (false){
				string input2 = "[{error: \"Account with that email exists\"}]";
				var jss2 = new JavaScriptSerializer();

				var array2 = jss2.Deserialize<Dictionary<string, object>[]>(input2);
				var dict2 = array2[0] as Dictionary<string, object>;
				Console.WriteLine(dict2["error"]);

				// More short with dynamic
				dynamic d2 = jss2.DeserializeObject(input2);
				Console.WriteLine(d2[0]["error"]);
			}
			*/
			jss.MaxJsonLength = 256000000;
			var array = jss.Deserialize<object[]>(input);
			{
				TextWriter sw = new StreamWriter("after Deserialize.txt");
				sw.Close();
			}

			MeshInfos data = new MeshInfos();
			data.vertexCount = array.Length;
			data.vertices = new Vector3[data.vertexCount];
			data.normals = new Vector3[data.vertexCount];
			data.colors = new Color[data.vertexCount];
			for (int i = 0; i< array.Length; ++i)
            {
				Dictionary<string, object> jp = array[i] as Dictionary<string, object>;
				//decimal dx = jp["x"];
				float x = Convert.ToSingle(jp["x"]);
				float y = Convert.ToSingle(jp["y"]);
				float z = Convert.ToSingle(jp["z"]);
				data.vertices[i] = new Vector3(x, y, z);

			}
			
			return data;
		}
	}
}
