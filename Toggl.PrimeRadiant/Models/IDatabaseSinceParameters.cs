﻿using System;
 using Toggl.Multivac.Models;

namespace Toggl.PrimeRadiant.Models
{
    public interface IDatabaseSinceParameter : IIdentifiable
    {
        DateTimeOffset? Since { get; }
    }
}
