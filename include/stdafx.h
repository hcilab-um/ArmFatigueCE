// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#ifdef _WINDOWS

#if ARMFATIGUECE_EXPORTS
#define EXPORT_OR_IMPORT __declspec(dllexport)
#else
#define EXPORT_OR_IMPORT __declspec(dllimport)
#endif

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers

// Windows Header Files:
#include <windows.h>

#else //Compiling for MacOS
#define EXPORT_OR_IMPORT //This is defined as empty so that all clases compile properly
#endif
