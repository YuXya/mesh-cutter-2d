using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceObject : MonoBehaviour
{
    private Vector2 result;

    bool is_move = false;
    float move_time = 0;
    float move_time_index = 0;
    Vector3 start_point;

    Vector3 target_point = Vector3.zero;
    private void OnEnable()
    {
        Tools.RestoreMeshEvent += OnRestoreMeshEvent;
    }

    private void OnDisable()
    {
        Tools.RestoreMeshEvent -= OnRestoreMeshEvent;
    }

    private void Update()
    {
        if (is_move)
        {
            move_time_index += Time.deltaTime;
            if (move_time_index < move_time)
            {
                transform.position = Vector3.Lerp(start_point, target_point, move_time_index / move_time);
            }
            else
            {
                move_time_index = 0;
                transform.position = target_point;
                is_move = false;
            }
        }
    }

    private void OnRestoreMeshEvent(float time)
    {
        move_time = time;
        is_move = true;
        start_point = transform.position;
    }

    public void Slice(Vector2 startPoint, Vector2 endPoint)
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        //顶点列表
        List<Vector3> vertList = new List<Vector3>(mf.mesh.vertices);
        List<int> triList = new List<int>(mf.mesh.triangles);
        List<Vector3> normals = new List<Vector3>(mf.mesh.normals);
        List<Vector2> uvs = new List<Vector2>(mf.mesh.uv);

        int vectorsCount = vertList.Count;
        #region 判断是否为有效切割
        bool isCross = false;
        for (int i = 0; i < triList.Count; i += 3)
        {
            int triPoint0 = triList[i];
            int triPoint1 = triList[i + 1];
            int triPoint2 = triList[i + 2];

            Vector2 point0 = vertList[triPoint0];
            Vector2 point1 = vertList[triPoint1];
            Vector2 point2 = vertList[triPoint2];
            Vector2 crossPoint;
            if (Tools.GetCrossPoint(startPoint, endPoint, point0, point1, out crossPoint) ||
                Tools.GetCrossPoint(startPoint, endPoint, point1, point2, out crossPoint) ||
                Tools.GetCrossPoint(startPoint, endPoint, point2, point0, out crossPoint))
            {
                isCross = true;
            }

            //如有一个点或者两个点在图片内，就是无效切割
            if (Tools.InnerGraphByAngle(startPoint, new Vector2[] { point0, point1, point2 }) ||
                Tools.InnerGraphByAngle(endPoint, new Vector2[] { point0, point1, point2 }))
            {
                print("不完整切割");
                return;
            }
        }
        if (!isCross)
        {
            print("无效切割，没有交点");
            return;
        }
        #endregion

        #region 切割

        for (int i = 0; i < triList.Count; i += 3)
        {
            int triPoint0 = triList[i];
            int triPoint1 = triList[i + 1];
            int triPoint2 = triList[i + 2];

            Vector2 point0 = vertList[triPoint0];
            Vector2 point1 = vertList[triPoint1];
            Vector2 point2 = vertList[triPoint2];

            Vector2 crossPoint0_1, crossPoint0_2, crossPoint1_2;

            //0-1,1-2两条边被切割
            if (Tools.GetCrossPoint(startPoint, endPoint, point0, point1, out crossPoint0_1) &&
                Tools.GetCrossPoint(startPoint, endPoint, point1, point2, out crossPoint1_2))
            {

                vertList.Add(crossPoint0_1);
                vertList.Add(crossPoint1_2);
                uvs.Add(GetUVOfsset(uvs[triPoint0], uvs[triPoint1], point0, point1, crossPoint0_1));
                uvs.Add(GetUVOfsset(uvs[triPoint1], uvs[triPoint2], point1, point2, crossPoint1_2));

                //0-1-2 => 0AB-BA1-0B2
                triList.Insert(i + 1, vertList.Count - 2);//A
                triList.Insert(i + 2, vertList.Count - 1);//B
                triList.Insert(i + 3, vertList.Count - 1);//B
                triList.Insert(i + 4, vertList.Count - 2);//A
                triList.Insert(i + 6, triPoint0);//0
                triList.Insert(i + 7, vertList.Count - 1);//B

                i += 6;
            }//1-2 2-0
            else if (Tools.GetCrossPoint(startPoint, endPoint, point1, point2, out crossPoint1_2) &&
                Tools.GetCrossPoint(startPoint, endPoint, point2, point0, out crossPoint0_2))
            {
                vertList.Add(crossPoint1_2);
                vertList.Add(crossPoint0_2);
                uvs.Add(GetUVOfsset(uvs[triPoint1], uvs[triPoint2], point1, point2, crossPoint1_2));
                uvs.Add(GetUVOfsset(uvs[triPoint0], uvs[triPoint2], point0, point2, crossPoint0_2));

                //0-1-2 => 01B-AB2-A0B
                triList.Insert(i + 2, vertList.Count - 2);

                triList.Insert(i + 3, vertList.Count - 1);
                triList.Insert(i + 4, vertList.Count - 2);

                triList.Insert(i + 6, vertList.Count - 1);
                triList.Insert(i + 7, triPoint0);
                triList.Insert(i + 8, vertList.Count - 2);

                i += 6;
            }//0-1 0-2
            else if (Tools.GetCrossPoint(startPoint, endPoint, point0, point1, out crossPoint0_1) &&
                Tools.GetCrossPoint(startPoint, endPoint, point0, point2, out crossPoint0_2))
            {
                vertList.Add(crossPoint0_1);
                vertList.Add(crossPoint0_2);
                uvs.Add(GetUVOfsset(uvs[triPoint0], uvs[triPoint1], point0, point1, crossPoint0_1));
                uvs.Add(GetUVOfsset(uvs[triPoint0], uvs[triPoint2], point0, point2, crossPoint0_2));

                //
                triList.Insert(i + 1, vertList.Count - 2);
                triList.Insert(i + 2, vertList.Count - 1);
                triList.Insert(i + 3, vertList.Count - 2);
                triList.Insert(i + 6, triPoint2);
                triList.Insert(i + 7, vertList.Count - 1);
                triList.Insert(i + 8, vertList.Count - 2);

                i += 6;
            }
        }
        #endregion

        #region 分离成两个mesh网格

        MyTempMesh LeftMesh = new MyTempMesh();
        MyTempMesh RightMesh = new MyTempMesh();

        for (int i = 0; i < triList.Count; i += 3)
        {
            Vector2 middle = (vertList[triList[i]] + vertList[triList[i + 1]] + vertList[triList[i + 2]]) / 3;
            bool is_left = Tools.LeftOfLine(startPoint, endPoint, middle);
            if (is_left)
            {
                LeftMesh.AddTriangles(vertList, uvs, triList, i);
            }
            else
            {
                RightMesh.AddTriangles(vertList, uvs, triList, i);
            }
        }

        GameObject right_obj = Instantiate(gameObject, transform.position, transform.rotation);
        SliceObject so = right_obj.GetComponent<SliceObject>();
        so.SetMeshFilter(RightMesh.vertList, RightMesh.uvs, RightMesh.triList);
        SetMeshFilter(LeftMesh.vertList, LeftMesh.uvs, LeftMesh.triList);

        //旋转线段90度
        Vector2 normal_2d = (Quaternion.AngleAxis(90, Vector3.forward) * (startPoint - endPoint)).normalized;
        right_obj.transform.position += (Vector3)normal_2d * 0.2f;
        transform.position -= (Vector3)normal_2d * 0.2f;
        #endregion
    }

    public Vector2 GetUVOfsset(Vector2 startUV,Vector2 endUV,Vector2 startPoint,Vector2 endPoint,Vector2 middlePoint)
    {
        float relativeDist = (startPoint - middlePoint).magnitude / (startPoint - endPoint).magnitude;
        Vector2 offset = Vector2.zero;
        offset.x = Mathf.Lerp(startUV.x, endUV.x, relativeDist);
        offset.y = Mathf.Lerp(startUV.y, endUV.y, relativeDist);
        return offset;
    }

    public void SetMeshFilter(List<Vector3> m_vertList, List<Vector2> m_uvs, List<int> m_triList)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh.Clear();
        mf.mesh.SetVertices(m_vertList);
        mf.mesh.SetTriangles(m_triList, 0);
        mf.mesh.SetUVs(0, m_uvs);
        mf.mesh.RecalculateNormals();

        GetComponent<MeshCollider>().sharedMesh = mf.mesh;
    }
}
