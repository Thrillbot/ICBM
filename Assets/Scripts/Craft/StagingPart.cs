using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StagingPart : CraftPart
{
	public TMP_Text stageText;
	public Button inc;
	public Button dec;

	public int stageNum = 1;

	public void OnLaunch()
	{
		Destroy(inc);
		Destroy(dec);
	}

	public void OnStage(int currentStage)
	{
		if (currentStage == stageNum)
		{
			DetachChildren();
			////////play stage bolts blow particle effect//////////////////
			Destroy(gameObject);
		}
	}

	public void Inc()
	{
		int num = Convert.ToInt32(stageText.text);
		if (num < 10)
		{
			num++;
			stageNum = num;
			stageText.text = num.ToString();
		}
	}	
	public void Dec()
	{
		int num = Convert.ToInt32(stageText.text);
		if (num > 1)
		{
			num--;
			stageNum = num;
			stageText.text = num.ToString();
		}
	}
}
