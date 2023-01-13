﻿using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Notes;
using System;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Application.Collections
{
    public class LabelCollection : BindableCollection<Label>
    {
        public LabelCollection()
        {
        }

        public LabelCollection(IEnumerable<Label> labels) : base(labels)
        {
        }

        public void Insert(Label label)
        {
            throw new NotImplementedException();
        }
    }
}
