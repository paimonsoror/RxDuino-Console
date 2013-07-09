/************************************************************************
 * RXDUINO Software, All Rights Reserved 2012
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *************************************************************************/
 
using System;
using System.Text;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class Win32DeviceMgt
{
    private const UInt32 DIGCF_PRESENT = 0x00000002;
    private const UInt32 DIGCF_DEVICEINTERFACE = 0x00000010;
    private const UInt32 SPDRP_DEVICEDESC = 0x00000000;
    private const UInt32 DICS_FLAG_GLOBAL = 0x00000001;
    private const UInt32 DIREG_DEV = 0x00000001;
    private const UInt32 KEY_QUERY_VALUE = 0x0001;
    private const string GUID_DEVINTERFACE_COMPORT = "86E0D1E0-8089-11D0-9CE4-08003E301F73";

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_DEVINFO_DATA
    {
        public Int32  cbSize;
        public Guid   ClassGuid;
        public Int32  DevInst;
        public UIntPtr Reserved;
    };

    [DllImport("setupapi.dll")]
    private static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("setupapi.dll")]
    private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, Int32 MemberIndex, ref  SP_DEVINFO_DATA DeviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData,
        uint property, out UInt32 propertyRegDataType, StringBuilder propertyBuffer, uint propertyBufferSize, out UInt32 requiredSize);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, UInt32 iEnumerator, IntPtr hParent, UInt32 nFlags);

    [DllImport("Setupapi", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, uint scope,
        uint hwProfile, uint parameterRegistryValueKind, uint samDesired);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
    private static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, out uint lpType,
        StringBuilder lpData, ref uint lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    private static extern int RegCloseKey(IntPtr hKey);

    [DllImport("kernel32.dll")]
    private static extern Int32 GetLastError();

    public struct DeviceInfo
    {
        public string name;
        public string decsription;
    }

    public static List<DeviceInfo> GetAllCOMPorts()
    {
        Guid guidComPorts = new Guid(GUID_DEVINTERFACE_COMPORT);
        IntPtr hDeviceInfoSet = SetupDiGetClassDevs(
            ref guidComPorts, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
        if (hDeviceInfoSet == IntPtr.Zero)
        {
            throw new Exception("Failed to get device information set for the COM ports");
        }

        try {
            List<DeviceInfo> devices = new List<DeviceInfo>();
            Int32 iMemberIndex = 0;
            while (true)
            {
                SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
                deviceInfoData.cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
                bool success = SetupDiEnumDeviceInfo(hDeviceInfoSet, iMemberIndex, ref deviceInfoData);
                if (!success)
                {
                    // No more devices in the device information set
                    break;
                }

                DeviceInfo deviceInfo = new DeviceInfo();
                deviceInfo.name = GetDeviceName(hDeviceInfoSet, deviceInfoData);
                deviceInfo.decsription = GetDeviceDescription(hDeviceInfoSet, deviceInfoData);
                devices.Add(deviceInfo);

                iMemberIndex++;
            }
            return devices;
        }
        finally {
            SetupDiDestroyDeviceInfoList(hDeviceInfoSet);
        }
    }

    private static string GetDeviceName(IntPtr pDevInfoSet, SP_DEVINFO_DATA deviceInfoData)
    {
        IntPtr hDeviceRegistryKey = SetupDiOpenDevRegKey(pDevInfoSet, ref deviceInfoData,
            DICS_FLAG_GLOBAL, 0, DIREG_DEV, KEY_QUERY_VALUE);
        if (hDeviceRegistryKey == IntPtr.Zero)
        {
            throw new Exception("Failed to open a registry key for device-specific configuration information");
        }

        StringBuilder deviceNameBuf = new StringBuilder(256);
        try
        {
            uint lpRegKeyType;
            uint length = (uint)deviceNameBuf.Capacity;
            int result = RegQueryValueEx(hDeviceRegistryKey, "PortName", 0, out lpRegKeyType, deviceNameBuf, ref length);
            if (result != 0)
            {
                throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
            }
        }
        finally
        {
            RegCloseKey(hDeviceRegistryKey);
        }

        string deviceName = deviceNameBuf.ToString();
        return deviceName;
    }

    private static string GetDeviceDescription(IntPtr hDeviceInfoSet, SP_DEVINFO_DATA deviceInfoData)
    {
        StringBuilder descriptionBuf = new StringBuilder(256);
        uint propRegDataType;
        uint length = (uint)descriptionBuf.Capacity;
        bool success = SetupDiGetDeviceRegistryProperty(hDeviceInfoSet, ref deviceInfoData, SPDRP_DEVICEDESC,
            out propRegDataType, descriptionBuf, length, out length);
        if (!success)
        {
            throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
        }
        string deviceDescription = descriptionBuf.ToString();
        return deviceDescription;
    }

}