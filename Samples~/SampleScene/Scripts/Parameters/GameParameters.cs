using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Parameters
{
	public enum GameMode
	{
		Pvp,
		BotEasy,
		BotNormal,
		BotHard
	}

	public class GameParameters : MonoBehaviour
	{
		public GameMode Mode { get; set; }

		private void Start()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}
