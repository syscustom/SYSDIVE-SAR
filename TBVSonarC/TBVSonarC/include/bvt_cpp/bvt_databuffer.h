/*
    This file has been generated by bvtidl.pl. DO NOT MODIFY!
*/
#ifndef __CPP_BVTDATABUFFER_H__
#define __CPP_BVTDATABUFFER_H__

#include <string>
#include <bvt_cpp/bvt_retval.h>

namespace BVTSDK
{

/** A DataBuffer is used to pass raw bytes between the SDK and client code, instead of passing function pointers to
 * memory allocators and deallocators.
 */
class DataBuffer
{
public:
	/// Create the object
	DataBuffer()
	{ m_ptr = BVTDataBuffer_Create(); }

	/// Destroy the object
	~DataBuffer()
	{ BVTDataBuffer_Destroy(m_ptr); }

#ifndef DOXY_IGNORE
	DataBuffer(BVTDataBuffer ptr)
	{ m_ptr = ptr; }

	operator BVTDataBuffer()
	{ return m_ptr; }
	operator BVTDataBuffer*()
	{ return &m_ptr; }
	operator const BVTDataBuffer() const
	{ return m_ptr; }
#endif

	/** Return the length of this DataBuffer in bytes.
	 * \param len The size of this DataBuffer in bytes. 
	 */
	RetVal GetLength(unsigned int* len)
	{
		return BVTDataBuffer_GetLength( m_ptr, len );
	}

	/** Return a pointer to the data.
	 */
	const void* GetData()
	{
		return BVTDataBuffer_GetData( m_ptr );
	}

	/** Copy from the user specified buffer into this DataBuffer.
	 * \param data Pointer to a valid buffer 
	 * \param len The size of the buffer in bytes. 
	 */
	RetVal SetFrom(const void* data, unsigned int len)
	{
		return BVTDataBuffer_SetFrom( m_ptr, data, len );
	}

	/** Copy from this DataBuffer to the user specified buffer.
	 * \param data Pointer to a valid buffer 
	 * \param len The size of the buffer in bytes. 
	 */
	RetVal CopyTo(void* data, unsigned int len)
	{
		return BVTDataBuffer_CopyTo( m_ptr, data, len );
	}


private:
	BVTDataBuffer m_ptr;

	/// Prevent this object from being coppied
	DataBuffer(const DataBuffer&);
	DataBuffer&operator=(const DataBuffer&);
};
}

#endif
