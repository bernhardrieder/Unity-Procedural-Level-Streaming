using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public struct LevelSegmentProperties
{
    public LevelSegmentProperties(LevelSegmentTypes type, Vector3 pos, Quaternion rot)
    {
        Type = type;
        Position = pos;
        Rotation = rot;
        DoorMapping = new Dictionary<int, LevelSegmentProperties>();
    }

    public LevelSegmentTypes Type;
    public Vector3 Position;
    public Quaternion Rotation;
    public Dictionary<int, LevelSegmentProperties> DoorMapping;
}
public class LevelGenerator : MonoBehaviour
{
    public GameObject[] LevelSegmentPrefabs;
    public int RandomSeed = 1234;
    public int LevelSegments = 100;

    public List<LevelSegmentProperties> GenerateLevel()
    {
        Random.InitState(RandomSeed);
        var root = createRandomSegment(Vector3.zero);

        //add root segment
        List<LevelSegmentProperties> segmentProperties = new List<LevelSegmentProperties> { new LevelSegmentProperties(root.Type, root.transform.position, root.transform.rotation) };
        List<LevelSegment> segmentInstances = new List<LevelSegment> { root };

        //loop until LevelSegments + 1 element, so that last created element gets his neighbours assigned!
        for (int i = 1; i <= LevelSegments; ++i)
        {
            int neighbourIndex = i - 1;

            //neighbour available?
            if (neighbourIndex >= segmentInstances.Count)
                break;

            //get neighbour and add new random segments on each door!
            var neighbour = segmentInstances[neighbourIndex];
            for (int doorIndex = 0; doorIndex < neighbour.Doors.Length; ++doorIndex)
            {
                var pos = neighbour.GetNeigbourPosition(doorIndex);
                bool continueLoop = false;

                //is another instance on this position?
                foreach (var instance in segmentInstances)
                {
                    if (instance.gameObject.transform.position == pos)
                    {
                        continueLoop = true;
                        segmentProperties[neighbourIndex].DoorMapping.Add(doorIndex, segmentProperties[segmentInstances.IndexOf(instance)]);
                        break;
                    }
                }

                //if another instance is on this pos for a new segment, decline it
                if (continueLoop || segmentInstances.Count >= LevelSegments)
                    continue;

                //if not, create new suitable segment
                var suitableSegment = createSuitableSegment(neighbour, pos);
                segmentInstances.Add(suitableSegment);
                segmentProperties.Add(new LevelSegmentProperties(suitableSegment.Type, suitableSegment.transform.position, suitableSegment.transform.rotation));
                segmentProperties[neighbourIndex].DoorMapping.Add(doorIndex, segmentProperties[segmentInstances.IndexOf(suitableSegment)]);
            }
        }
        foreach (var instance in segmentInstances)
            GameObject.DestroyImmediate(instance.gameObject);

        return segmentProperties;
    }

    LevelSegment createSuitableSegment(LevelSegment suitableFor, Vector3 newSegmentPos)
    {
        LevelSegment newSegment = null;
        do
        {
            if (newSegment != null)
            {
                GameObject.DestroyImmediate(newSegment.gameObject);
                newSegment = null;
            }
            newSegment = createRandomSegment(newSegmentPos);
        }
        while (!checkIfNeighbourDoor(suitableFor, newSegment));
        return newSegment;
    }

    bool checkIfNeighbourDoor(LevelSegment segment, LevelSegment other)
    {
        foreach (var segmentDoor in segment.Doors)
        {
            foreach (var otherDoor in other.Doors)
            {
                if (segmentDoor.position == otherDoor.position)
                    return true;
            }
        }
        return false;
    }

    LevelSegment createRandomSegment(Vector3 pos)
    {
        int randomSegment = Random.Range(0, LevelSegmentPrefabs.Length);
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 5) * 90, 0);
        var go = GameObject.Instantiate(LevelSegmentPrefabs[randomSegment], pos, randomRotation);
        return go.GetComponent<LevelSegment>();
    }
}