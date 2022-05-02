
//  ==> COPYRIGHT (C) 2021 XSENS TECHNOLOGIES OR SUBSIDIARIES WORLDWIDE <==
//  WARNING: COPYRIGHT (C) 2021 XSENS TECHNOLOGIES OR SUBSIDIARIES WORLDWIDE. ALL RIGHTS RESERVED.
//  THIS FILE AND THE SOURCE CODE IT CONTAINS (AND/OR THE BINARY CODE FILES FOUND IN THE SAME
//  FOLDER THAT CONTAINS THIS FILE) AND ALL RELATED SOFTWARE (COLLECTIVELY, "CODE") ARE SUBJECT
//  TO AN END USER LICENSE AGREEMENT ("AGREEMENT") BETWEEN XSENS AS LICENSOR AND THE AUTHORIZED
//  LICENSEE UNDER THE AGREEMENT. THE CODE MUST BE USED SOLELY WITH XSENS PRODUCTS INCORPORATED
//  INTO LICENSEE PRODUCTS IN ACCORDANCE WITH THE AGREEMENT. ANY USE, MODIFICATION, COPYING OR
//  DISTRIBUTION OF THE CODE IS STRICTLY PROHIBITED UNLESS EXPRESSLY AUTHORIZED BY THE AGREEMENT.
//  IF YOU ARE NOT AN AUTHORIZED USER OF THE CODE IN ACCORDANCE WITH THE AGREEMENT, YOU MUST STOP
//  USING OR VIEWING THE CODE NOW, REMOVE ANY COPIES OF THE CODE FROM YOUR COMPUTER AND NOTIFY
//  XSENS IMMEDIATELY BY EMAIL TO INFO@XSENS.COM. ANY COPIES OR DERIVATIVES OF THE CODE (IN WHOLE
//  OR IN PART) IN SOURCE CODE FORM THAT ARE PERMITTED BY THE AGREEMENT MUST RETAIN THE ABOVE
//  COPYRIGHT NOTICE AND THIS PARAGRAPH IN ITS ENTIRETY, AS REQUIRED BY THE AGREEMENT.
//  
//  THIS SOFTWARE CAN CONTAIN OPEN SOURCE COMPONENTS WHICH CAN BE SUBJECT TO 
//  THE FOLLOWING GENERAL PUBLIC LICENSES:
//  ==> Qt GNU LGPL version 3: http://doc.qt.io/qt-5/lgpl.html <==
//  ==> LAPACK BSD License:  http://www.netlib.org/lapack/LICENSE.txt <==
//  ==> StackWalker 3-Clause BSD License: https://github.com/JochenKalmbach/StackWalker/blob/master/LICENSE <==
//  ==> Icon Creative Commons 3.0: https://creativecommons.org/licenses/by/3.0/legalcode <==
//  

