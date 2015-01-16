#include <string.h>
#include <jni.h>

jstring Java_dllImportTest_MainActivity_Foo( JNIEnv* env, jobject thiz)
{
    return (*env)->NewStringUTF(env, "Hello from dot42 JNI !");
}
