# Flarial Loader
An unofficial minimal injector for [Flarial Client](https://github.com/flarialmc).

## Features

- Hash checking for updates.
  
  - Flarial Loader performs hash checking to compare the local dynamic link library against the one on the CDN.

  - This mechanism removes the need to always redownload the dynamic link library to ensure the latest version is used. 

- Improved injection system, improving reliability & simplifying code.

  - `IApplicationActivationManager` is used to launch Minecraft: Bedrock Edition.
     
     - This API is specifically designed to launch UWP apps.

     - This API can wait for a given UWP app to fully launch & then return it's process ID.

  - Injection is performed directly within the application instead of using a stub dynamic link library.

- UWP App Suspension Prevention.
  
  - Enables debug mode for Minecraft: Bedrock Edition preventing it from suspending.

## Usage

- Download the latest release from [GitHub Releases](https://github.com/Aetopia/Flarial.Loader/releases/latest).

- Ensure Minecraft: Bedrock Edition is installed.

- Launch Flarial Loader & wait for it to download Flarial Client.

- Once downloaded, the game will automatically launch and the client will be injected!

## Building
1. Download the following:
    - [.NET SDK](https://dotnet.microsoft.com/en-us/download)
    - [.NET Framework 4.8.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net481-developer-pack-offline-installer)

2. Run the following command to compile:

    ```cmd
    dotnet publish "src\Flarial.Loader.csproj"
    ```