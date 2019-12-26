using GoogleCast;
using GoogleCast.Channels;
using GoogleCast.Models.Media;
using GoogleCast.Models.Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CastInLine
{
    class Program
    {

        static IPEndPoint endpoint;
        static Sender sender;
        static Receiver rec;
        static ReceiverStatus recstatus;
        static IMediaChannel mediaChannel;
        static MediaStatus mediastatus;
        static IReceiverChannel recChanel;

        static void Main(string[] args)
        {
            try
            {
                if (args.Contains("discover"))
                {
                    Task.Run(detectedCast).Wait();
                }

                if (args.Contains("target"))
                {
                    var ep = args[Array.IndexOf(args, "target") + 1].Split(':');
                    var port = 8009;
                    if (ep.Count() > 1)
                    {
                        port = int.Parse(ep[1]);
                    }
                    endpoint = new IPEndPoint(IPAddress.Parse(ep[0]), port);

                    Task.Run(connect).Wait();
                }

                string title = null;
                if (args.Contains("title"))
                {
                    title = args[Array.IndexOf(args, "title") + 1];
                }

                string subtitle = null;
                if (args.Contains("subtitle"))
                {
                    subtitle = args[Array.IndexOf(args, "subtitle") + 1];
                }

                string poster = null;
                if (args.Contains("poster"))
                {
                    poster = args[Array.IndexOf(args, "poster") + 1];
                }

                if (args.Contains("play"))
                {
                    Task.Run(() => play(args[Array.IndexOf(args, "play") + 1], title, subtitle, poster)).Wait();
                }

                if (args.Contains("pause"))
                {
                    Task.Run(connectToChannel).Wait();
                    Task.Run(pause).Wait();
                }

                if (args.Contains("resume"))
                {
                    Task.Run(connectToChannel).Wait();
                    Task.Run(resume).Wait();
                }

                if (args.Contains("seek"))
                {
                    Task.Run(connectToChannel).Wait();
                    Task.Run(() => seek(double.Parse(args[Array.IndexOf(args, "seek") + 1]))).Wait();
                }

                if (args.Contains("stop"))
                {
                    Task.Run(connectToChannel).Wait();
                    Task.Run(stop).Wait();
                }

                if (args.Contains("volume"))
                {
                    Task.Run(connectToChannel).Wait();
                    Task.Run(() => volume(double.Parse(args[Array.IndexOf(args, "volume") + 1]))).Wait();
                }

                if (args.Contains("status"))
                {
                    var recStatus = receiverStatus();
                    if (recStatus == null)
                    {
                        Console.WriteLine("Receiver Not Found");
                    }
                    else
                    {
                        Console.WriteLine("IsIDLE\tDisplayName\tStatusText");
                        foreach (var app in recStatus.Applications)
                        {
                            Console.WriteLine(app.IsIdleScreen.ToString() + '\t' + app.DisplayName + '\t' + app.StatusText);
                        }
                    }


                    if (mediastatus == null)
                    {
                        Console.WriteLine("MediaChannel Not Found");
                    }
                    else
                    {
                        Console.WriteLine("Volume\tCurrentTime\tPlayerState");
                        Console.WriteLine(mediastatus.Volume.Level.ToString() + '\t' + mediastatus.CurrentTime.ToString() + '\t' + mediastatus.PlayerState);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async Task detectedCast()
        {
            var receivers = (await new DeviceLocator().FindReceiversAsync());
            Console.WriteLine("CastName\tIP:Port\tID");
            foreach (var rec in receivers)
            {
                Console.WriteLine(rec.FriendlyName + "\t" + rec.IPEndPoint.ToString() + "\t" + rec.Id);
            }
        }

        public static async Task connect()
        {
            sender = new Sender();
            rec = new Receiver();
            rec.IPEndPoint = endpoint;
            await sender.ConnectAsync(rec);
        }

        public static async Task connectToChannel()
        {
            mediaChannel = sender.GetChannel<IMediaChannel>();
            mediastatus = await mediaChannel.GetStatusAsync();
        }


        public static async Task play(string url, string title, string subtitle, string poster)
        {
            mediaChannel = sender.GetChannel<IMediaChannel>();
            await sender.LaunchAsync(mediaChannel);
            var mediaInfo = new MediaInformation() { ContentId = url };
            mediaInfo.Metadata = new GenericMediaMetadata();
            if (title != null) mediaInfo.Metadata.Title = title;
            if (subtitle != null) mediaInfo.Metadata.Subtitle = subtitle;
            if (poster != null)
            {
                mediaInfo.Metadata.Images = new GoogleCast.Models.Image[1];
                mediaInfo.Metadata.Images[0] = new GoogleCast.Models.Image() { Url = poster };
            }
            var mediaStatus = await mediaChannel.LoadAsync(mediaInfo);

        }

        public static async Task pause()
        {
            await mediaChannel.PauseAsync();
        }

        public static async Task resume()
        {
            await mediaChannel.PlayAsync();
        }

        public static async Task seek(double time)
        {
            await mediaChannel.SeekAsync(time);
        }

        public static async Task volume(double time)
        {
            // await mediaChannel.(time);
        }

        public static async Task stop()
        {
            await mediaChannel.StopAsync();
        }

        public static MediaStatus mediaStatus()
        {
            return getStatus<MediaStatus>("urn:x-cast:com.google.cast.media");
        }

        public static ReceiverStatus receiverStatus()
        {
            return getStatus<ReceiverStatus>("urn:x-cast:com.google.cast.receiver");
        }

        private static T getStatus<T>(string str)
        {
            var statuses = sender.GetStatuses();
            if (statuses.ContainsKey(str) && statuses[str] != null)
            {
                if (statuses[str] is T)
                {
                    return (T)statuses[str];
                }
                else
                {
                    return ((T[])(statuses[str])).FirstOrDefault();
                }
            }
            return default(T);
        }
    }
}
