#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public static class TransformDataHolder
{
    public static TransformData data = null;
    public static void CopyData(TransformData copy)
    {
        data = copy;
    }

    public static TransformData PasteData()
    {
        if (data == null) return data;

        TransformData cloned = data;
        data = null;
        return cloned;
    }
}
#endif
