﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Services
{
    public interface ILocalizationService
    {
        string Get(string key);
    }
}