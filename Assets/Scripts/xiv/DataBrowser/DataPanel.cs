using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR.Interaction.Toolkit;

public class DataPanel : MonoBehaviour
{
    public Text Title { get; private set; }
    private Text SiteIndicator;
    public List<Data> Datas { get; private set; }
    public DataContainer[] DataContainers { get; private set; }
    private int MaxSites;
    private Button NextSite;
    private Button PreviousSite;
    public Button ParentDir { get; private set; }
    public Button Root { get; private set; }
    public DataBrowser Browser { get; private set; }

    private int _containerPointer;
    public int ContainerPointer
    {
        get { return _containerPointer; }
        set
        {
            SetComponentStatus(Datas != null);
            if (Datas == null) return;
            _containerPointer = Mathf.Max(0, Mathf.Min(value, (Datas.Count / DataContainers.Length) * DataContainers.Length));
            ActualizeDataContainers();
            ActualizeSiteVariables();
        }
    }

    private bool _baseInit;
    private void BaseInit()
    {
        Browser = transform.parent.GetComponent<DataBrowser>();
        Title = transform.Find("Title").GetComponent<Text>();
        SiteIndicator = transform.Find("SiteIndicator").GetComponent<Text>();
        DataContainers = GetComponentsInChildren<DataContainer>();
        PreviousSite = transform.Find("ButtonPrevious").GetComponent<Button>();
        PreviousSite.onClick.AddListener(PreviousClick);
        NextSite = transform.Find("ButtonNext").GetComponent<Button>();
        NextSite.onClick.AddListener(NextClick);
        ParentDir = transform.Find("ButtonParent").GetComponent<Button>();
        Root = transform.Find("ButtonRoot").GetComponent<Button>();

        _baseInit = true;
    }

    public void Init(string title = null, IEnumerable<Data> datas = null)
    {
        if (!_baseInit) BaseInit();
        Datas = (datas == null) ? null : new List<Data>(datas);
        if (title == null)
            Title.text = "Nothing to show, please choose a data space.";
        else
            Title.text = title;

        if (Datas != null)
            MaxSites = Mathf.CeilToInt(Datas.Count / (float)DataContainers.Length);

        ContainerPointer = 0;
    }

    public void SetComponentStatus(bool status)
    {
        SiteIndicator.gameObject.SetActive(status);
        PreviousSite.gameObject.SetActive(status);
        NextSite.gameObject.SetActive(status);
        ParentDir.gameObject.SetActive(status);
        Root.gameObject.SetActive(status);
        foreach (DataContainer dc in DataContainers)
            dc.gameObject.SetActive(status);
    }

    private void ActualizeSiteVariables()
    {
        SiteIndicator.text = "Site " + ((ContainerPointer / DataContainers.Length) + 1) + " of " + Mathf.Max(1, MaxSites);
        PreviousSite.interactable = ContainerPointer > 0;
        NextSite.interactable = (ContainerPointer + DataContainers.Length) < Datas.Count;
    }

    private void ActualizeDataContainers()
    {
        for (int i = 0; i < DataContainers.Length; i++)
        {
            DataContainers[i].gameObject.SetActive((_containerPointer + i) < Datas.Count);
            if (DataContainers[i].gameObject.activeInHierarchy)
            {
                DataContainers[i].Resource = Datas[i + _containerPointer];
                if ((string)DataContainers[i].Resource.ID != "")
                {
                    if (i == 0)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(0); });
                    else if (i == 1)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(1); });
                    else if (i == 2)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(2); });
                    else if (i == 3)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(3); });
                    else if (i == 4)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(4); });
                    else if (i == 5)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(5); });
                    else if (i == 6)
                        DataContainers[i].GetComponent<Button>().onClick.AddListener(delegate { DataContainerClicked(6); });
                }
            }

        }
    }

    private void DataContainerClicked(int i)
    {
        Debug.Log(this);
        Debug.Log(this.gameObject);
        LoadObject((string)DataContainers[i].Resource.ID);
    }
    private void NextClick()
    {
        ContainerPointer += DataContainers.Length;
    }
    private void PreviousClick()
    {
        ContainerPointer -= DataContainers.Length;
    }

    public void LoadObject(string ID)
    {
        Debug.Log("Load Object: " + ID);
        ShapeNetInterface inter = GameObject.Find("ShapeNetInterface").gameObject.GetComponent<ShapeNetInterface>();

        ShapeNetModel shapeObj = inter.ShapeNetModels[ID];

        StartCoroutine(inter.GetModel((string)shapeObj.ID, (path) =>
        {
            Debug.Log("Scale & Reorientate Obj");
            GameObject GameObject = ObjectLoader.LoadObject(path + "\\" + shapeObj.ID + ".obj", path + "\\" + shapeObj.ID + ".mtl");
            GameObject GhostObject = ObjectLoader.Reorientate_Obj(GameObject, shapeObj.Up, shapeObj.Front, shapeObj.Unit);
            GhostObject.AddComponent<BoxCollider>();
            GhostObject.AddComponent<Rigidbody>();
            GhostObject.AddComponent<XRGrabInteractable>();
            GhostObject.transform.position = new Vector3(0, 5, 0);
            // Instantiate(GameObject);
            // GameObject GhostObject = ObjectLoader.Reorientate_Obj(GameObject, shapeObj.Up, shapeObj.Front, shapeObj.Unit);

            // BoxCollider _collider = GhostObject.AddComponent<BoxCollider>();
            // _collider.size = shapeObj.AlignedDimensions / 100;
            //     //_collider.center = Vector3.up * _collider.size.y / 2;

            //     LineRenderer lines = GhostObject.AddComponent<LineRenderer>();
            // lines.enabled = false;

            // GhostObject.transform.position = entity.Position.Vector;
            // GhostObject.transform.rotation = entity.Rotation.Quaternion;
            // GhostObject.transform.localScale = entity.Scale.Vector;
            // entity.Object3D = GhostObject;
            // _loading_obj = false;
            //Builder.SceneBuilderControl.LoadedModels.Add((string)ShapeNetObject.ID, new GameObject[2]);
            //Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][0] = GhostObject;
            //Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][1] = Instantiate(GhostObject);
            //Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][1].SetActive(false);
            //MakeGhostObject(GhostObject);
            //GhostObject.SetActive(ghostActive);
            //_collider.enabled = false;
            //OnModelLoaded();
            //_objectInstance = Instantiate(Instantiate(GhostObject));
            //_objectInstance.SetActive(true);
        }));
    }
}
