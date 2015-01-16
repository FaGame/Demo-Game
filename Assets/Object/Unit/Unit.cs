﻿using UnityEngine;
using System.Collections;
using RTS;
using Newtonsoft.Json;

public class Unit : WorldObject
{
		public float moveSpeed;
		public float rotateSpeed;

		protected bool moving;
		protected bool rotating;

		private Vector3 destination;
		private Quaternion targetRotation;
		private GameObject destinationTarget;

		protected override void Awake ()
		{
				base.Awake ();
		}
	
		protected override void Start ()
		{
				base.Start ();
		}
	
		protected override void Update ()
		{
				base.Update ();
				if (rotating) {
						TurnToTarget ();
				} else if (moving) {
						MakeMove ();
				}
		}
		protected override void OnGUI ()
		{
				base.OnGUI ();
		}

		public override void SetHoverState (GameObject hoverObject)
		{
				base.SetHoverState (hoverObject);
				if (player && player.human && currentlySelected) {
						bool moveHover = false;
						if (hoverObject.name == "Ground")
								moveHover = true;
						else {
								Resource resource = hoverObject.transform.parent.GetComponent<Resource> ();
								if (resource && resource.isEmpty ())
										moveHover = true;
						}
						if (moveHover)
								player.hud.SetCursorState (CursorState.Move);
				}
		}
		public override void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller)
		{
				base.MouseClick (hitObject, hitPoint, controller);
				if (player && player.human && currentlySelected) {
						bool clickedOnEmptyResource = false;
						if (hitObject.transform.parent) {
								Resource resource = hitObject.transform.parent.GetComponent<Resource> ();
								if (resource && resource.isEmpty ())
										clickedOnEmptyResource = true;
						}
						if ((hitObject.name == "Ground" || clickedOnEmptyResource) && hitPoint != ResourceManager.InvalidPosition) {
								float x = hitPoint.x;
								float y = hitPoint.y + player.SelectedObject.transform.position.y;
								float z = hitPoint.z;
								Vector3 destination = new Vector3 (x, y, z);
								StartMove (destination);
						}
				}
		} 

		public override void SaveDetails (JsonWriter writer)
		{
				base.SaveDetails (writer);
				SaveManager.WriteBoolean (writer, "Moving", moving);
				SaveManager.WriteBoolean (writer, "Rotating", rotating);
				SaveManager.WriteVector (writer, "Destination", destination);
				SaveManager.WriteQuaternion (writer, "TargetRotation", targetRotation);
				if (destinationTarget) {
						WorldObject destinationObject = destinationTarget.GetComponent<WorldObject> ();
						if (destinationObject)
								SaveManager.WriteInt (writer, "DestinationTargetId", destinationObject.ObjectId);	
				}
		}
		public virtual void StartMove (Vector3 destination)
		{
				this.destination = destination;
				destinationTarget = null;
				targetRotation = Quaternion.LookRotation (destination - transform.position);
				rotating = true;
				moving = false;
		}
		
		public virtual void StartMove (Vector3 destination, GameObject destinationTarget)
		{
				StartMove (destination);
				this.destinationTarget = destinationTarget;
		}
		
		public virtual void SetBuilding (Building creator)
		{
				//specific initialization for a unit can be specified here
		}

		private void TurnToTarget ()
		{
				transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, rotateSpeed);
				CalculateBounds ();
				Quaternion inverseTargetRotation = new Quaternion (-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
				if (transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
						rotating = false;
						moving = true;
						if (destinationTarget)
								CalculateTargetDestination ();	
				}
				
				
		}

		private void MakeMove ()
		{
				transform.position = Vector3.MoveTowards (transform.position, destination, Time.deltaTime * moveSpeed);
				if (transform.position == destination) {
						movingIntoPosition = false;
						moving = false;
				}
				CalculateBounds ();
				
		}
		
		private void CalculateTargetDestination ()
		{
				//calculate number of unit vectors from unit center to unit edge of bounds
				Vector3 originalExtents = selectionBounds.extents;
				Vector3 normalExtents = originalExtents;
				normalExtents.Normalize ();
				float numberOfExtents = originalExtents.x / normalExtents.x;
				int unitShift = Mathf.FloorToInt (numberOfExtents);
				
				//calculate number of unit vectors from target center to target edge of bounds
				WorldObject worldObject = destinationTarget.GetComponent<WorldObject> ();
				if (worldObject)
						originalExtents = worldObject.GetSelectionBounds ().extents;
				else
						originalExtents = new Vector3 (0.0f, 0.0f, 0.0f);
				normalExtents = originalExtents;
				normalExtents.Normalize ();
				numberOfExtents = originalExtents.x / normalExtents.x;
				int targetShift = Mathf.FloorToInt (numberOfExtents);
				//calculate number of unit vectors between unit centre and destination centre with bounds just touching
				int shiftAmount = targetShift + unitShift;
				//calculate direction unit needs to travel to reach destination in straight line and normalize to unit vector
				Vector3 origin = transform.position;
				Vector3 direction = new Vector3 (destination.x - origin.x, 0.0f, destination.z - origin.z);
				direction.Normalize ();
				//destination = center of destination - number of unit vectors calculated above
				//this should give us a destination where the unit will not quite collide with the target
				//giving the illusion of moving to the edge of the target and then stopping
				for (int i = 0; i<shiftAmount; i++)
						destination -= direction;
				destination.y = destinationTarget.transform.position.y;			
		}
	                          
}
