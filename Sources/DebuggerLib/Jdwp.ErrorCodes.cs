namespace Dot42.DebuggerLib
{
    partial class Jdwp
    {
        public static class ErrorCodes
        {
            public const int NONE = 0; //	No error has occurred.  
            public const int INVALID_THREAD = 10; //	Passed thread is null, is not a valid thread or has exited.  
            public const int INVALID_THREAD_GROUP = 11; //	Thread group invalid.  
            public const int INVALID_PRIORITY = 12; //	Invalid priority.  
            public const int THREAD_NOT_SUSPENDED = 13; //	If the specified thread has not been suspended by an event.  
            public const int THREAD_SUSPENDED = 14; //	Thread already suspended.  
            public const int INVALID_OBJECT = 20; //	If this reference type has been unloaded and garbage collected.  
            public const int INVALID_CLASS = 21; //	Invalid class.  
            public const int CLASS_NOT_PREPARED = 22; //	Class has been loaded but not yet prepared.  
            public const int INVALID_METHODID = 23; //	Invalid method.  
            public const int INVALID_LOCATION = 24; //	Invalid location.  
            public const int INVALID_FIELDID = 25; //	Invalid field.  
            public const int INVALID_FRAMEID = 30; //	Invalid jframeID.  
            public const int NO_MORE_FRAMES = 31; //	There are no more Java or JNI frames on the call stack.  
            public const int OPAQUE_FRAME = 32; //	Information about the frame is not available.  
            public const int NOT_CURRENT_FRAME = 33; //	Operation can only be performed on current frame.  
            public const int TYPE_MISMATCH = 34; //	The variable is not an appropriate type for the function used.  
            public const int INVALID_SLOT = 35; //	Invalid slot.  
            public const int DUPLICATE = 40; //	Item already set.  
            public const int NOT_FOUND = 41; //	Desired element not found.  
            public const int INVALID_MONITOR = 50; //	Invalid monitor.  
            public const int NOT_MONITOR_OWNER = 51; //	This thread doesn't own the monitor.  
            public const int INTERRUPT = 52; //	The call has been interrupted before completion.  

            public const int INVALID_CLASS_FORMAT = 60;
                             //	The virtual machine attempted to read a class file and determined that the file is malformed or otherwise cannot be interpreted as a class file.  

            public const int CIRCULAR_CLASS_DEFINITION = 61;
                             //	A circularity has been detected while initializing a class.  

            public const int FAILS_VERIFICATION = 62;
                             //	The verifier detected that a class file, though well formed, contained some sort of internal inconsistency or security problem.  

            public const int ADD_METHOD_NOT_IMPLEMENTED = 63; //	Adding methods has not been implemented.  
            public const int SCHEMA_CHANGE_NOT_IMPLEMENTED = 64; //	Schema change has not been implemented.  

            public const int INVALID_TYPESTATE = 65;
                             //	The state of the thread has been modified, and is now inconsistent.  

            public const int HIERARCHY_CHANGE_NOT_IMPLEMENTED = 66;
                             //	A direct superclass is different for the new class version, or the set of directly implemented interfaces is different and canUnrestrictedlyRedefineClasses is false.  

            public const int DELETE_METHOD_NOT_IMPLEMENTED = 67;
                             //	The new class version does not declare a method declared in the old class version and canUnrestrictedlyRedefineClasses is false.  

            public const int UNSUPPORTED_VERSION = 68; //	A class file has a version number not supported by this VM.  

            public const int NAMES_DONT_MATCH = 69;
                             //	The class name defined in the new class file is different from the name in the old class object.  

            public const int CLASS_MODIFIERS_CHANGE_NOT_IMPLEMENTED = 70;
                             //	The new class version has different modifiers and and canUnrestrictedlyRedefineClasses is false.  

            public const int METHOD_MODIFIERS_CHANGE_NOT_IMPLEMENTED = 71;
                             //	A method in the new class version has different modifiers than its counterpart in the old class version and and canUnrestrictedlyRedefineClasses is false.  

            public const int NOT_IMPLEMENTED = 99; //	The functionality is not implemented in this virtual machine.  
            public const int NULL_POINTER = 100; //	Invalid pointer.  
            public const int ABSENT_INFORMATION = 101; //	Desired information is not available.  
            public const int INVALID_EVENT_TYPE = 102; //	The specified event type id is not recognized.  
            public const int ILLEGAL_ARGUMENT = 103; //	Illegal argument.  

            public const int OUT_OF_MEMORY = 110;
                             //	The function needed to allocate memory and no more memory was available for allocation.  

            public const int ACCESS_DENIED = 111;
                             //	Debugging has not been enabled in this virtual machine. JVMDI cannot be used.  

            public const int VM_DEAD = 112; //	The virtual machine is not running.  
            public const int INTERNAL = 113; //	An unexpected internal error has occurred.  

            public const int UNATTACHED_THREAD = 115;
                             //	The thread being used to call this function is not attached to the virtual machine. Calls must be made from attached threads.  

            public const int INVALID_TAG = 500; //	object type id or class tag.  
            public const int ALREADY_INVOKING = 502; //	Previous invoke not complete.  
            public const int INVALID_INDEX = 503; //	Index is invalid.  
            public const int INVALID_LENGTH = 504; //	The length is invalid.  
            public const int INVALID_STRING = 506; //	The string is invalid.  
            public const int INVALID_CLASS_LOADER = 507; //	The class loader is invalid.  
            public const int INVALID_ARRAY = 508; //	The array is invalid.  
            public const int TRANSPORT_LOAD = 509; //	Unable to load the transport.  
            public const int TRANSPORT_INIT = 510; //	Unable to initialize the transport.  
            public const int NATIVE_METHOD = 511;
            public const int INVALID_COUNT = 512; //	The count is invalid.  
        }
    }
}
