// 这是主 DLL 文件。

#include "stdafx.h"
#include <bvt_sdk.h>
#include <unordered_map>
#include "TBVSonarC.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Drawing::Imaging;

namespace TBVSonarC {

	BVTHead head = NULL;
	BVTSonar son = NULL;
	std::unordered_map<void *, void *> dataHandles;

	BVTSonar TBVSonar::BVTSonar_Create_C()
	{
		return BVTSonar_Create();
	}

	RetVal TBVSonar::BVTSonar_Open_C(BVTSonar obj, const char* type, const char* type_params)
	{
		return BVTSonar_Open(obj, type, type_params);
	}

	int TBVSonar::BVTSonar_GetHeadCount_C(BVTSonar obj)
	{
		return BVTSonar_GetHeadCount(obj);
	}

	RetVal TBVSonar::BVTSonar_GetHead_C(BVTSonar obj, int head_num, BVTHead* head)
	{
		return BVTSonar_GetHead(obj, head_num, head);
	}

	int TBVSonar::InitSonar(const char* type_params)
	{
		int ret;
		// Create a new BVTSonar Object
		son = BVTSonar_Create();
		if (son == NULL)
		{
			return 1;
		}

		ret = BVTSonar_Open(son, "FILE", type_params);
		if (ret != 0)
		{
			return 1;
		}

		// Make sure we have the right number of heads
		int heads = -1;
		heads = BVTSonar_GetHeadCount(son);

		// Get the first head
		head = NULL;

		ret = BVTSonar_GetHead(son, 0, &head);
		if (ret != 0)
		{
			return 1;
		}

		// Check the ping count
		int pings = -1;
		pings = BVTHead_GetPingCount(head);

		// Check the min and max range in this file
		//printf("BVTHead_GetMinimumRange: %0.2f\n", BVTHead_GetMinimumRange(head));
		//printf("BVTHead_GetMaximumRange: %0.2f\n", BVTHead_GetMaximumRange(head));
	}

	float TBVSonar::GetMinimumRange()
	{
		return BVTHead_GetMinimumRange(head);
	}

	float TBVSonar::GetMaximumRange()
	{
		return BVTHead_GetMaximumRange(head);
	}

	int TBVSonar::GetImage()
	{
		int ret;
		BVTPing ping = NULL;
		ret = BVTHead_GetPing(head, 15, &ping);
		if (ret != 0)
		{
			return 1;
		}

		// Generate an image from the ping
		BVTMagImage img;
		ret = BVTPing_GetImageXY(ping, &img);
		if (ret != 0)
		{
			return 1;
		}

		// Check the image height and width out
		int height = BVTMagImage_GetHeight(img);
		//printf("BVTMagImage_GetHeight: %d\n", height);
		int width = BVTMagImage_GetWidth(img);
		//printf("BVTMagImage_GetWidth: %d\n", width);

		// Save it to a PGM (PortableGreyMap)
		//ret = BVTMagImage_SavePGM(img, "img.pgm");
		if (ret != 0)
		{
			//printf("BVTMagImage_SavePGM: ret=%d\n", ret);
			return 1;
		}


		// Build a color mapper
		BVTColorMapper mapper;
		mapper = BVTColorMapper_Create();
		if (mapper == NULL)
		{
			//printf("BVTColorMapper_Create: failed\n");
			return 1;
		}

		// Load the bone colormap
		ret = BVTColorMapper_Load(mapper, "copper.cmap");
		if (ret != 0)
		{
			//printf("BVTColorMapper_Load: ret=%d\n", ret);
			return 1;
		}


		// Perform the colormapping
		BVTColorImage cimg;
		ret = BVTColorMapper_MapImage(mapper, img, &cimg);
		if (ret != 0)
		{
			//printf("BVTColorMapper_MapImage: ret=%d\n", ret);
			return 1;
		}

		// Check the image height and width out
		height = BVTColorImage_GetHeight(cimg);
		//printf("BVTColorImage_GetHeight: %d\n", height);
		width = BVTColorImage_GetWidth(cimg);
		//printf("BVTColorImage_GetWidth: %d\n", width);


		// Save it to a PPM (PortablePixMap)
		ret = BVTColorImage_SavePPM(cimg, "cimg.ppm");

		
		if (ret != 0)
		{
			//printf("BVTColorImage_SavePPM: ret=%d\n", ret);
			return 1;
		}

		// Clean up
		BVTColorImage_Destroy(cimg);
		BVTMagImage_Destroy(img);
		BVTColorMapper_Destroy(mapper);
		BVTPing_Destroy(ping);
		BVTSonar_Destroy(son);
		return 0;
	}

}
