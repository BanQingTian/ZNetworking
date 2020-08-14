/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /**
    * @brief Native PointCloud API.
    */
    internal partial class NativePointCloud
    {
        private UInt64 m_DatabaseHandle;

        public bool Create()
        {
            var result = NativeApi.NRPointCloudCreat(ref m_DatabaseHandle);
            Debug.Log("[NativePointCloud] End to create worldmap :" + result);
            return result == NativeResult.Success;
        }

        public bool IsUpdatedThisFrame()
        {
            bool result = NativeApi.NRPointCloudIsUpdated(m_DatabaseHandle);
            Debug.Log("[NativePointCloud] IsUpdatedThisFrame :" + result);
            return result;
        }

        public UInt64 CreatPointCloudList()
        {
            UInt64 pointlistHandle = 0;
            NativeApi.NRPointCloudListCreat(m_DatabaseHandle, ref pointlistHandle);
            Debug.Log("[NativePointCloud] create list :" + pointlistHandle);
            return pointlistHandle;
        }

        public void UpdatePointCloudList(UInt64 pointlistHandle)
        {
            Debug.Log("[NativePointCloud] UpdatePointCloudList");
            NativeApi.NRPointCloudListUpdate(m_DatabaseHandle, pointlistHandle);
        }

        public void DestroyPointCloudList(UInt64 pointlistHandle)
        {
            NativeApi.NRPointCloudListDestroy(m_DatabaseHandle, pointlistHandle);
            Debug.Log("[NativePointCloud] destroy list :" + pointlistHandle);
        }

        public int GetSize(UInt64 pointlistHandle)
        {
            int size = 0;
            NativeApi.NRPointCloudListGetSize(m_DatabaseHandle, pointlistHandle, ref size);
            Debug.Log("[NativePointCloud] get size :" + size);
            return size;
        }

        public PointCloudPoint AquireItem(UInt64 pointlistHandle, int index)
        {
            PointCloudPoint point = new PointCloudPoint();
            NativeApi.NRPointCloudListAquireItem(m_DatabaseHandle, pointlistHandle, index, ref point);
            // Transform thr pos from oepngl to unity
            point.Position.Z = -point.Position.Z;
            return point;
        }

        public bool SaveMap(string path)
        {
            var result = NativeApi.NRPointCloudSave(m_DatabaseHandle, path);
            Debug.Log("[NativePointCloud] Save worldmap :" + result);
            return result == NativeResult.Success;
        }

        public bool Destroy()
        {
            var result = NativeApi.NRPointCloudDestroy(m_DatabaseHandle);
            Debug.Log("[NativePointCloud] End to destroy worldmap :" + result);
            return result == NativeResult.Success;
        }

        private partial struct NativeApi
        {
            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern NativeResult NRPointCloudCreat(ref UInt64 world_map_database_handle);

            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern NativeResult NRPointCloudListCreat(UInt64 world_map_database_handle, ref UInt64 points_list_handle);

            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern NativeResult NRPointCloudListUpdate(UInt64 world_map_database_handle, UInt64 points_list_handle);

            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern NativeResult NRPointCloudListGetSize(UInt64 world_map_database_handle, UInt64 points_list_handle, ref int size);

            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern NativeResult NRPointCloudListDestroy(UInt64 world_map_database_handle, UInt64 points_list_handle);

            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern NativeResult NRPointCloudListAquireItem(UInt64 world_map_database_handle, UInt64 points_list_handle, int index, ref PointCloudPoint point);

            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern bool NRPointCloudIsUpdated(UInt64 world_map_database_handle);

            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern NativeResult NRPointCloudSave(UInt64 world_map_database_handle, string world_map_database_file_path);

            [DllImport(NativeConstants.NRNativeAnchorPointLib)]
            public static extern NativeResult NRPointCloudDestroy(UInt64 world_map_database_handle);
        }
    }
}
