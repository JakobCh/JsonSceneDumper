using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace bepinex_JsonDump
{
    [BepInPlugin(GUID: "com.jakobch.jsonscenedumper", Name: "JsonSceneDumper", Version: "1.0")]
    public class JsonSceneDumper : BaseUnityPlugin
    {
        public int resetTime = 0;

        public void Start() {
            Logger.Log(0, "JsonDump Loaded");
        }


        public void OnGUI()
        {
            //GUILayout.Label("JSONDUMP LOADED");

            if (resetTime != 0) {
                resetTime -= 1;
                return;
            }

            if ( !Input.GetKeyDown(KeyCode.Delete) ) { return; }
            resetTime = 60;
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            string tempFileName = Path.GetTempFileName();
            using (FileStream fileStream = File.OpenWrite(tempFileName))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    streamWriter.WriteLine("{");
                    GameObject[] array = rootGameObjects;
                    for (int i = 0; i < array.Length; i++)
                    {
                        GameObject obj = array[i];
                        if (i != array.Length - 1)
                        {
                            PrintRecursive(streamWriter, obj, 1, false);
                        }
                        else
                        {
                            PrintRecursive(streamWriter, obj, 1, true);
                        }

                    }
                    streamWriter.WriteLine("}");
                }
            }
            Process.Start("notepad.exe", tempFileName);
            new WaitForSeconds(2);
        }

        private static void PrintRecursive(StreamWriter sw, GameObject obj, int d, bool last)
        {
            string str = new string(' ', 4 * d);
            string str2 = new string(' ', 4 * d * 2);
            string str3 = new string(' ', 4 * d * 3);

            sw.WriteLine(str + '"' + obj.name + "--" + obj.GetType().FullName + '"' + ": {");

            //grab gamobject children
            if (obj.transform.childCount > 0)
            {
                foreach (Transform transform in obj.transform)
                {
                    foreach (Transform child in transform)
                    {
                        PrintRecursive(sw, child.gameObject, d + 1, false);
                    }
                }
            }


            //grab components
            Component[] comps = obj.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                Component comp = comps[i];
                sw.WriteLine(str + str + '"' + comp.name + "--" + comp.GetType().FullName + '"' + ": {");  // "CoolComponent--UnityEngine.Transform": 1

                PropertyInfo[] properties = comp.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

                for (int j = 0; j < properties.Length; j++)
                {
                    PropertyInfo pInfo = properties[j];
                    try
                    {
                        string value = pInfo.GetValue(comp, null).ToString();

                        //TODO
                        //this replaces wierd characters (that I've found so far could be more in other games?) with spaces
                        //maybe replace them with something else becuse they have to be there for a reason?
                        value = value.Replace("	", " ");
                        value = value.Replace("\n", " ");

                        if (j == properties.Length - 1) //if this is the last Property
                        {
                            //dont add a "," after
                            sw.WriteLine(str3 + '"' + pInfo.Name + "<" + pInfo.PropertyType.Name + ">\": \"" + value + "\"");
                        }
                        else
                        {
                            sw.WriteLine(str3 + '"' + pInfo.Name + "<" + pInfo.PropertyType.Name + ">\": \"" + value + "\",");
                        }


                        /*sw.WriteLine(string.Concat(new object[]
                        {
                            str3,
                            '"',
                            pInfo.Name,
                            "<",
                            pInfo.PropertyType.Name,
                            ">\": \"",
                            value,
                            "\","
                        }));*/
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(BepInEx.Logging.LogLevel.Debug, ex);


                        //TODO
                        //I really only added this so the last propertie in the json file wouldn't end with a "," if the last propertie crashed

                        if (j == properties.Length - 1) //if this is the last Property
                        {
                            //dont add a "," after
                            sw.WriteLine(str3 + "\"Exception" + j.ToString() + "\": \"1\"");
                        }
                        else
                        {
                            sw.WriteLine(str3 + "\"Exception" + j.ToString() + "\": \"1\",");
                        }
                    }
                }


                if (i == comps.Length - 1) //if this is the last one dont add a ","
                {
                    sw.WriteLine(str + str + "}");
                }
                else
                {
                    sw.WriteLine(str + str + "}" + ",");
                }
            }

            if (last)
            {
                sw.WriteLine(str + "}");
            }
            else
            {
                sw.WriteLine(str + "}" + ",");
            }

        }



    }
}
