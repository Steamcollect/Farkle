using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SelectionOutline
{
    [Serializable]
    [VolumeComponentMenu("Selection Outline")]
    public class SelectableOutlineVolume : VolumeComponent
    {
        public ColorParameter OutlineColor = new(Color.red);
        public FloatParameter OutlineWidth = new(1f);
    }
}