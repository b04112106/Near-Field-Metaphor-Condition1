using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IndiePixel.VR;
using VRTK;
public class OriginalBoundingBox : MonoBehaviour
{
    // public GameObject Edge;
    private Vector3 originalLocalScale;
    private float origianlRadius;
    public bool doUnTouch = true;
    private GameObject menu;
    // Start is called before the first frame update
    void Start()
    {
        originalLocalScale = transform.localScale;
        origianlRadius = GetComponent<CapsuleCollider>().radius;
        menu = GameObject.Find("RadialMenu_Canvas");
    }

    public void OnTouch()
    {
        // if(menu.GetComponent<IP_VR_RadialMenu>().menuMode == 0)
        // {
        //     Edge.SetActive(true);
        //     this.gameObject.SetActive(false);
        //     return;
        // }
        
        // Debug.Log("Touch");
        originalLocalScale = transform.localScale;
        origianlRadius = GetComponent<CapsuleCollider>().radius;
        transform.localScale = new Vector3(originalLocalScale.x*5f, originalLocalScale.y, originalLocalScale.z*5f);
        GetComponent<VRTK_InteractObjectHighlighter>().objectToHighlight.transform.localScale = transform.localScale;
        GetComponent<CapsuleCollider>().radius = origianlRadius / 3f;
    }
    public void OnUnTouch()
    {
        
        // Debug.Log(transform.localScale.x);
        if(doUnTouch)
        {
            transform.localScale = originalLocalScale;
            GetComponent<VRTK_InteractObjectHighlighter>().objectToHighlight.transform.localScale = transform.localScale;
        }
        else
        {    
            doUnTouch = true;
        }
        GetComponent<CapsuleCollider>().radius = origianlRadius;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
