/*
    This file has been generated by bvtidl.pl. DO NOT MODIFY!
*/
#ifndef __CPP_BVTCOLORIMAGE_H__
#define __CPP_BVTCOLORIMAGE_H__

#include <string>
#include <bvt_cpp/bvt_retval.h>

namespace BVTSDK
{

/** Store a color image.  The API is nearly identical to MagImage.  The main difference is the 
 * pixel datatype. In ColorImage, each pixel is a single unsigned int. 
 * - Byte 0: Red Value
 * - Byte 1: Green Value
 * - Byte 2: Blue Value
 * - Byte 3: Alpha Value
 */
class ColorImage
{
public:
	/// Create the object
	ColorImage()
	{ m_ptr = BVTColorImage_Create(); }

	/// Destroy the object
	~ColorImage()
	{ BVTColorImage_Destroy(m_ptr); }

#ifndef DOXY_IGNORE
	ColorImage(BVTColorImage ptr)
	{ m_ptr = ptr; }

	operator BVTColorImage()
	{ return m_ptr; }
	operator BVTColorImage*()
	{ return &m_ptr; }
	operator const BVTColorImage() const
	{ return m_ptr; }
#endif

	/** Return the height (in pixels) of this image
	 */
	int GetHeight()
	{
		return BVTColorImage_GetHeight( m_ptr );
	}

	/** Return the width (in pixels) of this image
	 */
	int GetWidth()
	{
		return BVTColorImage_GetWidth( m_ptr );
	}

	/** Return the range resolution of this image.
	 * The resolution is returned in meters per pixel
	 */
	double GetRangeResolution()
	{
		return BVTColorImage_GetRangeResolution( m_ptr );
	}

	/** Retrieve the image row of the origin.
	 * In most cases the origin will be outside of the image boundaries.  
	 * The origin is the 'location' (in pixels) of the sonar head in image plane
	 */
	int GetOriginRow()
	{
		return BVTColorImage_GetOriginRow( m_ptr );
	}

	/** Retrieve the image column of the origin.
	 * In most cases the origin will be outside of the image boundaries.  
	 * The origin is the 'location' (in pixels) of the sonar head in image plane
	 */
	int GetOriginCol()
	{
		return BVTColorImage_GetOriginCol( m_ptr );
	}

	/** Retrieve the range (from the sonar head) of the specified pixel
	 * \param row Origin row 
	 * \param col Origin col 
	 */
	double GetPixelRange(int row, int col)
	{
		return BVTColorImage_GetPixelRange( m_ptr, row, col );
	}

	/** Retrieve the bearing relative to the sonar head of the specified pixel
	 * \param row Origin row 
	 * \param col Origin col 
	 */
	double GetPixelRelativeBearing(int row, int col)
	{
		return BVTColorImage_GetPixelRelativeBearing( m_ptr, row, col );
	}

	/** Return the minimum angle for the sonar's imaging field of view. 
	 * The angle is returned in degrees.
	 */
	double GetFOVMinAngle()
	{
		return BVTColorImage_GetFOVMinAngle( m_ptr );
	}

	/** Return the maximum angle for the sonar's imaging field of view. 
	 * The angle is returned in degrees.
	 */
	double GetFOVMaxAngle()
	{
		return BVTColorImage_GetFOVMaxAngle( m_ptr );
	}

	/** Return the value of the pixel at (row, col)	
	 * \param row Requested row 
	 * \param col Requested col 
	 */
	unsigned int GetPixel(int row, int col)
	{
		return BVTColorImage_GetPixel( m_ptr, row, col );
	}

	/** Return a pointer to a row of pixels	
	 * \param row Requested row 
	 */
	unsigned int* GetRow(int row)
	{
		return BVTColorImage_GetRow( m_ptr, row );
	}

	/** Return a pointer to the entire image.
	 * The image or organized in Row-Major order (just like C/C++).
	 */
	unsigned int* GetBits()
	{
		return BVTColorImage_GetBits( m_ptr );
	}

	/** Copy the raw image data to the user specified buffer. See GetBits for more info.
	 * \param data Pointer to a valid buffer 
	 * \param len The size of the buffer pointed to by data in pixels NOT bytes. 
	 */
	RetVal CopyBits(unsigned int data[], unsigned int len)
	{
		return BVTColorImage_CopyBits( m_ptr, data, len );
	}

	/** Save the image in PPM (PortablePixMap) format.
	 * \param file_name File name to save to 
	 */
	RetVal SavePPM(std::string file_name)
	{
		return BVTColorImage_SavePPM( m_ptr, file_name.c_str() );
	}


private:
	BVTColorImage m_ptr;

	/// Prevent this object from being coppied
	ColorImage(const ColorImage&);
	ColorImage&operator=(const ColorImage&);
};
}

#endif
