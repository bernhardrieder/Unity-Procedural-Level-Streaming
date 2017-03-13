using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct LevelSegmentMapping
{
    public LevelSegmentTypes Type;
    public SceneAsset Asset;
}
public class WorldManager : MonoBehaviour
{
    public LevelSegmentMapping[] LevelSegmentMapping;
    public LevelGenerator LevelGenerator;
    
    private List <Vector3> m_usedPositions;
    private List <LevelSegment> m_loadedLevelSegments;
    private Dictionary <LevelSegmentTypes, SceneAsset> m_levelSegmentMapping;
    private Dictionary <LevelSegmentTypes, long> m_levelSegmentTypeCounter;
    private GameSystem m_gameSystem = null;
    void Start()
    {
        m_gameSystem = GameObject.FindObjectOfType<GameSystem>();
        initalizeLevelSegmentListsAndDictionaries();

        //create world root
        var worldRoot = LevelGenerator.GenerateLevel()[0];
        StartCoroutine(loadWorldRootSegment(worldRoot));
    }

    void initalizeLevelSegmentListsAndDictionaries()
    {
        m_usedPositions = new List<Vector3>();
        m_loadedLevelSegments = new List<LevelSegment>();
        m_levelSegmentMapping = new Dictionary <LevelSegmentTypes, SceneAsset>();
        m_levelSegmentTypeCounter = new Dictionary <LevelSegmentTypes, long>();
        foreach (var mapping in LevelSegmentMapping)
        {
            m_levelSegmentMapping.Add(mapping.Type, mapping.Asset);
            m_levelSegmentTypeCounter.Add(mapping.Type, 0);
        }
    }

    private IEnumerator loadWorldRootSegment(LevelSegmentProperties rootProps)
    {
        yield return loadLevelSegment(rootProps);
        Scene loadedScene = SceneManager.GetSceneByName(m_levelSegmentMapping[rootProps.Type].name + (m_levelSegmentTypeCounter[rootProps.Type]-1));
        while (!loadedScene.isLoaded)
            yield return new WaitForEndOfFrame();
        LevelSegment rootSegment = loadedScene.GetRootGameObjects()[0].GetComponent<LevelSegment>();
        yield return loadLevelSegments(rootSegment, rootProps.DoorMapping.Values.ToArray());
    }

    private IEnumerator loadLevelSegments(LevelSegment root, params LevelSegmentProperties[] segmentProps)
    {
        foreach (var activeSegment in m_loadedLevelSegments)
            activeSegment.IsNeeded = false;
        
        if (root != null)
            root.IsNeeded = true;

        foreach (var props in segmentProps)
            yield return loadLevelSegment(props);

        unloadUnneededSegments();
    }

    private IEnumerator loadLevelSegment(LevelSegmentProperties segmentProps)
    {
        //check if there is already an segment on this position
        if (m_usedPositions.Contains(segmentProps.Position))
        {
            //if yes then mark it as needed
            m_loadedLevelSegments[m_usedPositions.IndexOf(segmentProps.Position)].IsNeeded = true;
            yield break;
        }

        //if there isnt an existing segment on this position then load the related scene
        string sceneName = m_levelSegmentMapping[segmentProps.Type].name;
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (!async.isDone)
            yield return new WaitForEndOfFrame();
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        while (!loadedScene.isLoaded)
            yield return new WaitForEndOfFrame();

        //create a new scene to ensure correct scene unloading
        string newSceneName = sceneName + m_levelSegmentTypeCounter[segmentProps.Type]++;
        Scene newScene = SceneManager.CreateScene(newSceneName);
        SceneManager.MergeScenes(loadedScene, newScene);
        m_gameSystem.AddAdditionalMainScene(newScene);

        //get segment and set its properties
        LevelSegment segment = newScene.GetRootGameObjects()[0].GetComponent<LevelSegment>();
        m_usedPositions.Add(segmentProps.Position);
        m_loadedLevelSegments.Add(segment);
        segment.transform.position = segmentProps.Position;
        segment.transform.rotation = segmentProps.Rotation;
        segment.IsNeeded = true;
        segment.OnTriggerEnterAction = (LevelSegment self) =>
                                       {
                                           StartCoroutine(loadLevelSegments(self,
                                               segmentProps.DoorMapping.Values.ToArray()));
                                       };
        segment.gameObject.SetActive(true);
    }

    private void unloadUnneededSegments()
    {
        List <LevelSegment> unloadSegments = new List <LevelSegment>();

        foreach (var loadedSegment in m_loadedLevelSegments)
        {
            if (loadedSegment.IsNeeded)
                continue;
            unloadSegments.Add(loadedSegment);
        }

        foreach (var seg in unloadSegments)
        {
            m_gameSystem.RemoveAdditionalMainScene(seg.gameObject.scene);
            m_usedPositions.Remove(seg.transform.position);
            m_loadedLevelSegments.Remove(seg);
            SceneManager.UnloadSceneAsync(seg.gameObject.scene.name);
        }
    }
}
