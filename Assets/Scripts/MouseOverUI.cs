using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseOverUI : MonoBehaviour
{
	public List<RectTransform> _uiElements;

	public bool mouseOverUI;

	void Update()
    {
		mouseOverUI = (_uiElements?.Any(x => x.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(x, Input.mousePosition, null))).GetValueOrDefault(false);
	}

	public bool MousedOver
	{
		get { return mouseOverUI; }
	}
}
