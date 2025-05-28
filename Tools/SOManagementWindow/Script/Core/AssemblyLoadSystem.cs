#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LJS.Editing.SOManagement
{
    [Serializable]
    public struct AssemblyInfo
    {
        public string assemblyName;
        public List<string> typeNameList;
    }
    
    [Serializable]
    [CreateAssetMenu(fileName = "AssemblyLoadSystem", menuName = "LJS/SO/AssemblyLoadSystem")]
    public class AssemblyLoadSystem : ScriptableObject
    {
        public List<Type> GetLoadTypeList { get; } = new();
        [SerializeField] private List<AssemblyInfo> GetAssemblyInfoList;
        
        private Stopwatch sw = new Stopwatch();

        /// <summary>
        /// TypeList 반환 해주는 함수
        /// </summary>
        /// <param name="sorting"></param>
        /// <returns></returns>
        public List<Type> ReturnLoadTypeList(bool sorting = false)
        {
            // if (sorting)
            //     return GetLoadTypeList.OrderBy(x => x.Name).ToList();
            
            return GetLoadTypeList;
        }
        
        /// <summary>
        /// Type 갱신 함수
        /// </summary>
        public void RefreshType()
        {
            sw.Start();
            LoadAssembly();
            GetLoadTypeList.Clear();
            foreach (var info in GetAssemblyInfoList)
            {
                Assembly assemblyInfo = Assembly.Load(info.assemblyName);
                foreach (var typeName in info.typeNameList)
                {
                    GetLoadTypeList.Add(assemblyInfo.GetType(typeName));
                }

            }
            foreach (var type in GetLoadTypeList)
            {
                // Debug.Log($"RefreshType : {type}");
            }
            sw.Stop();
            
            Debug.Log($"Late Time : {(float)sw.ElapsedMilliseconds / 1000}S");
        }
        
        /// <summary>
        /// 어셈블리에서 SO를 포함하고 있는 Type을 들고 온다.
        /// 그 뒤 유니티 기본 제공 Assembly를 제외한 Type를 TypeList에 넣는다.
        /// ASDF 때문에 Assembly를 찾는 뒤 GetType를 해야한다.
        /// </summary>
        public void LoadAssembly()
        {
            List<AssemblyInfo> newAssemblyInfoList = new();
            var listOfBs = (
                from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in domainAssembly.GetTypes()
                where typeof(ScriptableObject).IsAssignableFrom(type)
                      && !typeof(EditorWindow).IsAssignableFrom(type)
                      && !typeof(Editor).IsAssignableFrom(type)
                select type).ToArray();
            if (GetLoadTypeList.Count != listOfBs.Length)
            {
                foreach (var item in listOfBs)
                {
                    string typeName = item.ToString();
                    if (typeName.StartsWith("UnityEngine.") == false
                     && typeName.StartsWith("UnityEditor.") == false
                     && typeName.StartsWith("Unity.") == false
                     && typeName.StartsWith("UnityEditorInternal.") == false
                     && typeName.StartsWith("TreeEditor.") == false
                     && typeName.StartsWith("SpriteShapeObjectPlacementEditor") == false
                     && typeName.StartsWith("AddressableAssets.") == false
                     && typeName.StartsWith("DownloadableSample") == false
                     && typeName.StartsWith("LightPlacementTool") == false
                     && typeName.StartsWith("FullScreenPassRendererFeatureEditor") == false
                     && typeName.StartsWith("FullScreenPassRendererFeature") == false
                     && typeName.StartsWith("Packages.") == false
                     && typeName.StartsWith("TMPro.") == false
                     && typeName.StartsWith("Timeline") == false
                     && typeName.StartsWith("TestRunner.") == false
                     && typeName.StartsWith("System.") == false
                     && typeName.StartsWith("LJS.Editing.SOManagement.") == false
                     && typeName.StartsWith("JetBrains.") == false
                     && typeName.StartsWith("FIMSpace.") == false
                     && typeName.StartsWith("FMODUnity.") == false
                     && typeName.StartsWith("Febucci.") == false)
                    {
                        // Debug.Log(item.Assembly.GetName().Name);
                        // Debug.Log(item);
                       AssemblyInfo newInfo = new AssemblyInfo();
                       newInfo.assemblyName = item.Assembly.GetName().Name;
                       newInfo.typeNameList = new();
                       newInfo.typeNameList.Add(typeName);
                       if (newAssemblyInfoList.Exists(x
                               => x.assemblyName == newInfo.assemblyName))
                       {
                           newAssemblyInfoList.Find(x => x.assemblyName == newInfo.assemblyName)
                               .typeNameList.Add(typeName);
                       }
                       else
                       {
                           newAssemblyInfoList.Add(newInfo);
                       }
                    }
                }
                if (newAssemblyInfoList.Count == GetAssemblyInfoList.Count)
                {
                    for (int i = 0; i < GetAssemblyInfoList.Count; ++i)
                    {
                        AssemblyInfo assemblyInfo = GetAssemblyInfoList[i];
                        try
                        {
                            var newAssemblyInfo = newAssemblyInfoList.Find(x => x.assemblyName
                                == assemblyInfo.assemblyName);
                            
                            if (newAssemblyInfo.typeNameList.Count
                                == assemblyInfo.typeNameList.Count)
                            {
                                // Debug.Log("AssemblyInfoList Count is Same");
                                if (!newAssemblyInfo.typeNameList.
                                        Equals(assemblyInfo.typeNameList))
                                {
                                    // Debug.Log("Find different Point");
                                    foreach (var item in newAssemblyInfo.typeNameList)
                                    {
                                        // Debug.Log(item);
                                        if (!assemblyInfo.typeNameList.Contains(item))
                                        {
                                            assemblyInfo.typeNameList.Add(item);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Debug.Log("AssemblyInfoList Count is Not Same");
                                assemblyInfo.typeNameList
                                    = newAssemblyInfo.typeNameList;
                            }

                            GetAssemblyInfoList[i] = assemblyInfo;
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                            Debug.Log(assemblyInfo);
                        }
                    }
                }
                else
                {
                    GetAssemblyInfoList = newAssemblyInfoList;
                }
            }
        }

        public void ResetAll()
        {
            GetAssemblyInfoList.Clear();
        }
    }
}
#endif
