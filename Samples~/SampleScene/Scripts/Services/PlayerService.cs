using System;
using System.Collections.Generic;
using UnityEngine;

namespace Services
{
	public class PlayerService
	{
		private const string Break = "~";
		// Key Audio
		private const string MusicVolumeKey = "mvl";
		private const string SoundVolumeKey = "svl";
		private const string VibrateKey = "vbr";

		// Key Save Data
		private const string levelKey = "lvl";
		private const string scoreKey = "scr";

		#region Audio
		public Action<float> OnMusicVolumeChange;
		public Action<float> OnSoundVolumeChange;

		public Action<bool> OnVibrateChange;
		public float GetMusicVolume()
		{
			return PlayerPrefs.GetFloat(MusicVolumeKey, 1.0f);
		}
		public void SetMusicVolume(float volume)
		{
			PlayerPrefs.SetFloat(MusicVolumeKey, volume);
			OnMusicVolumeChange?.Invoke(volume);
		}
		public float GetSoundVolume()
		{
			return PlayerPrefs.GetFloat(SoundVolumeKey, 1.0f);
		}
		public void SetSoundVolume(float volume)
		{
			PlayerPrefs.SetFloat(SoundVolumeKey, volume);
			OnSoundVolumeChange?.Invoke(volume);
		}
		public bool GetVibrate()
		{
			return PlayerPrefs.GetInt(VibrateKey, 1) != 0;
		}
		public void SetVibrate(bool isVibrate)
		{
			OnVibrateChange?.Invoke(isVibrate);
			if (isVibrate == true)
			{
				PlayerPrefs.SetInt(VibrateKey, 1);
			}
			else
			{
				PlayerPrefs.SetInt(VibrateKey, 0);
			}
		}
		#endregion
		#region Get - Set List
		private void SaveList<T>(string key, List<T> value)
		{
			if (value == null)
			{
				Logger.Warning("Input list null");
				value = new List<T>();
			}
			if (value.Count == 0)
			{
				PlayerPrefs.SetString(key, string.Empty);
				return;
			}
			if (typeof(T) == typeof(string))
			{
				foreach (var item in value)
				{
					string tempCompare = item.ToString();
					if (tempCompare.Contains(Break))
					{
						throw new Exception("Invalid input. Input contain '~'.");
					}
				}
			}
			PlayerPrefs.SetString(key, string.Join(Break, value));
		}
		private List<T> GetList<T>(string key, List<T> defaultValue)
		{
			if (PlayerPrefs.HasKey(key) == false)
			{
				return defaultValue;
			}
			if (PlayerPrefs.GetString(key) == string.Empty)
			{
				return new List<T>();
			}
			string temp = PlayerPrefs.GetString(key);
			string[] listTemp = temp.Split(Break);
			List<T> list = new List<T>();

			foreach (string s in listTemp)
			{
				list.Add((T)Convert.ChangeType(s, typeof(T)));
			}
			return list;
		}
		#endregion
		// Get - Set Score
		public void SetScore(int score)
		{
			PlayerPrefs.SetInt(scoreKey, score);
		}
		public int GetScore()
		{
			return PlayerPrefs.GetInt(scoreKey, 0);
		}
		// Get - Set Level
		public void SetLevel(int level)
		{
			PlayerPrefs.SetInt(levelKey, level);
		}
		public int GetLevel()
		{
			return PlayerPrefs.GetInt(levelKey, 0);
		}
		public void Save()
		{
			PlayerPrefs.Save();
		}
	}
}