//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.1
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace XDA {

public enum XsResultValue {
  XRV_OK = 0,
  XRV_NOBUS = 1,
  XRV_BUSNOTREADY = 2,
  XRV_INVALIDPERIOD = 3,
  XRV_INVALIDMSG = 4,
  XRV_INITBUSFAIL1 = 16,
  XRV_INITBUSFAIL2 = 17,
  XRV_INITBUSFAIL3 = 18,
  XRV_SETBIDFAIL1 = 20,
  XRV_SETBIDFAIL2 = 21,
  XRV_MEASUREMENTFAIL1 = 24,
  XRV_MEASUREMENTFAIL2 = 25,
  XRV_MEASUREMENTFAIL3 = 26,
  XRV_MEASUREMENTFAIL4 = 27,
  XRV_MEASUREMENTFAIL5 = 28,
  XRV_MEASUREMENTFAIL6 = 29,
  XRV_TIMEROVERFLOW = 30,
  XRV_BAUDRATEINVALID = 32,
  XRV_INVALIDPARAM = 33,
  XRV_MEASUREMENTFAIL7 = 35,
  XRV_MEASUREMENTFAIL8 = 36,
  XRV_WIRELESSFAIL = 37,
  XRV_DEVICEERROR = 40,
  XRV_DATAOVERFLOW = 41,
  XRV_BUFFEROVERFLOW = 42,
  XRV_EXTTRIGGERERROR = 43,
  XRV_SAMPLESTREAMERROR = 44,
  XRV_POWER_DIP = 45,
  XRV_POWER_OVERCURRENT = 46,
  XRV_OVERHEATING = 47,
  XRV_BATTERYLOW = 48,
  XRV_INVALIDFILTERPROFILE = 49,
  XRV_INVALIDSTOREDSETTINGS = 50,
  XRV_ACCESSDENIED = 51,
  XRV_FILEERROR = 52,
  XRV_OUTPUTCONFIGERROR = 53,
  XRV_FILE_SYSTEM_CORRUPT = 54,
  XRV_ERROR = 256,
  XRV_NOTIMPLEMENTED = 257,
  XRV_TIMEOUT = 258,
  XRV_TIMEOUTNODATA = 259,
  XRV_CHECKSUMFAULT = 260,
  XRV_OUTOFMEMORY = 261,
  XRV_NOTFOUND = 262,
  XRV_UNEXPECTEDMSG = 263,
  XRV_INVALIDID = 264,
  XRV_INVALIDOPERATION = 265,
  XRV_INSUFFICIENTSPACE = 266,
  XRV_INPUTCANNOTBEOPENED = 267,
  XRV_OUTPUTCANNOTBEOPENED = 268,
  XRV_ALREADYOPEN = 269,
  XRV_ENDOFFILE = 270,
  XRV_COULDNOTREADSETTINGS = 271,
  XRV_NODATA = 272,
  XRV_READONLY = 273,
  XRV_NULLPTR = 274,
  XRV_INSUFFICIENTDATA = 275,
  XRV_BUSY = 276,
  XRV_INVALIDINSTANCE = 277,
  XRV_DATACORRUPT = 278,
  XRV_READINITFAILED = 279,
  XRV_NOXMFOUND = 280,
  XRV_DEVICECOUNTZERO = 282,
  XRV_MTLOCATIONINVALID = 283,
  XRV_INSUFFICIENTMTS = 284,
  XRV_INITFUSIONFAILED = 285,
  XRV_OTHER = 286,
  XRV_NOFILEOPEN = 287,
  XRV_NOPORTOPEN = 288,
  XRV_NOFILEORPORTOPEN = 289,
  XRV_PORTNOTFOUND = 290,
  XRV_INITPORTFAILED = 291,
  XRV_CALIBRATIONFAILED = 292,
  XRV_CONFIGCHECKFAIL = 293,
  XRV_ALREADYDONE = 294,
  XRV_SYNC_SINGLE_SLAVE = 295,
  XRV_SYNC_SECOND_MASTER = 296,
  XRV_SYNC_NO_SYNC = 297,
  XRV_SYNC_NO_MASTER = 298,
  XRV_SYNC_DATA_MISSING = 299,
  XRV_VERSION_TOO_LOW = 300,
  XRV_VERSION_PROBLEM = 301,
  XRV_ABORTED = 302,
  XRV_UNSUPPORTED = 303,
  XRV_PACKETCOUNTERMISSED = 304,
  XRV_MEASUREMENTFAILED = 305,
  XRV_STARTRECORDINGFAILED = 306,
  XRV_STOPRECORDINGFAILED = 307,
  XRV_RADIO_CHANNEL_IN_USE = 311,
  XRV_UNEXPECTED_DISCONNECT = 312,
  XRV_TOO_MANY_CONNECTED_TRACKERS = 313,
  XRV_GOTOCONFIGFAILED = 314,
  XRV_OUTOFRANGE = 315,
  XRV_BACKINRANGE = 316,
  XRV_EXPECTED_DISCONNECT = 317,
  XRV_RESTORE_COMMUNICATION_FAILED = 318,
  XRV_RESTORE_COMMUNICATION_STOPPED = 319,
  XRV_EXPECTED_CONNECT = 320,
  XRV_IN_USE = 321,
  XRV_PERFORMANCE_WARNING = 322,
  XRV_PERFORMANCE_OK = 323,
  XRV_SHUTTINGDOWN = 400,
  XRV_GNSSCONFIGURATIONERROR = 401,
  XRV_GNSSCOMMTIMEOUT = 402,
  XRV_GNSSERROR = 403,
  XRV_DEVICE_NOT_CALIBRATED = 404
}

}
