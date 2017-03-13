using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
    
public enum LevelSegmentTypes
{
    Corridor,
    Cross,
    Curve,
    StartEnd,
    T,
    None
}

public class LevelSegment : MonoBehaviour
{
    public LevelSegmentTypes Type;
    public Transform[] Doors;
    public int SegmentWidth = 9;
    
    public System.Action<LevelSegment> OnTriggerEnterAction = null;
    [HideInInspector] public bool IsNeeded = false;

    public Vector3 GetNeigbourPosition(int doorIndex)
    {
        var door = Doors[doorIndex];
        var vectorToDoor = door.position - gameObject.transform.position;
        return gameObject.transform.position + vectorToDoor.normalized * SegmentWidth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            //load neighbours and unload unneeded segments;
            if (OnTriggerEnterAction != null)
                OnTriggerEnterAction(this);
        }
    }
}