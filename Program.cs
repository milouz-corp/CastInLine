using GoogleCast;
using GoogleCast.Channels;
using GoogleCast.Models.Media;
using GoogleCast.Models.Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                var result = new Dictionary<string, object>();

                var errors = new List<object>();
                result.Add("errors", errors);

                if (args.Count() == 0 || args.Contains("help"))
                {
                    Console.WriteLine("Description:");
                    Console.WriteLine("\tYou can remotly control a Google Cast compatible device with the following commands and get the result as json format");
                    Console.WriteLine("");
                    Console.WriteLine("Usage:");
                    Console.WriteLine("\tdiscover\t\tlists the devices on the network");
                    Console.WriteLine("\ttarget IP[:Port]\tspecify the target device of the command");
                    Console.WriteLine("\tpauseAtEnd\twait for input key to close (for debug purpose)");
                    Console.WriteLine("\tstatus\t\tget the status of the device");
                    Console.WriteLine("\tplay {url} [title {text}] [subtitle {text} [image {url}]\tplay a media");
                    Console.WriteLine("\tpause\tpause the current media");
                    Console.WriteLine("\tresume\tresume the current media");
                    Console.WriteLine("\tstop\tstop the current media");
                    Console.WriteLine("\tvolume {value} \tset the volume (0->1)");
                    Console.WriteLine("\tseek {value} \tset player position is seconds");
                }

                if (args.Contains("discover"))
                {
                    Task.Run(async () =>
                   {
                       var receivers = await detectedCast();
                       var devices = new List<object>();
                       foreach (var rec in receivers)
                       {
                           var device = new Dictionary<string, object>();
                           device.Add("FriendlyName", rec.FriendlyName);
                           device.Add("IPEndPoint", rec.IPEndPoint.ToString());
                           device.Add("ID", rec.Id);
                           devices.Add(device);
                       }
                       result.Add("devices", devices);
                   }).Wait();
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

                string image = null;
                if (args.Contains("image"))
                {
                    image = args[Array.IndexOf(args, "image") + 1];
                }

                if (args.Contains("play"))
                {
                    var index = Array.IndexOf(args, "play");
                    if (args.Count() <= index + 1)
                    {
                        errors.Add("play parameter missing");
                    }
                    else
                    {
                        Task.Run(() => play(args[index + 1], title, subtitle, image)).Wait();
                    }
                }

                if (args.Contains("pause") || args.Contains("resume") || args.Contains("seek") || args.Contains("stop"))
                {
                    if (mediaChannel.Status == null || mediaChannel.Status.Count() == 0)
                    {
                        errors.Add("cannot connect to mediaChannel");
                    }
                    else
                    {
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
                    }
                }

                if (args.Contains("volume"))
                {
                    Task.Run(() => volume(float.Parse(args[Array.IndexOf(args, "volume") + 1]))).Wait();
                }

                if (args.Contains("status"))
                {
                    var receiver = new Dictionary<string, object>();
                    if (recStatus == null)
                    {
                        receiver.Add("Status", "notfound");
                    }
                    else
                    {
                        receiver.Add("Volume", recStatus.Volume.Level.ToString());

                        foreach (var app in recStatus.Applications)
                        {
                            var application = new Dictionary<string, object>();
                            application.Add("AppId", app.AppId);
                            application.Add("IsIdleScreen", app.IsIdleScreen);
                            application.Add("DisplayName", app.DisplayName);
                            application.Add("StatusText", app.StatusText);
                            application.Add("SessionId", app.SessionId);
                            application.Add("TransportId", app.TransportId);
                            receiver.Add("application", application);
                        }

                    }
                    result.Add("receiver", receiver);

                    var media = new Dictionary<string, object>();

                    if (mediastatus == null)
                    {
                        media.Add("Status", "notfound");
                    }
                    else
                    {
                        media.Add("CurrentTime", mediastatus.CurrentTime);
                        media.Add("PlayerState", mediastatus.PlayerState);
                        if (mediastatus.Media != null)
                        {
                            media.Add("ContentId", mediastatus.Media.ContentId);
                            media.Add("ContentType", mediastatus.Media.ContentType);
                            media.Add("Duration", mediastatus.Media.Duration);
                            media.Add("StreamType", mediastatus.Media.StreamType);

                            foreach (var mediatrack in mediastatus.Media.Tracks)
                            {
                                var track = new Dictionary<string, object>();
                                track.Add("Name", mediatrack.Name);
                                track.Add("Language", mediatrack.Language);
                                track.Add("SubType", mediatrack.SubType);
                                track.Add("TrackId", mediatrack.TrackId);
                                media.Add("Tracks", track);
                            }
                            if (mediastatus.Media.Metadata != null)
                            {
                                var metadata = new Dictionary<string, object>();
                                metadata.Add("Title", mediastatus.Media.Metadata.Title);
                                metadata.Add("Subtitle", mediastatus.Media.Metadata.Subtitle);
                                metadata.Add("MetadataType", mediastatus.Media.Metadata.MetadataType);

                                if (mediastatus.Media.Metadata.Images != null)
                                {
                                    foreach (var medatadataImage in mediastatus.Media.Metadata.Images)
                                    {
                                        var images = new Dictionary<string, object>();
                                        images.Add("Url", medatadataImage.Url);
                                        images.Add("Width", medatadataImage.Width);
                                        images.Add("Height", medatadataImage.Height);
                                        metadata.Add("Tracks", images);
                                    }
                                }
                                media.Add("Metadata", metadata);
                            }
                        }
                    }
                    result.Add("media", media);
                }
                Console.WriteLine(printJson(result));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (args.Contains("pauseAtEnd"))
            {
                Console.ReadKey();
            }

        }

        private static string addSlashes(string value)
        {
            return value.Replace("\"", "\\\"");
        }

        private static string printJson(object data)
        {
            var values = new List<string>();

            if (data == null)
            {
                return "\"null\"";
            }
            else if (data.GetType() == typeof(Dictionary<string, object>))
            {
                foreach (var item in (Dictionary<string, object>)data)
                {
                    values.Add("\"" + addSlashes(item.Key) + "\" : " + printJson(item.Value));
                }
                return "{" + string.Join(",", values) + "}";
            }
            else if (data.GetType() == typeof(List<object>))
            {
                foreach (var item in (List<object>)data)
                {
                    values.Add(printJson(item));
                }
                return "[" + string.Join(",", values) + "]";

            }
            else
            {
                return "\"" + addSlashes(data.ToString()) + "\"";
            }

        }

        public static async Task<IEnumerable<IReceiver>> detectedCast()
        {
            return (await new DeviceLocator().FindReceiversAsync());
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
            try
            {
                mediaChannel = sender.GetChannel<IMediaChannel>();
                mediastatus = await mediaChannel.GetStatusAsync();
            }
            catch { }
        }


        public static async Task play(string url, string title, string subtitle, string image)
        {
            mediaChannel = sender.GetChannel<IMediaChannel>();
            await sender.LaunchAsync(mediaChannel);
            var mediaInfo = new MediaInformation() { ContentId = url };
            mediaInfo.Metadata = new GenericMediaMetadata();
            if (title != null) mediaInfo.Metadata.Title = title;
            if (subtitle != null) mediaInfo.Metadata.Subtitle = subtitle;
            if (image != null)
            {
                mediaInfo.Metadata.Images = new GoogleCast.Models.Image[1];
                mediaInfo.Metadata.Images[0] = new GoogleCast.Models.Image() { Url = image };
            }
            mediastatus = await mediaChannel.LoadAsync(mediaInfo);
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

        public static async Task stop()
        {
            await mediaChannel.StopAsync();
        }

        public static async Task openURL(string url)
        {
            await sender.LaunchAsync(mediaChannel);
        }



        public static async Task volume(float volume)
        {
            await recChanel.SetVolumeAsync(volume);
        }


    }
}
