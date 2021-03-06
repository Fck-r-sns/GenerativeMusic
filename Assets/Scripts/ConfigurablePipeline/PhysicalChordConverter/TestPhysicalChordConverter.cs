using System.Collections.Generic;
using System.Linq;
using MusicAlgebra;
using UnityEngine;

namespace ConfigurablePipeline
{
    public class TestPhysicalChordConverter : AbstractPhysicalChordConverter
    {
        [SerializeField]
        private bool _useCounterMelody = true;

        [SerializeField]
        private bool _useMelody = true;

        [SerializeField]
        private bool _useBass = true;

        private void Start()
        {
            context.beatManager.AddBeatEventListener(OnBeatEvent, transform.GetSiblingIndex());
        }

        private void OnBeatEvent(BeatEvent beatEvent)
        {
            Queue<AcademicChord> academicChordsQueue = context.academicChordsQueue;
            PlayableSoundQueue queue = context.playableSoundQueue;
            while (queue.count < MAX_QUEUE_SIZE && academicChordsQueue.Count > 0)
            {
                AcademicChord academicChord = academicChordsQueue.Dequeue();
                PlayableSound lastSound = queue.GetLastSound();
                int timeQuantumNumber = beatEvent.timeQuantumNumber;
                if (lastSound != null)
                {
                    timeQuantumNumber = lastSound.startTimeQuantumNumber + lastSound.durationTimeQuanta;
                }

                int beatCounter = timeQuantumNumber / context.beatManager.timeQuantaPerBeat;
                bool isStrong = beatCounter % context.beatManager.measure == 0;
                float volume = isStrong ? 1f : 0.75f;

                if (_useMelody)
                {
                    for (int i = 0; i < academicChord.notes.Length; ++i)
                    {
                        Note chordNote = academicChord.notes[i];
                        Pitch pitch = new Pitch(chordNote, (i == 0 || chordNote > academicChord.notes[0]) ? 4 : 5);
                        queue.AddSound(new PlayableSound(pitch, volume, timeQuantumNumber, context.beatManager.timeQuantaPerBeat));
                    }
                }

                if (_useBass)
                {
                    Pitch bass = new Pitch(academicChord.notes[0], 2);
                    queue.AddSound(new PlayableSound(bass, volume, timeQuantumNumber, context.beatManager.timeQuantaPerBeat));
                }

                if (_useCounterMelody)
                {
                    List<Pitch> arpeggioPitches = new List<Pitch>();
                    for (int i = 0; i < academicChord.notes.Length; ++i)
                    {
                        Note chordNote = academicChord.notes[i];
                        arpeggioPitches.Add(new Pitch(chordNote, (i == 0 || chordNote > academicChord.notes[0]) ? 6 : 7));
                    };

                    const int arpeggioNoteDurationTimeQuanta = 8;
                    List<int> indices = GetIndices(arpeggioPitches.Count, context.beatManager.timeQuantaPerBeat / arpeggioNoteDurationTimeQuanta);
                    int arpeggioTimeQuantumNumber = timeQuantumNumber;
                    foreach (int index in indices)
                    {
                        queue.AddSound(new PlayableSound(arpeggioPitches[index], volume, arpeggioTimeQuantumNumber, arpeggioNoteDurationTimeQuanta));
                        arpeggioTimeQuantumNumber += arpeggioNoteDurationTimeQuanta;
                    }
                }
            }

            if (queue.count < MAX_QUEUE_SIZE && academicChordsQueue.Count == 0)
            {
                Debug.LogWarning("Playable sounds queue is not full, but academic chords queue is empty");
            }
        }

        private List<int> GetIndices(int pitchesCount, int targetNotesCount)
        {
            List<int> indices = Enumerable.Range(1, pitchesCount)
                .Select(x => x - 1)
                .ToList();

            indices.Shuffle();

            int repeatCount = Mathf.CeilToInt(targetNotesCount / (float)pitchesCount);
            return Enumerable.Repeat(indices, repeatCount)
                .SelectMany(x => x)
                .Take(targetNotesCount)
                .ToList();
        }
    }
}