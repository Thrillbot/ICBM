using UnityEngine;

public class TestExplode : MonoBehaviour
{
    public float strength = 1;
    public float radius = 10;
    public Generator planet;

	private void Update()
	{
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                planet.Explode(hit.point, strength, radius);
            }
        }
    }
}
