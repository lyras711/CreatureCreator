using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudBun;

public class Bone : MonoBehaviour
{
    public Bone mirrorBone;
    private MudSphere mudSphere;

    private MudBox mudBox;

    public bool isSphere = true;
    private Renderer editRef;
    public Material selectedMat;
    public Material defaultMat;

    public float minBlend;
    public float maxBlend;
    public float minRadius;
    public float maxRadius;

    private float currBlend;
    private float currRadius;

    // Start is called before the first frame update
    void Start()
    {
        if (isSphere)
        {
            mudSphere = GetComponent<MudSphere>();
            currBlend = mudSphere.Blend;
            currRadius = mudSphere.Radius;
        }
        else
        {
            mudBox = GetComponent<MudBox>();
            currBlend = mudBox.Blend;
            currRadius = mudBox.Round;
        }
        editRef = transform.GetChild(0).GetComponent<Renderer>();
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

    public void SetBlend(float value, bool mirror)
    {
        if (value > minBlend && value < maxBlend)
        {
            if (isSphere)
            {
                mudSphere.Blend = value;
                currBlend = value;
                currRadius = mudSphere.Radius;
            }
            else
            {
                mudBox.Blend = value;
                currBlend = value;
                currRadius = mudBox.Round;
            }


            if (mirrorBone != null && mirror)
            {
                mirrorBone.SetBlend(value, false);
            }
        }
    }

    public void SetRadius(float value, bool mirror)
    {
        if (value > minRadius && value < maxRadius)
        {
            if (isSphere)
            {
                mudSphere.Radius = value;
            }
            else
            {
                mudBox.Round = value;
            }


            if(mirrorBone != null && mirror)
            {
                mirrorBone.SetRadius(value, false);
            }
        }
    }

    public float GetCurrentRadius()
    {
        return currRadius;
    }

    public float GetCurrentBlend()
    {
        return currBlend;
    }
}
