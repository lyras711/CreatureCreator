using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CamController : MonoBehaviour
{
    private Vector3 defPos = new Vector3();
    private Quaternion defRot = new Quaternion();

    public GameObject deformLayer1;
    public GameObject deformLayer2;
    public GameObject deformLayer3;
    public GameObject deformLayer4;

    bool deformlayer1 = true;
    bool middleLayer = false;

    private void Start()
    {
        defPos = transform.position;
        defRot = transform.rotation;
    }

    public void GoToBone(Transform boneTransform)
    {
        var dist = transform.position - boneTransform.position;
        Vector3 normalized = dist.normalized * 4;
 
        transform.DOMove(boneTransform.position + normalized, 1f);
        transform.parent.DORotate(Vector3.up * 180, 1f);
        transform.DOLookAt(boneTransform.position, 1f);
    }

    public void SetToEditMode()
    {
        Vector3 newRot = new Vector3(0, 180, 0);
        transform.parent.DORotate(newRot, 1f);
    }

    public void GoBackToTransform()
    {
        transform.DOMove(defPos, 1f);
        transform.DORotateQuaternion(defRot, 1f);
    }

    bool rotating = false;
    public void RotateCam()
    {
        if (rotating)
            return;

        rotating = true;
        float curRot = transform.parent.rotation.eulerAngles.y;
        Vector3 newRot = new Vector3(0, curRot + 90, 0);
        transform.parent.DORotate(newRot, 0.5f).OnComplete(() => rotating = false);

        deformlayer1 = !deformlayer1;
        UIManager.instance.TriggerChangeLayersButton(!deformlayer1);

        if (deformlayer1)
        {
            deformLayer1.SetActive(true);
            deformLayer2.SetActive(false);
            deformLayer3.SetActive(false);
            deformLayer4.SetActive(false);
        }
        else
        {
            deformLayer1.SetActive(false);
            deformLayer2.SetActive(true);
            deformLayer3.SetActive(true);
            deformLayer4.SetActive(false);
        }
    }

    public void ChangeLayer()
    {
        middleLayer = !middleLayer;

        if(middleLayer)
        {
            deformLayer1.SetActive(false);
            deformLayer2.SetActive(false);
            deformLayer3.SetActive(false);
            deformLayer4.SetActive(true);
        }
        else
        {
            deformLayer1.SetActive(false);
            deformLayer2.SetActive(true);
            deformLayer3.SetActive(true);
            deformLayer4.SetActive(false);
        }
    }
}
