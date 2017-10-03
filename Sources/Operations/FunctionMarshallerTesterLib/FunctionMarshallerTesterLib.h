#pragma once
#include <Windows.h>
#include <string>

using namespace std;

EXTERN_C {
	__declspec(dllexport) void __cdecl ProcessIntActionDelegate(void(*delegate)(int i));
	__declspec(dllexport) void __cdecl ProcessStringActionDelegate(void (*delegate)(const char* s));
}