using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

public static class Tools
{
    public static bool InnerGraphByAngle(Vector2 point, Vector2[] polygon)
    {
        List<Vector2> edges = new List<Vector2>();
        for (int i = 0; i < polygon.Length; i++)
        {
            edges.Add(polygon[i] - point);
        }

        float angle = 0;
        for (int i = 0; i < edges.Count - 1; i++)
        {
            angle += Vector2.Angle(edges[i], edges[i + 1]);
        }
        angle += Vector2.Angle(edges[0], edges[edges.Count - 1]);
        if (angle > 359 && angle < 361)
        {
            return true;
        }
        else
        {
            return false;
        }  

    }

    public static bool GetCrossPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d,out Vector2 result)
    {
        result = Vector2.zero;
        /** 1 解线性方程组, 求线段交点. **/
        // 如果分母为0 则平行或共线, 不相交  
        double denominator = (b.y - a.y) * (d.x - c.x) - (a.x - b.x) * (c.y - d.y);
        if (denominator == 0)
        {
            return false;
        }

        // 线段所在直线的交点坐标 (x , y)      
        double x = ((b.x - a.x) * (d.x - c.x) * (c.y - a.y)
                    + (b.y - a.y) * (d.x - c.x) * a.x
                    - (d.y - c.y) * (b.x - a.x) * c.x) / denominator;
        double y = -((b.y - a.y) * (d.y - c.y) * (c.x - a.x)
                    + (b.x - a.x) * (d.y - c.y) * a.y
                    - (d.x - c.x) * (b.y - a.y) * c.y) / denominator;

        /** 2 判断交点是否在两条线段上 **/
        if (
            // 交点在线段1上  
            (x - a.x) * (x - b.x) <= 0 && (y - a.y) * (y - b.y) <= 0
             // 且交点也在线段2上  
             && (x - c.x) * (x - d.x) <= 0 && (y - c.y) * (y - d.y) <= 0
            )
        {

            // 返回交点p  
            result = new Vector2((float)x, (float)y);
            return true;
        }
        //否则不相交  
        return false;


    }
    /// <summary>
    /// 分辨在ab这条线的哪一边
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    public static bool LeftOfLine(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
    }

    public static Action<float> RestoreMeshEvent;

    public static void RestoreMeshAction(float time)
    {
        RestoreMeshEvent?.Invoke(time);
    }

}

public class MyTempMesh
{
    public List<Vector3> vertList = new List<Vector3>(64);
    public List<int> triList = new List<int>(64 * 3);
    public List<Vector2> uvs = new List<Vector2>(64);

    public Dictionary<int, int> vMapping = new Dictionary<int, int>();

    public void AddTriangles(List<Vector3> m_vertList, List<Vector2> m_uvs, List<int> m_triList, int index)
    {
        for (int i = index; i < index + 3; i++)
        {
            if (!vMapping.ContainsKey(m_triList[i]))
            {
                vMapping[m_triList[i]] = vertList.Count;
                vertList.Add(m_vertList[m_triList[i]]);
                uvs.Add(m_uvs[m_triList[i]]);
            }
            triList.Add(vMapping[m_triList[i]]);
        }
    }

}