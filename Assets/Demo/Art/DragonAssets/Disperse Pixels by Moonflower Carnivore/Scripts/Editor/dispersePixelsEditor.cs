using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(dispersePixels)), CanEditMultipleObjects]
public class dispersePixelsEditor : Editor {
	public void OnSceneGUI() {
		dispersePixels dp = (target as dispersePixels);
		if (dp.targetObject.Length != 0) {
			if (dp.targetObject[0]) {
			for (int i = 0; i < dp.targetObject.Length; i++) {
				if (dp.maxParticles[i] * dp.maxParticlesMultiplier > 0) {
					EditorGUI.BeginChangeCheck();
					//Vector3 gizmoPos = dp.targetObject[i].transform.position + dp.targetObject[i].transform.rotation * dp.pSystemOffset[i];
					Vector3 gizmoPos = dp.targetObject[i].transform.localToWorldMatrix.MultiplyPoint3x4(dp.pSystemOffset[i]);
					Vector3 pos = Handles.PositionHandle(gizmoPos, dp.targetObject[i].transform.rotation);
					Vector3 scale = Handles.ScaleHandle(dp.emitterBoxSize[i], gizmoPos, dp.targetObject[i].transform.rotation, HandleUtility.GetHandleSize(gizmoPos) * 0.75f);
					if (EditorGUI.EndChangeCheck()) {
						Undo.RecordObject(target, "Scaled Emitter Box of Disperse Pixels");
						dp.pSystemOffset[i] = Quaternion.Inverse(dp.targetObject[i].transform.rotation) * (pos - dp.targetObject[i].transform.position);
						dp.emitterBoxSize[i] = scale;
					}
					Handles.Label(dp.targetObject[i].transform.localToWorldMatrix.MultiplyPoint3x4(dp.pSystemOffset[i] - dp.emitterBoxSize[i] * 0.5f), "Disperse Pixels of ["+i+"]'"+dp.targetObject[i].name+"'\nEmitter Box Size: "+dp.emitterBoxSize[i]+"\nP System Offset: "+dp.pSystemOffset[i]);
				}
			}
			}
		}
	}
}