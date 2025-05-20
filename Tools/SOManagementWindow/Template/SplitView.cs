#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;

public class SplitView : TwoPaneSplitView {
    
    [Obsolete("SplitView is Obsolete")]
    public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
}
#endif
