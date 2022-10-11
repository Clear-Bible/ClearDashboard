using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models
{
    public class ParallelProject
    {
        public Guid ParallelCorpusId { get; set; }
        public ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus parallelTextRows { get; set; }
    }
}
