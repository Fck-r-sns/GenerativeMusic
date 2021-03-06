﻿using System.Collections.Generic;
using MusicAlgebra;

namespace ConfigurablePipeline
{
    public class Context
    {
        public BeatManager beatManager { get; set; }
        public Note rootNote { get; set; }
        public ScaleType scaleType { get; set; }
        public AcademicScale academicScale { get; set; }
        public AcademicChord[] academicChords { get; set; }
        public Queue<AcademicChord> academicChordsQueue => _academicChordsQueue;
        public PlayableSoundQueue playableSoundQueue { get; set; }

        private Queue<AcademicChord> _academicChordsQueue = new Queue<AcademicChord>();
    }
}