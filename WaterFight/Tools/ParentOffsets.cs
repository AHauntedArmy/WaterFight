using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;

using UnityEngine;

namespace WaterFight.Tools
{
    internal class ParentOffsets : MonoBehaviour
    {
        public GameObject RightHandParent => rightHandParent;
        [SerializeField] GameObject rightHandParent;

        public GameObject LeftHandParent => leftHandParent;
        [SerializeField] GameObject leftHandParent;
    }
}
