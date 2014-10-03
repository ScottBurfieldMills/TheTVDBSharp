﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using TheTVDBSharp.Models;

namespace TheTVDBSharp.Services
{
    public class UpdateParseService : IUpdateParseService
    {
        public UpdateContainer Parse(Stream updateContainerStream, Interval interval)
        {
            if (updateContainerStream == null) throw new ArgumentNullException("updateContainerStream");

            using (var archive = new ZipArchive(updateContainerStream, ZipArchiveMode.Read))
            {
                var entryName = string.Format("updates_{0}.xml", interval.ToApiString());
                var updateContainerRaw = archive.GetEntry(entryName).ReadToEnd();
                return ParseUncompressed(updateContainerRaw);
            }
        }

        public UpdateContainer ParseUncompressed(string updateContainerRaw)
        {
            if (updateContainerRaw == null) throw new ArgumentNullException("updateContainerRaw");

            // If xml cannot be created return null
            XDocument doc;
            try
            {
                doc = XDocument.Parse(updateContainerRaw);
            }
            catch (XmlException e)
            {
                throw new ParseException("Search series collection string cannot be parsed into a xml document.", e);
            }

            var updateContainerXml = doc.Element("Data");
            if (updateContainerXml == null) throw new ParseException("Error while parsing update xml document. Xml Element 'Data' is missing.");

            var updateContainer = new UpdateContainer();

            uint lastUpdatedEpoch;
            var lastUpdatedRaw = updateContainerXml.Attribute("time").Value;
            if (lastUpdatedRaw != null && uint.TryParse(lastUpdatedRaw, out lastUpdatedEpoch))
            {
                updateContainer.LastUpdated = lastUpdatedEpoch.ToDateTime();
            }

            updateContainer.SeriesCollection = updateContainerXml.Elements("Series")
                .Select(ParseSeriesUpdate)
                .ToList();

            updateContainer.EpisodeCollection = updateContainerXml.Elements("Episode")
                .Select(ParseEpisodeUpdate)
                .ToList();

            updateContainer.BannerCollection = updateContainerXml.Elements("Banner")
                .Select(ParseBannerUpdate)
                .ToList();

            return updateContainer;
        }

        private static SeriesUpdate ParseSeriesUpdate(XElement seriesUpdateXml)
        {
            if (seriesUpdateXml == null) throw new ArgumentNullException("seriesUpdateXml");

            return new SeriesUpdate
            {
                Id = seriesUpdateXml.ElementAsUInt("id").GetValueOrDefault(),
                LastUpdated = seriesUpdateXml.ElementFromEpochToDateTime("time").GetValueOrDefault()
            };
        }

        private static EpisodeUpdate ParseEpisodeUpdate(XElement episodeUpdateXml)
        {
            if (episodeUpdateXml == null) throw new ArgumentNullException("episodeUpdateXml");

            return new EpisodeUpdate
            {
                Id = episodeUpdateXml.ElementAsUInt("id").GetValueOrDefault(),
                SeriesId = episodeUpdateXml.ElementAsUInt("Series").GetValueOrDefault(),
                LastUpdated = episodeUpdateXml.ElementFromEpochToDateTime("time").GetValueOrDefault()
            };
        }

        private static BannerUpdate ParseBannerUpdate(XElement bannerUpdateXml)
        {
            if (bannerUpdateXml == null) throw new ArgumentNullException("bannerUpdateXml");

            return new BannerUpdate
            {
                SeriesId = bannerUpdateXml.ElementAsUInt("Series").GetValueOrDefault(),
                RemotePath = bannerUpdateXml.ElementAsString("path"),
                SeasonNumber = bannerUpdateXml.ElementAsUInt("SeasonNum"),
                Language = bannerUpdateXml.ElementAsString("language").ToLanguage(),
                LastUpdated = bannerUpdateXml.ElementFromEpochToDateTime("time").GetValueOrDefault()
            };
        }
    }
}
