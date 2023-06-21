using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudBun;

public class TemplateController : MonoBehaviour
{
    public static TemplateController instance;

    UtilityFunctions utils;

    public List<Transform> bones = new List<Transform>();
    public List<Transform> newBones = new List<Transform>();

    private Bone selectedBone;
    private NewBone selectedNewBone;

    bool itemActive = false;

    public GameObject deformObj;
    private Transform deformObjTransform;
    private Transform deformObjTransformMirror;
    private Transform blobParent;
    private Transform mirrorParent;

    bool colouringActive = false;

    public Transform[] animRigParts;
    public GameObject skeletonRig;

    [Header("Mud References")]
    public Transform[] legsParts;
    public MudRenderer[] legRenderers;
    public Transform curParent;

    public int blobsRemaining = 20;

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

    // Start is called before the first frame update
    void Start()
    {
        utils = UtilityFunctions.instance;

        UIManager.instance.SetBlobsRemaining(blobsRemaining);
    }

    public void CompleteMesh()
    {
        for (int i = 0; i < legRenderers.Length; i++)
        {
            legRenderers[i].GetComponent<MudRenderer>().LockMesh(false, false);
        }
        //AttatchToRig();
    }

    void AttatchToRig()
    {
        skeletonRig.SetActive(true);
        for (int i = 0; i < legRenderers.Length; i++)
        {
            legRenderers[i].GetComponent<BoneSetter>().SetParent();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray1 = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo1;

            if(Physics.Raycast(ray1, out hitInfo1, 100f))
            {
                if(hitInfo1.collider.GetComponent<Bone>())
                {
                    if(selectedBone != hitInfo1.collider.GetComponent<Bone>())
                    {
                        if(selectedBone != null)
                        {
                            selectedBone.TriggerBoneSelection(false);
                            selectedBone = null;
                        }
                        if (selectedNewBone != null)
                        {
                            selectedNewBone.TriggerBoneSelection(false);
                            selectedNewBone = null;
                        }
                        if (colouringActive)
                            UIManager.instance.TriggerBoneColouringUI(true);
                        selectedBone = hitInfo1.collider.GetComponent<Bone>();
                        selectedBone.TriggerBoneSelection(true);
                        UIManager.instance.SetSelectedBone(selectedBone);
                    }
                }
                else if (hitInfo1.collider.GetComponent<NewBone>())
                {
                    if (selectedNewBone != hitInfo1.collider.GetComponent<NewBone>())
                    {
                        if (selectedNewBone != null)
                        {
                            selectedNewBone.TriggerBoneSelection(false);
                            selectedNewBone = null;
                        }
                        if (selectedBone != null)
                        {
                            selectedBone.TriggerBoneSelection(false);
                            selectedBone = null;
                        }
                        if (colouringActive)
                            UIManager.instance.TriggerBoneColouringUI(true);
                        selectedNewBone = hitInfo1.collider.GetComponent<NewBone>();
                        selectedNewBone.TriggerBoneSelection(true);
                        //UIManager.instance.SetSelectedBone(selectedBone);
                    }
                }

                if (hitInfo1.collider.GetComponent<NewBone>() && deformObjTransform == null)
                {
                    deformObjTransform = hitInfo1.collider.transform;

                    Destroy(deformObjTransform.gameObject.GetComponent<SphereCollider>());
                    Destroy(deformObjTransform.GetChild(0).gameObject.GetComponent<SphereCollider>());
                }
            }
        }

        if (deformObjTransform != null && !utils.IsPointerOverUIObject() && !colouringActive)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100f))
            {
                if (Input.GetMouseButton(0))
                {
                    deformObjTransform.position = hitInfo.point;

                    Transform closestBone = GetClosestBone(deformObjTransform, legsParts);
                    curParent = closestBone.parent;
                    if (deformObjTransform.parent != closestBone.parent)
                    {
                        blobParent = closestBone.parent;
                        deformObjTransform.parent = blobParent;

                        BoneSetter boneSetter = blobParent.GetComponent<BoneSetter>();
                        if (boneSetter.mirrorParent != null)
                            mirrorParent = boneSetter.mirrorParent;
                    }

                    float distanceToMiddle = Vector3.Distance(hitInfo.point, new Vector3(0, hitInfo.point.y, 0));

                    if (distanceToMiddle > 0.5f)
                    {
                        if(deformObjTransformMirror == null)
                        {
                            CreateMirrorBlob();
                        }
                        else
                        {
                            if(deformObjTransformMirror.parent != mirrorParent)
                                deformObjTransformMirror.parent = mirrorParent;

                            float xPos;
                            float zPos;

                            if (deformObjTransform.localPosition.x > 0)
                                xPos = -deformObjTransform.localPosition.x;
                            else
                                xPos = Mathf.Abs(deformObjTransform.localPosition.x);

                            if (deformObjTransform.localPosition.z > 0)
                                zPos = -deformObjTransform.localPosition.z;
                            else
                                zPos = Mathf.Abs(deformObjTransform.localPosition.z);

                            deformObjTransformMirror.localPosition = new Vector3(xPos, deformObjTransform.localPosition.y, zPos);
                        }
                    }
                    else
                    {
                        if(deformObjTransformMirror != null)
                        {
                            DestroyMirrorSphere();
                        }
                    }

                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (deformObjTransform != null)
            {
                ConfirmDeform();
            }
        }
    }

    Transform GetClosestBone(Transform objPos, Transform[] bones)
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = objPos.position;
        foreach (Transform t in bones)
        {
            float dist = Vector3.Distance(t.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }



    public void SetBoneColour(Color color)
    {
        if(selectedBone != null)
        {
            selectedBone.GetComponent<MudBun.MudMaterial>().Color = color;
        }
        if (selectedNewBone != null)
        {
            selectedNewBone.GetComponent<MudBun.MudMaterial>().Color = color;
        }
    }
    

    public void TriggerItem()
    {
        itemActive = !itemActive;
    }

    public void TriggerBonesVisible(bool active)
    {
        for (int i = 0; i < bones.Count; i++)
        {
            bones[i].gameObject.SetActive(active);
        }
    }

    public void TriggerNewBonesVisible(bool active)
    {
        if(newBones.Count > 0)
        {
            for (int i = 0; i < newBones.Count; i++)
            {
                if (newBones[i] != null)
                    newBones[i].gameObject.SetActive(active);
                else
                    newBones.RemoveAt(i);
            }
        }
    }

    public void TriggerColoursVisible(bool active)
    {
        colouringActive = active;

        //TriggerBonesVisible(active);
        TriggerNewBonesVisible(active);
    }

    public void SetBodyColour(Color color)
    {
        if(newBones.Count > 0)
        {
            for (int i = 0; i < newBones.Count; i++)
            {
                newBones[i].transform.parent.GetComponent<MudMaterial>().Color = color;
            }
        }

        for (int x = 0; x < bones.Count; x++)
        {
            bones[x].GetComponent<MudMaterial>().Color = color;
        }
    }

    public void Enable2LegTemplate()
    {
        //mudRenderer.gameObject.SetActive(true);
        UIManager.instance.EnableNavButtons();
    }


    float blobSize;
    int blobsToRemove = 1;
    bool newBlob = false;
    public void CreateDeformSphere(float size)
    {
        blobSize = size;

        if (!HaveBlobs())
            return;

        newBlob = true;
        if (deformObjTransform != null)
        {
            DestroyDeformSphere();
        }

        Vector3 position = Vector3.zero;
        TriggerNewBonesVisible(false);

        position = new Vector3(0, 1, 0);

        deformObjTransform = Instantiate(deformObj, position, Quaternion.identity, legsParts[0].parent).transform;
        deformObjTransform.GetComponent<MudSphere>().Radius = size;
        deformObjTransform.gameObject.SetActive(true);

        UIManager.instance.SetDeformSphere(deformObjTransform.GetComponent<MudSphere>());
        UIManager.instance.TriggerBlobButtons(true);
    }

    void CreateMirrorBlob()
    {
        deformObjTransformMirror = Instantiate(deformObj, deformObjTransform.position, Quaternion.identity, legsParts[0]).transform;
        deformObjTransformMirror.GetComponent<MudSphere>().Radius = blobSize;
        deformObjTransformMirror.gameObject.SetActive(true);

        float xPos;

        if (deformObjTransform.localPosition.x > 0)
            xPos = -deformObjTransform.localPosition.x;
        else
            xPos = Mathf.Abs(deformObjTransform.localPosition.x);

        deformObjTransformMirror.localPosition = new Vector3(xPos, deformObjTransform.localPosition.y, deformObjTransform.localPosition.z);
    }

    bool HaveBlobs()
    {
        if (blobSize == 0.2f)
        {
            if (blobsRemaining > 0)
            {
                blobsToRemove = 1;
                return true;
            }
        }
        if (blobSize == 0.5f)
        {
            if (blobsRemaining > 1)
            {
                blobsToRemove = 2;
                return true;
            }
        }
        if (blobSize == 0.8f)
        {
            if (blobsRemaining > 2)
            {
                blobsToRemove = 3;
                return true;
            }
        }

        return false;
    }


    GameObject blobToDestroy;
    public void ConfirmDeform()
    {
        if (deformObjTransform == null)
            return;

        blobToDestroy = deformObjTransform.gameObject;
        newBlob = false;
        blobsRemaining -= blobsToRemove;
        deformObjTransform.GetChild(0).gameObject.SetActive(false);
        deformObjTransform.GetChild(0).gameObject.AddComponent<SphereCollider>();
        deformObjTransform.gameObject.AddComponent<SphereCollider>();
        deformObjTransform.GetComponent<SphereCollider>().isTrigger = true;

        if(!newBones.Contains(deformObjTransform.GetChild(0)))
            newBones.Add(deformObjTransform.GetChild(0));
        deformObjTransform = null;
        TriggerNewBonesVisible(true);
        //UIManager.instance.TriggerBlobButtons(false);
        UIManager.instance.SetBlobsRemaining(blobsRemaining);
    }

    public void DestroyDeformSphere()
    {
        if (blobToDestroy == null)
            return;

        if (!newBlob)
        {
            blobsRemaining++;
            UIManager.instance.SetBlobsRemaining(blobsRemaining);
        }
        else
            newBlob = false;
        Destroy(blobToDestroy);
        blobToDestroy = null;
        TriggerNewBonesVisible(true);
        //UIManager.instance.TriggerBlobButtons(false);
    }

    void DestroyMirrorSphere()
    {
        Destroy(deformObjTransformMirror.gameObject);
        deformObjTransformMirror = null;
    }
}
