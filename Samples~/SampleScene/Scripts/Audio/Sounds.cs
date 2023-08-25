using Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Audio
{

	public class Sounds : MonoBehaviour
	{
		[SerializeField] private List<Sound> sounds;

		private AudioService audioService;

		// Cache
		private readonly Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
		private Dictionary<string, float> soundVolumes = new Dictionary<string, float>();

		private void Awake()
		{
			foreach (var sound in sounds)
			{
				var audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.clip = sound.AudioClip;
				audioSource.playOnAwake = false;

				audioSources.Add(sound.Name, audioSource);
				soundVolumes.Add(sound.Name, sound.Volume);
			}
		}

		private void Start()
		{
			foreach (var sound in audioSources)
			{
				sound.Value.volume = audioService.SoundVolume * soundVolumes[sound.Key];
			}
		}

		private void OnDisable()
		{
			audioService.OnSoundChanged -= AudioService_OnSoundChanged;
			audioService.OnSoundVolumeChanged -= AudioService_OnSoundVolumeChanged;
		}

		private void AudioService_OnSoundChanged(bool isOn)
		{
			if(isOn == false)
			{
				foreach(var sound in audioSources)
				{
					sound.Value.Stop();
				}
			}
		}

		private void AudioService_OnSoundVolumeChanged(float volume)
		{
			foreach (var sound in audioSources)
			{
				sound.Value.volume = volume * soundVolumes[sound.Key]; ;
			}
		}

		public void PlaySound(string name)
		{
			if (audioSources.ContainsKey(name))
			{
				audioSources[name].Play();
			}
			else
			{
				Logger.Warning($"Sound: {name} not found!");
			}
		}
		public void FadeSound(string name, float time)
		{
			if (audioSources.ContainsKey(name))
			{
				AudioSource audioSource = audioSources[name];
				if(audioSource.isPlaying == false)
				{
					audioSource.Play();
				}
				StartCoroutine(FadeSoundCoroutine(audioSource, time));
			}
			else
			{
				Logger.Warning($"Sound: {name} not found!");
			}
		}
		private IEnumerator FadeSoundCoroutine(AudioSource audioSource, float time)
		{
			float volumeTemp = audioSource.volume;
			float step = time / Time.fixedDeltaTime;
			float stepVolume = volumeTemp / step;
			for (float i = 0.0f; i < time; i += Time.fixedDeltaTime)
			{
				yield return new WaitForSeconds(Time.fixedDeltaTime);
				audioSource.volume -= stepVolume;
			}
			audioSource.Stop();
			audioSource.volume = volumeTemp;
		}
		public void StopSound(string name)
		{
			if (audioSources.ContainsKey(name))
			{
				audioSources[name].Stop();
			}
			else
			{
				Logger.Warning($"Sound: {name} not found!");
			}
		}

		public bool IsSoundPlaying(string name)
		{
			if (audioSources.ContainsKey(name))
			{
				if (audioSources[name].isPlaying == true)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			Logger.Warning($"Sound: {name} not found!");
			return false;
		}


		public void Initialized(AudioService audioService)
		{
			this.audioService = audioService;

			audioService.OnSoundChanged += AudioService_OnSoundChanged;
			audioService.OnSoundVolumeChanged += AudioService_OnSoundVolumeChanged;
		}
	}

}
