#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public struct FolderInfo
{
    public string size;
    public FileInfo[] fileArray;
    public DirectoryInfo directory;
}
public class FileTree : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    #region IconDict
    
    private readonly (string, string)[] _iconNameList 
        = new (string, string)[24]
    {
        (".cs", "cs Script Icon"),
        (".shader", "Shader Icon"),
        (".mat", "Material Icon"),
        (".asset", "TextAsset Icon"),
        (".prefab", "Prefab Icon"),
        (".unity", "SceneAsset Icon"),
        (".anim", "AnimationClip Icon"),
        (".fbx", "ModelImporter Icon"),
        (".png", "RawImage Icon"),
        (".vfx", "VisualEffect Icon"),
        (".compute", "ComputeShader Icon"),
        (".mask", "AvatarMask Icon"),
        (".avatar", "Avatar Icon"),
        (".overrideController", "AnimatorOverrideController Icon"),
        (".renderTexture", "RenderTexture Icon"),
        (".lighting", "Light Icon"),
        (".timeline", "TimelineAsset Icon"),
        (".signal", "SignalAsset Icon"),
        (".preset", "Preset Icon"),
        (".uxml", "UxmlScript Icon"),
        (".uss", "UssScript Icon"),
        (".asmdef", "AssemblyDefinitionAsset Icon"),
        (".asmref", "AssemblyDefinitionReferenceAsset Icon"),
        (".unitypackage", "UnityLogo"),
    };

    private Dictionary<string, Texture> _iconDict;

    #endregion

    #region USSClass

    private readonly string _fileVisual = "file-visual";
    private readonly string _fileNameLabel = "file-name-label";
    private readonly string _fileIconLabel = "file-icon";

    #endregion

    #region MyElement

    private Foldout _rootFoldout;
    private TextField _nameField;
    private VisualElement _fileCreateList;
    private VisualElement _currentVisualFileList;
    private Label _folderNameLabel;
    private Label _typeNameLabel;
    
    #endregion

    /// <summary>
    /// Foldout에 따라 Directory를 반환하는 Dictionary
    /// </summary>
    private Dictionary<Foldout, FolderInfo> _directoryInfoDict;
    /// <summary>
    /// 현재 Directory
    /// </summary>
    private DirectoryInfo _currentDirectory;

    /// <summary>
    /// 만들어진 FileVisual Element를 저장하는 Dict
    /// </summary>
    private Dictionary<string, VisualElement> _savedCreateElementDict;
    
    private Type _currentType;
    private Action<ScriptableObject> _endCallback;
    private StringBuilder sb = new StringBuilder();

    /// <summary>
    /// 기본 Info 설정
    /// </summary>
    /// <param name="type"></param>
    /// <param name="endCallback"></param>
    public void SetInfo(Type type, Action<ScriptableObject> endCallback)
    {
        _currentType = type;
        _endCallback = endCallback;

        sb.Clear();
        sb.Append($"Current Type : {_currentType.Name}");
        _typeNameLabel.text = sb.ToString();
    }

    /// <summary>
    /// 생성 함수
    /// </summary>
    public void CreateGUI()
    {
        _iconDict = new();
        _directoryInfoDict = new();
        _savedCreateElementDict = new();
        foreach (var item in _iconNameList)
        {
            _iconDict.Add(item.Item1, EditorGUIUtility.IconContent(item.Item2).image);
        }
        
        VisualElement root = rootVisualElement;
        root.style.flexGrow = 1;
        
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        
        _rootFoldout =  root.Q<Foldout>("RootFoldout");
        _nameField = root.Q<TextField>("FileNameField");
        _fileCreateList = root.Q<VisualElement>("FileVisualList");
        _folderNameLabel = root.Q<Label>("FolderNameLabel");
        _typeNameLabel = root.Q<Label>("TypeNameLabel");

        _rootFoldout.RegisterValueChangedCallback(HandleValueChangedFoldoutEvent);
        _rootFoldout.text = "Asset";

        FolderInfo folderInfo = new FolderInfo();
        DirectoryInfo rootDirectory = new DirectoryInfo(Application.dataPath);
        folderInfo.size = GetFileSize(FolderSize(rootDirectory));
        folderInfo.fileArray = rootDirectory.GetFiles();
        folderInfo.directory = rootDirectory;
        _directoryInfoDict.Add(_rootFoldout, folderInfo);

        root.Q<Button>("CreateBtn").clicked += HandleCreateBtnClickEvent;
        root.Q<Button>("CancelBtn").clicked += HandleCancelBtnClickEvent;
    }
    
    /// <summary>
    /// 폴더 사이즈 구하는 함수
    /// 하위 디렉토리 다 돌면서 크기를 구합니다.
    /// </summary>
    /// <param name="d">디렉토리</param>
    /// <returns></returns>
    private long FolderSize(DirectoryInfo directory)
    {
        long size = 0;
        // 파일 사이즈.
        FileInfo[] fis = directory.GetFiles();
        foreach (FileInfo fi in fis)
        {
            size += fi.Length;
        }
        // 하위 디렉토리 사이즈.
        DirectoryInfo[] dis = directory.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            size += FolderSize(di);
        }
        return size;
    }


    #region Event Method

    /// <summary>
    /// 만들기 버튼을 눌렀다면 실행되는 Event
    /// </summary>
    private void HandleCreateBtnClickEvent()
    {
        ScriptableObject newItem = CreateInstance(_currentType);
        Guid typeGuid = Guid.NewGuid();
        if (_nameField.text.Length == 0)
        {
            newItem.name = typeGuid.ToString();
            Debug.Log($"a random GUID was assigned due to the absence of input.");
            Debug.Log($"random GUID : {typeGuid}");
        }
        else
        {
            newItem.name = _nameField.text;
        }
        
        string path = _currentDirectory.FullName;
        path = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
        AssetDatabase.CreateAsset(newItem,
            $"{path}/{newItem.name}.asset");
            
        Debug.
            Log($"Success Create SO, Name : {newItem.name} Path : {path}/{newItem.name}.asset");
            
        AssetDatabase.SaveAssets();
        _endCallback?.Invoke(newItem);
        Close();
    }
    
    private void HandleCancelBtnClickEvent()
    {
        Close();
    }

    /// <summary>
    /// Foldout의 Value가 변경 되었을 때 실행 되는 이벤트
    /// 만약 자식이 이미 만들어졌다면 (Directory 캐싱) _currentDirectory, Foldout 만 변경시켜준다.
    /// </summary>
    /// <param name="evt"></param>
    private void HandleValueChangedFoldoutEvent(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            if (evt.target is Foldout foldout)
            {
                FolderInfo folderInfo = _directoryInfoDict[foldout];
                DirectoryInfo directoryInfo = folderInfo.directory;
                _currentDirectory = directoryInfo;
            
                // Create Child
                if (foldout.childCount <= 0)
                {
                    foreach (var directory in directoryInfo.GetDirectories())
                    {
                        Foldout childFoldout = new Foldout();
                        childFoldout.value = false;
                    
                        childFoldout.text = directory.Name;

                        FolderInfo newFolderInfo = new FolderInfo();
                        newFolderInfo.size = GetFileSize(FolderSize(directory));
                        newFolderInfo.fileArray = directory.GetFiles();
                        newFolderInfo.directory = directory;
                    
                        _directoryInfoDict.Add(childFoldout, newFolderInfo);

                        if (directory.GetFiles().Length <= 0)
                        {
                            childFoldout.toggleOnLabelClick = false;
                        }
                    
                        foldout.Add(childFoldout);
                    }
                }
            
                MakeFileVisual(folderInfo.fileArray, folderInfo.directory.Name, folderInfo.size.ToString());
            }
        }
        else
        {
            if (evt.target is Foldout foldout)
            {
                foreach (var child in foldout.Children())
                {
                    if (child is Foldout childFoldout)
                    {
                        childFoldout.value = false;
                    }
                } 
            }
        }
    }


    #endregion
    
    /// <summary>
    /// 실제 File의 비주얼 만들어 주는 함수
    /// Dict에 없다면 만들어주고 있다면 그걸 띄워줌
    /// </summary>
    /// <param name="fileInfoArray">파일 정보 Array</param>
    /// <param name="folderName">폴더 이름</param>
    /// <param name="folderSize">폴더 크기</param>
    private void MakeFileVisual(FileInfo[] fileInfoArray, string folderName, string folderSize)
    {
        if(_currentVisualFileList != null)
            ChangeDisplayType(_currentVisualFileList, DisplayStyle.None);
        
        if (_savedCreateElementDict.TryGetValue(folderName, out var element))
        {
            ChangeDisplayType(element, DisplayStyle.Flex);
            
            _currentVisualFileList = element;
            sb.Clear();
            sb.Append(folderName);
            sb.Append(" - size : ");
            sb.Append(folderSize);
            _folderNameLabel.text = sb.ToString();
            return;
        }
        
        VisualElement rootElement = new VisualElement();
        rootElement.name = folderName;
        
        foreach (FileInfo fileInfo in fileInfoArray)
        {
            if(fileInfo.Extension == ".meta") continue;
            
            VisualElement fileVisual = new VisualElement();
            fileVisual.AddToClassList(_fileVisual);
            Label fileNameLabel = new Label();
            fileNameLabel.AddToClassList(_fileNameLabel);
            fileNameLabel.text = fileInfo.Name;
            VisualElement icon = new VisualElement();
            icon.AddToClassList(_fileIconLabel);

            string extension = fileInfo.Extension.ToLower();
            if (_iconDict.TryGetValue(extension, out var value))
            {
                ChangeBackGroundImage(icon, value);
            }
            else
            {
                ChangeBackGroundImage(icon, EditorGUIUtility.IconContent("TextAsset Icon").image);
            }
            
            fileVisual.Add(icon);
            fileVisual.Add(fileNameLabel);
            
            rootElement.Add(fileVisual);
        }
        _fileCreateList.Add(rootElement);
        _savedCreateElementDict.Add(folderName, rootElement);
        _currentVisualFileList = rootElement;

        sb.Clear();
        sb.Append(folderName);
        sb.Append("- size : ");
        sb.Append(folderSize);
        _folderNameLabel.text = sb.ToString();
    }
    
    /// <summary>
    /// Display 변경해주는 편의성 함수
    /// </summary>
    /// <param name="element">target</param>
    /// <param name="displayStyle">Flex = true, None = false</param>
    private void ChangeDisplayType(VisualElement element, DisplayStyle displayStyle){
        var display = element.style.display;
        display.value = displayStyle;
        element.style.display = display;
    }

    /// <summary>
    /// 백그라운드 변경해주는 편의성 함수
    /// </summary>
    /// <param name="element">target</param>
    /// <param name="texture">image</param>
    private void ChangeBackGroundImage(VisualElement element, Texture texture)
    {
        var background = element.style.backgroundImage.value;
        background.texture = texture as Texture2D;
        element.style.backgroundImage = background;
    }
    
    /// <summary>
    /// 파일 사이즈 구하기
    /// </summary>
    /// <param name="byteCount"></param>
    /// <returns></returns>
    private string GetFileSize(double byteCount)
    {
        string size = "0 Bytes";

        sb.Clear();
        if (byteCount >= 1073741824.0)
        {
            sb.Append(String.Format("{0:##.##}", byteCount / 1073741824.0));
            sb.Append(" GB");
            
        }
        else if (byteCount >= 1048576.0)
        {
            sb.Append(String.Format("{0:##.##}", byteCount / 1048576.0));
            sb.Append(" MB");
        }
        else if (byteCount >= 1024.0)
        {
            sb.Append(String.Format("{0:##.##}", byteCount / 1024.0));
            sb.Append(" KB");
        }
        else if (byteCount > 0 && byteCount < 1024.0)
        {
            sb.Append(byteCount.ToString());
            sb.Append(" Bytes");
        }
        
        size = sb.ToString();
        return size;
    }
}
#endif
