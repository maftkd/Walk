using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkMan : MonoBehaviour
{
	Vector3 _flatForward;
	Vector3 _flatRight;
	Vector3 _groundPoint;
	Vector3 _prevPos;
	Vector3 _movement;

	public LayerMask _walkable;

	float _playerHeight=1.5f;
	float _maxDropHeight=10f;
	float _stepHeight=0.5f;

	//speed
	float _walkSpeed;
	float _defaultWalkSpeed=5f;
	float _yLerp=10f;

	//collision
	Collider[] _colliders;
	int _maxColliders=10;
	float _wallHitRadius=0.5f;

	//fall physics
	float _grav=10;
	float _fallVel=0;
	float _fallFactor=0;

	int _lockState=1;

    // Start is called before the first frame update
    void Start()
    {
		SnapStartingHeight();
		_walkSpeed=_defaultWalkSpeed;
		_colliders = new Collider[_maxColliders];
		Cursor.visible=false;
		Cursor.lockState = CursorLockMode.Locked;

		//temp
		Cursor.visible=true;
		Cursor.lockState = CursorLockMode.None;
		_lockState=0;
    }
	
	void SnapStartingHeight(){
		RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, _walkable))
        {
			_groundPoint=hit.point;
			transform.position=_groundPoint+Vector3.up*_playerHeight;
        }
	}

    // Update is called once per frame
    void Update()
    {

		//disable stuff
		if(Input.GetKeyDown(KeyCode.P)){
			Cursor.visible=true;
			Cursor.lockState = CursorLockMode.None;
			_lockState=0;
		}
		if(Input.GetKeyDown(KeyCode.U)){
			Cursor.visible=false;
			Cursor.lockState = CursorLockMode.Locked;
			_lockState=1;
		}

		if(_lockState==0)
			return;

		//look
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");
		transform.Rotate(Vector3.up*mouseX);
		transform.Rotate(-Vector3.right*mouseY);
		Vector3 eulers = transform.eulerAngles;
		eulers.z=0;
		transform.eulerAngles=eulers;

		_flatForward=transform.forward;
		_flatForward.y=0;
		_flatForward.Normalize();
		_flatRight = Vector3.Cross(Vector3.up,_flatForward);

		//try movement
		_movement = Vector3.zero;
		_movement+=_flatForward*Input.GetAxis("Vertical");
		_movement+=_flatRight*Input.GetAxis("Horizontal");
		if(_movement.sqrMagnitude>1)
			_movement.Normalize();
		transform.position+=_movement*Time.deltaTime*_walkSpeed;

		//check for wall
		int numColliders= Physics.OverlapSphereNonAlloc(transform.position, 
				_wallHitRadius, _colliders);
		Vector3 closePoint=Vector3.zero;

		int attempts=0;
		while(numColliders>0&&attempts<5){
			for(int i=0; i<numColliders; i++){
				closePoint=_colliders[i].ClosestPoint(transform.position);
				//check if transform is still within collider
				if((closePoint-_colliders[i].transform.position).sqrMagnitude <
						(transform.position-_colliders[i].transform.position).sqrMagnitude)
				{
					//offset player from wall
					transform.position=closePoint+
						(transform.position-closePoint).normalized*_wallHitRadius;
				}
			}

			numColliders= Physics.OverlapSphereNonAlloc(transform.position, 
					_wallHitRadius, _colliders);
			attempts++;
		}

		//check for ground
		RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, _walkable))
        {
			//stepping up
			if(hit.distance<_playerHeight && hit.distance>_playerHeight-_stepHeight){
				_groundPoint=hit.point;
			}
			//stepping down
			else if(hit.distance>_playerHeight){
				//step down
				if(hit.distance<_playerHeight+_stepHeight){
					_groundPoint=hit.point;
				}
				//drop down
				else if(hit.distance<_playerHeight+_maxDropHeight){
					_groundPoint=hit.point;
					if(_fallVel==0)
					{
						_fallVel=0.1f;
						_fallFactor=((transform.position.y-_playerHeight)-hit.point.y)/
							_maxDropHeight;
					}
					//temp - until we code in drop
					//transform.position=_prevPos;
				}
				//cliff
				else{
					//no jump
					transform.position=_prevPos;
				}
			}
			//flat walking
			else{
				_groundPoint=hit.point;
				//transform.position=_groundPoint+Vector3.up*_playerHeight;
			}
        }

		//animate y coord
		Vector3 p = transform.position;
		//falling
		if(_fallVel>0){
			_fallVel+=_grav*Time.deltaTime;
			p.y-=_fallVel*Time.deltaTime;
			//done falling
			if(p.y<_groundPoint.y+_playerHeight-_stepHeight*_fallFactor)
				_fallVel=0;
		}
		//normal walking or step up/down
		else
			p.y=Mathf.Lerp(p.y,_groundPoint.y+_playerHeight,_yLerp*Time.deltaTime);
		transform.position=p;

		_prevPos=transform.position;
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.DrawRay(transform.position,_flatForward);
		Gizmos.color=Color.red;
		Gizmos.DrawRay(transform.position,_flatRight);
		Gizmos.color=Color.yellow;
		Gizmos.DrawSphere(_groundPoint,0.15f);
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,0.5f);
	}
}
