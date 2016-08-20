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

                    // convert "back" to WAV
                    // create media foundation reader to read the AAC encoded file
                    using (MediaFoundationReader reader = new MediaFoundationReader(Path.Combine(destinationURL, video.Title + audioHelperExtension)))
                    {
                        // resample the file to PCM with same sample rate, channels and bits per sample
                        using (ResamplerDmoStream resampledReader = new ResamplerDmoStream(reader,
                            new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.BitsPerSample, reader.WaveFormat.Channels)))
                        {
                            // create WAVe file
                            using (WaveFileWriter waveWriter = new WaveFileWriter(Path.Combine(destinationURL, video.Title + audioFinalExtension), resampledReader.WaveFormat))
                            {
                                // copy samples
                                resampledReader.CopyTo(waveWriter);
                            }
                        }
                    }

                    // delete aac file from directory
                    File.Delete(Path.Combine(destinationURL, video.Title + audioHelperExtension));

                    return $"{video.Title} is saved succesfully at the following location: {Path.Combine(destinationURL, video.Title + audioFinalExtension)}";
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

            var videoDownloader = new VideoDownloader(video, Path.Combine(destinationURL, video.Title + video.AudioExtension));

            videoDownloader.Execute();
            return video;
        }
        #endregion
    }
}
