using ClearDashboard.DAL.Alignment.Corpora;
using System;

namespace ClearDashboard.Wpf.Application.Models
{
    public class ParallelProject
    {
        public Guid ParallelCorpusId { get; set; }
        public ParallelCorpus? ParallelCorpus { get; set; }
    }
}
