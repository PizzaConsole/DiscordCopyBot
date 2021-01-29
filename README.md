# PoisnCopy
Discord channel copy bot

Add the bot to BOTH of your servers using this link. (NOTE: You must be the OWNER (Not just an admin) of BOTH servers)
https://discordapp.com/api/oauth2/authorize?client_id=768322637574570015&scope=bot&permissions=8

**DISCLAIMER: There is no guarantee that I will keep this bot running on the server at any given point, use at your own risk**

Use: `pc.copychannel`
The bot will list the channels in the server that you are typing in. You must enter the ID of the channel that you want to copy and then I will give you a command to copy and paste in the new server.

**Make sure your bot can see which channel you are typing in (check left hand side)**
![image](https://user-images.githubusercontent.com/60050783/107395699-49d3a300-6aba-11eb-8b1c-d4e4b41cd6f3.png)

*Note: The `pc.copychannel` command is just to list the IDs, once you have them, you can just use the `pc.loadchannel`command.

i.e.

-> `pc.copychannel`

-> "Which Channel would you like to copy? (copy and paste the Id) Pleaes wait while I find all of your channels, I will give you a message when I have found them all."

-> `123456752146761758`

-> "Copy command: "

-> `pc.loadchannel 123456737220528146 123456752146761758` **Paste this in the Server that you want to put copy the channel into**

-> "What do you want the new channel to be named?"

-> `Channel Name` **It will create a new channel in the server that you are in** ![image](https://user-images.githubusercontent.com/60050783/107396172-c4042780-6aba-11eb-8ec4-88cf4b750e6a.png)

-> "Starting copy...

-> Collecting messages...

-> Organizing messages...

-> Creating channel...

-> Posting 218 messages... (this could take awhile)

-> new-chan copy complete!" **It will tell you when it is done, copying hundreds/thousands of messages can take a LONG time, be patient**

Contributions are welcome! Just create a PR and I will review it.

In order to run the project on your own, you will need to rename the `test-config.json` file to `config.json` and then you need to fill in the corresponding fields from your https://discord.com/developers/applications specific application. I will not go into specifics on creating your own discord bot here.