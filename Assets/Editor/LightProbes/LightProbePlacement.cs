using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class LightProbePlacement : EditorWindow {

	static float progress = 0.0f;
	static string current = "Hello";
	static bool working = false;

	float mergeDistance = 1;
	GameObject probeObject;

	[MenuItem ("Window/Generate Light Probes")]
	static void Init() {
		EditorWindow window = GetWindow (typeof(LightProbePlacement));
		window.Show ();
	}

	void PlaceProbes () {
		GameObject probe = probeObject;
		if(probe != null) {
			LightProbeGroup p = probe.GetComponent<LightProbeGroup>();//FindObjectOfType<LightProbeGroup>();//

			if(p != null) {

				//p.gameObject.transform.position = Vector3.zero;
				probe.transform.position = Vector3.zero;
					
				working = true;

				progress = 0.0f;
				current = "Triangulating navmesh...";
				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);

				UnityEngine.AI.NavMeshTriangulation navMesh = UnityEngine.AI.NavMesh.CalculateTriangulation ();


				current = "Generating necessary lists...";
				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);

				Vector3[] newProbes = navMesh.vertices;
				List<Vector3> probeList = new List<Vector3>(newProbes);
				List<ProbeGenPoint> probeGen = new List<ProbeGenPoint>();

				foreach(Vector3 pg in probeList) {
					probeGen.Add (new ProbeGenPoint(pg, false));
				}

				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);

				List<Vector3> mergedProbes = new List<Vector3>();

				int probeListLength = newProbes.Length;

				int done = 0;
				foreach(ProbeGenPoint pro in probeGen) {
					if(pro.used == false) {
						current = "Checking point at " + pro.point.ToString ();
						progress = (float)done / (float)probeListLength;
						EditorUtility.DisplayProgressBar ("Generating probes", current, progress);
						List<Vector3> nearbyProbes = new List<Vector3>();
						nearbyProbes.Add (pro.point);
						pro.used = true;
						foreach(ProbeGenPoint pp in probeGen) {
							if(pp.used == false) {
								current = "Checking point at " + pro.point.ToString ();
								//EditorUtility.DisplayProgressBar ("Generating probes", current, progress);
								if(Vector3.Distance (pp.point, pro.point) <= mergeDistance) {
									pp.used = true;
									nearbyProbes.Add (pp.point);
								}
							}
						}

						Vector3 newProbe = new Vector3();
						foreach(Vector3 prooo in nearbyProbes) {
							newProbe += prooo;
						}
						newProbe /= nearbyProbes.ToArray ().Length;
						newProbe += Vector3.up;

						mergedProbes.Add (newProbe);
						done += 1;
						//Debug.Log ("Added probe at point " + newProbe.ToString ());
					}
				}

				/*for(int i=0; i<newProbes.Length; i++) {
					newProbes[i] = newProbes[i] + Vector3.up;
				}*/


				current = "Final steps...";
				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);

				//Renderer[] renderers = FindObjectsOfType<Renderer> ();
				//Bounds bounds = renderers [0].bounds;
				//foreach (Renderer renderer in renderers) {
				//	bounds.Encapsulate (renderer.bounds);
				//}

				//float density = 0.5f;
				//Debug.Log (bounds);
				//for (float X = bounds.min.x; X < bounds.max.x; X += density) {
				//	for (float Y = bounds.min.y; Y < bounds.max.y; Y += density) {
				//		for (float Z = bounds.min.z; Z < bounds.max.z; Z += density) {
				//			mergedProbes.Add (new Vector3 (X, Y, Z));
				//		}
				//	}
				//}

				List<Vector3> dirs = new List<Vector3> ();
				dirs.Add (new Vector3 (1.0f, 0.0f, 0.0f));
				dirs.Add (new Vector3 (-1.0f, 0.0f, 0.0f));
				dirs.Add (new Vector3 (0.0f, 1.0f, 0.0f));
				dirs.Add (new Vector3 (0.0f, -1.0f, 0.0f));
				dirs.Add (new Vector3 (0.0f, 0.0f, 1.0f));
				dirs.Add (new Vector3 (0.0f, 0.0f, -1.0f));
				List<Vector3> addProbes = new List<Vector3> ();
				foreach (Vector3 pos in mergedProbes) {
					foreach (Vector3 dir in dirs) {
						RaycastHit hit;
						if (Physics.Raycast (pos, dir, out hit)) {
							Vector3 endPos = hit.point + (hit.normal * 0.5f);
							Vector3 dirStep = endPos - pos; 
							dirStep = dirStep.normalized * 0.95f;
							int limit = Mathf.RoundToInt(Vector3.Distance (pos, endPos));
							Vector3 curPos = pos + dirStep;
							for (int counter = 0; counter < limit; counter++) {
								addProbes.Add (curPos);
								curPos += dirStep;
							}
							addProbes.Add (endPos);
						}
					}
				}
				//Debug.Log (addProbes.Count);
				mergedProbes.AddRange (addProbes);
				
				p.probePositions = mergedProbes.ToArray ();
				EditorUtility.DisplayProgressBar ("Generating probes", current, progress);

				working = false;


			} else {
				EditorUtility.DisplayDialog("Error", "Probe object does not have a Light Probe Group attached to it", "OK");
			}

		} else {
			EditorUtility.DisplayDialog("Error", "Probe object not set", "OK");
		}
	}

	void OnGUI() {

		if(GUILayout.Button("Generate probes")) {
			PlaceProbes ();
		}
		mergeDistance = EditorGUILayout.FloatField ("Vector merge distance",mergeDistance);
		probeObject = (GameObject)EditorGUILayout.ObjectField ("Probe GameObject" , probeObject, typeof(GameObject), true);
		EditorGUILayout.LabelField ("This script will automatically generate light probe positions based on the current navmesh.");
		EditorGUILayout.LabelField ("Please make sure that you have generated a navmesh before using the script.");

		if(working) {
			EditorUtility.DisplayProgressBar ("Generating probes", current, progress);
		} else {
			EditorUtility.ClearProgressBar ();
		}
	}

	void OnInspectorUpdate() {
		Repaint();
	}

}

public class ProbeGenPoint {

	public Vector3 point;
	public bool used = false;

	public ProbeGenPoint(Vector3 p, bool u) {
		point = p;
		used = u;
	}

}
