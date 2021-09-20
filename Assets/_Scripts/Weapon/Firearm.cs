using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum FirearmReloadType {
	BASIC,
	SHOTGUN
}

public class Firearm : WeaponBase {

	public float FOV = 60.0f;
	public float sightFOV = 40.0f;
	public float playerSightFOVAdd = -20.0f;
	public int maxAmmoSet = 1;

	private Transform offset;
	private Transform head;
	private Transform cameraBone;
	private Transform cameraObj;
	private Vector3 forward;
	private Vector3 forwardVelocity = Vector3.zero;
	private float forwardSmoothTime = 0.1f;
	private Vector3 forwardOffset = Vector3.zero;
	private Vector3 offsetOrigin = Vector3.zero;

	private Animator animator;
	private float transitionSpeed = 5.0f;
	private float walk = -1.0f;
	[HideInInspector] public AnimationSystem animationSystem;

	[HideInInspector] public bool inSight = false;
	private Vector3 cameraOrigin;
	private Transform sight;

	private int _ammo = 0;
	private int _maxAmmo = 0;
	private int _reloadAmmo = 0;

	private FOVEffect sightEffect;
	private Camera camera;
	private Camera muzzleCamera;

	private FOVEffect shootEffect;
	private float shootFOVTarget = -12.0f;

	private bool takenUp = false;

	private bool shooting = false;
	private float shootTimer = 0.0f;

	private GameObject muzzleFlashPrefab;
	private Transform muzzleFlashPos;

	private GameObject ammoDropPrefab;
	private Transform ammoDropPos;

	public int ammo {
		set {
			_ammo = value;
			if (_ammo > maxAmmo) {
				_ammo = _maxAmmo;
			}
			HUDAmmo.ammo = _ammo;
		} 

		get {
			return _ammo;
		}
	}

	public int maxAmmo {
		set {
			_maxAmmo = value;
		} 

		get {
			return _maxAmmo;
		}
	}

	public int reloadAmmo {
		set {
			_reloadAmmo = value;
			HUDAmmo.reloadAmmo = _reloadAmmo;
		} 

		get {
			return _reloadAmmo;
		}
	}

	//[HideInInspector] public string assetBundle;

	private bool reloading = false;

	public int ammoDropFrame = 0;

	public FirearmReloadType reloadType = FirearmReloadType.BASIC;

	public float visualRecoilPower = 1.0f;

	public int bulletCount = 1;
	public float spread = 1.0f;
	public float accuracyAngle = 5.0f;
	public float sightSpreadMultiplier = 0.3f;
	private float currentSpread = 0.0f;
	public float verticalRecoil = 1.0f;
	public float horizontalRecoil = 0.25f;
	public int damage = 20;

	void Start () {
		inSight = false;
		offset = transform.Find("Offset");
		//takenUp = true;

		HUDAmmo.ammo = ammo;
		HUDAmmo.reloadAmmo = reloadAmmo;

		offset.localRotation = Quaternion.identity;
		forward = Vector3.zero;

		Transform helper = new GameObject("Helper").transform;
		helper.SetParent (transform);
		helper.localPosition = Vector3.zero;
		helper.localRotation = Quaternion.identity;

		sight = transform.Find ("Sight");

		Transform hands = offset.Find ("Hands");
		Transform weapon = offset.Find ("Weapon");
		Transform humanRig = offset.Find ("HumanRig");

		muzzleFlashPos = humanRig.Find ("WeaponMuzzleFlashPos");
		ammoDropPos = humanRig.Find ("WeaponAmmoDrop");

		if (muzzleFlashPos != null || ammoDropPos != null) {
			AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "AssetBundles/" + assetBundle));

			if (muzzleFlashPos != null) {
				muzzleFlashPrefab = bundle.LoadAsset<GameObject> ("PlayerMuzzleFlash");
			} 

			if (ammoDropPos != null) {
				ammoDropPrefab = bundle.LoadAsset<GameObject> ("PlayerAmmoDrop");
			}

