using Hybel.EditorUtils;
using UnityEditor;

using static Hybel.EditorUtils.EditorGUILayoutExtras;
using static UnityEditor.EditorGUILayout;

namespace Hybel.ObjectPooling.Editor
{
    [CustomEditor(typeof(ObjectPoolAsset))]
    public class ObjectPoolAssetEditor : UnityEditor.Editor
    {
        private SerializedObject _so;
        private SerializedProperty _propPrefab;
        private SerializedProperty _propOverflowMode;
        private SerializedProperty _propOverflowIncrement;
        private SerializedProperty _propAmountToStartWith;
        private SerializedProperty _propInstantiationType;
        private SerializedProperty _propBatchAmount;
        private SerializedProperty _propLogHighestAmount;
        private SerializedProperty _propLogAverageAmount;

        private void OnEnable()
        {
            _so = serializedObject;

            _propPrefab = _so.FindProperty("prefab");
            _propOverflowMode = _so.FindProperty("overflowMode");
            _propOverflowIncrement= _so.FindProperty("overflowIncrement");
            _propAmountToStartWith = _so.FindProperty("amountToStartWith");
            _propInstantiationType = _so.FindProperty("instantiationType");
            _propBatchAmount = _so.FindProperty("batchAmount");
            _propLogHighestAmount = _so.FindProperty("logHighestAmount");
            _propLogAverageAmount = _so.FindProperty("logAverageAmount");
        }

        public override void OnInspectorGUI()
        {
            // Script field for easy access.
            ScriptField(target);
            using (new UpdateApplyScope(_so))
            {
                PropertyField(_propPrefab);
                PropertyField(_propOverflowMode);

                if (_propOverflowMode.intValue == (int)OverflowMode.IncreaseSize)
                    PropertyField(_propOverflowIncrement);

                PropertyField(_propAmountToStartWith);

                PropertyField(_propInstantiationType);

                if (_propInstantiationType.intValue == (int)ObjectPoolAsset.InstantiationType.BatchesPerFrame)
                    PropertyField(_propBatchAmount);

                PropertyField(_propLogHighestAmount);
                PropertyField(_propLogAverageAmount);
            }
        }
    }
}