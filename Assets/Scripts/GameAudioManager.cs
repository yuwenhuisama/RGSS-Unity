using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using NAudio;

namespace RGSSUnity
{
    using System.Collections.Generic;
    using NAudio.Wave;

    public class GameAudioManager
    {
        internal enum PlayType
        {
            Bgm,
            Bgs,
            Me,
            Se
        }

        public static readonly GameAudioManager Instance = new GameAudioManager();

        private AudioSource bgmSource;
        private AudioSource bgsSource;
        private AudioSource meSource;
        private AudioSource seSource;
        private Action<IEnumerator> startCoroutineFn;
        private Dictionary<string, AudioClip> Cache_ = new();

        public void Init(
            AudioSource bgmSource,
            AudioSource bgsSource,
            AudioSource meSource,
            AudioSource seSource,
            Action<IEnumerator> startCoroutineFn)
        {
            this.bgmSource = bgmSource;
            this.bgmSource.loop = true;

            this.bgsSource = bgsSource;
            this.bgsSource.loop = true;

            this.meSource = meSource;
            this.meSource.loop = false;

            this.seSource = seSource;
            this.meSource.loop = false;

            this.startCoroutineFn = startCoroutineFn;
        }

        internal void Play(PlayType type, string filename, float volume, float pitch, float pos)
        {
            var source = this.GetSource(type);

            if (this.Cache_.TryGetValue(filename, out var value))
            {
                source.clip = value;
                source.volume = volume;
                source.pitch = pitch;
                source.time = pos;
                source.Play();
                return;
            }

            var extension = System.IO.Path.GetExtension(filename);

            if (extension == ".wma")
            {
                this.startCoroutineFn(LoadWmaFileAndConvertToClipWithWebRequest(filename, PlayClip));
            }
            else if (extension == ".ogg" || extension == ".wav" || extension == ".mp3")
            {
                var audioType = extension switch
                {
                    ".ogg" => AudioType.OGGVORBIS,
                    ".wav" => AudioType.WAV,
                    ".mp3" => AudioType.MPEG,
                    _ => throw new ArgumentOutOfRangeException(nameof(extension), extension, null),
                };

                this.startCoroutineFn(LoadAudioClipWithWebRequest(filename, audioType, PlayClip));
            }
            else
            {
                throw new NotSupportedException($"Unsupported audio file {filename} with audio type {extension}");
            }

            void PlayClip(AudioClip clip)
            {
                this.Cache_.Add(filename, clip);
                source.clip = clip;
                source.volume = volume;
                source.pitch = pitch;
                source.time = pos;
                source.Play();
            }
        }

        internal void Stop(PlayType type) => this.GetSource(type).Stop();

        internal void Fade(PlayType type, float time)
        {
            var source = this.GetSource(type);
            this.startCoroutineFn(FadeOutCoroutine(source, time));
        }

        internal float Pos(PlayType type) => this.GetSource(type).time;

        private static IEnumerator FadeOutCoroutine(AudioSource audioSource, float duration)
        {
            float startVolume = audioSource.volume;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime * 1000;
                audioSource.volume = Mathf.Lerp(startVolume, 0, timeElapsed / duration);
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume; // Reset volume to initial value
        }

        private static IEnumerator LoadAudioClipWithWebRequest(string audioFileName, AudioType audioType, Action<AudioClip> callbackFn)
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, audioFileName);
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(www);
                callbackFn(clip);
            }
        }

        private static IEnumerator LoadWmaFileAndConvertToClipWithWebRequest(string audioFileName, Action<AudioClip> callbackFn)
        {
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, audioFileName);
            using UnityWebRequest www = UnityWebRequest.Get(path);
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                byte[] rawData = www.downloadHandler.data;
                using var memoryStream = new System.IO.MemoryStream(rawData);
                using var reader = new StreamMediaFoundationReader(memoryStream);
                var sampleProvider = reader.ToSampleProvider();

                var audioBuffer = reader.WaveFormat.BitsPerSample switch
                {
                    16 => new float[reader.Length / 2],
                    32 => new float[reader.Length / 4],
                    24 => new float[reader.Length / 3],
                    8 => new float[reader.Length],
                    _ => throw new NotSupportedException($"Unsupported bit depth {reader.WaveFormat.BitsPerSample}")
                };

                int samplesRead = sampleProvider.Read(audioBuffer, 0, audioBuffer.Length);

                AudioClip audioClip = AudioClip.Create(
                    "ConvertedClip",
                    samplesRead,
                    reader.WaveFormat.Channels,
                    reader.WaveFormat.SampleRate,
                    false);
                audioClip.SetData(audioBuffer, 0);

                callbackFn(audioClip);
            }
        }

        private AudioSource GetSource(PlayType type) => type switch
        {
            PlayType.Bgm => this.bgmSource,
            PlayType.Bgs => this.bgsSource,
            PlayType.Me => this.meSource,
            PlayType.Se => this.seSource,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}