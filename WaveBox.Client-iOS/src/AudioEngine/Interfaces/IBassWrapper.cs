using System;
using Un4seen.Bass;

namespace WaveBox.Client.AudioEngine
{
	public interface IBassWrapper
	{
		bool BassInit();
		void LogError();
		bool IsFileError();
		void PrintChannelInfo(int channel);
		string FormatForChannel(int channel);
		string StringFromErrorCode(BASSError errorCode);
		int EstimateBitrate(BassStream bassStream);
	}
}

