nuget restore
msbuild EchoBot.sln -p:DeployOnBuild=true -p:PublishProfile=waterfallticket-Web-Deploy.pubxml -p:Password=QbskyNHcTcLsMtnTMzPcfqXJKCgu8PLwZMjMFaElty9QE9xwMnWvYoKxfnp8

