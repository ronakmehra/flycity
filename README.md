# FlyCity Roleplay

**FlyCity Roleplay** — premium gamemode for **RAGE:MP (GTA V)**.

> High-quality roleplay experience built on a solid and feature-rich foundation.

---

## 🛩️ About

FlyCity Roleplay is a fully-featured GTA V multiplayer roleplay server built on RAGE:MP. It includes a rich CEF interface, advanced HUD system, character creation, businesses, fractions, casino, and much more.

---

## 🚀 Getting Started

### Requirements
- RAGE:MP Server (latest)
- Node.js 16+
- .NET 6+

### Installation

```bash
# Clone the repository
git clone <repository-url>

# Navigate to src_cef and install dependencies
cd src_cef
npm install

# Build the interface
npm run build
```

---

## 🏗️ Project Structure

```
├── client_packages/     # Client-side scripts & CEF interface
├── src_cef/             # Svelte CEF source (UI)
├── src_client/          # Client-side JavaScript modules
├── dotnet/              # C# server-side scripts
├── database/            # Database schemas & migrations
├── settings/            # Server configuration files
└── conf.json            # RAGE:MP server config
```

---

## 🎮 Features

- 🔐 **Authentication System** — Login, register, restore password
- 🧍 **Character Creation** — Full character customization
- 🗺️ **Interactive Map** — Real-time minimap with blips
- 🏢 **Businesses** — Auto shops, weapon shops, pet shops, clothing stores
- 🚔 **Fractions** — Police, emergency services, criminal organizations
- 🎰 **Casino** — Blackjack, roulette, jackpot, horse racing
- 🏠 **Housing System** — Buy, furnish and manage properties
- 🎒 **Inventory** — Full drag & drop item management
- 🚗 **Vehicle System** — Car market, LS Customs tuning, rentals
- 📱 **Phone System** — Calls, messages, radio, apps
- ⚔️ **Events** — Wars, airdrops, competitions
- 🎯 **Quests** — Daily quests and battle pass progression
- 🛡️ **Admin Tools** — Anti-cheat, spectate, player management

---

## 🛠️ Development

### Building the UI

```bash
cd src_cef

# Production build (Russian locale)
npm run build

# Development server
npm run dev
```

### Tech Stack

- **UI Framework**: Svelte 3
- **Build Tool**: Webpack 5
- **Styling**: SASS/SCSS
- **Client Language**: JavaScript (RAGE:MP API)
- **Server Language**: C# (.NET 6)

---

## 📄 License

**MIT License**

Copyright (c) 2024-2026 **void&co**

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

---

## 👨‍💻 Development Team

**Developed by [void&co](https://github.com/voidandco)**

| Role | Name |
|------|------|
| Lead Developer | void&co |
| UI/UX Design | void&co |
| Server Architecture | void&co |

---

## 🌐 Community

- **Discord**: discord.gg/flycityrp
- **Website**: flycityroleplay.com

---

*FlyCity Roleplay — Take to the skies.*
