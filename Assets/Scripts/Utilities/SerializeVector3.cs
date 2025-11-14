using UnityEngine;

[System.Serializable]
public class SerializeVector3       //转换Vector3序列化保存
{
    public float x, y, z;

    public SerializeVector3(Vector3 vec)
    {
        x= vec.x;
        y= vec.y;
        z= vec.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
    
}
