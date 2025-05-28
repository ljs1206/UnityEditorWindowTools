using System.Collections.Generic;
using UnityEngine;

namespace LJS.Editing.SOManagement
{   
    [CreateAssetMenu(fileName = "NoLoadDataListSO", menuName = "SO/NoLoadDataListSO")]
    public class NoLoadDataListSO : ScriptableObject
    {
        public List<string> NoLoadDataList = new();
    }
}
