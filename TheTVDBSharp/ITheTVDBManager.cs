﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TheTVDBSharp.Models;

namespace TheTVDBSharp
{
    public interface ITheTvdbManager
    {
        Task<IReadOnlyCollection<Series>> SearchSeries(string query, Language language);

        Task<Series> GetSeries(uint showId, Language language, bool compression = true);

        Task<Episode> GetEpisode(uint episodeId, Language language);

        Task<UpdateContainer> GetUpdates(Interval interval, bool compression = true);

        Task<byte[]> GetBanner(string remotePath);
    }
}
