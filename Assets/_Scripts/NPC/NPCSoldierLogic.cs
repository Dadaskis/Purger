using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCSoldierLogicState {
	IDLE,
	ENEMY_DETECTED
}

public class NPCSoldierLogic {

	[HideInInspector] public NPCSoldier npc;

	private NPCSoldierLogicState state = NPCSoldierLogicState.IDLE;

	[HideInInspector] public Character enemy;
	private float enemyMinDistance = 1.0f;
	private float enemyMaxDistance = 10.0f;	

	public NPCSoldierLogic(NPCSoldier npc) {
		this.npc = npc;
	}

	private void CheckState() {
		if (state != NPCSoldierLogicState.ENEMY_DETECTED) {
			List<Character> characters = npc.GetVisibleCharacters ();
			foreach (Character character in characters) {
				if (character.faction != npc.faction) {
					state = NPCSoldierLogicState.ENEMY_DETECTED;
					enemy = character;
					Debug.Log (enemy);
					break;
				}
			}
		}
	}

	private void StateIdle() {
		
	}

	private void StateEnemyDetected() {
		//npc.RotateLook (enemy.head.position);
		npc.LookAt(enemy.head.position);
		Vector3 direction = (enemy.transform.position - npc.transform.position).normalized; 
		if (Vector3.Angle (direction, npc.transform.forward) > 60.0f) {
			npc.MoveBack (-direction, 0.5f);
			npc.RotateLook (enemy.head.position, 1.0f);
		}

		float enemyDistance = Vector3.Distance (npc.transform.position, enemy.transform.position);
		if (enemyDistance < enemyMinDistance) {
			npc.MoveBack (-direction, 2.0f);
		} else if (enemyDistance > enemyMaxDistance) {
			npc.MoveFront (direction, 2.0f);
		}
	}

	private void ProcessState() {
		switch (state) {
		case NPCSoldierLogicState.IDLE:
			StateIdle ();
			break;
		case NPCSoldierLogicState.ENEMY_DETECTED:
			StateEnemyDetected ();
			break;
		}
	}

	public void Update() {
		CheckState ();
		ProcessState ();
	}
}
