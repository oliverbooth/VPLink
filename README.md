<p align="center">
<img src="banner.png">
<a href="https://github.com/oliverbooth/VPLink/actions/workflows/dotnet.yml"><img src="https://img.shields.io/github/actions/workflow/status/oliverbooth/VPLink/dotnet.yml?style=flat-square" alt="GitHub Workflow Status" title="GitHub Workflow Status"></a>
<a href="https://github.com/oliverbooth/VPLink/issues"><img src="https://img.shields.io/github/issues/oliverbooth/VPLink?style=flat-square" alt="GitHub Issues" title="GitHub Issues"></a>
<a href="https://github.com/oliverbooth/VPLink/releases"><img alt="GitHub release" src="https://img.shields.io/github/v/release/oliverbooth/VPLink?style=flat-square"></a>
<a href="https://github.com/oliverbooth/VPLink/blob/master/LICENSE.md"><img src="https://img.shields.io/github/license/oliverbooth/VPLink?style=flat-square" alt="MIT License" title="MIT License"></a>
</p>

### About
VPLink is a simple and customisable bot for both Discord and Virtual Paradise, which bridges chat messages between a
designated Discord channel, and the world where the bot is running. It is written in C# and uses the Discord.NET
library, as well as a [wrapper for the Virtual Paradise SDK](https://github.com/oliverbooth/VpSharp) that I wrote
myself.

## Installation
### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (or later)
- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) (or later)
- A [Virtual Paradise](https://www.virtualparadise.org/) user account
- A [Discord](https://discord.com/) user account, and a bot application

### Setup (docker-compose)

1. Clone the repository to your local machine.
2. Edit the `docker-compose.yml` file to your needs, including validating the mount paths.
3. /app/data (relative to the container) must contain a config.toml file with the fields populated, see the
    [example config file](config.example.toml) for the available fields.
4. Run `docker-compose up -d` to start the bot.

### Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
Please see the [contributing guidelines](CONTRIBUTING.md) for more information.

### License

VPlink is licensed under the [MIT License](LICENSE.md). See the LICENSE.md file for more information.