// TBVSonarC.h

#pragma once

#include <bvt_sdk.h>

using namespace System;

namespace TBVSonarC {


	public ref class TBVSonar
	{
	public:

		BVTSonar BVTSonar_Create_C();

		RetVal BVTSonar_Open_C(BVTSonar obj, const char* type, const char* type_params);

		int BVTSonar_GetHeadCount_C(BVTSonar obj);

		RetVal BVTSonar_GetHead_C(BVTSonar obj, int head_num, BVTHead* head);

		int InitSonar(const char*);

		float GetMinimumRange();

		float GetMaximumRange();

		int GetImage();
		// TODO:  在此处添加此类的方法。
	private:
	};
}
