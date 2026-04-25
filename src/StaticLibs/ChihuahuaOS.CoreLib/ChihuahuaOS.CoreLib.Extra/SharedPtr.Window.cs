using System;
using ChihuahuaOS.CoreLib.Extra.Runtime;

namespace ChihuahuaOS.CoreLib.Extra;

public partial class SharedPtr<T>
{
    public class Window : IDisposable
    {
        private readonly SharedPtr<T> _parentPtr;

        public Window(SharedPtr<T> parentPtr)
        {
            _parentPtr = parentPtr;
            _parentPtr._numReferences++;
        }

        /// <summary>
        /// Creates a new window from the main one (another reference).
        /// </summary>
        /// <returns></returns>
        public Window Fork()
        {
            return new Window(_parentPtr);
        }

        public void Dispose()
        {
            _parentPtr._numReferences--;
            if (_parentPtr._numReferences <= 0)
            {
                _parentPtr._value.Dispose();
                _parentPtr.IsValueDisposed = true;
            }

            MemUtils.FreeMemory(this);
        }
    }
}