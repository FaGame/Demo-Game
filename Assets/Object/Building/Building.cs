﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Newtonsoft.Json;

public class Building : WorldObject
{
		public float maxBuildProgress;
		public Texture2D rallyPointImage;
		public Texture2D sellImage;
		public AudioClip finishedJobSound;
		public float finishedJobVolume = 1.0f;

		protected Queue <string> buildQueue;
		protected Vector3 rallyPoint;

		private float currentBuildProgress = 0.0f;
		private Vector3 spawnPoint;
		private bool needsBuilding = false;
		//private bool settingRallyPoint = false;

		protected override void Awake ()
		{
				base.Awake ();
				
				buildQueue = new Queue<string> ();
				

				SetSpawnPoint ();
				
				rallyPoint = spawnPoint;
		}

		protected override void Start ()
		{
				base.Start ();
		}
	
		protected override void Update ()
		{
				base.Update ();
				ProcessBuildQueue ();
		}
		protected override void OnGUI ()
		{
				base.OnGUI ();
				if (needsBuilding)
						DrawBuildProgress ();
		}


		protected override void InitializeAudio ()
		{
				base.InitializeAudio ();
				if (finishedJobVolume < 0.0f)
						finishedJobVolume = 0.0f;
				if (finishedJobVolume > 1.0f)
						finishedJobVolume = 1.0f;
				List<AudioClip> sounds = new List<AudioClip> ();
				List<float> volumes = new List<float> ();
				sounds.Add (finishedJobSound);
				volumes.Add (finishedJobVolume);
				audioElement.Add (sounds, volumes);
		}

		public override void SetSelection (bool selected, Rect playingArea)
		{
				base.SetSelection (selected, playingArea);
				if (player) {
						RallyPoint flag = player.GetComponentInChildren<RallyPoint> ();
						if (selected) {
								if (flag && player.human && spawnPoint != ResourceManager.InvalidPosition) {
										flag.transform.localPosition = rallyPoint;
										flag.transform.forward = transform.forward;
										flag.Enable ();
								} 
						} else if (flag && player.human) {
								flag.Disable ();
						}
				}
		}

		public override string[] GetActions ()
		{
				if (!needsBuilding)
						return actions;
				return new string[]{};
		}
		
		public override void SetHoverState (GameObject hoverObject)
		{
				base.SetHoverState (hoverObject);
				if (player && player.human && currentlySelected) {
						if (WorkManager.ObjectIsGround (hoverObject)) {
								if (player.hud.GetCursorState () == CursorState.RallyPoint)
								//if (settingRallyPoint)
										player.hud.SetCursorState (CursorState.RallyPoint);
						}
				}
		}
		
		public override void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller)
		{
				base.MouseClick (hitObject, hitPoint, controller);
				if (player && player.human && currentlySelected) {
						if (WorkManager.ObjectIsGround (hitObject)) {
								if (/*player.hud.GetPreviousCursorState () == CursorState.RallyPoint ||*/ player.hud.GetCursorState () == CursorState.RallyPoint && hitPoint != ResourceManager.InvalidPosition) {
										SetRallyPoint (hitPoint);
										player.hud.SetCursorState (CursorState.Select);
								}
						}
				}
		}

		public override void SaveDetails (JsonWriter writer)
		{
				base.SaveDetails (writer);
				SaveManager.WriteBoolean (writer, "NeedsBuilding", needsBuilding);
				SaveManager.WriteVector (writer, "SpawnPoint", spawnPoint);
				SaveManager.WriteVector (writer, "RallyPoint", rallyPoint);
				SaveManager.WriteFloat (writer, "BuildProgress", currentBuildProgress);
				SaveManager.WriteStringArray (writer, "BuildQueue", buildQueue.ToArray ());
				if (needsBuilding)
						SaveManager.WriteRect (writer, "PlayingArea", playingArea);
		}

		protected void CreateUnit (string unitName)
		{
				buildQueue.Enqueue (unitName);
				GameObject unit = ResourceManager.GetUnit (unitName);
				Unit unitObject = unit.GetComponent<Unit> ();
				if (player && unitObject)
						player.RemoveResource (ResourceType.Money, unitObject.cost);
		}

		private void ProcessBuildQueue ()
		{
				if (buildQueue.Count > 0) {
						currentBuildProgress += Time.deltaTime * RTS.ResourceManager.BuildSpeed;
						if (currentBuildProgress > maxBuildProgress) {
								if (player) {
										if (audioElement != null)
												audioElement.Play (finishedJobSound);
										player.AddUnit (buildQueue.Dequeue (), spawnPoint, rallyPoint, transform.rotation, this);
								}
								currentBuildProgress = 0.0f;
						}
				}
		}

		public string[] getBuildQueueValues ()
		{
				string [] values = new string[buildQueue.Count];
				int pos = 0;
				foreach (string unit in buildQueue)
						values [pos++] = unit;
				return values;
		}
		public float getBuildPercentage ()
		{
				return currentBuildProgress / maxBuildProgress;
		}
		
		public bool hasSpawnPoint ()
		{
				return spawnPoint != RTS.ResourceManager.InvalidPosition && rallyPoint != RTS.ResourceManager.InvalidPosition;
		}
		
		public void SetRallyPoint (Vector3 position)
		{
				rallyPoint = position;
				if (player && player.human && currentlySelected) {
						RallyPoint flag = player.GetComponentInChildren<RallyPoint> ();
						if (flag)
								flag.transform.localPosition = rallyPoint;
				}
		}
		
		public void Sell ()
		{
				if (player)
						player.AddResource (ResourceType.Money, sellValue);
				if (currentlySelected)
						SetSelection (false, playingArea);
				Destroy (this.gameObject);
		}

		public void StartConstruction ()
		{
				CalculateBounds ();
				needsBuilding = true;
				hitPoints = 0;
				SetSpawnPoint ();
				rallyPoint = spawnPoint;
		}

		public bool UnderConstruction ()
		{
				return needsBuilding;
		}

		public void Construct (int amount)
		{
				hitPoints += amount;
				if (hitPoints >= maxHitPoints) {
						hitPoints = maxHitPoints;
						needsBuilding = false;
						RestoreMaterials ();
						SetTeamColor ();
				}
		}

		private void DrawBuildProgress ()
		{
				GUI.skin = ResourceManager.SelectBoxSkin;
				Rect selectBox = WorkManager.CalculateSelectionBox (selectionBounds, playingArea);
				GUI.BeginGroup (playingArea);
				CalculateCurrentHealth (0.5f, 0.99f);
				DrawHealthBar (selectBox, "Building ...");
				GUI.EndGroup ();
		}

		private void SetSpawnPoint ()
		{
				float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
				float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
				spawnPoint = new Vector3 (spawnX, 0.0f, spawnZ);
		}
		
		protected override void HandleLoadedProperty (JsonTextReader reader, string propertyName, object readValue)
		{
				base.HandleLoadedProperty (reader, propertyName, readValue);
				switch (propertyName) {
				case "NeedsBuilding":
						needsBuilding = (bool)readValue;
						break;
				case "SpawnPoint":
						spawnPoint = LoadManager.LoadVector (reader);
						break;
				case "RallyPoint":
						rallyPoint = LoadManager.LoadVector (reader);
						break;
				case "BuildProgress":
						currentBuildProgress = (float)(double)readValue;
						break;
				case "BuildQueue":
						buildQueue = new Queue<string> (LoadManager.LoadStringArray (reader));
						break;
				case "PlayingArea":
						playingArea = LoadManager.LoadRect (reader);
						break;
				default:
						break;
				}
		}
}
