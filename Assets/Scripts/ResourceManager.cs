﻿using UnityEngine;
using System.Collections;
namespace RTS
{
		public static class ResourceManager
		{
				public static float CameraMaxHeight { get { return 100; } }
				public static float CameraMinHeight { get { return 20; } }
				public static Vector3 InvalidPosition{ get { return invalidPosition; } }
				public static float KeyboadScrollSpeed { get { return 100; } }
				public static float MaxSpeed { get { return 300; } }
				public static float RotateAmount { get { return 10; } }
				public static float RotateSpeed { get { return 100; } }
				public static float ScrollSpeed { get { return 15; } }
				public static float ScrollWidth { get { return 90; } }
				public static float SpeedMultiplier { get { return 25; } }
				public static float ZoomSpeed { get { return 250; } }

				private static Vector3 invalidPosition = new Vector3 (-99999, -99999, -99999);
	

		}
}