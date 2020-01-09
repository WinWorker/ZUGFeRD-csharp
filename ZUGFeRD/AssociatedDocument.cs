﻿using System.Collections.Generic;

namespace s2industries.ZUGFeRD
{
    public class AssociatedDocument
    {
        public List<Note> Notes { get; set; } = new List<Note>();
        // ReSharper disable once InconsistentNaming
        public int? LineID { get; set; }

        public AssociatedDocument()
        {

        }

        public AssociatedDocument(int? lineId)
        {
            this.LineID = lineId;
        }
    }
}
