# CastInLine 
Google Cast remote in command line giving the result in json format

It a simple .NET 4.7.2 console application using [GoogleCast by kakone](https://github.com/kakone/GoogleCast "GoogleCast by kakone") 

## Description:
You can remotly control a Google Cast compatible device with the following commands and get the result as json format

## Usage:
commands list :

    discover					lists the devices on the network
    target IP[:Port]			specify the target device of the command. Port default value is 8009
    pauseAtEnd				    wait for input key to close (for debug purpose)
    status					    get the status of the device
    play {url} [title {text}] [subtitle {text} [image {url}]        play a media
    pause					    pause the current media
    resume					    resume the current media
    stop						stop the current media
    volume {value}			    set the volume (0->1)
    seek {value}				set player position in seconds
  
## Example

### Discover
`CastInLine.exe discover`

```json
{ 
   "errors":[ 

   ],
   "devices":[ 
      { 
         "FriendlyName":"Cuisine",
         "IPEndPoint":"192.168.0.38:8009",
         "ID":"xxxxxxxxxx"
      },
      { 
         "FriendlyName":"Salon",
         "IPEndPoint":"192.168.0.90:8009",
         "ID":"xxxxxxxxxx"
      },
      { 
         "FriendlyName":"Maison",
         "IPEndPoint":"192.168.0.38:42065",
         "ID":"xxxxxxxxxx"
      }
   ]
}
```
### Get Status
`CastInLine.exe  target 192.168.0.38 status`
```json
{ 
   "errors":[ 

   ],
   "receiver":{ 
      "Volume":"1",
      "application":{ 
         "AppId":"CC1AD845",
         "IsIdleScreen":"False",
         "DisplayName":"Default Media Receiver",
         "StatusText":"Casting: A Movie",
         "SessionId":"xxxxxxxxxx",
         "TransportId":"xxxxxxxxxx"
      }
   },
   "media":{ 
      "CurrentTime":"3,562",
      "PlayerState":"PLAYING",
      "ContentId":"https://xxxxxxx.mp4",
      "ContentType":"video/mp4",
      "Duration":"381,376",
      "StreamType":"Buffered",
      "Tracks":{ 
         "Name":"null",
         "Language":"null",
         "SubType":"Subtitles",
         "TrackId":"1"
      },
      "Metadata":{ 
         "Title":"A Movie",
         "Subtitle":"the best one",
         "MetadataType":"Default",
         "Tracks":{ 
            "Url":"https://xxxxxxx.jpg",
            "Width":"null",
            "Height":"null"
         }
      }
   }
}
```
## More
Tested on Windows 10 and Raspberry PI with mono

# Download Binaries

You can download binaries here [Here](https://github.com/milouz-corp/CastInLine/releases/download/1/Release.zip)

