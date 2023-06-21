using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBone : MonoBehaviour
{
    public Renderer editRef;
    public Material selectedMat;
    public Material defaultMat;

    public int size = 1;
    public bool canPlace = false;

    public GameObject mirrorBone;

    public void SetMirrorBone(GameObject mirrorBone)
    {
        this.mirrorBone = mirrorBone;
    }

    public void TriggerBoneSelection(bool active)
    {
        if (active)
        {
            editRef.material = selectedMat;
        }
        else
            editRef.material = defaultMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Bone>() || other.GetComponent<NewBone>())
        {
            canPlace = true;
        }
    }
}
