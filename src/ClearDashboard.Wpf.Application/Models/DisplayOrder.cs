using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models
{
    public class DisplayOrder
    {
        public enum MessageType
        {
            ShowTokenizationWindowMessage,
            ShowParallelTranslationWindowMessage
        }

        public MessageType MsgType { get; set; }
        public object Data { get; set; }
    }
}
