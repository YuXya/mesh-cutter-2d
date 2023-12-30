using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutLine : MonoBehaviour
{
    LineRenderer line;
    Vector3 startPoint;
    Vector3 endPoint;
    bool isDraw = false;

    public SliceObject[] slice_objects;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isDraw) 
        {
            isDraw = true;
            line.positionCount = 0;
            startPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            line.positionCount = 2;
            line.SetPosition(0, startPoint);
        }
        if (isDraw)
        {
            endPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            line.SetPosition(1, endPoint);
        }
        if (Input.GetMouseButtonUp(1) && isDraw)
        {
            isDraw = false;
            Slice();
        }
    }

    public void Slice()
    {
        slice_objects = GameObject.FindObjectsOfType<SliceObject>();
        for (int i = 0; i < slice_objects.Length; i++)
        {
            Vector3 startPointInLocal = slice_objects[i].transform.InverseTransformPoint(startPoint);
            Vector3 endPointInLocal = slice_objects[i].transform.InverseTransformPoint(endPoint);
            slice_objects[i].Slice(startPointInLocal, endPointInLocal);
        }
    }
}
