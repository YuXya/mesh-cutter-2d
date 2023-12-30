using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class DragMove : MonoBehaviour
{
    bool isDraw = false;

    public Transform draw_target;

    public Vector3 offset;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isDraw)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit))
            {
                CreateTarget(hit);
            }

        }
        if (isDraw)
        {
            MoveTarget();
        }
        if (Input.GetMouseButtonUp(0) && isDraw)
        {
            isDraw = false;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Tools.RestoreMeshAction(0.5f);
        }
    }

    public void CreateTarget(RaycastHit hit)
    {
        isDraw = true;
        draw_target = hit.transform;
        Vector3 startPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        offset = startPoint - draw_target.position;
    }

    public void MoveTarget()
    {
        Vector3 endPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        draw_target.position = endPoint - offset;
    }
}
