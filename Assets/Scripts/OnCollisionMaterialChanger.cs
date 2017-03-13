using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionMaterialChanger : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public Material[] Materials;
    public Rigidbody Rigidbody;
    public AudioSource HitSound;

    public int CurrentMaterialIndex
    {
        get { return m_currentMaterialIndex; }
        set
        {
            m_currentMaterialIndex = value;
            MeshRenderer.material = Materials[m_currentMaterialIndex];
        }
    }

    private int m_currentMaterialIndex = 3;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            CurrentMaterialIndex = CurrentMaterialIndex + 1 < Materials.Length ? ++CurrentMaterialIndex : 0;
            Rigidbody.AddForce(collision.gameObject.transform.forward * 1000, ForceMode.Force);
            HitSound.PlayOneShot(HitSound.clip);
        }
    }
}
