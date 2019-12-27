# CastInLine 
Google Cast remote in command line giving the result in json format

## Description:
You can remotly control a Google Cast compatible device with the following commands and get the result as json format

## Usage:
commands list :

    discover					lists the devices on the network
    target IP[:Port]				specify the target device of the command
    pauseAtEnd				wait for input key to close (for debug purpose)
    status					get the status of the device
    play {url} [title {text}] [subtitle {text} [image {url}]        play a media
    pause					pause the current media
    resume					resume the current media
    stop						stop the current media
    volume {value}			set the volume (0->1)
    seek {value}				set player position in seconds
