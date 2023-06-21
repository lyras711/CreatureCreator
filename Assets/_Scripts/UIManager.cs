using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MudBun;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public CamController camController;
    public Slider blendSlider;
    public Slider radiusSlider;
    public Slider pivotSlider;

    public Text selectedBoneNameText;

    public Transform camPivot;

    private Bone bone;
    private MudSphere deformSphere;

    public GameObject fillPanel;
    public GameObject deformPanel;
    public GameObject rotateButton;
    public GameObject boneColoring;

    [Header("Nav")]
    public Button meshButton;
    public Button paintButton;
    public Button addButton;

    [Header("Editing")]
    public GameObject blobDestroyButton;
    public GameObject blobConfirmButton;
    public Image toggleBG;
    public Text blobsRemainingText;
    public Color deformColor;
    public Color destroyColor;

    bool deform = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ToggleDeformDestroy()
    {
        deform = !deform;

        if(deform)
        {
            blobDestroyButton.SetActive(false);
            blobConfirmButton.SetActive(true);
            toggleBG.color = deformColor;
        }
        else
        {
            blobDestroyButton.SetActive(true);
            blobConfirmButton.SetActive(false);
            toggleBG.color = destroyColor;
        }
    }

    public bool InAddMode()
    {
        return deform;
    }

    public void SetSelectedBone(Bone bone) 
    {
        this.bone = bone;

        selectedBoneNameText.text = bone.gameObject.name;

        radiusSlider.value = bone.GetCurrentRadius();

        blendSlider.gameObject.SetActive(true);
        radiusSlider.gameObject.SetActive(true);
        rotateButton.SetActive(true);
    }

    public void TriggerBoneColouringUI(bool active)
    {
        boneColoring.SetActive(active);
    }

    public void EnableNavButtons()
    {
        meshButton.interactable = true;
        paintButton.interactable = true;
        addButton.interactable = true;
    }

    public void SetBlend(float value)
    {
        bone.SetBlend(value, true);
    }

    public void SetDeformSphere(MudSphere sphere)
    {
        deformSphere = sphere;
    }

    public void SetRadius(float value)
    {
        bone.SetRadius(value, true);
    }

    public void SetDeformRadius(float value)
    {
        deformSphere.Radius = value;
    }

    public void SetDeformBlend(float value)
    {
        deformSphere.Blend = value;
    }

    public void SetCamPivot(float value)
    {
        var rot = camPivot.rotation;
        Vector3 euler = rot.eulerAngles;
        euler.y = value;
        rot.eulerAngles = euler;
        camPivot.rotation = rot;
    }

    public void FillPanel()
    {
        fillPanel.SetActive(true);
        deformPanel.SetActive(false);

        DeselectBone();
    }

    public void DeformPanel()
    {
        if(bone!= null)
        {
            camController.SetToEditMode();
            //camController.GoToBone(bone.transform);
        }

        fillPanel.SetActive(false);
        deformPanel.SetActive(true);
    }

    public void SetBlobsRemaining(int blobs)
    {
        blobsRemainingText.text = "Blobs Remaining: " + blobs;
    }

    public void DeselectBone()
    {
        if(bone != null)
        {
            bone.TriggerBoneSelection(false);
            bone = null;
        }
        camController.GoBackToTransform();
    }
}
