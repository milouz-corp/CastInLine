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
        static ReceiverStatus recStatus;
        static IMediaChannel mediaChannel;
        static MediaStatus mediastatus;
        static IReceiverChannel recChanel;

        static void Main(string[] args)
        {
            try
            {
                if (args.Count() == 0 || args.Contains("help"))
                {
                    Console.WriteLine("Description:");
                    Console.WriteLine("\tYou can remotly controle a Google Cast compatible device with the following commands");
                    Console.WriteLine("");
                    Console.WriteLine("Usage:");
                    Console.WriteLine("\tdiscover\t\tlists the devices on the network");
                    Console.WriteLine("\ttarget IP[:Port]\tspecify the target device of the command");
                    Console.WriteLine("\tstatus\t\tget the status of the device");
                    Console.WriteLine("\tplay url [title --] [subtitle --] [poster --]\tplay a media");
                }

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
                    if (!args.Contains("play"))
                    {
                        Task.Run(connectToChannel).Wait();
                    }
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
                    Task.Run(pause).Wait();
                }

                if (args.Contains("resume"))
                {
                    Task.Run(resume).Wait();
                }

                if (args.Contains("seek"))
                {
                    Task.Run(() => seek(double.Parse(args[Array.IndexOf(args, "seek") + 1]))).Wait();
                }

                if (args.Contains("stop"))
                {
                    Task.Run(stop).Wait();
                }

                if (args.Contains("quit"))
                {
                    Task.Run(quit).Wait();
                }

                if (args.Contains("volume"))
                {
                    Task.Run(() => volume(float.Parse(args[Array.IndexOf(args, "volume") + 1]))).Wait();
                }

                if (args.Contains("status"))
                {
                    if (recStatus == null)
                    {
                        Console.WriteLine("Receiver Not Found");
                    }
                    else
                    {
                        Console.WriteLine("Volume\tIsIDLE\tDisplayName\tStatusText");
                        foreach (var app in recStatus.Applications)
                        {
                            Console.WriteLine(recStatus.Volume.Level.ToString() + '\t' + app.IsIdleScreen.ToString() + '\t' + app.DisplayName + '\t' + app.StatusText);
                        }
                    }


                    if (mediastatus == null)
                    {
                        Console.WriteLine("MediaChannel Not Found");
                    }
                    else
                    {
                        Console.WriteLine("CurrentTime\tPlayerState\tMediaUrl");
                        Console.WriteLine(mediastatus.CurrentTime.ToString() + '\t' + mediastatus.PlayerState + '\t' + mediastatus.Media.ContentId);
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
            sender.GetChannel<IReceiverChannel>().StatusChanged += ReceiverChannelStatusChanged;
            await sender.ConnectAsync(rec);
        }

        private static void ReceiverChannelStatusChanged(object sender, EventArgs e)
        {
            recChanel = (IReceiverChannel)sender;
            Task.Run(async () =>
            {
                recStatus = await recChanel.GetStatusAsync();
            });
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

            mediastatus = await mediaChannel.LoadAsync(mediaInfo);

        }

        public static async Task openURL(string url)
        {
            await sender.LaunchAsync(mediaChannel);
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

        public static async Task volume(float volume)
        {
            await recChanel.SetVolumeAsync(volume);
        }

        public static async Task stop()
        {
            await mediaChannel.StopAsync();
        }

        public static async Task quit()
        {

        }

    }
}
