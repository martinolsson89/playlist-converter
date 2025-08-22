# Playlist Converter

A Blazor WebAssembly (.NET 9) app with an ASP.NET Core Web API backend that helps convert a Spotify playlist into a YouTube playlist.

- Frontend: PlaylistConverter.Client (Blazor WebAssembly)
- Backend: PlaylistConverter.Api (ASP.NET Core Web API)
- Styling: Tailwind CSS (built via PostCSS)
- API Docs: Swagger UI at /swagger

## Features

- Fetch a Spotify playlist by URL or ID and list its tracks
- Caching for fetched Spotify playlists (30 minutes)
- YouTube integration primitives (create playlist, search videos, add to playlist)
- Simple auth state tracking in the client for YouTube token presence

## Prerequisites

- .NET SDK 9
- Node.js 18+ (for Tailwind build)
- Credentials:
  - Spotify: SPOTIFY_CLIENT_ID, SPOTIFY_CLIENT_SECRET
  - YouTube: YOUTUBE_CLIENT_ID, YOUTUBE_CLIENT_SECRET, YOUTUBE_REDIRECT_URI, YOUTUBE_API_KEY

## Configuration

### Backend (PlaylistConverter.Api)

Set these environment variables (User Secrets recommended for local dev):

- SPOTIFY_CLIENT_ID
- SPOTIFY_CLIENT_SECRET
- YOUTUBE_CLIENT_ID
- YOUTUBE_CLIENT_SECRET
- YOUTUBE_REDIRECT_URI (e.g., https://localhost:5001/api/YouTube/callback)
- YOUTUBE_API_KEY

## Live Demo
See live demo at: https://gentle-cliff-08c3f2203.1.azurestaticapps.net/
Contact me at martinolsson89@gmail.com if you want to try out the app!
