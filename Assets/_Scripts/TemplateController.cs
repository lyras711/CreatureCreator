using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MudBun;

public class TemplateController : MonoBehaviour
{
    public static TemplateController instance;

    UtilityFunctions utils;

    public Transform mudRenderer;
    public GameObject mudSphere;

    public Transform template;
    public List<Transform> bones = new List<Transform>();
    public List<Transform> newBones = new List<Transform>();

    public GameObject colours;

    public Bone selectedBone;
    public NewBone selectedNewBone;

    bool itemActive = false;
    public GameObject objRef;

    GameObject itemObj1;
    GameObject itemObj2;

    public GameObject deformObj;
    private Transform deformObjTransform;

    bool colouringActive = false;

    public Transform[] animRigParts;
    public GameObject skeletonRig;

    [Header("Mud References")]
    public Transform[] legsParts;
    public MudRenderer[] legRenderers;
    public Transform curParent;

    public GameObject pelvisObj;

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
    }

    void SpawnNewBones()
    {
        for (int i = 0; i < bones.Count; i++)
        {
            if (bones[i].GetComponent<MeshRenderer>())
            {
                newBones.Add(bones[i]);
            }
        }

        for (int i = 0; i < bones.Count; i++)
        {
            Instantiate(mudSphere, bones[i].position, bones[i].rotation, mudRenderer);
        }

        /*        foreach (Transform item in template)
                {
                    Instantiate(mudSphere, item.position, item.rotation, mudRenderer);
                }*/

        template.gameObject.SetActive(false);
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
                        deformObjTransform.parent = closestBone.parent;
                    }
                }
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


    public void CreateDeformSphere()
    {
        if(deformObjTransform != null)
        {
            DestroyDeformSphere();
        }

        Vector3 position = Vector3.zero;
        TriggerNewBonesVisible(false);
        if(selectedBone == null)
        {
            position = new Vector3(0, 1, 0);
        }
        else
            position = selectedBone.transform.position;
        deformObjTransform = Instantiate(deformObj, position, Quaternion.identity, legsParts[0]).transform;
        deformObjTransform.gameObject.SetActive(true);

        UIManager.instance.SetDeformSphere(deformObjTransform.GetComponent<MudBun.MudSphere>());
        
        //TOdo set ui
    }

    public void ConfirmDeform()
    {
        if (deformObjTransform == null)
            return;

        deformObjTransform.GetChild(0).gameObject.SetActive(false);
        deformObjTransform.GetChild(0).gameObject.AddComponent<SphereCollider>();
        deformObjTransform.gameObject.AddComponent<SphereCollider>();
        deformObjTransform.GetComponent<SphereCollider>().isTrigger = true;
        newBones.Add(deformObjTransform.GetChild(0));
        deformObjTransform = null;
        TriggerNewBonesVisible(true);
        //Set UI
    }

    public void DestroyDeformSphere()
    {
        if (deformObjTransform == null)
            return;
        Destroy(deformObjTransform.gameObject);
        deformObjTransform = null;
        TriggerNewBonesVisible(true);
        //TODO: set ui
    }
}
