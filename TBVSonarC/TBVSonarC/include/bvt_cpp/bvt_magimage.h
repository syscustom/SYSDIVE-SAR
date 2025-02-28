/*
    This file has been generated by bvtidl.pl. DO NOT MODIFY!
*/
#ifndef __CPP_BVTMAGIMAGE_H__
#define __CPP_BVTMAGIMAGE_H__

#include <string>
#include <bvt_cpp/bvt_retval.h>

namespace BVTSDK
{

/** MagImage is short for MagnitudeImage.  It provides access to a 2d 
 * image where each pixel is intensity of the return from a particular
 * point on a plane emanating from the head.  It can be thought of as 
 * a 16bit grey-scale image.
 */
class MagImage
{
public:
	/// Create the object
	MagImage()
	{ m_ptr = NULL; }

	/// Destroy the object
	~MagImage()
	{ BVTMagImage_Destroy(m_ptr); }

#ifndef DOXY_IGNORE
	MagImage(BVTMagImage ptr)
	{ m_ptr = ptr; }

	operator BVTMagImage()
	{ return m_ptr; }
	operator BVTMagImage*()
	{ return &m_ptr; }
	operator const BVTMagImage() const
	{ return m_ptr; }
#endif

	/** Return the height (in pixels) of this image
	 */
	int GetHeight()
	{
		return BVTMagImage_GetHeight( m_ptr );
	}

	/** Return the width (in pixels) of this image
	 */
	int GetWidth()
	{
		return BVTMagImage_GetWidth( m_ptr );
	}

	/** Return the range resolution of this image.
	 * The resolution is returned in meters per pixel.
	 */
	double GetRangeResolution()
	{
		return BVTMagImage_GetRangeResolution( m_ptr );
	}

	/** Only valid for R-Theta images.
	 * Returns the bearing resolution, in degrees per column.
	 */
	double GetBearingResolution()
	{
		return BVTMagImage_GetBearingResolution( m_ptr );
	}

	/** Retrieve the image row of the origin.
	 * In most cases the origin will be outside of the image boundaries.  The origin is the 'location' 
	 * (in pixels) of the sonar head in image plane
	 */
	int GetOriginRow()
	{
		return BVTMagImage_GetOriginRow( m_ptr );
	}

	/** Retrieve the image column of the origin.
	 * In most cases the origin will be outside of the image boundaries.  The origin is the 'location' 
	 * (in pixels) of the sonar head in image plane
	 */
	int GetOriginCol()
	{
		return BVTMagImage_GetOriginCol( m_ptr );
	}

	/** Retrieve the range (from the sonar head) of the specified pixel
	 * \param row Origin row 
	 * \param col Origin col 
	 */
	double GetPixelRange(int row, int col)
	{
		return BVTMagImage_GetPixelRange( m_ptr, row, col );
	}

	/** Retrieve the bearing relative to the sonar head of the specified pixel
	 * \param row Origin row 
	 * \param col Origin col 
	 */
	double GetPixelRelativeBearing(int row, int col)
	{
		return BVTMagImage_GetPixelRelativeBearing( m_ptr, row, col );
	}

	/** Return the minimum angle for the sonar's imaging field of view. 
	 * The angle is returned in degrees.
	 */
	double GetFOVMinAngle()
	{
		return BVTMagImage_GetFOVMinAngle( m_ptr );
	}

	/** Return the maximum angle for the sonar's imaging field of view. 
	 * The angle is returned in degrees.
	 */
	double GetFOVMaxAngle()
	{
		return BVTMagImage_GetFOVMaxAngle( m_ptr );
	}

	/** Return the value of the pixel at (row, col)	
	 * \param row Requested row 
	 * \param col Requested col 
	 */
	unsigned short GetPixel(int row, int col)
	{
		return BVTMagImage_GetPixel( m_ptr, row, col );
	}

	/** Return a pointer to a row of pixels	
	 * \param row Requested row 
	 */
	unsigned short* GetRow(int row)
	{
		return BVTMagImage_GetRow( m_ptr, row );
	}

	/** Return a pointer to the entire image.
	 * The image or organized in Row-Major order (just like C/C++).
	 */
	unsigned short* GetBits()
	{
		return BVTMagImage_GetBits( m_ptr );
	}

	/** Copy the raw image data to the user specified buffer. See GetBits for more info.
	 * \param data Pointer to a valid buffer 
	 * \param len The size of the buffer pointed to by data in pixels NOT bytes. 
	 */
	RetVal CopyBits(unsigned short data[], unsigned int len)
	{
		return BVTMagImage_CopyBits( m_ptr, data, len );
	}

	/** Save the image in PGM (PortableGreyMap) format.
	 * Note that few programs actually support loading a 16bit PGM.
	 * \param file_name File name to save to 
	 */
	RetVal SavePGM(std::string file_name)
	{
		return BVTMagImage_SavePGM( m_ptr, file_name.c_str() );
	}


private:
	BVTMagImage m_ptr;

	/// Prevent this object from being coppied
	MagImage(const MagImage&);
	MagImage&operator=(const MagImage&);
};
}

#endif
