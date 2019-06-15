using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotionController : MonoBehaviour {

	public Camera camera;

	public float sensitivityX = 3.4f;
	public float sensitivityY = 3.4f;
	//public bool freeLook = true;

	public float walkSpeed = 6;
	public float runSpeed = 10;
	public float strafeSpeed = 5;
	public float gravity = 20;
	public float jumpHeight = 2;
	public bool canJump = true;

	public bool smoothTest = false;

	private Rigidbody rigidBody;
	private bool mouseGrabbed = false;
	private bool isGrounded = false;

	void Start () {
		this.Awake ();

		setCursorState(true);
	}

	void Awake () {
		this.rigidBody = GetComponent<Rigidbody> ();
		this.rigidBody.freezeRotation = true;
		this.rigidBody.useGravity = false;
	}

	void Update () {
		if(!this.smoothTest) checkGrounded();

        if (this.smoothTest) {
            this.transform.position = new Vector3(Mathf.Cos(Time.time), 0, Mathf.Sin(Time.time));
            return;
        }

        bool unlocked = isMouseGrabbed();
        // get correct speed
        float forwardAndBackSpeed = walkSpeed;

        // if running, set run speed
        /*if (isRunning) {
			forwardAndBackSpeed = runSpeed;
		}*/

        // calculate how fast it should be moving
        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal") * strafeSpeed, 0, Input.GetAxis("Vertical") * forwardAndBackSpeed);
        targetVelocity = this.camera.transform.TransformDirection(targetVelocity) * (unlocked ? 1.0f : 0.0f);

        // apply a force that attempts to reach our target velocity
        Vector3 velocity = this.rigidBody.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.y = 0;
        this.rigidBody.AddForce(velocityChange, ForceMode.VelocityChange);

        // jump
        if (canJump && isGrounded && Input.GetButton("Jump") && unlocked)
        {
            this.rigidBody.velocity = new Vector3(velocity.x, Mathf.Sqrt(2 * jumpHeight * gravity), velocity.z);
            isGrounded = false;
        }

        // apply gravity
        this.rigidBody.AddForce(new Vector3(0, -gravity * this.rigidBody.mass, 0));

        if (mouseGrabbed) {
			float rotationX = this.camera.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			float rotationY = this.camera.transform.localEulerAngles.x - Input.GetAxis ("Mouse Y") * sensitivityY;

            //if(!freeLook)  rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            rotationY = UnwrapAngle(Mathf.Clamp(WrapAngle(rotationY), -90.0f, 90.0f));

            this.camera.transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
		}

		if(Input.GetKeyDown(KeyCode.Escape)) {
			setCursorState(!mouseGrabbed);
		}
	}

	void checkGrounded() {
		/* ==============
         * REMEMBER
         * ==============
         * If you change the size of the prefab, you may have
         * to change the length of the ray to ensure it hits
         * the ground.
         * 
         * All obstacles/walls/floors must have rigidbodies
         * attached to them. If not, Unity physics may get
         * confused and the player can jump really high
         * when in a corner between 2 walls for example.
         */
		float rayLength = 2.0f;
		RaycastHit hit;
		Ray ray = new Ray(transform.position, -transform.up);
		//Debug.DrawRay(ray.origin, ray.direction * rayLength);
		// if there is something directly below the player
		if (Physics.Raycast(ray, out hit, rayLength)) {
			isGrounded = true;
		}
	}

	public void setCursorState(bool state) {
		mouseGrabbed = state;
		Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = !state;
	}

	public bool isMouseGrabbed() {
		return mouseGrabbed;
	}

    private static float WrapAngle(float angle) {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    private static float UnwrapAngle(float angle) {
        if (angle >= 0)
            return angle;

        angle = -angle % 360;

        return 360 - angle;
    }
}
