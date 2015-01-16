using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

namespace Case662
{
 public struct HRESULT 
{ 
private enum EHRESULT : uint 
{ 
S_OK = 0, 
S_FALSE = 1, 

E_UNEXPECTED = 0x8000FFFF, 
E_NOTIMPL = 0x80004001, 
E_OUTOFMEMORY = 0x8007000E, 
E_INVALIDARG = 0x80070057, 
E_NOINTERFACE = 0x80004002, 
E_POINTER = 0x80004003, 
E_HANDLE = 0x80070006, 
E_ABORT = 0x80004004, 
E_FAIL = 0x80004005, 
E_ACCESSDENIED = 0x80070005, 
} 

public const uint S_OK = 0; 
public const uint S_FALSE = 1; 

public const uint E_UNEXPECTED = 0x8000FFFF; 
public const uint E_NOTIMPL = 0x80004001; 
public const uint E_OUTOFMEMORY = 0x8007000E; 
public const uint E_INVALIDARG = 0x80070057; 
public const uint E_NOINTERFACE = 0x80004002; 
public const uint E_POINTER = 0x80004003; 
public const uint E_HANDLE = 0x80070006; 
public const uint E_ABORT = 0x80004004; 
public const uint E_FAIL = 0x80004005; 
public const uint E_ACCESSDENIED = 0x80070005; 

public uint Code; 

public HRESULT(uint hr) 
{ 
Code = hr; 
} 

public static implicit operator HRESULT(int code) 
{ 
return new HRESULT((uint)code); 
} 

public static implicit operator HRESULT(uint code) 
{ 
return new HRESULT(code); 
} 

public static implicit operator uint(HRESULT hr) 
{ 
return hr.Code; 
} 

public static bool SUCCEEDED(HRESULT hr) 
{ 
return ((hr.Code & 0x80000000) == 0); 
} 

public static bool FAILED(HRESULT hr) 
{ 
return ((hr.Code & 0x80000000) != 0); 
} 

public override string ToString() 
{ 
EHRESULT hr = (EHRESULT)Code; 
return String.Format("{0} ({1:X})", hr, Code); 
} 

public void ThrowExceptionIfFailed() 
{ 
//System.Runtime.InteropServices.Marshal.ThrowExceptionForHR((int)Code); 
if (FAILED(this)) 
{ 
//throw new System.Runtime.InteropServices.COMException(null, (int)Code); 
int code = (int)Code; 
throw new Exception("COMException " + code.ToString()); 
} 
} 
} 
}
