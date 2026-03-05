//
//SpingManager.cs for unity-chan!
//
//Original Script is here:
//ricopin / SpingManager.cs
//Rocket Jump : http://rocketjump.skr.jp/unity3d/109/
//https://twitter.com/ricopin416
//
//Revised by N.Kobayashi 2014/06/24
//           Y.Ebata
//
using UnityEngine;
using System.Collections;

namespace UnityChan
{
	public class SpringManager : MonoBehaviour
	{
		//Kobayashi
		// DynamicRatio is paramater for activated level of dynamic animation 
		public float dynamicRatio = 1.0f;

		//Ebata
		public float stiffnessForce;
		public AnimationCurve stiffnessCurve;
		public float dragForce;
		public AnimationCurve dragCurve;
		public SpringBone[] springBones;

		void Start()
		{
			UpdateParameters();
		}

		void Update()
		{
#if UNITY_EDITOR
		//Kobayashi
		if(dynamicRatio >= 1.0f)
			dynamicRatio = 1.0f;
		else if(dynamicRatio <= 0.0f)
			dynamicRatio = 0.0f;
		//Ebata
		UpdateParameters();
#endif
		}

		private void LateUpdate()
		{
			//Kobayashi
			if (dynamicRatio != 0.0f)
			{
				for (int i = 0; i < springBones.Length; i++)
				{
					if (dynamicRatio > springBones[i].threshold)
					{
						springBones[i].UpdateSpring();
					}
				}
			}
		}

		[ContextMenu("绑定骨骼")]
		public void SetAllBones()
		{
			Transform[] transforms = GetComponentsInChildren<Transform>();
			foreach (Transform t in transforms)
			{
				if (t.name.StartsWith("裙") || t.name.StartsWith("Ctr") || t.name.StartsWith("髮")|| t.name.StartsWith("Sleeve"))
				{
					//裙子："裙"  头发："Ctr"或"髮"  披肩："Sleeve"
					if (t.childCount > 0)
					{
						SpringBone sb = t.GetComponent<SpringBone>();
						if (sb == null)
						{
							sb = t.gameObject.AddComponent<SpringBone>();
						}
						sb.child = t.GetChild(0);
						sb.boneAxis = new Vector3(0, 1, 0);
					}
				}
				springBones = GetComponentsInChildren<SpringBone>();
			}
		}

		private void UpdateParameters()
		{
			UpdateParameter("stiffnessForce", stiffnessForce, stiffnessCurve);
			UpdateParameter("dragForce", dragForce, dragCurve);
		}

		private void UpdateParameter(string fieldName, float baseValue, AnimationCurve curve)
		{
			// 检查 springBones 是否有效
			if (springBones == null || springBones.Length == 0)
				return;
			// 检查 curve 是否有效且有关键帧
			if (curve == null || curve.length == 0)
				return;

			var start = curve.keys[0].time;
			var end = curve.keys[curve.length - 1].time;
			var prop = springBones[0].GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

			for (int i = 0; i < springBones.Length; i++)
			{
				if (!springBones[i].isUseEachBoneForceSettings)
				{
					float t = (springBones.Length == 1) ? 0f : (float)i / (springBones.Length - 1);
					var scale = curve.Evaluate(start + (end - start) * t);
					prop.SetValue(springBones[i], baseValue * scale);
				}
			}
		}
	}
}