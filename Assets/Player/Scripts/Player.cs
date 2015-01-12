﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Player : MonoBehaviour
{

		public string playerName;
		public bool human;
		public HUD hud;
		public WorldObject SelectedObject { get; set; }
		public int startMoney;
		public int startMoneyLimit;
		public int startPower;
		public int startPowerLimit;

		private Dictionary<ResourceType, int> resources;
		private Dictionary<ResourceType, int> resourceLimits;


		void Awake ()
		{
				resources = InitResourceList ();
				resourceLimits = InitResourceList ();
		}

		// Use this for initialization
		void Start ()
		{
				hud = GetComponentInChildren < HUD> ();
				AddStartResources ();
				AddStartResourceLimits ();
		}
	
		// Update is called once per frame
		void Update ()
		{
				if (human) {
						hud.SetResourceValues (resources, resourceLimits);
				}
		}

		private Dictionary<ResourceType, int> InitResourceList ()
		{
				Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int > ();
				list.Add (ResourceType.Money, 0);
				list.Add (ResourceType.Power, 0);
				return list;
		}

		private void AddStartResourceLimits ()
		{
				IncrementResourceLimit (ResourceType.Money, startMoneyLimit);
				IncrementResourceLimit (ResourceType.Power, startPowerLimit);
		}
		private void AddStartResources ()
		{
				AddResource (ResourceType.Money, startMoney);
				AddResource (ResourceType.Power, startPower);
		}

		public void AddResource (ResourceType type, int amount)
		{
				resources [type] += amount;
		}
		private void IncrementResourceLimit (ResourceType type, int amount)
		{
				resourceLimits [type] += amount; 
		}

		public void AddUnit (string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation, Building creator)
		{
				Units units = GetComponentInChildren<Units> ();
				GameObject newUnit = (GameObject)Instantiate (ResourceManager.GetUnit (unitName), spawnPoint, rotation);
				newUnit.transform.parent = units.transform;
				Unit unitObject = newUnit.GetComponent<Unit> ();
				if (unitObject) {
						unitObject.Init (creator);
						if (spawnPoint != rallyPoint)
								unitObject.StartMove (rallyPoint);
				}
		}
}