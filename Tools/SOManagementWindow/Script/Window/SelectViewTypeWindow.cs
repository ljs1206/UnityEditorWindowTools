using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LJS.Editing.SOManagement
{
    public class SelectViewTypeWindow : EditorWindow
    {
        private readonly string _toggleUSSClass = "ljs-toggle";
        
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField] private NoLoadDataListSO _noloadDataListSO;

        private WindowSearchSystem _windowSearchSystem;

        private VisualElement _toggleScrollView;
        private Toggle _allSelectToggle;
        private Toggle _cancelAllSelectToggle;
        private ToolbarPopupSearchField _toggleSearchField;

        private List<Type> _typeArray;
        private Dictionary<Toggle, Type> _typeDict = new();
        private List<Toggle> _spawnToggleList;
        private string[] _nameArray;

        [MenuItem("Window/UI Toolkit/SelectViewTypeWindow")]
        public static void ShowExample()
        {
            SelectViewTypeWindow wnd = GetWindow<SelectViewTypeWindow>();
            wnd.titleContent = new GUIContent("SelectViewTypeWindow");
        }

        public void SetInfo(List<Type> typeArray)
        {
            _typeDict = new();
            _spawnToggleList = new();

            SetUp();

            _typeArray = typeArray;

            foreach (var type in _typeArray)
            {
                if (type == null) continue;
                Toggle toggle = new Toggle();
                toggle.text = type.ToString();
                toggle.name = type.ToString();
                toggle.AddToClassList(_toggleUSSClass);

                toggle.RegisterValueChangedCallback(HandleToggleChangedFunc);

                _typeDict.Add(toggle, type);

                _toggleScrollView.Add(toggle);

                if (_noloadDataListSO.NoLoadDataList.Contains(type.ToString()))
                    toggle.value = true;
            }

            int count = 0;
            _nameArray = new string[_typeDict.Count];
            foreach (var item in _typeDict)
            {
                _nameArray[count] = item.Key.name;
                count++;
            }

            _windowSearchSystem = new WindowSearchSystem();
            _windowSearchSystem.SetupSearchSystem(_toggleSearchField, _nameArray);
            _windowSearchSystem.OnSearchFieldValueChangedAction += HandleSearchFnuc;
        }

        public void SetUp()
        {
            VisualElement root = rootVisualElement;
            root.style.flexGrow = 1;

            VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
            root.Add(labelFromUXML);

            _toggleScrollView = root.Q<VisualElement>("ToggleScrollView");
            _allSelectToggle = root.Q<Toggle>("Toggle_AllSelect");
            _cancelAllSelectToggle = root.Q<Toggle>("Toggle_None");
            _toggleSearchField = root.Q<ToolbarPopupSearchField>("TypeSearchField");

            root.Q<Button>("CompleteBtn").clicked += HandleCompleteClick;
            _allSelectToggle.RegisterValueChangedCallback(HandleAllSelectToggleChangeFunc);
            _cancelAllSelectToggle.RegisterValueChangedCallback(HandleCancelToggleChangeFunc);
        }

        private void HandleSearchFnuc(string[] dataArray)
        {
            foreach (var toggle in _typeDict)
            {
                ChangeDisplayType(toggle.Key, DisplayStyle.None);
            }
            
            foreach (var toggle in _typeDict)
            {
                for (int i = 0; i < dataArray.Length; ++i)
                {
                    if (dataArray[i] == toggle.Key.name)
                    {
                        ChangeDisplayType(toggle.Key, DisplayStyle.Flex);
                    }
                }
            }
        }

        private void ChangeDisplayType(VisualElement element, DisplayStyle displayStyle){
            var display = element.style.display;
            display.value = displayStyle;
            element.style.display = display;
        }

        private void HandleCancelToggleChangeFunc(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                _allSelectToggle.value = false;
                foreach (var item in _typeDict)
                {
                    item.Key.value = false;
                }
            }
        }

        private void HandleAllSelectToggleChangeFunc(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                _cancelAllSelectToggle.value = false;
                foreach (var item in _typeDict)
                {
                    item.Key.value = true;
                }
            }
            else
            {
                foreach (var item in _typeDict)
                {
                    item.Key.value = false;
                }
            }
        }

        private void HandleCompleteClick()
        {
            GetWindow<SOManagementWindow>().Close();
            GetWindow<SOManagementWindow>().Show();
            Close();
        }

        private void HandleToggleChangedFunc(ChangeEvent<bool> evt)
        {
            Debug.Log("Change");
            Type type = _typeDict[(evt.target as Toggle)!];
            if (evt.newValue)
            {
                if(!_noloadDataListSO.NoLoadDataList.Contains(type.ToString()))
                    _noloadDataListSO.NoLoadDataList.Add(type.ToString());
            }
            else
            {
                _noloadDataListSO.NoLoadDataList.Remove(type.ToString());
            }
        }
    }
}