# ALERT

PoisnCopy is now on 100 servers which is the capacity based on [this post from Discord](https://support.discord.com/hc/en-us/articles/4410940809111). I hope to verify this bot at some point, but it might be take a while.

Therefore, I created PoisnCopy2. Odds are the bot will be offline. If you need the bot turned on, then please feel free to donate and I can turn it on.
If you cannot get the project to work then you can try creating an issue in GitHub.

### Show Some Support - _Sponsor this project on Github_

# PoisnCopy

Discord channel copy bot

Add the bot to BOTH of your servers using this link. (NOTE: You must be the OWNER (Not just an admin) of BOTH servers)
https://discord.com/oauth2/authorize?client_id=1114582921827864687&scope=bot&permissions=396211112976

**DISCLAIMER: There is no guarantee that I will keep this bot running on the server at any given point, use at your own risk**

Use: `/copychannel`
The bot will list the channels in the server that you are typing in. You must enter the ID of the channel that you want to copy and then I will give you a command to copy and paste in the new server.

**Make sure your bot can see which channel you are typing in (check left hand side)**
![image](https://user-images.githubusercontent.com/60050783/107395699-49d3a300-6aba-11eb-8b1c-d4e4b41cd6f3.png)

\*Note: The `/copychannel` command is just to list the IDs, once you have them, you can just use the `/loadchannel`command.

i.e.

-> `/copychannel`

-> "Here is your list of channels:<br>
123456752146761758-general<br>
--Thats all of the channels!--<br>
Copy command: /loadchannel server_id:123456737220528146 channel_id:###<br>
Export command: /exportchannel server_id:123456737220528146 channel_id:###"

-> `/loadchannel server_id: 123456737220528146 channel_id: 123456752146761758 new_channel_name: testing` **Paste this in the Server that you want to put copy the channel into**

-> `Channel Name` **It will create a new channel in the server that you are in** ![image](https://user-images.githubusercontent.com/60050783/107396172-c4042780-6aba-11eb-8ec4-88cf4b750e6a.png)

-> "Starting copy...

-> Collecting messages...

-> Organizing messages...

-> Creating channel...

-> Posting 218 messages... (this could take awhile)

-> new-chan copy complete!" **It will tell you when it is done, copying hundreds/thousands of messages can take a LONG time, be patient**

-> `/exportchannel server_id: 768249837220528146 channel_id: 768562880751730718` **This will compile all of your message into a CSV File**

Contributions are welcome! Just create a PR and I will review it.

In order to run the project on your own, you will need to rename the `test-config.json` file to `config.json` and then you need to fill in the corresponding fields from your https://discord.com/developers/applications specific application. I will not go into specifics on creating your own discord bot here.
