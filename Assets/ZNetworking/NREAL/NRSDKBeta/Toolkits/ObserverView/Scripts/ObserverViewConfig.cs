/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal.ObserverView
{
    public struct ObserverViewConfig
    {
        public string serverIP;
        public bool useDebugUI;

        public ObserverViewConfig(string serverip, bool usedebug = false)
        {
            this.serverIP = serverip;
            this.useDebugUI = usedebug;
        }
    }
}
