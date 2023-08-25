using System.Collections.Generic;
using UnityEngine;
using System;
using Audio;

namespace Services
{
	public class AudioService
	{
		public event Action<bool> OnSoundChanged;
		public event Action<float> OnSoundVolumeChanged;

		public event Action<bool> OnMusicChanged;
		public event Action<float> OnMusicVolumeChanged;

		private bool soundOn;
		private bool musicOn;
		private float soundVolume;
		private float musicVolume;

		private bool vibrateOn;

		private Music music;
		private Dictionary<string, AudioSource> soundAudioSources;

		// Cache
		private Dictionary<string, float> soundVolumes = new();

		public AudioService(Music music, List<Sound> sounds, GameObject soundObject)
		{
			this.music = music;
			this.music.Initialized(this);

			soundAudioSources = new Dictionary<string, AudioSource>();
			foreach (var sound in sounds)
			{
				AudioSource soundSource = soundObject.AddComponent<AudioSource>();
				soundSource.clip = sound.AudioClip;
				soundSource.volume = sound.Volume;
				soundSource.playOnAwake = false;
				soundAudioSources.Add(sound.Name, soundSource);
			}

			foreach (var audioSource in soundAudioSources)
			{
				soundVolumes.Add(audioSource.Key, audioSource.Value.volume);
			}
		}
		// Play Sound
		public void PlayButton1()
		{
			if (soundOn == true && soundVolume > 0.0f)
			{
				var audioSouce = soundAudioSources["button1"];
				audioSouce.volume = soundVolume * soundVolumes["button1"];
				audioSouce.Play();
			}
		}

		public void PlayButton2()
		{
			if (soundOn == true && soundVolume > 0.0f)
			{
				var audioSouce = soundAudioSources["button2"];
				audioSouce.volume = soundVolume * soundVolumes["button2"];
				audioSouce.Play();
			}
		}
		public void PlayWin()
		{
			if (soundOn == true && soundVolume > 0.0f)
			{
				var audioSouce = soundAudioSources["win"];
				audioSouce.volume = soundVolume * soundVolumes["win"];
				audioSouce.Play();
			}
		}
		public void PlayCountDown()
		{
			if (soundOn == true && soundVolume > 0.0f)
			{
				var audioSouce = soundAudioSources["countdown"];
				audioSouce.volume = soundVolume * soundVolumes["countdown"];
				audioSouce.Play();
			}
		}
		// Play Music
		public void PlayMusic()
		{
			if (musicOn == true && musicVolume > 0.0f)
			{
				music.PlayMusic("music");
			}
		}
		// Fade Music
		public void FadeMusic(float time)
		{
			if (musicOn == true && musicVolume > 0.0f)
			{
				music.FadeMusic("music", time);
			}
		}
		// End
		public void StopAllSound()
		{
			foreach (var audioSource in soundAudioSources)
			{
				audioSource.Value.Stop();
			}
		}
		public void StopMusic()
		{
			music.StopMusic("music");
		}
		public bool IsMusicPlaying()
		{
			return music.IsMusicPlaying("music");
		}
		public void SetMusicVolume(float volume)
		{
			MusicVolume = volume;
		}
		public void SetSoundVolume(float volume)
		{
			SoundVolume = volume;
		}
		public void SetVibrate(bool isOn)
		{
			VibrateOn = isOn;
		}
		public void Vibrate()
		{
			if(vibrateOn == true)
			{
				Handheld.Vibrate();
			}
		}

		#region Get - Set
		// GET - SET
		public float SoundVolume
		{
			get { return soundVolume; }
			set
			{
				soundVolume = value;
				foreach (var audioSource in soundAudioSources)
				{
					audioSource.Value.volume = soundVolume * soundVolumes[audioSource.Key];
				}
				OnSoundVolumeChanged?.Invoke(soundVolume);
			}
		}

		public float MusicVolume
		{
			get { return musicVolume; }
			set
			{
				musicVolume = value;
				OnMusicVolumeChanged?.Invoke(musicVolume);
			}
		}
		public bool SoundOn
		{
			get { return soundOn; }
			set
			{
				soundOn = value;
				if (SoundOn == false)
				{
					StopAllSound();
				}
				OnSoundChanged?.Invoke(soundOn);
			}
		}

		public bool MusicOn
		{
			get { return musicOn; }
			set
			{
				musicOn = value;
				if (musicOn == true && musicVolume > 0)
				{
					music.PlayMusic("music");
				}
				else
				{
					music.StopMusic("music");
				}
				OnMusicChanged?.Invoke(musicOn);
			}
		}
		public bool VibrateOn
		{
			get { return vibrateOn; }
			set
			{
				vibrateOn = value;
			}
		}
		#endregion
	}
}
