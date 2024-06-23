using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FocalPoint : MonoBehaviour
{
    public Transform cameraTransform;
    public Volume globalVolume;
	public float offset = 1.25f;

    private DepthOfField dof;

	private void Start()
	{
		VolumeProfile profile = globalVolume.sharedProfile;
		profile.TryGet(out dof);
	}

	void LateUpdate()
    {
		if (dof == null)
			return;

        MinFloatParameter newFocalDistance = dof.focusDistance;
		newFocalDistance.value = cameraTransform.position.z + offset;
		dof.focusDistance = newFocalDistance;
	}
}
