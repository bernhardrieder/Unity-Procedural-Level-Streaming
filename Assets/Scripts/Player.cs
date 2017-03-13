using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
    public Rigidbody Body;

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetRotation(Quaternion quat)
    {
        this.gameObject.transform.rotation = quat;
    }

    void OnEnable()
    {
        Body.velocity = Vector3.zero;
    }
}
