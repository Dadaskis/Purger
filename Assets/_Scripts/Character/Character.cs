using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Events.Character {
	class HealthChanged {
		public int health;

		public HealthChanged(int health) {
			this.health = health;
		}
	}

	class ArmorChanged {
		public int armor;

		public ArmorChanged(int armor) {
			this.armor = armor;
		}
	}
}

public class Character : MonoBehaviour {

	public EventManager.LocalEvent healthChanged = new EventManager.LocalEvent();
	public EventManager.LocalEvent armorChanged = new EventManager.LocalEvent();

	private int _health = 100;
	public int health {
		set {
			_health = value;
			if (value > maxHealth) {
				maxHealth = value;
			}
			healthChanged.Invoke (new Events.Character.HealthChanged (_health));
		}

		get {
			return _health;
		}
	}

	private int _maxHealth = 100;
	public int maxHealth {
		set {
			_maxHealth = value;
		}

		get {
			return _maxHealth;
		}
	}

	private int _armor = 0;
	public int armor {
		set {
			_armor = value;
			if (value > maxArmor) {
				maxArmor = value;
			}
			armorChanged.Invoke (new Events.Character.ArmorChanged (_armor));
		}

		get {
			return _armor;
		}
	}

	private int _maxArmor = 100;
	public int maxArmor {
		set {
			_maxArmor = value;
		}

		get {
			return _maxArmor;
		}
	}

	private string _faction = "";
	public string faction {
		set {
			_faction = value;
		}

		get {
			return _faction;
		}
	}

	private Transform _head;
	public Transform head {
		set {
			_head = value;
		}

		get {
			return _head;
		}
	}

	void Start() {
		CharacterManager.characters.Add (this);
	}

	public void Damage(int damage, bool ignoreArmor = true) { 
		if (armor > 0 && !ignoreArmor) {
			armor -= damage / 3;
			if (armor < 0) {
				armor = 0;
			}
			damage = Mathf.RoundToInt(((float)damage) * 0.1f);
		}
		health -= damage;
		if (health < 0) {
			health = 0;
		}
	}

}
