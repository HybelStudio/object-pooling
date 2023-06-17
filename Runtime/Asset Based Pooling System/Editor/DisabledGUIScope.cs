using UnityEngine;
using System;

namespace Hybel.EditorUtils
{
    public class DisabledGUIScope : IDisposable
    {
        private bool _previousGUIState;

        public DisabledGUIScope() : this(true) { }

        public DisabledGUIScope(bool condition)
        {
            _previousGUIState = GUI.enabled;
            if (condition) GUI.enabled = false;
        }

        public void Dispose() => GUI.enabled = _previousGUIState;
    }
}
