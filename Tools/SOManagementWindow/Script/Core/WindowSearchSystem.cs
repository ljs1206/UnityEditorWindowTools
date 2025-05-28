#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LJS.Editing.SOManagement
{
    public class WindowSearchSystem
    {
        private ToolbarPopupSearchField _searchField;
        private string[] _searchFieldContainer;

        public event Action<string[]> OnSearchFieldValueChangedAction;

        /// <summary>
        /// 데이터 셋업 SO
        /// </summary>
        /// <param name="searchField">SerachField Component</param>
        /// <param name="dataArray">현재 Type의 SOArray</param>
        public void SetupSearchSystem(ToolbarPopupSearchField searchField, string[] dataArray)
        {
            _searchField = searchField;
            Debug.Log(dataArray);
            _searchFieldContainer = dataArray;

            _searchField.RegisterValueChangedCallback(HandleSOCurrentValueChangeEvent);
        }

        /// <summary>
        /// SearchField의 값이 변경되었을 때 발생하는 이벤트
        /// 현재 SOArray에 이 문자열을 포함하고 있는 것들을 가지고 온다.
        /// </summary>
        /// <param name="evt"> 뭐가 써졌는지 또는 기본적인 변경 정보등</param>
        private void HandleSOCurrentValueChangeEvent(ChangeEvent<string> evt)
        {
            List<string> containElementLists = new(_searchFieldContainer.Length);
            if (evt.newValue == "")
            {
                OnSearchFieldValueChangedAction?.Invoke(_searchFieldContainer);
                return;
            }

            for (int i = 0; i < _searchFieldContainer.Length; i++)
            {
                if (_searchFieldContainer[i].Contains(evt.newValue))
                {
                    containElementLists.Add(_searchFieldContainer[i]);
                }
            }
            OnSearchFieldValueChangedAction?.Invoke(containElementLists.ToArray());
        }
    }
}
#endif
