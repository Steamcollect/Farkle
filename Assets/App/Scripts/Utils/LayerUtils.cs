using UnityEngine;

public static class LayerUtils
{
    public static LayerMask ToLayerMask(this int layer)
    {
        return 1 << layer;
    }

    public static int ToLayer(this LayerMask layerMask)
    {
        return (int)Mathf.Log(layerMask.value, 2);
    }
}