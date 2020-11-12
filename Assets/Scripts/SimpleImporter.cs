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
//using System.Web.Script.Serialization;
using System.Web;
using UnityEngine.UIElements;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace PointCloudExporter
{
	
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

		[DllImport("C:\\Users\\admin\\poly2tri\\out\\build\\x64-Release\\pcdReader.dll", EntryPoint = "readPCD", CharSet = CharSet.Ansi)]
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
			Debug.Log("readPCD");
			int anslen = readPCD(buffer, filePath.Length, ref ansp);
			Debug.Log("after readPCD.txt");
			char[] ans = new char[anslen];
			Marshal.Copy(ansp, ans, 0, anslen);


			string input = new string(ans);

			//object va = JsonConvert.DeserializeObject(input);
			JArray receivedObject = JArray.Parse(input);
			//dynamic receivedObject2 = JObject.Parse(input);

			//var list = receivedObject.Count
			Debug.Log("after Deserialize.txt");

			MeshInfos data = new MeshInfos();
			data.vertexCount = receivedObject.Count;
			data.vertices = new Vector3[data.vertexCount];
			data.normals = new Vector3[data.vertexCount];
			data.colors = new Color[data.vertexCount];
			for (int i = 0; i< receivedObject.Count; ++i)
            {
				Dictionary<string, object> jp = receivedObject[i].ToObject<Dictionary<string, object>>();
				//decimal dx = jp["x"];
				float x = Convert.ToSingle(jp["x"]);
				float y = Convert.ToSingle(jp["y"]);
				float z = Convert.ToSingle(jp["z"]);
				data.vertices[i] = new Vector3(x, y, z);
				float r = Convert.ToSingle(jp["r"])/255;
				float g = Convert.ToSingle(jp["g"])/255;
				float b = Convert.ToSingle(jp["b"])/255;
				data.colors[i] = new Color(r, g, b);
			}
			Debug.Log("after MeshInfos.txt");
			return data;
		}
	}
}
