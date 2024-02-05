<!--
 Copyright (c) 2024 TheModdersDen | https://github.com/TheModdersDen

 Permission is hereby granted, free of charge, to any person obtaining a copy of
 this software and associated documentation files (the "Software"), to deal in
 the Software without restriction, including without limitation the rights to
 use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 the Software, and to permit persons to whom the Software is furnished to do so,
 subject to the following conditions:

 The above copyright notice and this permission notice shall be included in all
 copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 -->

# WindowsGSM.Palworld-Modded

## A Plugin for the Palworld Modding API (and the Dedicated Server) for WindowsGSM

This is a plugin for WindowsGSM that allows you to run a server for the game Palworld with support for the PalworldModdingKit (found [here](https://pwmodding.wiki/)).

This plugin is based on the [WindowsGSM.Palworld](https://github.com/ohmcodes/WindowsGSM.Palworld) plugin, but with extended features and support for auto-downloading and updating of the PalworldModdingKit, as well as automagic public IP retrieval. Credit goes to the original author ([ohmcodes](https://github.com/ohmcodes)) for the base plugin.

### Requirements

- WindowsGSM
- A semi-strong computer running Windows (Windows 10/Windows Server 2016 or later recommended)
- A copy of the game Palworld
- Two forwarded port numbers (one for the game server and one for query port)
- A working and active internet connection
- A basic understanding of how to use WindowsGSM, the Microsoft Windows operating system, the Windows command line, and the game Palworld

### Installation

1. Download the latest release of WindowsGSM from the [official WindowsGSM website](https://windowsgsm.com/products/windowsgsm-desktop/).
2. Install WindowsGSM by running the installer and following the on-screen instructions.
3. Download the latest release of this plugin from the [releases page](https://github.com/TheModdersDen/WindowsGSM.Palworld-Modded/releases).
   1. At the time of writing, there are **NO** releases, as this plugin is still a WIP. Please check back later for releases.
      1. I AM NOT RESPONSIBLE FOR ANY DAMAGE CAUSED BY USING THIS PLUGIN. USE AT YOUR OWN RISK.
4. Extract the contents of the .zip file to your WindowsGSM installation directory (usually `C:\WindowsGSM`).
   1. If you are prompted to overwrite any files, click `Yes` to all.
5. Navigate to WindowsGSM and click on the `Plugins` tab (the little puzzle icon).
6. At the top right of the WindowsGSM GUI, click the button that says `Reload Plugins`.
7. You should now see the `Palworld Modded` plugin in the list of plugins.
8. Go to the `Server` tab and click the `Install` button.
   1. When asked to choose a server type, select `Palworld Modded`.
9. Follow the on-screen instructions to install the server.
10. Before starting the server, update it by clicking the `Update` button in the `Server` tab.
    1. If the server fails to update, you may need to manually download the latest version of the PalworldModdingKit from the [official website](https://pwmodding.wiki/) and place it in the server's installation directory.
    2. If you are still having issues, please open an issue on this repository.
11. Once the server is installed, you can start it by clicking the `Start` button in the `Server` tab.
12. If the server starts successfully, you should see a green `Online` status in the `Server` tab.

### Configuration

You can configure the server by clicking the `Config` button in the `Server` tab. This will open a window where you can change the server's settings, such as the server name, the server password, the server port, and the query port.

### Contributing

If you would like to contribute to this project, please open a pull request with your changes. Please make sure to follow the [contributing guidelines](docs/CONTRIBUTING./md) and the [code of conduct](docs/CODE_OF_CONDUCT.md) when contributing.

### License

This project is licensed under the MIT License. For more information, please see the [LICENSE.md](LICENSE.md) file.

### Contact

If you have any questions, comments, or concerns, please feel free to contact me at [my website](https://themoddersden.com/contact).

### TODO

- [ ] Add support for auto-downloading and updating the PalworldModdingKit
- [ ] Add support for auto-downloading and updating the Palworld Dedicated Server
- [ ] Actually get the plugin to run without errors
- [ ] Add support for the PalworldModdingKit's configuration file
- [ ] Add support for the Palworld Dedicated Server's configuration file
- [ ] Auto-create the Palworld Dedicated Server's configuration file if it does not exist
- [ ] Add support for the Palworld Dedicated Server's logs
- [ ] Add support for the PalworldModdingKit's logs
- [ ] Add support for CurseForge or other mod hosting sites
  - [ ] Allow the user to specify a list of mods to download and install
- [ ] Get and set the public IP address of the server (from the internet), for convenience
  - [ ] Do this securely
- [ ] Auto-update the PalworldModdingKit
- [ ] Auto-update the Palworld Dedicated Server
- [ ] Test the plugin thoroughly
- [ ] Create the first release
- [ ] Add more ideas here

### Credits

- [ohmcodes](https://github.com/ohmcodes) for the original plugin (and codebase)
- [TheModdersDen](https://themoddersden.com) for the extended features and support (and the current maintainer of this plugin)
- [The Palworld Modding Community](https://pwmodding.wiki/) for the modding kit for Palworld
- [The Palworld Developers](https://www.pocketpair.jp/palworld) for the game itself
- [The WindowsGSM Developers](https://windowsgsm.com/) for the WindowsGSM project
- [The WindowsGSM Discord Community](https://windowsgsm.com/discord) for support and help programming this plugin
- [GitHub](https://github.com) for hosting this project
- [Git](https://git-scm.com) for the version control system
- [Microsoft](https://microsoft.com) for the Windows operating system
- [GitKraken](https://www.gitkraken.com) for their **awesome** Git client
- [Visual Studio Code](https://code.visualstudio.com) for the awesome code editor used to make this plugin
- [Ipify](https://www.ipify.org) for the public IP address API used to retrieve the server's public IP address
