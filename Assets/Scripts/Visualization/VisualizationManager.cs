﻿using System;
using System.Collections.Generic;
using MusicAlgebra;
using UnityEngine;

namespace Visualization
{
    public class VisualizationManager : MonoBehaviour
    {
        private const float Z_DEPTH = 5;
        private const int PLAYED_SOUNDS_TIME_TO_LIVE = 24; // in quarter beats
        private const int NOTE_GRID_OFFSET = -4;
        private const int OCTAVE_GRID_OFFSET = -4;

        public Action<Sound> onSoundCreated;
        public Action<Sound> onSoundUpdated;
        public Action<int> onSoundRemoved;

        [SerializeField]
        private BeatManager _beatManager;

        [SerializeField]
        private PlayableSoundQueue _queue;

        [SerializeField]
        private Sound _soundPrefab;

        [SerializeField]
        private float _quarterBeatStep;

        [SerializeField]
        private float _pitchStep;

        [SerializeField]
        private Color _defaultSoundColor = Color.red;

        [SerializeField]
        private Color _playingSoundColor = Color.blue;

        private Dictionary<int, Sound> _sounds;
        private HashSet<int> _soundsToRemove;
        private Transform _soundsParent;

        private void Awake()
        {
            _sounds = new Dictionary<int, Sound>();
            _soundsToRemove = new HashSet<int>();

            _beatManager.onQuarterBeatEvent += OnQuarterBeatEvent;
            _queue.onSoundAdded += OnSoundAdded;

            _soundsParent = new GameObject("Sounds").transform;
            _soundsParent.parent = this.transform;
        }

        void Update()
        {
            int stepsCount = 48;
            float width = 20f;
            float height = 20f;

            for (int i = -stepsCount / 2; i <= stepsCount / 2; ++i)
            {
                float x = i * _quarterBeatStep;
                Color color;
                if (i == 0)
                {
                    color = Color.blue;
                }
                else
                {
                    color = i % 4 == 0 ? Color.red : Color.white;
                }
                Debug.DrawLine(new Vector3(x, -height / 2f, Z_DEPTH), new Vector3(x, height / 2f, Z_DEPTH), color);
            }

            for (int i = -stepsCount / 2; i <= stepsCount / 2; ++i)
            {
                float y = i * _pitchStep;
                Debug.DrawLine(new Vector3(-width / 2f, y, Z_DEPTH), new Vector3(width / 2f, y, Z_DEPTH));
            }
        }

        private void OnQuarterBeatEvent(QuarterBeatEvent quarterBeatEvent)
        {
            foreach (var kv in _sounds)
            {
                Sound sound = kv.Value;
                if (!sound.gameObject.activeSelf)
                {
                    sound.gameObject.SetActive(true);
                }

                int beatGridIndex = sound.playableSound.startQuarterBeatNumber - quarterBeatEvent.quarterBeatNumber;
                float x = beatGridIndex * _quarterBeatStep;

                Pitch pitch = sound.playableSound.pitch;
                int noteGridIndex = (int)pitch.note + NOTE_GRID_OFFSET + (pitch.octave + OCTAVE_GRID_OFFSET) * Defines.SEMITONES_COUNT;
                float y = noteGridIndex * _pitchStep;

                sound.transform.localPosition = new Vector3(x, y, Z_DEPTH);

                if (beatGridIndex + sound.playableSound.durationQuarterBeats < -PLAYED_SOUNDS_TIME_TO_LIVE)
                {
                    _soundsToRemove.Add(sound.playableSound.id);
                }

                if (beatGridIndex <= 0 && Mathf.Abs(beatGridIndex) < sound.playableSound.durationQuarterBeats)
                {
                    sound.SetColor(_playingSoundColor);
                }
                else
                {
                    sound.SetColor(_defaultSoundColor);
                }
                
                onSoundUpdated?.Invoke(sound);
            }

            foreach (int id in _soundsToRemove)
            {
                Destroy(_sounds[id].gameObject);
                _sounds.Remove(id);
                onSoundRemoved?.Invoke(id);
            }
            _soundsToRemove.Clear();
        }

        private void OnSoundAdded(PlayableSound playableSound)
        {
            Sound sound = Instantiate(_soundPrefab, _soundsParent);
            sound.gameObject.SetActive(false);
            sound.playableSound = playableSound;
            sound.SetSize(_quarterBeatStep * playableSound.durationQuarterBeats, _pitchStep);
            _sounds[playableSound.id] = sound;

            onSoundCreated?.Invoke(sound);
        }
    }
}