			bundle.Unload (false);
		}

		hands.SetParent (helper, true);
		weapon.SetParent (helper, true);
		humanRig.SetParent (helper, true);

		helper.SetParent (offset, true);

		animator = GetComponent<Animator> ();
		animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

		animationSystem = gameObject.AddComponent<AnimationSystem> ();
		animationSystem.assetBundle = assetBundle;
		animationSystem.Initialize ("Weapon");

		animator.Update (Time.deltaTime);

		cameraObj = transform.Find ("Camera");
		cameraObj.localRotation = Quaternion.identity;
		camera = cameraObj.gameObject.AddComponent<Camera> ();
		camera.targetTexture = Player.GetApplyMatRenderTexture("Weapon");
		camera.fieldOfView = FOV;
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.backgroundColor = new Color (0.0f, 0.0f, 0.0f, 0.0f);
		camera.cullingMask = 512;
		camera.nearClipPlane = 0.01f;
		cameraOrigin = cameraObj.localPosition;

		GameObject muzzleFlashCameraObj = new GameObject ("MuzzleFlash");
		Transform muzzleFlash = muzzleFlashCameraObj.transform;
		muzzleFlash.SetParent (cameraObj);
		muzzleFlash.localPosition = Vector3.zero;
		muzzleFlash.localRotation = Quaternion.identity;

		muzzleCamera = muzzleFlashCameraObj.AddComponent<Camera> ();
		muzzleCamera.targetTexture = Player.GetApplyMatRenderTexture("MuzzleFlash");
		muzzleCamera.fieldOfView = FOV;
		muzzleCamera.clearFlags = CameraClearFlags.SolidColor;
		muzzleCamera.backgroundColor = new Color (0.0f, 0.0f, 0.0f, 1.0f);
		muzzleCamera.cullingMask = 1024;
		muzzleCamera.nearClipPlane = 0.01f;
		//animator.Update (Time.deltaTime * 3000.0f);

		head = Player.controller.mouseLook.head;

		cameraBone = humanRig.Find ("CameraBone");

		offsetOrigin = offset.localPosition;

		sightEffect = FOVEffects.RegisterEffect ();
		shootEffect = FOVEffects.RegisterEffect ();
		takenUp = true;
		animator.SetBool ("TakenUp", true);

		maxAmmo = maxAmmoSet;
		ammo = maxAmmo;
		reloadAmmo = maxAmmo;
	}

	private float inAirAdd = 0.0f;
	private float recoilAdd = 0.0f;
	void ProcessDragging() {
		shootTimer += Time.fixedDeltaTime;

		if (Player.controller.inAir && Player.controller.inAirTimer > 0.05f) {
			inAirAdd = Mathf.Lerp (0.0f, 0.5f, Time.fixedDeltaTime * 6.0f);
		} else {
			inAirAdd = Mathf.Lerp (inAirAdd, 0.0f, Time.fixedDeltaTime * 16.0f);
		}

		if (shooting && shootTimer < 0.1f) {
			recoilAdd = Mathf.Lerp (0.0f, 2.0f * visualRecoilPower, Time.fixedDeltaTime * 32.0f);
		} else {
			recoilAdd = Mathf.Lerp (recoilAdd, 0.0f, Time.fixedDeltaTime * 12.0f);
		}

		Vector3 offsetPos = offset.localPosition;

		float height = Mathf.Lerp(offsetPos.y, offsetOrigin.y + inAirAdd + (recoilAdd * 0.01f), 12.0f * Time.fixedDeltaTime);
		height = Mathf.Lerp(offsetPos.y, height + (recoilAdd * 0.00025f), 32.0f * Time.fixedDeltaTime);
		offsetPos.y = height;
		offsetPos.z = Mathf.Lerp (offsetPos.z, offsetOrigin.z - (recoilAdd * 0.06f), 32.0f * Time.fixedDeltaTime);

		offset.localPosition = offsetPos;

		float multiplier = 0.002f * 0.6f;
		float X = -Input.GetAxis ("Horizontal") * multiplier;
		float Y = -Input.GetAxis ("Vertical") * multiplier;
		Vector3 targetForward = new Vector3 (Y, X, 1.0f) * 0.05f;
		targetForward += forwardOffset;
		targetForward.y += inAirAdd * 0.1f;
		targetForward.y += recoilAdd * 0.0005f;

		float delta = Vector3.Angle(forward, targetForward);
		if (delta > 0.0f) {
			Vector3 newForward = Vector3.SmoothDamp (forward, targetForward, ref forwardVelocity, forwardSmoothTime, 10000.0f, Time.fixedDeltaTime);
			forward = Vector3.Slerp(forward, newForward, 24.0f * Time.fixedDeltaTime);
		}

		offset.localRotation = Quaternion.LookRotation (forward);
	}

	void FixedUpdate () {
		ProcessDragging ();
		shootEffect.addFOV = Mathf.Lerp (shootEffect.addFOV, 0.0f, 2.0f * Time.fixedDeltaTime);
	}

	void Idle() {
		if (shooting || reloading) {
			//walk = Mathf.Lerp (walk, -1.0f, transitionSpeed * 7.0f * Time.deltaTime);
			//animator.SetFloat ("Sight", Mathf.Lerp(animator.GetFloat("Sight"), 1.0f, 8.0f * Time.deltaTime));
			walk = -1.0f;
			animator.SetFloat ("Walk", walk);
			animator.SetFloat ("Sight", 1.0f);
			return;
		}

		if (Player.controller.inAir) {
			inSight = false;
			walk = Mathf.Lerp (walk, 0.0f, 0.5f * Time.deltaTime);
			animator.SetFloat ("Walk", walk);
			animator.SetFloat ("Sight", 0.0f);
			return;
		}

		if (InputManager.GetButton ("PlayerForward") ||
		    InputManager.GetButton ("PlayerBackward") ||
		    InputManager.GetButton ("PlayerLeft") ||
		    InputManager.GetButton ("PlayerRight")) {
			if (InputManager.GetButton ("PlayerRun")) {
				walk = Mathf.Lerp (walk, 2.0f, transitionSpeed * Time.deltaTime);
			} else {
				walk = Mathf.Lerp (walk, 1.0f, transitionSpeed * Time.deltaTime);
			}
		} else {
			walk = Mathf.Lerp (walk, 0.0f, transitionSpeed * Time.deltaTime);
		}
		animator.SetFloat ("Walk", walk);

		if (inSight) {
			animator.SetFloat ("Sight", 1.0f);
		} else {
			animator.SetFloat ("Sight", 0.0f);
		}
	}

	void Controls() {
		if (Player.controller.state != PlayerControllerState.NORMAL) {
			return;
		}

		if (reloading) {
			return;
		}

		if (InputManager.GetButton ("PlayerShoot")) {
			PrimaryFire ();
		} 

		if (InputManager.GetButton ("PlayerSight")) {
			SecondaryFire ();
		}

		if (InputManager.GetButtonDown ("PlayerShoot")) {
			SinglePrimaryFire ();
		} 

		if (InputManager.GetButtonDown ("PlayerSight")) {
			SingleSecondaryFire ();
		}

		if (InputManager.GetButton ("PlayerRun")) {
			inSight = false;
			HUDCrosshair.enable = !inSight;
		}

		if (InputManager.GetButtonDown ("PlayerReload")) {
			Reload ();
		}
	}

	private void ReloadAnimationEnd() {
		reloading = false;
		inSight = false;
		ammo = maxAmmo;
	}

	private IEnumerator ShotgunReload() {
		float startSecs = animationSystem.GetLength ("ReloadStart");
		float cycleSecs = animationSystem.GetLength ("ReloadCycle");
		float endSecs = animationSystem.GetLength ("ReloadEnd");
		animator.SetTrigger ("Reload");
		yield return new WaitForSeconds (startSecs);
		while (true) {
			yield return new WaitForSeconds (cycleSecs);
			ammo++;
			if (ammo >= maxAmmo) {
				break;
			}
		}
		animator.SetTrigger ("ReloadOver");
		yield return new WaitForSeconds (endSecs);
		reloading = false;
	}

	private IEnumerator ReloadProcess(float seconds) {
		yield return new WaitForSeconds (seconds);
		ReloadAnimationEnd ();
	}

	public void Reload() {
		if (ammo >= maxAmmo) {
			return;
		}

		if (reloading) {
			return;
		}

		inSight = false;
		reloading = true;

		HUDCrosshair.enable = !inSight;

		if (reloadType == FirearmReloadType.BASIC) {
			if (ammo <= 0) {
				//animationSystem.SetTrigger ("ReloadFull", ReloadAnimationEnd);
				StartCoroutine(ReloadProcess(animationSystem.GetLength("ReloadFull")));
				animator.SetTrigger ("ReloadFull");
			} else {
				//animationSystem.SetTrigger ("Reload", ReloadAnimationEnd);
				StartCoroutine(ReloadProcess(animationSystem.GetLength("Reload")));
				animator.SetTrigger ("Reload");
			}
		} else if (reloadType == FirearmReloadType.SHOTGUN) {
			StartCoroutine (ShotgunReload ());
		}
	}

	public override void Climbing() {
		if (!reloading) {
			animator.SetTrigger ("Climbing");
			//animator.Play("Climbing");
		}
	}

	public override void TakeUp() {
		takenUp = true;
		animator.SetBool ("TakenUp", true);
		HUDElements.AddElement ("Crosshair");
	}

	private IEnumerator TakeUpProcess(float seconds) {
		yield return new WaitForSeconds (seconds);
		TakeUp ();
	}

	public override float TakeOff() {
		inSight = false;
		takenUp = false;
		animator.SetBool ("TakenUp", false);
		HUDElements.RemoveElement ("Crosshair");
		return animationSystem.GetLength ("TakeDown");
	}

	private bool Raycast(Vector3 position, Vector3 direction, out RaycastHit hit) {
		hit = new RaycastHit ();
		RaycastHit[] rawHits = Physics.RaycastAll (position, direction);
		List<RaycastHit> hits = new List<RaycastHit> ();
		hits.AddRange (rawHits);
		hits.Sort ((a, b) => {
			if (Vector3.Distance(a.point, position) > Vector3.Distance(b.point, position)) {
				return 1;
			} else if (Vector3.Distance(a.point, position) < Vector3.Distance(b.point, position)) {
				return -1;
			}
			return 0;
		});
		foreach(RaycastHit hitTest in hits) {
			if(hitTest.collider.GetComponent<Player>() == null) {
				hit = hitTest;
				return true;
			}
		}
		return false;
	}

	private void FireBulletFake(Vector3 direction, float radius, int damage = 0) {
		DamagableObject.damagables.Sort ((DamagableObject x, DamagableObject y) => {
			if (x.priority > y.priority) {
				return -1;
			} else if (x.priority < y.priority) {
				return 1;
			}
			return 0;
		});
		foreach (DamagableObject target in DamagableObject.damagables) {
			Vector3 dir = (target.transform.position - head.position).normalized;
			float angle = Vector3.Angle (direction, dir);
			angle = Mathf.Abs (angle);
			if (angle <= radius) {
				//Debug.Log (target);
				RaycastHit checkHit;
				if (Raycast (head.position + (dir * 0.1f), dir, out checkHit)) {
					DamagableObject targetObj = checkHit.collider.GetComponent<DamagableObject> ();
					if (targetObj != null && targetObj.damagable) {
						targetObj.OnDamage (damage, direction);
						break;
					}
				}
			}
		}
	}

	public void FireBullet(Vector3 direction, float radius, int damage = 0, int bulletCount = 1, float spread = 1.0f) {
		direction = direction.normalized;
		Vector3 originalDirection = direction;
		float bulletSpread = spread * 0.05f;
		for (int counter = 0; counter < bulletCount; counter++) {
			direction = originalDirection;
			direction.x += Random.Range (-bulletSpread, bulletSpread);
			direction.y += Random.Range (-bulletSpread, bulletSpread);
			direction.z += Random.Range (-bulletSpread, bulletSpread);
			direction = direction.normalized;
			RaycastHit hit;
			if (Raycast (head.position, direction, out hit)) {
				Vector3 position = hit.point;
				DamagableObject obj = hit.collider.GetComponent<DamagableObject> ();
				if (obj != null && obj.damagable) {
					obj.OnDamage (damage, direction);
				} else {
					FireBulletFake (direction, radius, damage);
				}
				MaterialManager.PlaceHitDecal (hit);
			} else {
				FireBulletFake (direction, radius, damage);
			}
		}
	}

	public void UpdateShootAnimationData() {
		shooting = true;
		shootTimer = 0.0f;
	}

	public IEnumerator LaunchAmmoDrop() {
		if (ammoDropPrefab != null) {
			yield return new WaitForSeconds (ammoDropFrame / 60.0f);

			GameObject ammoDrop = Instantiate (ammoDropPrefab, ammoDropPos);
			Rigidbody body = ammoDrop.GetComponent<Rigidbody> ();
			Vector3 force = head.right;
			force *= 200.0f;
			force.y += 100.0f;
			force.z += Random.Range (-25.0f, 25.0f);
			body.AddForce (force);

			Vector3 torque = new Vector3 (20.0f, 0.0f, 0.0f);
			torque.x += Random.Range (-100.0f, 100.0f);
			torque.y += Random.Range (-100.0f, 100.0f);
			torque.z += Random.Range (-100.0f, 100.0f);
			body.AddTorque (torque);
			ammoDrop.transform.SetParent (null, true);
		}
	}

	public virtual void PrimaryFire () {
		if (!shooting) {
			if (ammo <= 0) {
				return;
			}
			if (!inSight) {
				currentSpread += spread;
			} else {
				currentSpread += spread * sightSpreadMultiplier;
			}
			if (muzzleFlashPrefab != null) {
				for (int counter = 0; counter < bulletCount; counter++) {
					Transform muzzleFlash = Instantiate (muzzleFlashPrefab, muzzleFlashPos).transform;
					Quaternion rotation = muzzleFlash.localRotation;
					float muzzleFlashSpread = currentSpread * 0.01f;
					rotation.x += Random.Range (-muzzleFlashSpread, muzzleFlashSpread);
					rotation.y += Random.Range (-muzzleFlashSpread, muzzleFlashSpread);
					rotation.z += Random.Range (-muzzleFlashSpread, muzzleFlashSpread);
					muzzleFlash.localRotation = rotation;
				}
			}
			FireBullet (cameraObj.forward, accuracyAngle, damage, bulletCount, currentSpread);
			animationSystem.SetTrigger ("Shoot", ShootOver);
			UpdateShootAnimationData ();
			StartCoroutine (LaunchAmmoDrop ());
			ammo -= 1;
			HUDCrosshair.spread = currentSpread * 50.0f;

			CameraPuncher.Punch (
				new Vector3 (
					-verticalRecoil * Random.Range(6.0f, 10.0f), 
					horizontalRecoil * Random.Range(-10.0f, 10.0f), 
					0.0f
				)
			);

			shootEffect.addFOV = Mathf.Lerp (shootEffect.addFOV, shootFOVTarget, 5.0f * Time.deltaTime);
		}
	}

	public virtual void SecondaryFire () {
	}

	private void ShootOver() {
		shooting = false;
	}

	public virtual void SinglePrimaryFire () {		
	}

	public virtual void SingleSecondaryFire () {
		inSight = !inSight;
		HUDCrosshair.enable = !inSight;
	}


	void Sight() {
		if (inSight) {
			cameraObj.localPosition = Vector3.Lerp(cameraObj.localPosition, sight.localPosition, 10.0f * Time.deltaTime);
			float distance = Vector3.Distance (cameraObj.localPosition, sight.localPosition);
			distance = Mathf.Clamp01 (distance * 0.5f);
			distance = -distance;
			float height = Mathf.Lerp (0.0f, distance * 1.0f, -distance);
			forwardOffset.y = Mathf.Lerp (forwardOffset.y, height, Time.deltaTime * 8.0f);
			float cameraFOV = Mathf.Lerp(sightFOV, FOV, distance);
			float addFOV = Mathf.Lerp (playerSightFOVAdd, 0.0f, distance);
			camera.fieldOfView = Mathf.Lerp (camera.fieldOfView, cameraFOV, 6.0f * Time.deltaTime);
			muzzleCamera.fieldOfView = camera.fieldOfView;
			sightEffect.addFOV = Mathf.Lerp (sightEffect.addFOV, addFOV, 6.0f * Time.deltaTime);
		} else {
			cameraObj.localPosition = Vector3.Lerp(cameraObj.localPosition, cameraOrigin, 6.0f * Time.deltaTime);
			camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, FOV, 4.0f * Time.deltaTime);
			muzzleCamera.fieldOfView = camera.fieldOfView;
			sightEffect.addFOV = Mathf.Lerp (sightEffect.addFOV, 0.0f, 4.0f * Time.deltaTime);
		}
	}

	void Update() {
		Idle ();
		Controls ();
		Sight ();

		currentSpread = Mathf.Lerp (currentSpread, 0.0f, 5.0f * Time.deltaTime);
	}

	void OnDrawGizmos() {
		Transform offset = transform.Find ("Offset");
		if (offset != null) {
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere (offset.position, 0.1f);
		}
	}

	void OnDestroy() {
		//sightEffect.addFOV = 0.0f;
		FOVEffects.Remove(sightEffect);
	}

}
