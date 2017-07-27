using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoLibrary;
using YoutubeExtractor;

namespace YoutubeDownloader
{
    public static class YoutubeDownloadPerformer
    {
        private static string audioFinalExtension = ".wav";
        private static string audioHelperExtension = ".aac";

        public async static Task<string> DownloadVideo(string sourceURL, string destinationURL)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // destination URL is a folder
                    using (var service = Client.For(YouTube.Default))
                    {
                        var video = service.GetVideo(sourceURL);
                        string path = Path.Combine(destinationURL, video.FullName);
                        File.WriteAllBytes(path, video.GetBytes());
                        return $"{video.FullName} is saved succesfully at the following location: {path}";
                    }
                });
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred. Please try again");
            }
        }

        public async static Task<string> DownloadAudio(string sourceURL, string destinationURL)
        {
            try
            {
                return await Task.Run(() =>
                {
                    VideoInfo video = DownloadAACFile(sourceURL, destinationURL);

                    string videoTitle = RemoveInvalidCharacters(video.Title);

                    // convert "back" to WAV
                    // create media foundation reader to read the AAC encoded file
                    using (MediaFoundationReader reader = new MediaFoundationReader(Path.Combine(destinationURL, videoTitle + audioHelperExtension)))
                    {
                        // resample the file to PCM with same sample rate, channels and bits per sample
                        using (ResamplerDmoStream resampledReader = new ResamplerDmoStream(reader,
                            new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels)))
                        {
                            // create WAVe file
                            using (WaveFileWriter waveWriter = new WaveFileWriter(Path.Combine(destinationURL, videoTitle + audioFinalExtension), resampledReader.WaveFormat))
                            {
                                // copy samples
                                resampledReader.CopyTo(waveWriter);
                            }
                        }
                    }

                    // delete aac file from directory
                    File.Delete(Path.Combine(destinationURL, videoTitle + audioHelperExtension));

                    return $"{videoTitle} is saved succesfully at the following location: {Path.Combine(destinationURL, videoTitle + audioFinalExtension)}";
                });
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred. Please try again");
            }
        }

        #region Audio Helper Methods
        private static VideoInfo DownloadAACFile(string sourceURL, string destinationURL)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(sourceURL);

            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 0);

            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            string videoTitle = RemoveInvalidCharacters(video.Title);

            var videoDownloader = new VideoDownloader(video, Path.Combine(destinationURL, videoTitle + video.AudioExtension));

            videoDownloader.Execute();
            return video;
        }

        private static string RemoveInvalidCharacters(string videoTitle)
        {
            return videoTitle.Contains("|") ? videoTitle.Replace('|', '-') : videoTitle;
        }
        #endregion


    }
}
