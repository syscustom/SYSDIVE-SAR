/*
    This file has been generated by bvtidl.pl. DO NOT MODIFY!
*/
#ifndef __CPP_BVTRANGEDATA_H__
#define __CPP_BVTRANGEDATA_H__

#include <string>
#include <bvt_cpp/bvt_retval.h>
#include <bvt_cpp/bvt_colorimage.h>

namespace BVTSDK
{

/** ** EXPERIMENTAL ** This functionality is still under development! ***
 * RangeData is a set of ranges from the sonar head, at various angles
 * from the sonar head. For each angle, the range, bearing and intensity
 * of the return beam at that range is stored.
 * NOTE: RangeData only applies to specialized BlueView sonars, such as the MB1350,
 * and has no use for our standard imaging sonars.
 */
class RangeData
{
public:
	/// Create the object
	RangeData()
	{ m_ptr = NULL; }

#ifndef DOXY_IGNORE
	RangeData(BVTRangeData ptr)
	{ m_ptr = ptr; }

	operator BVTRangeData()
	{ return m_ptr; }
	operator BVTRangeData*()
	{ return &m_ptr; }
	operator const BVTRangeData() const
	{ return m_ptr; }
#endif

	/** Returns the number of range values stored for this ping.
	 */
	int GetCount()
	{
		return BVTRangeData_GetCount( m_ptr );
	}

	/** Returns the resolution of the range values, in meters. 
	 */
	double GetRangeResolution()
	{
		return BVTRangeData_GetRangeResolution( m_ptr );
	}

	/** Returns the resolution of the bearing stored with each range value.
	 * This is the difference in bearing between each range value in the array.
	 * <br>
	 */
	double GetBearingResolution()
	{
		return BVTRangeData_GetBearingResolution( m_ptr );
	}

	/** Return the minimum angle for the sonar's imaging field of view.
	 * In other words, this is the angle of the first range value, as all
	 * angles are "left referenced."The angle is returned in degrees.
	 * Note that this may not represent the actual physical field of view
	 * of a particular sonar, but does represent the field of view of the
	 * data being returned. Some outside values may have range values
	 * indicating they are out of range.
	 */
	float GetFOVMinAngle()
	{
		return BVTRangeData_GetFOVMinAngle( m_ptr );
	}

	/** Return the maximum angle for the sonar's imaging field of view.
	 * In other words, this is the angle of the last range value, as all
	 * angles are "left referenced."The angle is returned in degrees.
	 * Note that this may not represent the actual physical field of view
	 * of a particular sonar, but does represent the field of view of the
	 * data being returned. Some outside values may have range values
	 * indicating they are out of range.
	 */
	float GetFOVMaxAngle()
	{
		return BVTRangeData_GetFOVMaxAngle( m_ptr );
	}

	/** Values greater than this indicate no range could be measured. 
	 */
	static const int MAX_RANGE	= 999;

	/** Copies the range values into the user specified buffer. The
	 * buffer must hold the entire number of ranges (See GetCount() above),
	 * or an error is returned.
	 * \param ranges Pointer to a valid buffer of type float.
	 * \param number_of_ranges Number of values the buffer can hold.
	 */
	RetVal CopyRangeValues(float ranges[], int number_of_ranges)
	{
		return BVTRangeData_CopyRangeValues( m_ptr, ranges, number_of_ranges );
	}

	/** Returns the range from the sonar head, in meters, at a particular
	 * index into the array. <br>
	 * NOTE: Check all returned values for validity. If range > BVTRANGEDATA_MAX_RANGE
	 * then the range could not be determined within the capabilities of the sonar. 
	 * Meaning that the closest object at that bearing was either out of
	 * view of the sonar, or the threshold was set too high to be detected.
	 * \param index index into the array of RangeData values  
	 */
	float GetRangeValue(int index)
	{
		return BVTRangeData_GetRangeValue( m_ptr, index );
	}

	/** Returns the intensity value at the specified index into the array. <br>
	 * \param index index into the array of RangeData values  
	 */
	float GetIntensityValue(int index)
	{
		return BVTRangeData_GetIntensityValue( m_ptr, index );
	}

	/** Returns the bearing from the center of the sonar head, in degrees (+/-),
	 * at a particular index into the array.
	 * \param index index into the array of RangeData values  
	 */
	float GetBearingValue(int index)
	{
		return BVTRangeData_GetBearingValue( m_ptr, index );
	}

	/** Returns the X coordinate for the pixel in the passed ColorImage, which
	 * maps to the range and bearing at the index passed. This allows placing
	 * of the range data on a colorimage, easing analysis of the algorithm
	 * used for thresholding.
	 * \param image ColorImage object where the pixel coordinate is needed 
	 */
	int GetColorImagePixelX(int rangeDataIndex, const ColorImage& image)
	{
		return BVTRangeData_GetColorImagePixelX( m_ptr, rangeDataIndex, image );
	}

	/** Returns the Y coordinate for the pixel in the passed ColorImage which
	 * maps to the range and bearing at the index passed. (see similar function,
	 * above, for more details)
	 * \param image ColorImage object where the pixel coordinate is needed 
	 */
	int GetColorImagePixelY(int rangeDataIndex, const ColorImage& image)
	{
		return BVTRangeData_GetColorImagePixelY( m_ptr, rangeDataIndex, image );
	}


private:
	BVTRangeData m_ptr;
};
}

#endif
