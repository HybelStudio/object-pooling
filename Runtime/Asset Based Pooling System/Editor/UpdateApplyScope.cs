using UnityEditor;
using UnityEngine;

namespace Hybel.EditorUtils
{
    public class UpdateApplyScope : GUI.Scope
    {
        private SerializedObject _serializedObject;

        public UpdateApplyScope(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            _serializedObject.Update();
        }

        protected override void CloseScope() => _serializedObject.ApplyModifiedProperties();
    }
}
