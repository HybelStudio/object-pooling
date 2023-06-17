using System;

namespace Hybel.EditorUtils
{
    /// <summary>
    /// Scopes are disposed in the order of priority where index 0 is highest priority.
    /// </summary>
    public class Scopes : IDisposable
    {
        private readonly IDisposable[] _scopes;

        /// <summary>
        /// Scopes are disposed in the order of priority where index 0 is highest priority.
        /// </summary>
        public Scopes(params IDisposable[] scopes) => _scopes = scopes;

        /// <summary>
        /// Scopes are disposed in the order of priority where index 0 is highest priority.
        /// </summary>
        public void Dispose()
        {
            for (int i = _scopes.Length - 1; i >= 0; i--)
                _scopes[i].Dispose();
        }
    }
}
