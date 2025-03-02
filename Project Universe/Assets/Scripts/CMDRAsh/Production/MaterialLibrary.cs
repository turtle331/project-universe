﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Data.Libraries
{

    public class MaterialLibrary : MonoBehaviour
    {
        public Material[] doorMaterials;
        public Material[] displayMaterials;
        public Material[] powerMaterials;

        //[SerializeField]
        private static Material[] doorStateMaterials;
        //[SerializeField]
        private static Material[] doorDisplayMaterials;
        private static Material[] powerStateMaterials;
        private static MaterialPropertyBlock commonLightPropertyBlock;
        void Awake()
        {
            doorStateMaterials = doorMaterials;
            doorDisplayMaterials = displayMaterials;
            powerStateMaterials = powerMaterials;

            commonLightPropertyBlock = new MaterialPropertyBlock();

        }

        public static Material[] GetDoorStateMaterials()
        {
            return doorStateMaterials;
        }
        public static Material GetDoorStateMaterials(int index)
        {
            return doorStateMaterials[index];
        }
        public static Material[] GetDoorDisplayMaterials()
        {
            return doorDisplayMaterials;
        }
        public static Material GetDoorDisplayMaterials(int index)
        {
            return doorDisplayMaterials[index];
        }
        public static Material[] GetPowerSystemStateMaterials()
        {
            return powerStateMaterials;
        }
        public static Material GetPowerSystemStateMaterials(int index)
        {
            return powerStateMaterials[index];
        }
        public static MaterialPropertyBlock GetMaterialPropertyBlockForCommonLights()
        {
            return commonLightPropertyBlock;
        }
    }
}
