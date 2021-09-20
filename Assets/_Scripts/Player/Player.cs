using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Player : MonoBehaviour {

	public PlayerController _controller;
	public static Player instance;
	public static PlayerController controller {
		get {
			return instance._controller;
		}
	}

	public PlayerControllerWater _waterController;
	public static PlayerControllerWater waterController {
		get {
			return instance._waterController;
		}
	}

	public static Vector3 position {
		get {
			return instance.transform.position - new Vector3(0.0f, 0.8f, 0.0f);
		}

		set {
			instance.transform.position = value + new Vector3(0.0f, 0.8f, 0.0f);
		}
	}

	public static Vector3 forward {
		get {
			return instance.transform.forward;
		}
	}

	[HideInInspector] public Character character;

	public static int health {
		set {
			instance.character.health = value;
		}

		get {
			return instance.character.health;
		}
	}

	public static int armor {
		set {
			instance.character.armor = value;
		}

		get {
			return instance.character.armor;
		}
	}

	public static int maxHealth {
		set {
			instance.character.maxHealth = value;
		}

		get {
			return instance.character.maxHealth;
		}
	}

	public static int maxArmor {
		set {
			instance.character.maxArmor = value;
		}

		get {
			return instance.character.maxArmor;
		}
	}

	[HideInInspector] public float _oxygen = 1.0f;
	public static float oxygen {
		set {
			instance._oxygen = Mathf.Clamp01(value);
			HUDOxygen.amount = Mathf.Clamp01(value);
		}

		get {
			return instance._oxygen;
		}
	}

	private WeaponManager weaponManager = new WeaponManager();
	private Transform camera;

	//public Material weaponMaterial;
	//public RenderTexture weaponRender;

	public Material[] applyMats;
	private Dictionary<string, RenderTexture> applyMatsRenders = new Dictionary<string, RenderTexture>();

	[HideInInspector] public WeaponBase _weapon = null;
	public static WeaponBase weapon {
		get {
			return instance._weapon;
		}
	}

	public static void SetWeapon(string assetBundle = null, bool ignoreWaterLimit = false) {
		if (!ignoreWaterLimit && Player.controller.state != PlayerControllerState.NORMAL) {
			return;
		}

		if (weapon == null && assetBundle != null) {
			HUDElements.AddElement ("Crosshair");
			instance._weapon = WeaponManager.SetPlayerWeapon (assetBundle);
		} else if(weapon != null) {
			float length = weapon.TakeOff ();
			instance.StartCoroutine(instance.WeaponTakeOff(length, assetBundle));
		} 
	}

	public IEnumerator WeaponTakeOff(float length, string assetBundle = null) {
		yield return new WaitForSeconds (length);
		if (assetBundle != null && assetBundle != Player.weapon.assetBundle) {
			instance._weapon = WeaponManager.SetPlayerWeapon (assetBundle);
			HUDElements.AddElement ("Crosshair");
		} else {
			WeaponManager.SetPlayerNoWeapon ();
			instance._weapon = null;
			HUDAmmo.NoWeapon ();
			HUDElements.RemoveElement ("Crosshair");
		}
	}

	public static void StopController () {
		controller.characterController.Move ((forward * 5.0f) * Time.deltaTime);
	}

	public EventData onHealthChanged(EventData data) {
		Events.Character.HealthChanged args = data.Get<Events.Character.HealthChanged> (0);
		HUDHealth.amount = (float)args.health / (float)character.maxHealth;
		return new EventData ();
	}

	public EventData onArmorChanged(EventData data) {
		Events.Character.ArmorChanged args = data.Get<Events.Character.ArmorChanged> (0);
		HUDArmor.amount = (float)args.armor / (float)character.maxArmor;
		return new EventData();
	}

	public EventData onGrounded(EventData data) {
		//Debug.Log (controller.characterController.velocity.magnitude);
		float magnitude = -controller.characterController.velocity.y + controller.characterController.velocity.magnitude;
		float inAirTimer = controller.inAirTimer;

		if (!controller.inAirJumped) {
			magnitude *= 5.0f * (inAirTimer * 0.2f);
			if (inAirTimer > 1.0f) {
				magnitude *= 5.0f;
			}
		} else {
			inAirTimer *= 0.6f;
		}

		//Debug.Log (magnitude + " " + inAirTimer + " ");

		if (inAirTimer < 0.65f) {
			return new EventData ();
		}

		int health = Mathf.RoundToInt(magnitude * 4.0f);

		character.health -= health;

		//Debug.Log ("DAMAGE: " + health);

		return new EventData ();
	}

	public static RenderTexture GetApplyMatRenderTexture(string name) {
		RenderTexture tex;
		if (instance.applyMatsRenders.TryGetValue (name, out tex)) {
			return tex;
		}
		return null;
	}

	void Start () {
		instance = this;

		//AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/animators"));
		AnimatorsLoader.Load();

		if (WeaponManager.instance == null) {
			weaponManager = new WeaponManager ();
			weaponManager.Start ();
		} else {
			weaponManager = WeaponManager.instance;
		}
		WeaponManager.InitializePlayerSide ();

		character = gameObject.AddComponent<Character> ();
		character.healthChanged.AddListener (onHealthChanged);
		character.armorChanged.AddListener (onArmorChanged);
		character.faction = "Player";

		EventManager.AddEventListener<Events.PlayerController.Grounded> (onGrounded);

		_controller = new PlayerController ();
		controller.Start (GetComponent<CharacterController>(), transform);
		_waterController = new PlayerControllerWater ();
		_waterController.head = transform.Find ("CrouchHelper");
		_waterController.mouseLook = _controller.mouseLook;
		_waterController.controller = GetComponent<CharacterController> ();

		Transform UI = transform.Find ("UI");
		UI.gameObject.SetActive (true);
		AssetBundle bundle = AssetBundle.LoadFromFile (Path.Combine (Application.dataPath, "AssetBundles/hud"));
		GameObject prefab = bundle.LoadAsset<GameObject> ("HUD");
		GameObject obj = Instantiate (prefab, UI);
		obj.name = "HUD";
		bundle.Unload (true);

		camera = transform.Find ("CrouchHelper").Find ("Head").Find ("WalkEffects").Find ("Camera");
		foreach (Material material in applyMats) {
			PostProcessingCaller matApply = camera.gameObject.AddComponent<PostProcessingCaller> ();
			matApply.material = material;
			applyMatsRenders.Add(material.name, material.GetTexture("_RenderTexture") as RenderTexture);
		}

		character.head = camera;

	}

	void Update () {
		if (controller.state == PlayerControllerState.WATER) {
			waterController.Update ();
		} else if (controller.state == PlayerControllerState.NORMAL || controller.state == PlayerControllerState.LADDER) {
			controller.Update ();
		}
	}

	void FixedUpdate() {
		if (controller.state != PlayerControllerState.NORMAL) {
			controller.cameraEffect.addFOV = Mathf.Lerp (controller.cameraEffect.addFOV, 0.0f, PlayerController.FOV_SPEED * Time.fixedDeltaTime);
			controller.mouseLook.sideWalk = FirstPersonHeadControllerSideWalk.NONE;
		}

		if (controller.state == PlayerControllerState.NORMAL || controller.state == PlayerControllerState.LADDER) {
			controller.FixedUpdate ();
		}
	}

	void OnDrawGizmos() {
		if (instance == null) {
			instance = this;
		}

		if (waterController == null) {
			_waterController = new PlayerControllerWater ();
		}


		waterController.OnDrawGizmos ();
	}
}
