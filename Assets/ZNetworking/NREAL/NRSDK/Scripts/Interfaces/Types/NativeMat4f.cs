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

    using System.Runtime.InteropServices;
    using UnityEngine;

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMat4f
    {
        [MarshalAs(UnmanagedType.Struct)]
        public NativeVector4f column0;
        [MarshalAs(UnmanagedType.Struct)]
        public NativeVector4f column1;
        [MarshalAs(UnmanagedType.Struct)]
        public NativeVector4f column2;
        [MarshalAs(UnmanagedType.Struct)]
        public NativeVector4f column3;

        public NativeMat4f(Matrix4x4 m)
        {
            column0 = new NativeVector4f(m.GetColumn(0));
            column1 = new NativeVector4f(m.GetColumn(1));
            column2 = new NativeVector4f(m.GetColumn(2));
            column3 = new NativeVector4f(m.GetColumn(3));
        }

        public Matrix4x4 ToUnityMat4f()
        {
            Matrix4x4 m = new Matrix4x4();
            m.SetColumn(0, column0.ToUnityVector4());
            m.SetColumn(1, column1.ToUnityVector4());
            m.SetColumn(2, column2.ToUnityVector4());
            m.SetColumn(3, column3.ToUnityVector4());
            return m;
        }

        public static NativeMat4f identity
        {
            get
            {
                return new NativeMat4f(Matrix4x4.identity);
            }
        }

        public override string ToString()
        {
            return string.Format("column0:{0}\ncolumn1:{1}\ncolumn2:{2}\ncolumn3:{3}\n",
                column0.ToString(), column1.ToString(), column2.ToString(), column3.ToString());
        }

        public float this[int row, int column]
        {
            get
            {
                return this[row * 4 + column];
            }
            set
            {
                this[row * 4 + column] = value;
            }
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.column0.X;
                    case 1:
                        return this.column1.X;
                    case 2:
                        return this.column2.X;
                    case 3:
                        return this.column3.X;
                    case 4:
                        return this.column0.Y;
                    case 5:
                        return this.column1.Y;
                    case 6:
                        return this.column2.Y;
                    case 7:
                        return this.column3.Y;
                    case 8:
                        return this.column0.Z;
                    case 9:
                        return this.column1.Z;
                    case 10:
                        return this.column2.Z;
                    case 11:
                        return this.column3.Z;
                    case 12:
                        return this.column0.W;
                    case 13:
                        return this.column1.W;
                    case 14:
                        return this.column2.W;
                    case 15:
                        return this.column3.W;
                    default:
                        throw new System.IndexOutOfRangeException("Invalid matrix index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.column0.X = value;
                        break;
                    case 1:
                        this.column1.X = value;
                        break;
                    case 2:
                        this.column2.X = value;
                        break;
                    case 3:
                        this.column3.X = value;
                        break;
                    case 4:
                        this.column0.Y = value;
                        break;
                    case 5:
                        this.column1.Y = value;
                        break;
                    case 6:
                        this.column2.Y = value;
                        break;
                    case 7:
                        this.column3.Y = value;
                        break;
                    case 8:
                        this.column0.Z = value;
                        break;
                    case 9:
                        this.column1.Z = value;
                        break;
                    case 10:
                        this.column2.Z = value;
                        break;
                    case 11:
                        this.column3.Z = value;
                        break;
                    case 12:
                        this.column0.W = value;
                        break;
                    case 13:
                        this.column1.W = value;
                        break;
                    case 14:
                        this.column2.W = value;
                        break;
                    case 15:
                        this.column3.W = value;
                        break;
                    default:
                        throw new System.IndexOutOfRangeException("Invalid matrix index!");
                }
            }
        }
    }
}
