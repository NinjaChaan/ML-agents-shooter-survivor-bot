using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
	public MeshRenderer meshRenderer;

	public Material originalMaterial;
	public Material hitMaterial;

    public void Flash() {
		meshRenderer.material = hitMaterial;
		Invoke("UnFlash", 0.3f);
	}

	public void UnFlash() {
		meshRenderer.material = originalMaterial;
	}
}
