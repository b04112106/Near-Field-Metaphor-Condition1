using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using IndiePixel.VR;

public class RotateHandle : MonoBehaviour
{
    public GameObject BoundingBox;
    public GameObject RotateContainer;
    public GameObject [] Face;
    public GameObject [] Edge;
    public GameObject [] SolidEdge;
    public Material oriMaterial;
    public Material touchMaterial;
    public Material grabMaterial;
    private bool isTouching;
    private bool isNearTouching;
    private GameObject Base;
    private Color BaseOriginalColor;
    private Vector3 [] edgeOriginalLocalPos;
    private GameObject coolObject;
    private GameObject rotateContainerChild;

    #region condition 2
    private GameObject menu;
    private bool isRotating = false;
    private int index = -1;
    private bool [] flag;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        /*this.gameObject.GetComponent<VRTK_InteractableObject>().usingState = 0;
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        isTouching = false;
        isNearTouching = false;
        Base = this.gameObject.transform.parent.GetChild(1).gameObject;
        Base.GetComponent<MeshRenderer>().enabled = false;
        BaseOriginalColor = Base.GetComponent<MeshRenderer>().material.color;
        // initial edge local position
        edgeOriginalLocalPos = new Vector3 [4];
        flag = new bool [4];
        for(int i=0; i<4; i++)
        {
            edgeOriginalLocalPos[i] = Edge[i].transform.localPosition;
            flag[i] = false;
        }
        if(name == "HandleY")
        {
            // create empty object
            if(!GameObject.Find("coolObject"))
                coolObject = new GameObject("coolObject");
            else
                coolObject = GameObject.Find("coolObject");
        }
        else 
        {
            if(!GameObject.Find("coolObject1"))
                coolObject = new GameObject("coolObject1");
            else
                coolObject = GameObject.Find("coolObject1");
        }

        #region condition 2
        menu = GameObject.Find("RadialMenu_Canvas");
        #endregion*/
        
    }
    public void onTouch()
    {
        /*isTouching = true;
        this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        Base.GetComponent<MeshRenderer>().enabled = true;
        if(this.gameObject.name == "HandleY")
            Base.GetComponent<MeshRenderer>().material.color = new Color(0.3490196f, 0.9450981f, 0.254902f, 0.3f);
        else if(this.gameObject.name == "HandleX")
            Base.GetComponent<MeshRenderer>().material.color = new Color(0.9528302f, 0.3280972f, 0.3647637f, 0.3f);
        else if(this.gameObject.name == "HandleZ")
            Base.GetComponent<MeshRenderer>().material.color = new Color(0.3294117f, 0.6028287f, 0.9529412f, 0.3f);*/
    }
    public void onUnTouch()
    {
        /*isTouching = false;
        Base.GetComponent<MeshRenderer>().material.color = BaseOriginalColor;*/
    }
    public void NearTouch()
    {
        /*this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        Base.GetComponent<MeshRenderer>().enabled = true;*/
        
    }
    public void NearUnTouch()
    {
        /*this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        Base.GetComponent<MeshRenderer>().enabled = false;*/
        
    }
    public void touchSolidEdge(string name)
    {
        /*if(isRotating)
        {
            index = -1;
            for(int i=0; i<4; i++)
            {
                if(name == Edge[i].name)
                {
                    index = i;
                    break;
                }
            }
            Edge[index].transform.localScale = SolidEdge[index].transform.localScale;
            Edge[index].GetComponent<CapsuleCollider>().radius = SolidEdge[index].GetComponent<CapsuleCollider>().radius;
            Edge[index].SetActive(true);
            flag[index] = false;
            SolidEdge[index].GetComponent<CapsuleCollider>().enabled = false;
            SolidEdge[index].GetComponent<MeshRenderer>().enabled = false;
            Debug.Log(name);
        }*/
    }
    public void unTouchEdge()
    {

    }
    public void PressTrigger()
    {
        /*this.gameObject.GetComponent<VRTK_InteractableObject>().usingState = 1;
        //Base.GetComponent<MeshRenderer>().material.color = this.gameObject.GetComponent<VRTK_InteractObjectHighlighter>().useHighlight;

        if(this.gameObject.name == "HandleY")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateY][Controllable][ArtificialBased][RotatorContainer]");    
            
        } 
        else if(this.gameObject.name == "HandleZ")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateZ][Controllable][ArtificialBased][RotatorContainer]");
        }
        else if(this.gameObject.name == "HandleX")
        {
            rotateContainerChild = GameObject.Find("[VRTK][AUTOGEN][RotateX][Controllable][ArtificialBased][RotatorContainer]");
        } 
        
        rotateContainerChild.transform.parent = coolObject.transform;
        for(int i=0; i<4; i++){
            SolidEdge[i].SetActive(false);
            Edge[i].SetActive(true);
            Edge[i].transform.localScale = SolidEdge[i].transform.localScale;
            SolidEdge[i].GetComponent<VRTK_InteractObjectHighlighter>().objectToHighlight.SetActive(false);
        }
        isRotating = true;*/
    }
    
    public void ReleaseTrigger()
    {
        /*this.gameObject.GetComponent<VRTK_InteractableObject>().usingState = 0;
        //Base.GetComponent<MeshRenderer>().material.color = new Color(0.3490196f, 0.9450981f, 0.254902f, 0.3f);
        // if(this.gameObject.name == "HandleY")
        //     Base.GetComponent<MeshRenderer>().material.color = new Color(0.3490196f, 0.9450981f, 0.254902f, 0.3f);
        // else if(this.gameObject.name == "HandleX")
        //     Base.GetComponent<MeshRenderer>().material.color = new Color(0.9528302f, 0.3280972f, 0.3647637f, 0.3f);
        // else if(this.gameObject.name == "HandleZ")
        //     Base.GetComponent<MeshRenderer>().material.color = new Color(0.3294117f, 0.6028287f, 0.9529412f, 0.3f);
        
        rotateContainerChild.transform.parent = RotateContainer.transform;
        for(int i=0; i<4; i++){
            Edge[i].SetActive(false);
            SolidEdge[i].SetActive(true);
            SolidEdge[i].GetComponent<VRTK_InteractObjectHighlighter>().objectToHighlight.SetActive(true);
        }
        // set edge local position to original
        for(int i=0; i<4; i++)
        {
            Edge[i].transform.localPosition = edgeOriginalLocalPos[i];
        }
        isRotating = false;*/
    }
    
    
    // Update is called once per frame
    void Update()
    {
        // if(this.gameObject.GetComponent<VRTK_InteractableObject>().usingState == 1)
        // {
        //     BoundingBox.transform.rotation = rotateContainerChild.transform.rotation;
        // }
        /*GameObject r1,r2,r3;
        r1 = GameObject.Find("[VRTK][AUTOGEN][RotateY][Controllable][ArtificialBased][RotatorContainer]");
        r2 = GameObject.Find("[VRTK][AUTOGEN][RotateX][Controllable][ArtificialBased][RotatorContainer]");
        r3 = GameObject.Find("[VRTK][AUTOGEN][RotateZ][Controllable][ArtificialBased][RotatorContainer]");
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 0)
        {
            PressTrigger();
            // when edge[i] is active and no touching, activate SolidEdge
            // if(index != -1)
            // {
            //     Debug.Log(Edge[index].transform.parent.parent.GetComponent<VRTK_InteractableObject>().IsTouched());
            //     if(!Edge[index].transform.parent.parent.GetComponent<VRTK_InteractableObject>().IsTouched())
            //     {
            //         if(flag[index])
            //         {
            //             Edge[index].SetActive(false);
            //             // SolidEdge[index].SetActive(true);
            //             SolidEdge[index].GetComponent<CapsuleCollider>().enabled = true;
            //             SolidEdge[index].GetComponent<MeshRenderer>().enabled = true;
            //             if(index == 0)
            //             {    
            //                 SolidEdge[index].transform.localScale = SolidEdge[index+1].transform.localScale;
            //                 SolidEdge[index].GetComponent<CapsuleCollider>().radius = SolidEdge[index+1].GetComponent<CapsuleCollider>().radius;
            //             }
            //             else   
            //             {
            //                 SolidEdge[index].transform.localScale = SolidEdge[index-1].transform.localScale;
            //                 SolidEdge[index].GetComponent<CapsuleCollider>().radius = SolidEdge[index-1].GetComponent<CapsuleCollider>().radius;
            //             }
            //             index = -1;
            //         }
            //         if(index != -1)
            //             flag[index] = true;
            //     }   
            // }
            
        }
        else
        {
            if(isRotating)
            {
                ReleaseTrigger();
            }
        }*/
    }
}
