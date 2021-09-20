using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {

	public static CharacterManager instance;

	public List<Character> _characters;
	public static List<Character> characters {
		get {
			return instance._characters;
		}
	}

	void Awake () {
		instance = this;
	}
}
