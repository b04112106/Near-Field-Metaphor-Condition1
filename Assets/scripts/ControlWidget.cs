using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IndiePixel.VR;
using VRTK;
using HighlightPlus;
using UnityEngine.SceneManagement;

public class ControlWidget : MonoBehaviour
{
    public GameObject Cam, menu;
    public AudioSource audioSource;
    private GameObject rightController;
    private float distance = 0.45f;
    private Vector3 widgetHeightOffset = new Vector3(0,-0.1f,0);
    private GameObject tCube, sCube, tCubeHL, sCubeHL;
    private int manipulationMode = -1; // -1:default, 0:TX, 1:TY, 2:TZ, 3:TXY, 4:TYZ, 5:TXZ, 6:TXYZ
                                       //             0:SX, 1:SY, 2:SZ, 3:SXY, 4:SYZ, 5:SXZ, 6:SXYZ
                                       //             7:ASX, 8:ASY, 9:ASZ, 10:ASXY, 11:ASYZ, 12:ASXZ, 13:ASXYZ
    private Quaternion originalRotation; // for tCube & sCube
    private Vector3 originalPosition, originalLocalPosition; // for tCube & sCube
    private GameObject CopyOfObject, SelectedObject;
    private float ScaleCoefficient;
    private GameObject rotateHandler, scaleHandler, selectTmpParent, selectOriParent, copyOriParent, objectAxis;
    private float scaleCubeXOffset;
    private int flag = 0, dir = 0;
    private bool firstT = true, firstR = true, firstS = true, firstAS = true, audioFirstPlay = true, firstFind = true;
    private int Tmode = -1; // -1:default, 1:X, 2:Y, 3:Z
    private int Smode = -1; // -1:default, 1:X, 2:Y, 3:Z
    private string reCh = string.Empty;
    private float reF = 10f;
    private bool [] tAxisPress;
    private bool [,] sAxisPress;
    private GameObject [] tAxis, tText, sText, rAxis, rotator, plane, sphere;
    private GameObject [,] sAxis; // {x,y,z}{+,-}
    private GameObject [] sFace, sEdge, sCorner;
    private Color [] axisOriColor, axisHlColor;
    private Color cubeOriColor, cubeHlColor;
    private float touchAlpha = 0.5f, selectAlpha = 0.8f;
    private int closestFace = -1, closestEdge = -1, closestCorner = -1;
    private GameObject GM;
    // Start is called before the first frame update
    void Start()
    {   
        originalRotation = Quaternion.identity;
        originalPosition = Vector3.zero;
        axisOriColor = new Color[3];
        axisHlColor = new Color[3];
        cubeOriColor = new Color(1,1,1,1);
        cubeHlColor = new Color(1,0.8039216f,0.3529412f,1);
        if(GameObject.Find("empty"))
            selectTmpParent = GameObject.Find("empty");
        else
            selectTmpParent = new GameObject("empty");
        rightController = GameObject.Find("RightControllerScriptAlias");
        objectAxis = transform.GetChild(3).gameObject;
        // T : x, y, z
        tAxis = new GameObject[3];
        tText = new GameObject[3];
        tAxisPress = new bool[3]{false, false, false}; 
        for(int i=0; i<3; i++)
        {
            tAxis[i] = transform.GetChild(0).GetChild(i).GetChild(0).gameObject;
            tText[i] = transform.GetChild(0).GetChild(i).GetChild(1).gameObject;
            axisOriColor[i] = tAxis[i].GetComponent<MeshRenderer>().material.color;
            axisHlColor[i] = axisOriColor[i];
        }
        tCube = transform.GetChild(0).GetChild(3).gameObject;
        tCubeHL = transform.GetChild(0).GetChild(4).gameObject;
        plane = new GameObject[3];
        sphere = new GameObject[3];
        for(int i=0; i<3; i++)
        {
            plane[i] = transform.GetChild(0).GetChild(5).GetChild(i).gameObject;
            sphere[i] = plane[i].transform.GetChild(1).gameObject;
        }
        // S : x+, x-, y+, y-, z+, z-
        sAxis = new GameObject[3, 2];
        sText = new GameObject[3];
        sAxisPress = new bool[3,2]{{false, false}, {false, false}, {false, false}};
        for(int i=0; i<3; i++)
        {
            sAxis[i, 0] = transform.GetChild(2).GetChild(i).GetChild(0).gameObject;
            sAxis[i, 1] = transform.GetChild(2).GetChild(i).GetChild(1).gameObject;
            sText[i] = transform.GetChild(2).GetChild(i).GetChild(2).gameObject;
        }
        sCube = transform.GetChild(2).GetChild(3).gameObject;
        sCubeHL = transform.GetChild(2).GetChild(4).gameObject;
        scaleHandler = transform.GetChild(2).GetChild(5).gameObject;
        sFace = new GameObject[6];
        sEdge = new GameObject[12];
        sCorner = new GameObject[8];
        for(int i=0; i<6; i++)
            sFace[i] = sCube.transform.GetChild(0).GetChild(i).gameObject;
        for(int i=0; i<12; i++)
            sEdge[i] = sCube.transform.GetChild(1).GetChild(i).gameObject;
        for(int i=0; i<8; i++)
            sCorner[i] = sCube.transform.GetChild(2).GetChild(i).gameObject;
        // R
        rotator = new GameObject[3];
        rotator[0] = GameObject.Find("[VRTK][AUTOGEN][XAxis][Controllable][ArtificialBased][RotatorContainer]");
        rotator[1] = GameObject.Find("[VRTK][AUTOGEN][YAxis][Controllable][ArtificialBased][RotatorContainer]");
        rotator[2] = GameObject.Find("[VRTK][AUTOGEN][ZAxis][Controllable][ArtificialBased][RotatorContainer]");
        rotator[0].GetComponent<VRTK_InteractableObject>().grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
        rotator[1].GetComponent<VRTK_InteractableObject>().grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
        rotator[2].GetComponent<VRTK_InteractableObject>().grabOverrideButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
        rAxis = new GameObject[6];
        for(int i=0; i<3; i++)
        {
            rAxis[i*2] = rotator[i].transform.GetChild(0).GetChild(0).gameObject;
            rAxis[i*2+1] = rotator[i].transform.GetChild(0).GetChild(1).gameObject;
        }
        rotateHandler = new GameObject("rotateHandler");
        transform.GetChild(0).position = new Vector3(0,-1000,0);
        transform.GetChild(1).position = new Vector3(0,-1000,0);
        transform.GetChild(2).position = new Vector3(0,-1000,0);
        if(SceneManager.GetActiveScene().name == "Testing")
            GM = GameObject.Find("GameManager");
    }
    public void touchCube()
    {
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            tCubeHL.GetComponent<MeshRenderer>().material.color = cubeHlColor; 
            tCubeHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CubeTouch"));
            tCubeHL.GetComponent<HighlightEffect>().highlighted = true;
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7 || menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
        {
            sCubeHL.GetComponent<MeshRenderer>().material.color = cubeHlColor; 
            sCubeHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CubeTouch"));
            sCubeHL.GetComponent<HighlightEffect>().highlighted = true;
        }
    }
    public void unTouchCube()
    {
        // reset ignored collider
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            tCubeHL.GetComponent<MeshRenderer>().material.color = cubeOriColor; 
            tCubeHL.GetComponent<HighlightEffect>().highlighted = false;
            tCube.GetComponent<VRTK_InteractableObject>().ResetIgnoredColliders();
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7 || menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
        {
            sCubeHL.GetComponent<MeshRenderer>().material.color = cubeOriColor;
            sCubeHL.GetComponent<HighlightEffect>().highlighted = false; 
            sCube.GetComponent<VRTK_InteractableObject>().ResetIgnoredColliders();
        }
    }
    public void grabCube()
    {
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            tCubeHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CubeGrab"));
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7)
        {
            sCubeHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CubeGrab"));
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
        {
            sCubeHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CubeGrab"));
            selectTmpParent.transform.position = SelectedObject.transform.position;
            selectTmpParent.transform.rotation = SelectedObject.transform.rotation;
            selectTmpParent.transform.Translate(Vector3.Scale(SelectedObject.GetComponent<MeshFilter>().mesh.bounds.center, SelectedObject.transform.lossyScale));
            Vector3 ex = SelectedObject.GetComponent<MeshFilter>().mesh.bounds.extents;
            if(manipulationMode == 0) // ASX
            {
                if(sAxisPress[0,0])
                {
                    scaleHandler.transform.position = sFace[3].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, 0, 0), SelectedObject.transform.lossyScale));
                }
                else
                {
                    scaleHandler.transform.position = sFace[1].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, 0, 0), SelectedObject.transform.lossyScale));    
                }    
            }
            else if(manipulationMode == 1) // ASY
            {
                if(sAxisPress[1,0])
                {
                    scaleHandler.transform.position = sFace[5].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(0, -ex.y, 0), SelectedObject.transform.lossyScale));
                }
                else
                {
                    scaleHandler.transform.position = sFace[0].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(0, ex.y, 0), SelectedObject.transform.lossyScale));  
                }  
            }
            else if(manipulationMode == 2) // ASZ
            {
                if(sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sFace[2].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(0, 0, -ex.z), SelectedObject.transform.lossyScale));
                }
                else
                {
                    scaleHandler.transform.position = sFace[4].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(0, 0, ex.z), SelectedObject.transform.lossyScale));  
                }  
            }
            else if(manipulationMode == 3) // ASXY
            {
                if(sAxisPress[0,0] && sAxisPress[1,0])
                {
                    scaleHandler.transform.position = sEdge[10].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, -ex.y, 0), SelectedObject.transform.lossyScale));
                }
                else if(sAxisPress[0,0] && sAxisPress[1,1])
                {
                    scaleHandler.transform.position = sEdge[2].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, ex.y, 0), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[0,1] && sAxisPress[1,0])
                {
                    scaleHandler.transform.position = sEdge[8].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, -ex.y, 0), SelectedObject.transform.lossyScale));    
                }
                else
                {
                    scaleHandler.transform.position = sEdge[0].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, ex.y, 0), SelectedObject.transform.lossyScale));    
                }
            }
            else if(manipulationMode == 4) // ASYZ
            {
                if(sAxisPress[1,0] && sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sEdge[9].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(0, -ex.y, -ex.z), SelectedObject.transform.lossyScale));
                }
                else if(sAxisPress[1,0] && sAxisPress[2,1])
                {
                    scaleHandler.transform.position = sEdge[11].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(0, -ex.y, ex.z), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[1,1] && sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sEdge[1].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(0, ex.y, -ex.z), SelectedObject.transform.lossyScale));    
                }
                else
                {
                    scaleHandler.transform.position = sEdge[3].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(0, ex.y, ex.z), SelectedObject.transform.lossyScale));    
                }
            }
            else if(manipulationMode == 5) // ASXZ
            {
                if(sAxisPress[0,0] && sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sEdge[5].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, 0, -ex.z), SelectedObject.transform.lossyScale));
                }
                else if(sAxisPress[0,0] && sAxisPress[2,1])
                {
                    scaleHandler.transform.position = sEdge[6].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, 0, ex.z), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[0,1] && sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sEdge[4].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, 0, -ex.z), SelectedObject.transform.lossyScale));    
                }
                else
                {
                    scaleHandler.transform.position = sEdge[7].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, 0, ex.z), SelectedObject.transform.lossyScale));    
                }
            }
            else if(manipulationMode == 6) // ASXYZ
            {
                if(sAxisPress[0,0] && sAxisPress[1,0] && sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sCorner[5].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, -ex.y, -ex.z), SelectedObject.transform.lossyScale));
                }
                else if(sAxisPress[0,0] && sAxisPress[1,0] && sAxisPress[2,1])
                {
                    scaleHandler.transform.position = sCorner[6].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, -ex.y, ex.z), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[0,0] && sAxisPress[1,1] && sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sCorner[1].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, ex.y, -ex.z), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[0,0] && sAxisPress[1,1] && sAxisPress[2,1])
                {
                    scaleHandler.transform.position = sCorner[2].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(-ex.x, ex.y, ex.z), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[0,1] && sAxisPress[1,0] && sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sCorner[4].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, -ex.y, -ex.z), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[0,1] && sAxisPress[1,0] && sAxisPress[2,1])
                {
                    scaleHandler.transform.position = sCorner[7].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, -ex.y, ex.z), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[0,1] && sAxisPress[1,1] && sAxisPress[2,0])
                {
                    scaleHandler.transform.position = sCorner[0].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, ex.y, -ex.z), SelectedObject.transform.lossyScale));    
                }
                else if(sAxisPress[0,1] && sAxisPress[1,1] && sAxisPress[2,1])
                {
                    scaleHandler.transform.position = sCorner[3].transform.position;
                    selectTmpParent.transform.Translate(Vector3.Scale(new Vector3(ex.x, ex.y, ex.z), SelectedObject.transform.lossyScale));    
                }
            }
            sCubeHL.transform.parent = scaleHandler.transform;
            SelectedObject.transform.parent = selectTmpParent.transform;
        }
    }
    public void unGrabCube()
    {
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            tCubeHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CubeTouch"));
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7)
        {
            sCubeHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CubeTouch"));
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
        {
            sCubeHL.GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("CubeTouch"));
            sCubeHL.transform.parent = transform.GetChild(2);
            if (selectOriParent != null)
                SelectedObject.transform.parent = selectOriParent.transform;
            else
                SelectedObject.transform.parent = null;
        }
        if(SceneManager.GetActiveScene().name == "Testing")
            GM.GetComponent<GameManager>().release(1f);
    }
    public void touchAxis(string s)
    {
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            if(!tAxisPress[ID0])
            {
                axisHlColor[ID0].a = touchAlpha;
                tAxis[ID0].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
            }
            tAxis[ID0].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + ID0));
            tAxis[ID0].GetComponent<HighlightEffect>().highlighted = true;
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            int ID1 = (int) char.GetNumericValue(s[1]);
            if(!sAxisPress[ID0, ID1])
            {
                axisHlColor[ID0].a = touchAlpha;
                sAxis[ID0, 0].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                sAxis[ID0, 1].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
            }
            sAxis[ID0, 0].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + ID0));
            sAxis[ID0, 0].GetComponent<HighlightEffect>().highlighted = true;
            sAxis[ID0, 1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + ID0));
            sAxis[ID0, 1].GetComponent<HighlightEffect>().highlighted = true;
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            int ID1 = (int) char.GetNumericValue(s[1]);
            if(!sAxisPress[ID0, ID1])
            {
                axisHlColor[ID0].a = touchAlpha;
                sAxis[ID0, ID1].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
            }
            sAxis[ID0, ID1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + ID0));
            sAxis[ID0, ID1].GetComponent<HighlightEffect>().highlighted = true;
        }
    }
    public void unTouchAxis(string s)
    {
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            if(!tAxisPress[ID0])
            {
                tAxis[ID0].GetComponent<MeshRenderer>().material.color = axisOriColor[ID0];
                tAxis[ID0].GetComponent<HighlightEffect>().highlighted = false;
            }
            else
            {
                tAxis[ID0].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + ID0));
            }
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            int ID1 = (int) char.GetNumericValue(s[1]);
            if(!sAxisPress[ID0, ID1])
            {
                sAxis[ID0, 0].GetComponent<MeshRenderer>().material.color = axisOriColor[ID0];
                sAxis[ID0, 0].GetComponent<HighlightEffect>().highlighted = false;
                sAxis[ID0, 1].GetComponent<MeshRenderer>().material.color = axisOriColor[ID0];
                sAxis[ID0, 1].GetComponent<HighlightEffect>().highlighted = false;
            }
            else
            {
                sAxis[ID0, 0].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + ID0));
                sAxis[ID0, 1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + ID0));
            }
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            int ID1 = (int) char.GetNumericValue(s[1]);
            if(!sAxisPress[ID0, ID1])
            {
                sAxis[ID0, ID1].GetComponent<MeshRenderer>().material.color = axisOriColor[ID0];
                sAxis[ID0, ID1].GetComponent<HighlightEffect>().highlighted = false;
            }
            else
            {
                sAxis[ID0, ID1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + ID0));
            }
        }
    }
    public void pressAxis(string s) // change mode
    {
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            tAxisPress[ID0] = !tAxisPress[ID0];
            // choose highlight profile
            if(tAxisPress[ID0])
            {
                axisHlColor[ID0].a = selectAlpha;
                tAxis[ID0].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                tAxis[ID0].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + ID0));
                // if tCube is not in the original position, then reset to be a new instance
                if(tCube.transform.localPosition != Vector3.zero)
                {
                    for(int i=0; i<3; i++)
                    {
                        if(i == ID0) continue;
                        tAxisPress[i] = false;
                        tAxis[i].GetComponent<MeshRenderer>().material.color = axisOriColor[i];
                        tAxis[i].GetComponent<HighlightEffect>().highlighted = false;
                    }
                    tCube.transform.localPosition = Vector3.zero;
                    tCubeHL.transform.localPosition = Vector3.zero;
                    originalPosition = tCube.transform.position;
                }
            }
            else if(!tAxisPress[ID0])
            {
                axisHlColor[ID0].a = touchAlpha;
                tAxis[ID0].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                tAxis[ID0].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + ID0));
                tCube.transform.localPosition = Vector3.zero;
                tCubeHL.transform.localPosition = Vector3.zero;
                originalPosition = tCube.transform.position;
            }
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            int ID1 = (int) char.GetNumericValue(s[1]);
            sAxisPress[ID0, ID1] = !sAxisPress[ID0, ID1];
            sAxisPress[ID0, (ID1+1)%2] = sAxisPress[ID0, ID1];
            // choose highlight profile
            if(sAxisPress[ID0, ID1])
            {
                axisHlColor[ID0].a = selectAlpha;
                sAxis[ID0, 0].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                sAxis[ID0, 0].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + ID0));
                sAxis[ID0, 1].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                sAxis[ID0, 1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + ID0));
                // if tCube is not in the original position, then reset to be a new instance
                if(sCube.transform.localScale != new Vector3(0.02f,0.02f,0.02f))
                {
                    for(int i=0; i<3; i++)
                    {
                        if(i == ID0) continue;
                        for(int j=0; j<2; j++)
                        {
                            sAxisPress[i, j] = false;
                            sAxis[i, j].GetComponent<MeshRenderer>().material.color = axisOriColor[i];
                            sAxis[i, j].GetComponent<HighlightEffect>().highlighted = false;
                        }
                    }
                    sCube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
                    sCubeHL.transform.localScale = sCube.transform.localScale;
                }
            }
            else if(!sAxisPress[ID0, ID1])
            {
                axisHlColor[ID0].a = touchAlpha;
                sAxis[ID0, 0].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                sAxis[ID0, 0].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + ID0));
                sAxis[ID0, 1].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                sAxis[ID0, 1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + ID0));
                sCube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
                sCubeHL.transform.localScale = sCube.transform.localScale;
            }
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
        {
            int ID0 = (int) char.GetNumericValue(s[0]);
            int ID1 = (int) char.GetNumericValue(s[1]);
            sAxisPress[ID0, ID1] = !sAxisPress[ID0, ID1];
            // choose highlight profile
            if(sAxisPress[ID0, ID1])
            {
                axisHlColor[ID0].a = selectAlpha;
                sAxis[ID0, ID1].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                sAxis[ID0, ID1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + ID0));
                // de-select another axis
                sAxisPress[ID0, (ID1+1)%2] = false;
                sAxis[ID0, (ID1+1)%2].GetComponent<MeshRenderer>().material.color = axisOriColor[ID0];
                sAxis[ID0, (ID1+1)%2].GetComponent<HighlightEffect>().highlighted = false;
                if(sCube.transform.localScale != new Vector3(0.02f,0.02f,0.02f))
                {
                    for(int i=0; i<3; i++)
                    {
                        if(i == ID0) continue;
                        for(int j=0; j<2; j++)
                        {
                            sAxisPress[i, j] = false;
                            sAxis[i, j].GetComponent<MeshRenderer>().material.color = axisOriColor[i];
                            sAxis[i, j].GetComponent<HighlightEffect>().highlighted = false;
                        }
                    }
                    sCube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
                    sCube.transform.localPosition = Vector3.zero;
                    sCubeHL.transform.localScale = sCube.transform.localScale;
                    sCubeHL.transform.localPosition = Vector3.zero;
                    originalPosition = sCube.transform.position;
                }
            }
            else if(!sAxisPress[ID0, ID1])
            {
                axisHlColor[ID0].a = touchAlpha;
                sAxis[ID0, ID1].GetComponent<MeshRenderer>().material.color = axisHlColor[ID0];
                sAxis[ID0, ID1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + ID0));
                sCube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
                sCube.transform.localPosition = Vector3.zero;
                sCubeHL.transform.localScale = sCube.transform.localScale;
                sCubeHL.transform.localPosition = Vector3.zero;
                originalPosition = sCube.transform.position;
            }
        }
    }
    private void DoTranslation()
    {
        Vector3 v = tCube.transform.position - originalPosition; // controller movement
        Vector3 u = Vector3.zero; // projection vector
        if(manipulationMode == 0) // TX
        {
            u = Vector3.Project(v, tCube.transform.right);
        }
        else if(manipulationMode == 1) // TY
        {
            u = Vector3.Project(v, tCube.transform.up);
        }
        else if(manipulationMode == 2) // TZ
        {
            u = Vector3.Project(v, tCube.transform.forward);
        }
        else if(manipulationMode == 3) // TXY
        {
            u = Vector3.ProjectOnPlane(v, tCube.transform.forward);
        }
        else if(manipulationMode == 4) // TYZ
        {
            u = Vector3.ProjectOnPlane(v, tCube.transform.right);
        }
        else if(manipulationMode == 5) // TXZ
        {
            u = Vector3.ProjectOnPlane(v, tCube.transform.up);
        }
        else if(manipulationMode == 6) // TXYZ
        {
            u = v;
        }
        CopyOfObject.transform.Translate(u, Space.World);
        tCube.transform.position = originalPosition;
        tCube.transform.Translate(u, Space.World);
        originalPosition = tCube.transform.position;
        SelectedObject.transform.Translate(u / ScaleCoefficient, Space.World);
    }
    private void DoScaling()
    {
        Vector3 v = sCube.transform.position - originalPosition; // controller movement
        Vector3 u = Vector3.zero; // projection vector
        Vector3 factor = Vector3.zero; // scaling factor
        Vector3 factor2 = Vector3.zero;
        Vector3 tmpV = Vector3.zero;
        float len = 0f;
        Vector3 arrowPos = rightController.transform.GetChild(1).position;
        float [] f = new float[6];
        float [] e = new float[12];
        float [] c = new float[8];
        for(int i=0; i<6; i++)
            f[i] = Vector3.Distance(arrowPos, sFace[i].transform.position);
        for(int i=0; i<12; i++)
            e[i] = Vector3.Distance(arrowPos, sEdge[i].transform.position);
        for(int i=0; i<8; i++)
            c[i] = Vector3.Distance(arrowPos, sCorner[i].transform.position);
        if(manipulationMode == 0) // SX
        {
            len = sCube.transform.lossyScale.x / 2f;
            tmpV = scaleDir(Mathf.Min(f[1], f[3]));
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(u.magnitude / len + 1, 1, 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(-u.magnitude / len + 1, 1, 1);
            factor2 = new Vector3((factor.x-1)*0.1f + 1, factor.y, factor.z);
        }
        else if(manipulationMode == 1) // SY
        {
            len = sCube.transform.lossyScale.y / 2f;
            tmpV = scaleDir(Mathf.Min(f[0], f[5]));
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(1, u.magnitude / len + 1, 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(1, -u.magnitude / len + 1, 1);
        }
        else if(manipulationMode == 2) // SZ
        {
            len = sCube.transform.lossyScale.z / 2f;
            tmpV = scaleDir(Mathf.Min(f[4], f[2]));
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(1, 1, u.magnitude / len + 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(1, 1, -u.magnitude / len + 1);
        }
        else if(manipulationMode == 3) // SXY
        {
            len = Vector3.Distance(sCube.transform.position, sEdge[0].transform.position);
            tmpV = scaleDir(Mathf.Min(f[0], f[1], f[3], f[5], e[0], e[2], e[8], e[10]));
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(u.magnitude / len + 1, u.magnitude / len + 1, 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(-u.magnitude / len + 1, -u.magnitude / len + 1, 1);
        }
        else if(manipulationMode == 4) // SYZ
        {
            len = Vector3.Distance(sCube.transform.position, sEdge[1].transform.position);
            tmpV = scaleDir(Mathf.Min(f[0], f[2], f[4], f[5], e[1], e[3], e[9], e[11]));
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(1, u.magnitude / len + 1, u.magnitude / len + 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(1, -u.magnitude / len + 1, -u.magnitude / len + 1);
        }
        else if(manipulationMode == 5) // SXZ
        {
            len = Vector3.Distance(sCube.transform.position, sEdge[4].transform.position);
            tmpV = scaleDir(Mathf.Min(f[1], f[2], f[3], f[4], e[4], e[5], e[6], e[7]));
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(u.magnitude / len + 1, 1, u.magnitude / len + 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(-u.magnitude / len + 1, 1, -u.magnitude / len + 1);
        }
        else if(manipulationMode == 6) // SXYZ
        {
            len = Vector3.Distance(sCube.transform.position, sCorner[0].transform.position);
            tmpV = scaleDir(Mathf.Min(f[0], f[1], f[2], f[3], f[4], f[5], e[0], e[1], e[2], e[3], e[4], e[5], e[6], e[7], e[8], e[9], e[10], e[11], c[0], c[1], c[2], c[3], c[4], c[5], c[6], c[7]));
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(u.magnitude / len + 1, u.magnitude / len + 1, u.magnitude / len + 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(-u.magnitude / len + 1, -u.magnitude / len + 1, -u.magnitude / len + 1);
        }
        if(factor.x != Mathf.Infinity && factor.x != -Mathf.Infinity && factor.x != 0 && factor.y != Mathf.Infinity && factor.y != -Mathf.Infinity && factor.y != 0 && factor.z != Mathf.Infinity && factor.z != -Mathf.Infinity && factor.z != 0)
        {
            sCube.transform.position = originalPosition;
            sCube.transform.localScale = Vector3.Scale(sCube.transform.localScale, factor);
            if(sCube.transform.localScale.x < 0 || sCube.transform.localScale.y < 0 || sCube.transform.localScale.z < 0)
            {
                sCube.transform.localScale = new Vector3(sCube.transform.localScale.x/factor.x, sCube.transform.localScale.y/factor.y, sCube.transform.localScale.z/factor.z);
                return;
            }
            sCubeHL.transform.localScale = Vector3.Scale(sCubeHL.transform.localScale, factor);
            SelectedObject.transform.localScale = Vector3.Scale(SelectedObject.transform.localScale, factor);
        }
    }
    private void DoAnchoredScaling()
    {
        Vector3 v = sCube.transform.position - originalPosition; // controller movement
        Vector3 u = Vector3.zero; // projection vector
        Vector3 factor = Vector3.zero; // scaling factor
        Vector3 tmpV = Vector3.zero;
        float len = 0f;
        if(manipulationMode == 0) // ASX
        {
            len = sCube.transform.lossyScale.x;
            if(sAxisPress[0,0])
                tmpV = sCube.transform.right;
            else if(sAxisPress[0,1])
                tmpV = -sCube.transform.right;
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(u.magnitude / len + 1, 1, 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(-u.magnitude / len + 1, 1, 1);
        }
        else if(manipulationMode == 1) // ASY
        {
            len = sCube.transform.lossyScale.y;
            if(sAxisPress[1,0])
                tmpV = sCube.transform.up;
            else if(sAxisPress[1,1])
                tmpV = -sCube.transform.up;
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(1, u.magnitude / len + 1, 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(1, -u.magnitude / len + 1, 1);
        }
        else if(manipulationMode == 2) // ASZ
        {
            len = sCube.transform.lossyScale.z;
            if(sAxisPress[2,0])
                tmpV = sCube.transform.forward;
            else if(sAxisPress[2,1])
                tmpV = -sCube.transform.forward;
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(1, 1, u.magnitude / len + 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(1, 1, -u.magnitude / len + 1);
        }
        else if(manipulationMode == 3) // ASXY
        {
            len = Vector3.Distance(sCube.transform.position, sEdge[0].transform.position) * 2f;
            if(sAxisPress[0,0] && sAxisPress[1,0])
                tmpV = sCube.transform.right + sCube.transform.up;
            else if(sAxisPress[0,0] && sAxisPress[1,1])
                tmpV = sCube.transform.right + -sCube.transform.up;
            else if(sAxisPress[0,1] && sAxisPress[1,0])
                tmpV = -sCube.transform.right + sCube.transform.up;
            else if(sAxisPress[0,1] && sAxisPress[1,1])
                tmpV = -sCube.transform.right + -sCube.transform.up;
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(u.magnitude / len + 1, u.magnitude / len + 1, 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(-u.magnitude / len + 1, -u.magnitude / len + 1, 1);
        }
        else if(manipulationMode == 4) // ASYZ
        {
            len = Vector3.Distance(sCube.transform.position, sEdge[1].transform.position) * 2f;
            if(sAxisPress[1,0] && sAxisPress[2,0])
                tmpV = sCube.transform.up + sCube.transform.forward;
            else if(sAxisPress[1,0] && sAxisPress[2,1])
                tmpV = sCube.transform.up + -sCube.transform.forward;
            else if(sAxisPress[1,1] && sAxisPress[2,0])
                tmpV = -sCube.transform.up + sCube.transform.forward;
            else if(sAxisPress[1,1] && sAxisPress[2,1])
                tmpV = -sCube.transform.up + -sCube.transform.forward;
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(1, u.magnitude / len + 1, u.magnitude / len + 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(1, -u.magnitude / len + 1, -u.magnitude / len + 1);
        }
        else if(manipulationMode == 5) // ASXZ
        {
            len = Vector3.Distance(sCube.transform.position, sEdge[4].transform.position) * 2f;
            if(sAxisPress[0,0] && sAxisPress[2,0])
                tmpV = sCube.transform.right + sCube.transform.forward;
            else if(sAxisPress[0,0] && sAxisPress[2,1])
                tmpV = sCube.transform.right + -sCube.transform.forward;
            else if(sAxisPress[0,1] && sAxisPress[2,0])
                tmpV = -sCube.transform.right + sCube.transform.forward;
            else if(sAxisPress[0,1] && sAxisPress[2,1])
                tmpV = -sCube.transform.right + -sCube.transform.forward;
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(u.magnitude / len + 1, 1, u.magnitude / len + 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(-u.magnitude / len + 1, 1, -u.magnitude / len + 1);
        }
        else if(manipulationMode == 6) // ASXYZ
        {
            len = Vector3.Distance(sCube.transform.position, sCorner[0].transform.position) * 2f;
            if(sAxisPress[0,0] && sAxisPress[1,0] && sAxisPress[2,0])
                tmpV = sCube.transform.right + sCube.transform.up + sCube.transform.forward;
            else if(sAxisPress[0,0] && sAxisPress[1,0] && sAxisPress[2,1])
                tmpV = sCube.transform.right + sCube.transform.up + -sCube.transform.forward;
            else if(sAxisPress[0,0] && sAxisPress[1,1] && sAxisPress[2,0])
                tmpV = sCube.transform.right + -sCube.transform.up + sCube.transform.forward;
            else if(sAxisPress[0,0] && sAxisPress[1,1] && sAxisPress[2,1])
                tmpV = sCube.transform.right + -sCube.transform.up + -sCube.transform.forward;
            else if(sAxisPress[0,1] && sAxisPress[1,0] && sAxisPress[2,0])
                tmpV = -sCube.transform.right + sCube.transform.up + sCube.transform.forward;
            else if(sAxisPress[0,1] && sAxisPress[1,0] && sAxisPress[2,1])
                tmpV = -sCube.transform.right + sCube.transform.up + -sCube.transform.forward;
            else if(sAxisPress[0,1] && sAxisPress[1,1] && sAxisPress[2,0])
                tmpV = -sCube.transform.right + -sCube.transform.up + sCube.transform.forward;
            else if(sAxisPress[0,1] && sAxisPress[1,1] && sAxisPress[2,1])
                tmpV = -sCube.transform.right + -sCube.transform.up + -sCube.transform.forward;
            u = Vector3.Project(v, tmpV);
            if(Vector3.Dot(v, tmpV) > 0)
                factor = new Vector3(u.magnitude / len + 1, u.magnitude / len + 1, u.magnitude / len + 1);
            else if(Vector3.Dot(v, tmpV) < 0)
                factor = new Vector3(-u.magnitude / len + 1, -u.magnitude / len + 1, -u.magnitude / len + 1);
        }
        if(factor.x != Mathf.Infinity && factor.x != -Mathf.Infinity && factor.x != 0 && factor.y != Mathf.Infinity && factor.y != -Mathf.Infinity && factor.y != 0 && factor.z != Mathf.Infinity && factor.z != -Mathf.Infinity && factor.z != 0)
        {
            sCube.transform.localScale = Vector3.Scale(sCube.transform.localScale, factor);
            if(sCube.transform.localScale.x < 0 || sCube.transform.localScale.y < 0 || sCube.transform.localScale.z < 0)
            {
                sCube.transform.localScale = new Vector3(sCube.transform.localScale.x/factor.x, sCube.transform.localScale.y/factor.y, sCube.transform.localScale.z/factor.z);
                return;
            }
            scaleHandler.transform.localScale = Vector3.Scale(scaleHandler.transform.localScale, factor);
            sCube.transform.position = sCubeHL.transform.position;
            originalPosition = sCube.transform.position;
            selectTmpParent.transform.localScale = Vector3.Scale(selectTmpParent.transform.localScale, factor);
        }
    }
    private Vector3 scaleDir(float min)
    {
        Vector3 arrowPos = rightController.transform.GetChild(1).position;
        float [] f = new float[6];
        float [] e = new float[12];
        float [] c = new float[8];
        Vector3 vector = Vector3.zero;
        // sCubeHL.transform.parent = transform.GetChild(2);
        for(int i=0; i<6; i++)
            f[i] = Vector3.Distance(arrowPos, sFace[i].transform.position);
        for(int i=0; i<12; i++)
            e[i] = Vector3.Distance(arrowPos, sEdge[i].transform.position);
        for(int i=0; i<8; i++)
            c[i] = Vector3.Distance(arrowPos, sCorner[i].transform.position);
        if(min == f[0])
            vector = sCube.transform.up;
        else if(min == f[1])
            vector = sCube.transform.right;// scaleHandler.transform.position = sFace[3].transform.position;}
        else if(min == f[2])
            vector = -sCube.transform.forward;
        else if(min == f[3])
            vector = -sCube.transform.right; //scaleHandler.transform.position = sFace[1].transform.position;}
        else if(min == f[4])
            vector = sCube.transform.forward;
        else if(min == f[5])
            vector = -sCube.transform.up;
        else if(min == e[0])
            vector = sCube.transform.right + sCube.transform.up;
        else if(min == e[1])
            vector = -sCube.transform.forward + sCube.transform.up;
        else if(min == e[2])
            vector = -sCube.transform.right + sCube.transform.up;
        else if(min == e[3])
            vector = sCube.transform.forward + sCube.transform.up;
        else if(min == e[4])
            vector = sCube.transform.right + -sCube.transform.forward;
        else if(min == e[5])
            vector = -sCube.transform.right + -sCube.transform.forward;
        else if(min == e[6])
            vector = -sCube.transform.right + sCube.transform.forward;
        else if(min == e[7])
            vector = sCube.transform.right + sCube.transform.forward;
        else if(min == e[8])
            vector = sCube.transform.right + -sCube.transform.up;
        else if(min == e[9])
            vector = -sCube.transform.forward + -sCube.transform.up;
        else if(min == e[10])
            vector = -sCube.transform.right + -sCube.transform.up;
        else if(min == e[11])
            vector = sCube.transform.forward + -sCube.transform.up;
        else if(min == c[0])
            vector = sCube.transform.right + sCube.transform.up + -sCube.transform.forward;
        else if(min == c[1])
            vector = -sCube.transform.right + sCube.transform.up + -sCube.transform.forward;
        else if(min == c[2])
            vector = -sCube.transform.right + sCube.transform.up + sCube.transform.forward;
        else if(min == c[3])
            vector = sCube.transform.right + sCube.transform.up + sCube.transform.forward;
        else if(min == c[4])
            vector = sCube.transform.right + -sCube.transform.up + -sCube.transform.forward;
        else if(min == c[5])
            vector = -sCube.transform.right + -sCube.transform.up + -sCube.transform.forward;
        else if(min == c[6])
            vector = -sCube.transform.right + -sCube.transform.up + sCube.transform.forward;
        else if(min == c[7])
            vector = sCube.transform.right + -sCube.transform.up + sCube.transform.forward;
        // sCubeHL.transform.parent = scaleHandler.transform;
        return vector;
    }
    private void determineDim()
    {
        if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1)
        {
            if(!tAxisPress[0] && !tAxisPress[1] && !tAxisPress[2]) // default
                manipulationMode = -1;
            else if(tAxisPress[0] && !tAxisPress[1] && !tAxisPress[2]) // TX
                manipulationMode = 0;
            else if(!tAxisPress[0] && tAxisPress[1] && !tAxisPress[2]) // TY
                manipulationMode = 1;
            else if(!tAxisPress[0] && !tAxisPress[1] && tAxisPress[2]) // TZ
                manipulationMode = 2;
            else if(tAxisPress[0] && tAxisPress[1] && !tAxisPress[2]) // TXY
                manipulationMode = 3;
            else if(!tAxisPress[0] && tAxisPress[1] && tAxisPress[2]) // TYZ
                manipulationMode = 4;
            else if(tAxisPress[0] && !tAxisPress[1] && tAxisPress[2]) // TXZ
                manipulationMode = 5;
            else if(tAxisPress[0] && tAxisPress[1] && tAxisPress[2]) // TXYZ
                manipulationMode = 6;
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7)
        {
            if(!sAxisPress[0,0] && !sAxisPress[0,1] && !sAxisPress[1,0] && !sAxisPress[1,1] && !sAxisPress[2,0] && !sAxisPress[2,1]) // default
                manipulationMode = -1;
            else if(sAxisPress[0,0] && sAxisPress[0,1] && !sAxisPress[1,0] && !sAxisPress[1,1] && !sAxisPress[2,0] && !sAxisPress[2,1]) // SX
                manipulationMode = 0;
            else if(!sAxisPress[0,0] && !sAxisPress[0,1] && sAxisPress[1,0] && sAxisPress[1,1] && !sAxisPress[2,0] && !sAxisPress[2,1]) // SY
                manipulationMode = 1;
            else if(!sAxisPress[0,0] && !sAxisPress[0,1] && !sAxisPress[1,0] && !sAxisPress[1,1] && sAxisPress[2,0] && sAxisPress[2,1]) // SZ
                manipulationMode = 2;
            else if(sAxisPress[0,0] && sAxisPress[0,1] && sAxisPress[1,0] && sAxisPress[1,1] && !sAxisPress[2,0] && !sAxisPress[2,1]) // SXY
                manipulationMode = 3;
            else if(!sAxisPress[0,0] && !sAxisPress[0,1] && sAxisPress[1,0] && sAxisPress[1,1] && sAxisPress[2,0] && sAxisPress[2,1]) // SYZ
                manipulationMode = 4;
            else if(sAxisPress[0,0] && sAxisPress[0,1] && !sAxisPress[1,0] && !sAxisPress[1,1] && sAxisPress[2,0] && sAxisPress[2,1]) // SXZ
                manipulationMode = 5;
            else if(sAxisPress[0,0] && sAxisPress[0,1] && sAxisPress[1,0] && sAxisPress[1,1] && sAxisPress[2,0] && sAxisPress[2,1]) // SXYZ
                manipulationMode = 6;
        }
        else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6)
        {
            if(!sAxisPress[0,0] && !sAxisPress[0,1] && !sAxisPress[1,0] && !sAxisPress[1,1] && !sAxisPress[2,0] && !sAxisPress[2,1]) // default
                manipulationMode = -1;
            else if((sAxisPress[0,0] || sAxisPress[0,1]) && !sAxisPress[1,0] && !sAxisPress[1,1] && !sAxisPress[2,0] && !sAxisPress[2,1]) // ASX
                manipulationMode = 0;
            else if(!sAxisPress[0,0] && !sAxisPress[0,1] && (sAxisPress[1,0] || sAxisPress[1,1]) && !sAxisPress[2,0] && !sAxisPress[2,1]) // ASY
                manipulationMode = 1;
            else if(!sAxisPress[0,0] && !sAxisPress[0,1] && !sAxisPress[1,0] && !sAxisPress[1,1] && (sAxisPress[2,0] || sAxisPress[2,1])) // ASZ
                manipulationMode = 2;
            else if((sAxisPress[0,0] || sAxisPress[0,1]) && (sAxisPress[1,0] || sAxisPress[1,1]) && !sAxisPress[2,0] && !sAxisPress[2,1]) // ASXY
                manipulationMode = 3;
            else if(!sAxisPress[0,0] && !sAxisPress[0,1] && (sAxisPress[1,0] || sAxisPress[1,1]) && (sAxisPress[2,0] || sAxisPress[2,1])) // ASYZ
                manipulationMode = 4;
            else if((sAxisPress[0,0] || sAxisPress[0,1]) && !sAxisPress[1,0] && !sAxisPress[1,1] && (sAxisPress[2,0] || sAxisPress[2,1])) // ASXZ
                manipulationMode = 5;
            else if((sAxisPress[0,0] || sAxisPress[0,1]) && (sAxisPress[1,0] || sAxisPress[1,1]) && (sAxisPress[2,0] || sAxisPress[2,1])) // ASXYZ
                manipulationMode = 6;
        }
    }
    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.02f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.001f;
        lr.endWidth = 0.001f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        GameObject.Destroy(myLine, duration);

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        sphere.transform.position = end;
        sphere.GetComponent<MeshRenderer>().material.color = color;
        GameObject.Destroy(sphere, duration);
    }
    public int getManipulationMode()
    {
        return manipulationMode;
    }
    // Update is called once per frame
    void Update()
    {
        if(GameObject.FindWithTag("SelectedObject") && firstFind)
        {
            SelectedObject = GameObject.FindWithTag("SelectedObject");
            if (SelectedObject.transform.parent != null)
                selectOriParent = SelectedObject.transform.parent.gameObject;
            ScaleCoefficient = SelectedObject.GetComponent<PersonalSpace>().ScaleCoefficient;
            CopyOfObject = GameObject.FindWithTag("CopyOfObject");
            copyOriParent = CopyOfObject.transform.parent.gameObject;
            firstFind = false;
        }
        if(GameObject.FindWithTag("CopyOfObject"))
        {
            if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 1) // translation mode
            {
                determineDim();
                // first change to T mode
                if(firstT)
                {
                    // move other two widget to be invisable
                    transform.GetChild(1).position = new Vector3(0,-1000,0);
                    transform.GetChild(2).position = new Vector3(0,-1000,0);
                    // set widget position
                    transform.GetChild(0).position = Cam.transform.position + Cam.transform.forward * distance + widgetHeightOffset;
                    transform.GetChild(0).rotation = SelectedObject.transform.rotation;
                    firstT = false; firstR = true; firstS = true; firstAS = true;
                    manipulationMode = -1;
                    tCube.transform.localPosition = Vector3.zero;
                    tCubeHL.transform.localPosition = tCube.transform.localPosition;
                    for(int i=0; i<3; i++)
                    {
                        tAxis[i].GetComponent<MeshRenderer>().material.color = axisOriColor[i];
                        tAxisPress[i] = false;
                        tAxis[i].GetComponent<HighlightEffect>().highlighted = false;
                    }
                }
                // set axis text enable
                for(int i=0; i<3; i++)
                {
                    tText[i].GetComponent<MeshRenderer>().enabled = tAxisPress[i];
                    tText[i].transform.rotation = Quaternion.LookRotation(Cam.transform.forward);
                }
                // set plane & pointer active
                for(int i=0; i<3; i++)
                {
                    if(manipulationMode == i+3 || manipulationMode == 6)
                    {
                        plane[i].SetActive(true);
                    }
                    else 
                    {
                        plane[i].SetActive(false);
                    }
                }
                if(manipulationMode == -1) // default
                {
                    tCube.transform.localPosition = Vector3.zero;
                    originalPosition = tCube.transform.position;
                    originalRotation = tCube.transform.rotation;
                    tCubeHL.transform.position = tCube.transform.position;
                    tCube.GetComponent<BoxCollider>().enabled = false;
                    for(int i=0; i<3; i++)
                        tAxis[i].transform.localScale = new Vector3(1f, 10f, 1f);
                    tText[0].transform.localPosition = new Vector3(13f, 0, 0);
                    tText[1].transform.localPosition = new Vector3(0, 13f, 0);
                    tText[2].transform.localPosition = new Vector3(0, 0, 13f);
                }
                else
                {
                    tCube.GetComponent<BoxCollider>().enabled = true;
                    tCube.transform.rotation = originalRotation;
                    DoTranslation();
                    tCubeHL.transform.position = tCube.transform.position;
                    // set axis text
                    tText[0].GetComponent<TextMesh>().text = (tCubeHL.transform.localPosition.x*100f).ToString("0.0");
                    tText[1].GetComponent<TextMesh>().text = (tCubeHL.transform.localPosition.y*100f).ToString("0.0");
                    tText[2].GetComponent<TextMesh>().text = (tCubeHL.transform.localPosition.z*100f).ToString("0.0");
                    // set pointer active
                    if(manipulationMode == 3) // TXY
                    {
                        Vector3 v = Vector3.Project(tAxis[0].transform.position - tCubeHL.transform.position, -tCubeHL.transform.up);
                        Vector3 u = Vector3.Project(tAxis[0].transform.position - tCubeHL.transform.position, -tCubeHL.transform.right);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + v, Color.green);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + u, Color.red);
                    }
                    else if(manipulationMode == 4) // TYZ
                    {
                        Vector3 v = Vector3.Project(tAxis[0].transform.position - tCubeHL.transform.position, -tCubeHL.transform.forward);
                        Vector3 u = Vector3.Project(tAxis[0].transform.position - tCubeHL.transform.position, -tCubeHL.transform.up);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + v, Color.blue);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + u, Color.green);
                    }
                    else if(manipulationMode == 5) // TXZ
                    {
                        Vector3 v = Vector3.Project(tAxis[0].transform.position - tCubeHL.transform.position, -tCubeHL.transform.forward);
                        Vector3 u = Vector3.Project(tAxis[0].transform.position - tCubeHL.transform.position, -tCubeHL.transform.right);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + v, Color.blue);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + u, Color.red);
                    }
                    // else if(manipulationMode == 6) // TXYZ
                    // {
                    //     Vector3 v = Vector3.ProjectOnPlane(tAxis[0].transform.position - tCubeHL.transform.position, tCubeHL.transform.right);
                    //     Vector3 u = Vector3.ProjectOnPlane(tAxis[0].transform.position - tCubeHL.transform.position, tCubeHL.transform.up);
                    //     Vector3 t = Vector3.ProjectOnPlane(tAxis[0].transform.position - tCubeHL.transform.position, tCubeHL.transform.forward);
                    //     DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + v, Color.red);
                    //     DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + u, Color.green);
                    //     DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + t, Color.blue);
                    // }
                    else if(manipulationMode == 6) // TXYZ
                    {
                        Vector3 x = tAxis[0].transform.position - tCubeHL.transform.position;
                        Vector3 v = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.right);
                        Vector3 u = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.up);
                        Vector3 t = x - Vector3.ProjectOnPlane(x, tCubeHL.transform.forward);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + v, Color.red);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + u, Color.green);
                        DrawLine(tCubeHL.transform.position, tCubeHL.transform.position + t, Color.blue);
                    }
                    // scale axis and set text position
                    // xText
                    if(tCubeHL.transform.localPosition.x >= 0.08f )
                    {
                        tAxis[0].transform.localScale = new Vector3(tAxis[0].transform.localScale.x, tCubeHL.transform.localPosition.x*100f+2f, tAxis[0].transform.localScale.z);
                        tText[0].transform.localPosition = new Vector3(tAxis[0].transform.localScale.y + 3f,0,0);
                    }
                    else if(tCubeHL.transform.localPosition.x <= -0.08f)
                    {
                        tAxis[0].transform.localScale = new Vector3(tAxis[0].transform.localScale.x, tCubeHL.transform.localPosition.x*100f-2f, tAxis[0].transform.localScale.z);
                        tText[0].transform.localPosition = new Vector3(tAxis[0].transform.localScale.y - 3f,0,0);
                    }
                    else if(tCubeHL.transform.localPosition.x < 0.08f && tCubeHL.transform.localPosition.x >= 0f)
                    {
                        tAxis[0].transform.localScale = new Vector3(tAxis[0].transform.localScale.x, 10f, tAxis[0].transform.localScale.z);
                        tText[0].transform.localPosition = new Vector3(13f,0,0);
                    }
                    else if(tCubeHL.transform.localPosition.x > -0.08f && tCubeHL.transform.localPosition.x < 0f)
                    {
                        tAxis[0].transform.localScale = new Vector3(tAxis[0].transform.localScale.x, 10f, tAxis[0].transform.localScale.z);
                        tText[0].transform.localPosition = new Vector3(-13f,0,0);
                    }
                    // yText
                    if(tCubeHL.transform.localPosition.y >= 0.08f )
                    {
                        tAxis[1].transform.localScale = new Vector3(tAxis[1].transform.localScale.x, tCubeHL.transform.localPosition.y*100f+2f, tAxis[1].transform.localScale.z);
                        tText[1].transform.localPosition = new Vector3(0,tAxis[1].transform.localScale.y + 3f,0);
                    }
                    else if(tCubeHL.transform.localPosition.y <= -0.08f)
                    {
                        tAxis[1].transform.localScale = new Vector3(tAxis[1].transform.localScale.x, tCubeHL.transform.localPosition.y*100f-2f, tAxis[1].transform.localScale.z);
                        tText[1].transform.localPosition = new Vector3(0,tAxis[1].transform.localScale.y - 3f,0);
                    }
                    else if(tCubeHL.transform.localPosition.y < 0.08f && tCubeHL.transform.localPosition.y >= 0f)
                    {
                        tAxis[1].transform.localScale = new Vector3(tAxis[1].transform.localScale.x, 10f, tAxis[1].transform.localScale.z);
                        tText[1].transform.localPosition = new Vector3(0,13f,0);
                    }
                    else if(tCubeHL.transform.localPosition.y > -0.08f && tCubeHL.transform.localPosition.y < 0f)
                    {
                        tAxis[1].transform.localScale = new Vector3(tAxis[1].transform.localScale.x, 10f, tAxis[1].transform.localScale.z);
                        tText[1].transform.localPosition = new Vector3(0,-13f,0);
                    }
                    // zText
                    if(tCubeHL.transform.localPosition.z >= 0.08f )
                    {
                        tAxis[2].transform.localScale = new Vector3(tAxis[2].transform.localScale.x, tCubeHL.transform.localPosition.z*100f+2f, tAxis[2].transform.localScale.z);
                        tText[2].transform.localPosition = new Vector3(0,0,tAxis[2].transform.localScale.y + 3f);
                    }
                    else if(tCubeHL.transform.localPosition.z <= -0.08f)
                    {
                        tAxis[2].transform.localScale = new Vector3(tAxis[2].transform.localScale.x, tCubeHL.transform.localPosition.z*100f-2f, tAxis[2].transform.localScale.z);
                        tText[2].transform.localPosition = new Vector3(0,0,tAxis[2].transform.localScale.y - 3f);
                    }
                    else if(tCubeHL.transform.localPosition.z < 0.08f && tCubeHL.transform.localPosition.z >= 0f)
                    {
                        tAxis[2].transform.localScale = new Vector3(tAxis[2].transform.localScale.x, 10f, tAxis[2].transform.localScale.z);
                        tText[2].transform.localPosition = new Vector3(0,0,13f);
                    }
                    else if(tCubeHL.transform.localPosition.z > -0.08f && tCubeHL.transform.localPosition.z < 0f)
                    {
                        tAxis[2].transform.localScale = new Vector3(tAxis[2].transform.localScale.x, 10f, tAxis[2].transform.localScale.z);
                        tText[2].transform.localPosition = new Vector3(0,0,-13f);
                    }
                }
            }
            else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 0) // rotation mode
            {
                // first change to R mode
                if(firstR)
                {
                    // move other two widget to be invisable
                    transform.GetChild(0).position = new Vector3(0,-1000,0);
                    transform.GetChild(2).position = new Vector3(0,-1000,0);
                    // set widget position
                    transform.GetChild(1).position = Cam.transform.position + Cam.transform.forward * distance + widgetHeightOffset;
                    firstT = true; firstR = false; firstS = true; firstAS = true;
                    manipulationMode = -1;
                    rotateHandler.transform.position = CopyOfObject.transform.position;
                    rotateHandler.transform.rotation = SelectedObject.transform.rotation;
                    CopyOfObject.transform.parent = rotateHandler.transform;
                    for(int i=0; i<3; i++)
                        rotator[i].transform.rotation = rotateHandler.transform.rotation;
                }
                if(rotator[0].transform.rotation != rotateHandler.transform.rotation)
                {    
                    rotateHandler.transform.rotation = rotator[0].transform.rotation;
                    rotator[1].transform.rotation = rotateHandler.transform.rotation;
                    rotator[2].transform.rotation = rotateHandler.transform.rotation;
                }
                else if(rotator[1].transform.rotation != rotateHandler.transform.rotation)
                {    
                    rotateHandler.transform.rotation = rotator[1].transform.rotation;
                    rotator[0].transform.rotation = rotateHandler.transform.rotation;
                    rotator[2].transform.rotation = rotateHandler.transform.rotation;
                }
                else if(rotator[2].transform.rotation != rotateHandler.transform.rotation)
                {    
                    rotateHandler.transform.rotation = rotator[2].transform.rotation;
                    rotator[0].transform.rotation = rotateHandler.transform.rotation;
                    rotator[1].transform.rotation = rotateHandler.transform.rotation;
                }
                for(int i=0; i<3; i++)
                {
                    if(rotator[i].GetComponent<VRTK_InteractableObject>().IsGrabbed()) // HL axis when grabbing
                    {
                        rAxis[i*2].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + i));
                        rAxis[i*2+1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisSelect" + i));
                        axisHlColor[i].a = selectAlpha;
                        rAxis[i*2].GetComponent<MeshRenderer>().material.color = axisHlColor[i];
                        rAxis[i*2+1].GetComponent<MeshRenderer>().material.color = axisHlColor[i];
                        if(SceneManager.GetActiveScene().name == "Testing")
                            GM.GetComponent<GameManager>().calRotation();
                        if(audioFirstPlay)
                        {
                            audioSource.PlayOneShot(Resources.Load<AudioClip>("Grab"));
                            if(SceneManager.GetActiveScene().name == "Testing")
                                GM.GetComponent<GameManager>().release(0.5f);
                            audioFirstPlay = false;
                        }
                    }
                    else if(rotator[i].GetComponent<VRTK_InteractableObject>().IsTouched()) // HL axis when touching
                    {
                        rAxis[i*2].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + i));
                        rAxis[i*2+1].GetComponent<HighlightEffect>().ProfileLoad(Resources.Load<HighlightProfile>("AxisTouch" + i));
                        rAxis[i*2].GetComponent<HighlightEffect>().highlighted = true;
                        rAxis[i*2+1].GetComponent<HighlightEffect>().highlighted = true;
                        axisHlColor[i].a = touchAlpha;
                        rAxis[i*2].GetComponent<MeshRenderer>().material.color = axisHlColor[i];
                        rAxis[i*2+1].GetComponent<MeshRenderer>().material.color = axisHlColor[i];
                        audioFirstPlay = true;
                    }
                    else // unHL
                    {
                        rAxis[i*2].GetComponent<HighlightEffect>().highlighted = false;
                        rAxis[i*2+1].GetComponent<HighlightEffect>().highlighted = false;
                        rAxis[i*2].GetComponent<MeshRenderer>().material.color = axisOriColor[i];
                        rAxis[i*2+1].GetComponent<MeshRenderer>().material.color = axisOriColor[i];
                    }
                }
                if(rightController.GetComponent<VRTK_ControllerEvents>().gripPressed)
                    Destroy(rotateHandler.transform.GetChild(0).gameObject);
            }
            else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 7) // uniform scaling mode
            {
                determineDim();
                // first change to S mode
                if(firstS)
                {
                    // move other two widget to be invisable
                    transform.GetChild(0).position = new Vector3(0,-1000,0);
                    transform.GetChild(1).position = new Vector3(0,-1000,0);
                    // set widget position
                    transform.GetChild(2).position = Cam.transform.position + Cam.transform.forward * distance + widgetHeightOffset;
                    transform.GetChild(2).rotation = CopyOfObject.transform.rotation;
                    firstT = true; firstR = true; firstS = false; firstAS = true;
                    manipulationMode = -1;
                    sCube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
                    sCube.transform.localPosition = Vector3.zero;
                    sCubeHL.transform.localScale = sCube.transform.localScale;
                    for(int i=0; i<3; i++)
                    {
                        for(int j=0; j<2; j++)
                        {
                            sAxis[i, j].GetComponent<MeshRenderer>().material.color = axisOriColor[i];
                            sAxisPress[i, j] = false;
                            sAxis[i, j].GetComponent<HighlightEffect>().highlighted = false;
                        }
                    }
                }
                // set axis text enable
                for(int i=0; i<3; i++)
                {
                    sText[i].GetComponent<MeshRenderer>().enabled = sAxisPress[i, 0];
                    sText[i].transform.rotation = Quaternion.LookRotation(Cam.transform.forward);
                }
                if(manipulationMode == -1) // default
                {
                    originalPosition = sCube.transform.position;
                    originalRotation = sCube.transform.rotation;
                    sCubeHL.transform.position = sCube.transform.position;
                    sCubeHL.transform.rotation = sCube.transform.rotation;
                    sCube.GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    sCube.transform.rotation = originalRotation;
                    sCube.GetComponent<BoxCollider>().enabled = true;
                    DoScaling();
                    sText[0].GetComponent<TextMesh>().text = (sCube.transform.lossyScale.x/0.024f).ToString("0.0");
                    sText[1].GetComponent<TextMesh>().text = (sCube.transform.lossyScale.y/0.024f).ToString("0.0");
                    sText[2].GetComponent<TextMesh>().text = (sCube.transform.lossyScale.z/0.024f).ToString("0.0");
                    // set cube collider size
                    if(sCube.transform.lossyScale.x > 0.1f)
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(1, sCube.GetComponent<BoxCollider>().size.y, sCube.GetComponent<BoxCollider>().size.z);
                    else
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(1.5f, sCube.GetComponent<BoxCollider>().size.y, sCube.GetComponent<BoxCollider>().size.z);
                    if(sCube.transform.lossyScale.y > 0.1f)
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(sCube.GetComponent<BoxCollider>().size.x, 1, sCube.GetComponent<BoxCollider>().size.z);
                    else
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(sCube.GetComponent<BoxCollider>().size.x, 1.5f, sCube.GetComponent<BoxCollider>().size.z);
                    if(sCube.transform.lossyScale.z > 0.1f)
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(sCube.GetComponent<BoxCollider>().size.x, sCube.GetComponent<BoxCollider>().size.y, 1);
                    else
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(sCube.GetComponent<BoxCollider>().size.x, sCube.GetComponent<BoxCollider>().size.y, 1.5f);
                    // scale axis and set text position
                    // xText
                    // if(sCubeHL.transform.localScale.x >= 0.16f)
                    // {
                    //     sAxis[0, 0].transform.localScale = new Vector3(sAxis[0, 0].transform.localScale.x, sCubeHL.transform.localScale.x*25f + 1f, sAxis[0, 0].transform.localScale.z);
                    //     sAxis[0, 1].transform.localScale = new Vector3(sAxis[0, 1].transform.localScale.x, sCubeHL.transform.localScale.x*25f + 1f, sAxis[0, 1].transform.localScale.z);
                    //     sText[0].transform.localPosition = new Vector3(sAxis[0, 0].transform.localScale.y*2f + 3f,0,0);
                    // }
                    // else
                    // {
                    //     sAxis[0, 0].transform.localScale = new Vector3(sAxis[0, 0].transform.localScale.x, 5f, sAxis[0, 0].transform.localScale.z);
                    //     sAxis[0, 1].transform.localScale = new Vector3(sAxis[0, 1].transform.localScale.x, 5f, sAxis[0, 1].transform.localScale.z);
                    //     sText[0].transform.localPosition = new Vector3(13f,0,0);
                    // }
                }
            }
            else if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 6) // anchored scaling mode
            {
                determineDim();
                // first change to S mode
                if(firstAS)
                {
                    // move other two widget to be invisable
                    transform.GetChild(0).position = new Vector3(0,-1000,0);
                    transform.GetChild(1).position = new Vector3(0,-1000,0);
                    // set widget position
                    transform.GetChild(2).position = Cam.transform.position + Cam.transform.forward * distance + widgetHeightOffset;
                    transform.GetChild(2).rotation = CopyOfObject.transform.rotation;
                    firstT = true; firstR = true; firstS = true; firstAS = false;
                    manipulationMode = -1;
                    sCube.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
                    sCube.transform.localPosition = Vector3.zero;
                    sCubeHL.transform.localScale = sCube.transform.localScale;
                    scaleHandler.transform.localScale = Vector3.one;
                    for(int i=0; i<3; i++)
                    {
                        for(int j=0; j<2; j++)
                        {
                            sAxis[i, j].GetComponent<MeshRenderer>().material.color = axisOriColor[i];
                            sAxisPress[i, j] = false;
                            sAxis[i, j].GetComponent<HighlightEffect>().highlighted = false;
                        }
                    }
                }
                // set axis text enable
                for(int i=0; i<3; i++)
                {
                    sText[i].GetComponent<MeshRenderer>().enabled = sAxisPress[i, 0] || sAxisPress[i, 1];
                    sText[i].transform.rotation = Quaternion.LookRotation(Cam.transform.forward);
                }
                if(manipulationMode == -1) // default
                {
                    originalPosition = sCube.transform.position;
                    originalRotation = sCube.transform.rotation;
                    sCubeHL.transform.position = sCube.transform.position;
                    sCubeHL.transform.rotation = sCube.transform.rotation;
                    sCube.GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    sCube.transform.rotation = originalRotation;
                    sCube.GetComponent<BoxCollider>().enabled = true;
                    DoAnchoredScaling();
                    sText[0].GetComponent<TextMesh>().text = (sCube.transform.lossyScale.x/0.024f).ToString("0.0");
                    sText[1].GetComponent<TextMesh>().text = (sCube.transform.lossyScale.y/0.024f).ToString("0.0");
                    sText[2].GetComponent<TextMesh>().text = (sCube.transform.lossyScale.z/0.024f).ToString("0.0");
                    if(sCube.transform.lossyScale.x > 0.1f)
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(1, sCube.GetComponent<BoxCollider>().size.y, sCube.GetComponent<BoxCollider>().size.z);
                    else
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(1.5f, sCube.GetComponent<BoxCollider>().size.y, sCube.GetComponent<BoxCollider>().size.z);
                    if(sCube.transform.lossyScale.y > 0.1f)
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(sCube.GetComponent<BoxCollider>().size.x, 1, sCube.GetComponent<BoxCollider>().size.z);
                    else
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(sCube.GetComponent<BoxCollider>().size.x, 1.5f, sCube.GetComponent<BoxCollider>().size.z);
                    if(sCube.transform.lossyScale.z > 0.1f)
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(sCube.GetComponent<BoxCollider>().size.x, sCube.GetComponent<BoxCollider>().size.y, 1);
                    else
                        sCube.GetComponent<BoxCollider>().size =  new Vector3(sCube.GetComponent<BoxCollider>().size.x, sCube.GetComponent<BoxCollider>().size.y, 1.5f);
                }
            }
            else // default mode
            {
                transform.GetChild(0).position = new Vector3(0,-1000,0);
                transform.GetChild(1).position = new Vector3(0,-1000,0);
                transform.GetChild(2).position = new Vector3(0,-1000,0);
                firstT = true; firstR = true; firstS = true; firstAS = true;
            }
            if(menu.GetComponent<IP_VR_RadialMenu>().menuMode != 0)
            {
                CopyOfObject.transform.parent = copyOriParent.transform;
            }
        }
        else 
        {

            transform.GetChild(0).position = new Vector3(0,-1000,0);
            transform.GetChild(1).position = new Vector3(0,-1000,0);
            transform.GetChild(2).position = new Vector3(0,-1000,0);
            firstFind = true;
            firstT = true; firstR = true; firstS = true; firstAS = true;
        }
    }
    
}
