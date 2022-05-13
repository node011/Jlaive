# Jlaive

Jlaive is an antivirus evasion tool that can convert .NET assemblies into undetectable batch files.

Support for native applications will come soon.

Join the Discord server for discussion and enquiries: https://discord.gg/RU5RjSe8WN.

## Screenshots
![image](https://media.discordapp.net/attachments/959762900443070485/974262553553293312/unknown.png)
![image](https://media.discordapp.net/attachments/959762900443070485/974469247021506590/unknown.png)
![image](https://media.discordapp.net/attachments/959762900443070485/973935543543033856/unknown.png)
![image](https://media.discordapp.net/attachments/959762900443070485/973935592670908456/unknown.png)

## Known issues

- `Assembly.GetEntryAssembly()` returns null. Use `Assembly.GetExecutingAssembly()` instead.
- `Hidden` option does not work on Windows Terminal.

## Disclaimer
This project was made for educational purposes only. I am not responsible if you choose to use this illegally/maliciously.
