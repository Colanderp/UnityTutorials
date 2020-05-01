using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
public class PrefabHandler
{
    string pathToMyPrefab;
    Transform transform;

    [ExecuteInEditMode]
    public PrefabHandler(Transform t, string s)
    {
        pathToMyPrefab = s;
        transform = t;
    }

    [ExecuteInEditMode]
    public void ChangePrefab(UnityAction change)
    {
        GameObject rootGO = PrefabUtility.LoadPrefabContents(pathToMyPrefab) as GameObject;
        if (PrefabUtility.IsPartOfPrefabInstance(transform))
            PrefabUtility.UnpackPrefabInstance(transform.gameObject,
                  PrefabUnpackMode.Completely,
                  InteractionMode.UserAction);

        change.Invoke();

        PrefabUtility.SaveAsPrefabAssetAndConnect(transform.gameObject, pathToMyPrefab, InteractionMode.UserAction);
        PrefabUtility.UnloadPrefabContents(rootGO);
    }

    [ExecuteInEditMode]
    public void RecreatePrefab()
    {
        if (transform == null) return;
        GameObject rootGO = PrefabUtility.LoadPrefabContents(pathToMyPrefab) as GameObject;
        GameObject recreated = GameObject.Instantiate(rootGO); recreated.transform.name = transform.name;

        PrefabUtility.SaveAsPrefabAssetAndConnect(recreated, pathToMyPrefab, InteractionMode.UserAction);
        PrefabUtility.UnloadPrefabContents(rootGO);
        GameObject.DestroyImmediate(transform.gameObject);
    }
}
#endif