﻿/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2023 SonarSource SA
 * mailto: info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SonarScanner.MSBuild.Common;

namespace SonarScanner.MSBuild.PreProcessor
{
    public sealed class WebClientDownloader : IDownloader
    {
        private readonly ILogger logger;
        private readonly HttpClient client;

        public WebClientDownloader(HttpClient client, string baseUri, ILogger logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            Contract.ThrowIfNullOrWhitespace(baseUri, nameof(baseUri));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            client.BaseAddress = WebUtils.CreateUri(baseUri);
        }

        public async Task<HttpResponseMessage> DownloadResource(string url) => await GetAsync(url);

        public async Task<Tuple<bool, string>> TryDownloadIfExists(string url, bool logPermissionDenied = false)
        {
            var response = await GetAsync(url);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new Tuple<bool, string>(false, null);
                case HttpStatusCode.Forbidden:
                    {
                        if (logPermissionDenied)
                        {
                            logger.LogWarning(Resources.MSG_Forbidden_BrowsePermission);
                        }

                        response.EnsureSuccessStatusCode();
                        break;
                    }
                default:
                    response.EnsureSuccessStatusCode();
                    break;
            }

            return new Tuple<bool, string>(true, await response.Content.ReadAsStringAsync());
        }

        public async Task<bool> TryDownloadFileIfExists(string url, string targetFilePath, bool logPermissionDenied = false)
        {
            logger.LogDebug(Resources.MSG_DownloadingFile, targetFilePath);
            var response = await GetAsync(url);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return false;
                case HttpStatusCode.Forbidden:
                    if (logPermissionDenied)
                    {
                        logger.LogWarning(Resources.MSG_Forbidden_BrowsePermission);
                    }
                    response.EnsureSuccessStatusCode();
                    break;
                default:
                    response.EnsureSuccessStatusCode();
                    break;
            }

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);
            await contentStream.CopyToAsync(fileStream);

            return true;
        }

        public async Task<string> Download(string url, bool logPermissionDenied = false)
        {
            var response = await GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                logger.LogInfo(Resources.MSG_DownloadFailed, response.RequestMessage.RequestUri, response.StatusCode);
            }

            if (logPermissionDenied && response.StatusCode == HttpStatusCode.Forbidden)
            {
                logger.LogWarning(Resources.MSG_Forbidden_BrowsePermission);
                response.EnsureSuccessStatusCode();
            }

            return null;
        }

        public async Task<Stream> DownloadStream(string url)
        {
            var response = await GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            else
            {
                logger.LogInfo(Resources.MSG_DownloadFailed, response.RequestMessage.RequestUri, response.StatusCode);
                return null;
            }
        }

        public void Dispose() =>
            client.Dispose();

        private async Task<HttpResponseMessage> GetAsync(string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                logger.LogDebug(Resources.MSG_Downloading, response.RequestMessage.RequestUri);
                return response;
            }
            catch (HttpRequestException e)
            {
                logger.LogError(e.Message);
                throw;
            }
        }
    }
}
