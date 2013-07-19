using System;
using Un4seen.Bass;

namespace WaveBox.Client.AudioEngine
{
	public class BassWrapper : IBassWrapper
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public bool IsBassInitialized { get; set; }

		public BassWrapper ()
		{
		}

		public bool BassInit()
		{
			if (this.IsBassInitialized)
				return true;

			// Initialize BASS
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_IOS_MIXAUDIO, 0); // Disable mixing.	To be called before BASS_Init.
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, Bass.BASS_GetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD) + 800); // set the buffer length to the minimum amount + 800ms
			Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true); // set DSP effects to use floating point math to avoid clipping within the effects chain
			if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)) 	// Initialize default device.
			{
				this.IsBassInitialized = true;

				Bass.BASS_Pause();
				return true;
			}
			else
			{
				logger.Error("[BassWrapper] Can't initialize device");
				LogError();
				return false;
			}
		}

		public void LogError()
		{
			BASSError errorCode = Bass.BASS_ErrorGetCode();
			logger.Error(@"[BassWrapper] BASS error: %i - %@", errorCode, StringFromErrorCode(errorCode));
		}

		public bool IsFileError()
		{
			BASSError errorCode = Bass.BASS_ErrorGetCode();
			return errorCode == BASSError.BASS_ERROR_FILEOPEN || 
				   errorCode == BASSError.BASS_ERROR_FORMAT || 
				   errorCode == BASSError.BASS_ERROR_FILEFORM || 
				   errorCode == BASSError.BASS_ERROR_CODEC;
		}

		public void PrintChannelInfo(int channel)
		{

		}

		public string FormatForChannel(int channel)
		{
			/*
			BASSConfig.BASS_CHANNELINFO i;
			BASS_ChannelGetInfo(channel, &i);

			// check built-in stream formats...
			//if (ctype==BASS_CTYPE_STREAM_FLAC) return @"FLAC";
			//if (ctype==BASS_CTYPE_STREAM_FLAC_OGG) return @"FLAC";
			if (i.ctype==BASS_CTYPE_STREAM_OGG) return @"OGG";
			if (i.ctype==BASS_CTYPE_STREAM_MP1) return @"MP1";
			if (i.ctype==BASS_CTYPE_STREAM_MP2) return @"MP2";
			if (i.ctype==BASS_CTYPE_STREAM_MP3) return @"MP3";
			if (i.ctype==BASS_CTYPE_STREAM_AIFF) return @"AIFF";
			if (i.ctype==BASS_CTYPE_STREAM_WAV_PCM) return @"PCM WAV";
			if (i.ctype==BASS_CTYPE_STREAM_WAV_FLOAT) return @"Float WAV";
			if (i.ctype&BASS_CTYPE_STREAM_WAV) return @"WAV";
			if (i.ctype==BASS_CTYPE_STREAM_CA) 
			{
				// CoreAudio codec
				const TAG_CA_CODEC *codec = (TAG_CA_CODEC*)BASS_ChannelGetTags(channel, BASS_TAG_CA_CODEC); // get codec info
				if (codec != NULL)
				{
					switch (codec->atype) 
					{
						case kAudioFormatLinearPCM:				return @"LPCM";
						case kAudioFormatAC3:					return @"AC3";
						case kAudioFormat60958AC3:				return @"AC3";
						case kAudioFormatAppleIMA4:				return @"IMA4";
						case kAudioFormatMPEG4AAC:				return @"AAC"; 
						case kAudioFormatMPEG4CELP:				return @"CELP";
						case kAudioFormatMPEG4HVXC:				return @"HVXC";
						case kAudioFormatMPEG4TwinVQ:			return @"TwinVQ";
						case kAudioFormatMACE3:					return @"MACE 3:1";
						case kAudioFormatMACE6:					return @"MACE 6:1";
						case kAudioFormatULaw:					return @"μLaw 2:1";
						case kAudioFormatALaw:					return @"aLaw 2:1";
						case kAudioFormatQDesign:				return @"QDMC";
						case kAudioFormatQDesign2:				return @"QDM2";
						case kAudioFormatQUALCOMM:				return @"QCPV";
						case kAudioFormatMPEGLayer1:			return @"MP1";
						case kAudioFormatMPEGLayer2:			return @"MP2";
						case kAudioFormatMPEGLayer3:			return @"MP3";
						case kAudioFormatTimeCode:				return @"TIME";
						case kAudioFormatMIDIStream:			return @"MIDI";
						case kAudioFormatParameterValueStream:	return @"APVS";
						case kAudioFormatAppleLossless:			return @"ALAC";
						case kAudioFormatMPEG4AAC_HE:			return @"AAC-HE";
						case kAudioFormatMPEG4AAC_LD:			return @"AAC-LD";
						case kAudioFormatMPEG4AAC_ELD:			return @"AAC-ELD";
						case kAudioFormatMPEG4AAC_ELD_SBR:		return @"AAC-SBR";
						case kAudioFormatMPEG4AAC_HE_V2:		return @"AAC-HEv2";
						case kAudioFormatMPEG4AAC_Spatial:		return @"AAC-S";
						case kAudioFormatAMR:					return @"AMR";
						case kAudioFormatAudible:				return @"AUDB";
						case kAudioFormatiLBC:					return @"iLBC";
						case kAudioFormatDVIIntelIMA:			return @"ADPCM";
						case kAudioFormatMicrosoftGSM:			return @"GSM";
						case kAudioFormatAES3:					return @"AES3";
						default:								return @" ";
					}
				}
			}
			return @"";*/
			return "";
		}

		public string StringFromErrorCode(BASSError errorCode)
		{
			switch (errorCode)
			{
				case BASSError.BASS_OK:				return @"No error! All OK";
				case BASSError.BASS_ERROR_MEM:		return @"Memory error";
				case BASSError.BASS_ERROR_FILEOPEN:	return @"Can't open the file";
				case BASSError.BASS_ERROR_DRIVER:		return @"Can't find a free/valid driver";
				case BASSError.BASS_ERROR_BUFLOST:	return @"The sample buffer was lost";
				case BASSError.BASS_ERROR_HANDLE:		return @"Invalid handle";
				case BASSError.BASS_ERROR_FORMAT:		return @"Unsupported sample format";
				case BASSError.BASS_ERROR_POSITION:	return @"Invalid position";
				case BASSError.BASS_ERROR_INIT:		return @"BASS_Init has not been successfully called";
				case BASSError.BASS_ERROR_START:		return @"BASS_Start has not been successfully called";
				case BASSError.BASS_ERROR_ALREADY:	return @"Already initialized/paused/whatever";
				case BASSError.BASS_ERROR_NOCHAN:		return @"Can't get a free channel";
				case BASSError.BASS_ERROR_ILLTYPE:	return @"An illegal type was specified";
				case BASSError.BASS_ERROR_ILLPARAM:	return @"An illegal parameter was specified";
				case BASSError.BASS_ERROR_NO3D:		return @"No 3D support";
				case BASSError.BASS_ERROR_NOEAX:		return @"No EAX support";
				case BASSError.BASS_ERROR_DEVICE:		return @"Illegal device number";
				case BASSError.BASS_ERROR_NOPLAY:		return @"Not playing";
				case BASSError.BASS_ERROR_FREQ:		return @"Illegal sample rate";
				case BASSError.BASS_ERROR_NOTFILE:	return @"The stream is not a file stream";
				case BASSError.BASS_ERROR_NOHW:		return @"No hardware voices available";
				case BASSError.BASS_ERROR_EMPTY:		return @"The MOD music has no sequence data";
				case BASSError.BASS_ERROR_NONET:		return @"No internet connection could be opened";
				case BASSError.BASS_ERROR_CREATE:		return @"Couldn't create the file";
				case BASSError.BASS_ERROR_NOFX:		return @"Effects are not available";
				case BASSError.BASS_ERROR_NOTAVAIL:	return @"Requested data is not available";
				case BASSError.BASS_ERROR_DECODE:		return @"The channel is a 'decoding channel'";
				case BASSError.BASS_ERROR_DX:			return @"A sufficient DirectX version is not installed";
				case BASSError.BASS_ERROR_TIMEOUT:	return @"Connection timedout";
				case BASSError.BASS_ERROR_FILEFORM:	return @"Unsupported file format";
				case BASSError.BASS_ERROR_SPEAKER:	return @"Unavailable speaker";
				case BASSError.BASS_ERROR_VERSION:	return @"Invalid BASS version (used by add-ons)";
				case BASSError.BASS_ERROR_CODEC:		return @"Codec is not available/supported";
				case BASSError.BASS_ERROR_ENDED:		return @"The channel/file has ended";
				case BASSError.BASS_ERROR_BUSY:		return @"The device is busy";
				case BASSError.BASS_ERROR_UNKNOWN:
				default:					return @"Unknown error.";
			}
		}


		public int EstimateBitrate(BassStream bassStream)
		{
			/*// Default to the player bitrate
			HSTREAM stream = bassStream.stream;
			QWORD startFilePosition = 0;
			QWORD currentFilePosition = BASS_StreamGetFilePosition(stream, BASS_FILEPOS_CURRENT);
			QWORD filePosition = currentFilePosition - startFilePosition;
			QWORD decodedPosition = BASS_ChannelGetPosition(stream, BASS_POS_BYTE|BASS_POS_DECODE); // decoded PCM position
			double bitrateDouble = filePosition * 8 / BASS_ChannelBytes2Seconds(stream, decodedPosition);
			NSUInteger bitrate = (NSUInteger)(bitrateDouble / 1000);
			bitrate = bitrate > 1000000 ? -1 : bitrate;

			BASS_CHANNELINFO i;
			Bass.BASS_ChannelGetInfo(bassStream.stream, &i);
			Song *songForStream = bassStream.song;*/

			return 128;
		}
	}
}

