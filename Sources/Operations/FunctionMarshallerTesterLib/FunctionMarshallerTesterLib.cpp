#pragma once
#include "FunctionMarshallerTesterLib.h"
#include <iostream>

void __cdecl ProcessIntActionDelegate(void(*delegate)(int i)) {
	delegate(8);
}
void __cdecl ProcessStringActionDelegate(void (*delegate)(const char* s)) {
	delegate("Hi from C++ ;)");
}