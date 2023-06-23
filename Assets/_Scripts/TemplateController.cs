using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudBun;

public class TemplateController : MonoBehaviour
{
    public static TemplateController instance;

    UtilityFunctions utils;
    UIManager uiManager;

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

    public Transform[] animRigParts;
    public GameObject skeletonRig;

    [Header("Mud References")]
    public Transform initialReference;
    public Transform[] legsParts;
    public MudRenderer[] legRenderers;
    public Transform curParent;

    public int blobsRemaining = 20;

    private Transform spawnedBlob;

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

        uiManager = UIManager.instance;
        uiManager.SetBlobsRemaining(blobsRemaining);
    }

    public void CompleteMesh()
    {
        for (int i = 0; i < legRenderers.Length; i++)
        {
            legRenderers[i].GetComponent<MudRenderer>().LockMesh(false, true);
        }
        AttatchToRig();
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit[] hitInfo1;

            hitInfo1 = Physics.RaycastAll(ray, 100f);

            for (int i = 0; i < hitInfo1.Length; i++)
            {
                RaycastHit hit = hitInfo1[i];

                if (hit.collider.GetComponent<NewBone>())
                {

                    if (selectedNewBone != hit.collider.GetComponent<NewBone>())
                    {
                        selectedNewBone = hit.collider.GetComponent<NewBone>();
                        selectedNewBone.TriggerBoneSelection(true);
                    }

                    if (!uiManager.InAddMode())
                    {
                        DestroyNewBone(selectedNewBone);
                    }

                    if (deformObjTransform == null)
                    {
                        deformObjTransform = hit.collider.transform;

                        if (selectedNewBone.mirrorBone != null)
                        {
                            deformObjTransformMirror = selectedNewBone.mirrorBone.transform;
                            Destroy(deformObjTransformMirror.gameObject.GetComponent<SphereCollider>());
                            Destroy(deformObjTransformMirror.GetChild(0).gameObject.GetComponent<SphereCollider>());
                            
                            deformObjTransform.parent = blobParent;

                            BoneSetter boneSetter = blobParent.GetComponent<BoneSetter>();
                            if (boneSetter.mirrorParent != null)
                                mirrorParent = boneSetter.mirrorParent;

                            float xPos;

                            if (deformObjTransform.localPosition.x > 0)
                                xPos = -deformObjTransform.localPosition.x;
                            else
                                xPos = Mathf.Abs(deformObjTransform.localPosition.x);

                            deformObjTransformMirror.parent = mirrorParent;
                            deformObjTransformMirror.localPosition = new Vector3(xPos, deformObjTransform.localPosition.y, deformObjTransform.localPosition.z);
                        }

                        Destroy(deformObjTransform.gameObject.GetComponent<SphereCollider>());
                        Destroy(deformObjTransform.GetChild(0).gameObject.GetComponent<SphereCollider>());
                    }
                }
            }
        }

        if (deformObjTransform != null && !utils.IsPointerOverUIObject())
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

                    if (selectedNewBone.mirrorBone != null)
                    {
                        deformObjTransformMirror = selectedNewBone.mirrorBone.transform;
                        if (deformObjTransformMirror.parent != mirrorParent)
                            deformObjTransformMirror.parent = mirrorParent;
                    }

                    float distanceToMiddle = Vector3.Distance(hitInfo.point, new Vector3(0, hitInfo.point.y, 0));

                    if (distanceToMiddle > 0.6f)
                    {
                        if(deformObjTransformMirror == null)
                        {
                            CreateMirrorBlob();
                        }
                        else
                        {
                            float xPos;

                            if (deformObjTransform.localPosition.x > 0)
                                xPos = -deformObjTransform.localPosition.x;
                            else
                                xPos = Mathf.Abs(deformObjTransform.localPosition.x);

                            deformObjTransformMirror.localPosition = new Vector3(xPos, deformObjTransform.localPosition.y, deformObjTransform.localPosition.z);
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
        uiManager.EnableNavButtons();
    }


    float blobSize;
    int blobsToRemove = 1;
    bool newBlob = false;
    public void CreateDeformSphere(float size)
    {
        if(blobToDestroy != null)
        {
            if(blobToDestroy.transform.parent == initialReference)
            {
                DestroyNewBone(blobToDestroy.GetComponent<NewBone>());
            }
        }
        blobSize = size;

        if (!HaveBlobs())
            return;

        newBlob = true;
        if (deformObjTransform != null)
        {
            DestroyDeformSphere();
        }

        //TriggerNewBonesVisible(false);

        Vector3 position = new Vector3(0, 1, 0);

        deformObjTransform = Instantiate(deformObj, position, Quaternion.identity, initialReference).transform;
        deformObjTransform.GetComponent<MudSphere>().Radius = size;
        deformObjTransform.GetComponent<NewBone>().size = blobsToRemove;
        deformObjTransform.gameObject.SetActive(true);

        uiManager.SetDeformSphere(deformObjTransform.GetComponent<MudSphere>());
    }

    void CreateMirrorBlob()
    {
        float radius = deformObjTransform.GetComponent<MudSphere>().Radius;
        int size = deformObjTransform.GetComponent<NewBone>().size;
        deformObjTransformMirror = Instantiate(deformObj, deformObjTransform.position, Quaternion.identity, initialReference).transform;
        deformObjTransformMirror.GetComponent<MudSphere>().Radius = radius;
        deformObjTransformMirror.GetComponent<NewBone>().size = size;
        deformObjTransform.GetComponent<NewBone>().SetMirrorBone(deformObjTransformMirror.gameObject);
        deformObjTransformMirror.GetComponent<NewBone>().SetMirrorBone(deformObjTransform.gameObject);
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
        if (newBlob)
        {
            blobsRemaining -= blobsToRemove;
            newBlob = false;
        }

        deformObjTransform.GetComponent<NewBone>().TriggerBoneSelection(false);

        deformObjTransform.GetChild(0).gameObject.SetActive(false);
        deformObjTransform.GetChild(0).gameObject.AddComponent<SphereCollider>();
        deformObjTransform.gameObject.AddComponent<SphereCollider>();
        deformObjTransform.GetComponent<SphereCollider>().isTrigger = true;

        if (!newBones.Contains(deformObjTransform.GetChild(0)))
            newBones.Add(deformObjTransform.GetChild(0));
        deformObjTransform = null;


        if (deformObjTransformMirror != null)
        {
            deformObjTransformMirror.GetChild(0).gameObject.SetActive(false);
            deformObjTransformMirror.GetChild(0).gameObject.AddComponent<SphereCollider>();
            deformObjTransformMirror.gameObject.AddComponent<SphereCollider>();
            deformObjTransformMirror.GetComponent<SphereCollider>().isTrigger = true;

            if (!newBones.Contains(deformObjTransformMirror.GetChild(0)))
                newBones.Add(deformObjTransformMirror.GetChild(0));
            deformObjTransformMirror = null;
        }

        TriggerNewBonesVisible(true);
        uiManager.SetBlobsRemaining(blobsRemaining);
    }

    public void DestroyDeformSphere()
    {
        if (blobToDestroy == null)
            return;

        if (!newBlob)
        {
            blobsRemaining++;
            uiManager.SetBlobsRemaining(blobsRemaining);
        }
        else
            newBlob = false;
        Destroy(blobToDestroy);
        blobToDestroy = null;
        DestroyMirrorSphere();
        TriggerNewBonesVisible(true);
        //UIManager.instance.TriggerBlobButtons(false);
    }

    void DestroyMirrorSphere()
    {
        if(deformObjTransformMirror != null)
        {
            Destroy(deformObjTransformMirror.gameObject);
            deformObjTransformMirror = null;
        }
    }

    bool CheckIfCanPlace()
    {
        SphereCollider col = deformObjTransform.GetComponent<SphereCollider>();
        NewBone bone = deformObjTransform.GetComponent<NewBone>();

        if (blobSize == 0.2f)
        {
            col.radius = 0.75f;
            if (bone.canPlace)
            {
                return true;
            }
        }
        if (blobSize == 0.5f)
        {
            col.radius = 0.1f;
            if (bone.canPlace)
            {
                return true;
            }
        }
        if (blobSize == 0.8f)
        {
            col.radius = 0.35f;
            if (bone.canPlace)
            {
                return true;
            }
        }

        return false;
    }

    void DestroyNewBone(NewBone bone)
    {
        blobsRemaining += bone.size;
        uiManager.SetBlobsRemaining(blobsRemaining);

        newBones.Remove(bone.transform);
        if(bone.mirrorBone != null)
        {
            newBones.Remove(bone.mirrorBone.transform);
            Destroy(bone.mirrorBone);
        }

        Destroy(bone.gameObject);
    }
}
