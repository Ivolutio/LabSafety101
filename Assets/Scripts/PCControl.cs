﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCControl : MonoBehaviour {

	#region Camera Control
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15f;
	public float sensitivityY = 15f;

	public float minimumX = -360f;
	public float maximumX = 360f;

	public float minimumY = -60f;
	public float maximumY = 60f;

	private float rotationY = 0f;
    #endregion
    public bool lockCursor;
	public new Camera camera;
	public float pointerRange;
    public Vector3 holdObjectOffset;
	private PickupObject targetPickup;
	private PickupObject heldPickup;
	private PlacementArea targetArea;
    private Container targetContainer;
    private CraftingGridSlot currCraftArea;
    private string _menuButton;
    [SerializeField] private MenuButtons menu;

    private void Start() {
        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
	}
	
	private void Update() {
        if(!Input.GetMouseButton(1)) {
            MouseRotation();
        }
        if(targetPickup == null || !Input.GetMouseButton(1)) {
            CheckPointer();
        }

        if (_menuButton != null)
        {
            if (Input.GetMouseButtonUp(0))
                menu.UseButton(_menuButton);
        }

        if (heldPickup != null) {
            if(Input.GetMouseButtonUp(0)) {
                heldPickup.Release(targetArea, targetContainer);
                GameController.Highlight(heldPickup.type, false);
                heldPickup = null;
                GameController._.ReportAction(GameController.Action.PlaceDown);
            }
        }else if(targetPickup != null) {
			if(Input.GetMouseButtonDown(0)) {
				heldPickup = targetPickup;
				heldPickup.Pickup(camera.transform, holdObjectOffset);
				GameController.Highlight(heldPickup.type, true);
                if(targetPickup.filled)
                    GameController._.ReportAction(GameController.Action.PickupFull);
                else
                    GameController._.ReportAction(GameController.Action.PickupEmpty);
            }
            if(targetPickup.currentArea is CraftingGridSlot) {
                currCraftArea = (CraftingGridSlot)targetPickup.currentArea;
                if(currCraftArea.canCraft) {
                    if(Input.GetMouseButtonDown(1)) {
                        currCraftArea.ActivateCrafting();
                        GameController._.ReportAction(GameController.Action.Pour);
                    } else if(Input.GetMouseButtonUp(1)) {
                        currCraftArea.DeactivateCrafting();
                        GameController._.ReportAction(GameController.Action.PourStop);
                    }
                }
            } 
        }

        if(currCraftArea != null) {
            if(Input.GetMouseButton(1)) {
                currCraftArea.item.transform.Rotate(new Vector3(0f, 0f, -Input.GetAxis("Mouse X") * sensitivityX), Space.Self);
            }
            if(Input.GetMouseButtonUp(1)) {
                currCraftArea.DeactivateCrafting();
                currCraftArea = null;
                GameController._.ReportAction(GameController.Action.PourStop);
            }
        }
	}

	private void CheckPointer(){
        RaycastHit hit;
        Ray ray = camera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 12))
            _menuButton = hit.collider.gameObject.name;
        else
            _menuButton = null;

        if (heldPickup == null){ //interactables
			int layerMask = (1 << 8);
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
				targetPickup = hit.collider.attachedRigidbody.GetComponent<PickupObject>();
			}else{
				targetPickup = null;
			}
		}
		
		if(heldPickup != null){ //placement areas
			int layerMask = (1 << 9);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                PlacementArea area = hit.collider.GetComponent<PlacementArea>();
                if (area.TargetHit(heldPickup.type))
                {
                    targetArea = area;
                }
                else
                {
                    targetArea = null;
                }
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 11))
            {
                Container waistContainer = hit.collider.GetComponent<Container>();
                if (waistContainer.TargetHit(heldPickup.type))
                    targetContainer = waistContainer;
                else
                    targetArea = null;
            }
            else
            {
                targetArea = null;
            }
		}
	}

	private void MouseRotation(){
		if (axes == RotationAxes.MouseXAndY){
			float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}else if (axes == RotationAxes.MouseX){
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
		}else{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
	}
}
