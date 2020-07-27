using System;

namespace NRKernal
{
    public class NRKernalError : ApplicationException
    {
        protected string error;
        protected Exception innerException;

        public NRKernalError(string msg, Exception innerException = null) : base(msg)
        {
            this.innerException = innerException;
            this.error = msg;
        }
        public string GetError()
        {
            return error;
        }
    }

    public class NRInvalidArgumentError : NRKernalError
    {
        public NRInvalidArgumentError(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }

    public class NRNotEnoughMemoryError : NRKernalError
    {
        public NRNotEnoughMemoryError(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }

    public class NRSdcardPermissionDenyError : NRKernalError
    {
        public NRSdcardPermissionDenyError(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }

    public class NRUnSupportedError : NRKernalError
    {
        public NRUnSupportedError(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }

    public class NRGlassesConnectError : NRKernalError
    {
        public NRGlassesConnectError(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }

    public class NRSdkVersionMismatchError : NRKernalError
    {
        public NRSdkVersionMismatchError(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }

    public class NRRGBCameraDeviceNotFindError : NRKernalError
    {
        public NRRGBCameraDeviceNotFindError(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }

    public class NRDPDeviceNotFindError : NRKernalError
    {
        public NRDPDeviceNotFindError(string msg, Exception innerException = null) : base(msg, innerException)
        {
        }
    }
}
