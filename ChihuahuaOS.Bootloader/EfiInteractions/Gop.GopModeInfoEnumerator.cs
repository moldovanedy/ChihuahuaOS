using System.Collections;
using System.Collections.Generic;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.ConsoleSupport;

namespace ChihuahuaOS.Bootloader.EfiInteractions;

public static unsafe partial class Gop
{
    public struct GopModeInfoEnumerator : IEnumerator<EfiGopModeInformation>
    {
        private EfiGop* _gopRef;
        private EfiGopModeInformation* _currentEnumModeInfo;
        private int _index;

        public EfiGopModeInformation Current => *_currentEnumModeInfo;

        object IEnumerator.Current => *_currentEnumModeInfo;

        internal GopModeInfoEnumerator(EfiGop* gopRef)
        {
            _gopRef = gopRef;
            Reset();
        }

        public bool MoveNext()
        {
            if (_gopRef == null)
            {
                return false;
            }

            if (_index >= _gopRef->Mode->MaxMode - 1)
            {
                return false;
            }

            _index++;
            ulong structSize = 0;
            EfiGopModeInformation* info;

            EfiStatus status = _gopRef->QueryMode(_gopRef, (uint)_index, &structSize, &info);
            if (status != EfiStatus.Success)
            {
                return false;
            }

            _currentEnumModeInfo = info;
            return true;
        }

        public void Reset()
        {
            _index = -1;
            _currentEnumModeInfo = _gopRef->Mode->Info;
        }

        public void Dispose()
        {
            _gopRef = null;
        }
    }
}