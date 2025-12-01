# Campofinale
[EN](README.md) | [IT](docs/README_it-IT.md) | [RU](docs/README_ru-RU.md) | [CN](docs/README_zh-CN.md) | [NL](docs/README_nl-NL.md)

Campofinale is an experimental server implementation for a certain factory building game.

## Current Features

* Login
* Character switch
* Team switch
* Scene switch
* Save data with MongoDB
* Combat system


## TODO
* Android Support
* Mission System
* Working buffs
* Fixing Factory system for new versions

## Installation Steps (Windows)

1. Install:
   * [.NET SDK](https://dotnet.microsoft.com/en-us/download) (8.0.12 is recommended)
   * [MongoDB](https://www.mongodb.com/try/download/community)
   * [mitmproxy](https://mitmproxy.org/)

    1. Make sure to setup Mitmproxy accordingly, and of course install the certificate system-wide.
    
2. Download the [precompiled build](https://git.teamstardust.org/Campofinale/Campofinale/releases/latest) or build it by yourself
3. Put the `Json`, `TableCfg` folder inside the `Campofinale.exe` folder (you can download a copy [here](https://git.teamstardust.org/Campofinale/EndfieldData)) | Also get `DynamicAssets` from [here](https://git.teamstardust.org/Campofinale/EndfieldData-Archive/src/branch/main/0.5.28/DynamicAssets) and put them in the same folder as `Campofinale.exe`
4. Run the server `Campofinale.exe`
5. Proxy post-install setup

    ```shell
    mitmweb -s ak.py --mode local:EndfieldTBeta2 --set stream_large_bodies=3m
    ```

   Get ak.py from [here](https://git.teamstardust.org/Campofinale/Campofinale/src/branch/development/docs/ak.py)
    
6. Run the Mitmproxy command (from above) if you haven't

7. Patch the game client (get the patch from our Discord) - Run launcher.exe after (Note: Only OS client is supported for now, CN CBT3 could work too because offsets are the same)
8. You must create an account using `account create (username)` in the server console, then login in the game with an email like `(username)@randomemailformathere.whatyouwant`. There is no password so you can input a random password for its field.

## Additional Information

You can find the description of all server commands [here](docs/CommandList/commands_en-US.md).<br>
The list of all scenes is [here](docs/LevelsTable.md).<br>
The list of all enemies is [here](docs/EnemiesTable.md).<br>
The list of all characters is [here](docs/CharactersTable.md).<br>
The list of all items is [here](docs/ItemsTable.md).<br>

If you want to open the in-game console, go to `Settings -> Platform & Account -> Account Settings (Access Account button)`. To view available commands, type `help`.

## Discord for support

If you want to discuss, ask for support or help with this project, join our [Discord Server](https://discord.gg/HdXZY2Q9vs)!

## Note

This project is developed independently, and all rights to the original game assets and intellectual property belong to their respective owners.

## Personal Notes (wahts_dis)

### oh WOW
You are sure to know how to use it.  
No? then leave.

Current job: Implement important game modules.
 - [x] ~Mission system~
 - [ ] Factory
 - [ ] Interactive (Unable to get full event list now, difficult to do)

TODOs in code:
- [x] ~Campofinale/Game/Spaceship/SpaceshipManager.cs: Cost item and increase chara favorability when gifting~
- [ ] Campofinale/Game/Inventory/Item.cs: Count costWeaponIds exp when upgrading
- [ ] Campofinale/Game/Inventory/InventoryManager.cs: Complete drops logic
- [ ] Campofinale/Game/Inventory/InventoryList.cs: Factory items finding and filtering
- [x] ~Campofinale/Game/GameConstants.cs: GAME_VERSION_ASSET_URL necessary on different platforms? Won't do it.~
- [ ] Campofinale/Resource/ResourceManager.cs: Split datas. Low priority.
- [ ] Campofinale/Game/Factory/FactoryChapter.cs: UseHealTowerPoint
- [ ] Campofinale/Game/Factory/FactoryChapter.cs: MoveItemCacheToBag
- [ ] Campofinale/Packets/Sc/PacketScGachaSync.cs: Pool open/close/banner
- [ ] Campofinale/Packets/Sc/PacketScFactorySyncScope.cs: Remove hardcoded factory scope
- [ ] Campofinale/Game/Entities/EntityInteractive.cs: pick up
- [ ] Campofinale/Packets/Cs/HandleCsMissionEventTrigger.cs: Event trigger
- [ ] Campofinale/Game/Char/Character.cs: cost items when talent up
- [ ] Campofinale/Game/Char/Character.cs: make weapon skills work
- [ ] Campofinale/Packets/Cs/HandleCsWeaponPuton.cs: gear assignment and removal cleanly
- [ ] Campofinale/Game/Adventure/AdventureBookManager.cs: refresh data and notify when next stage initialized
- [x] ~Campofinale/Database/Database.cs: wont do it~
- [ ] Campofinale/Packets/Cs/HandleCsEquipPuton.cs: gear assignment and removal cleanly
- [ ] Campofinale/Packets/Cs/HandleCsEquipPutoff.cs: gear assignment and removal cleanly
- [ ] Campofinale/Packets/Cs/HandleCsCharPotentialUnlock.cs: cost items when potential unlocks
- [ ] Campofinale/Packets/Cs/HandleCsBattleOp.cs: AbilityManager
- [ ] Campofinale/Packets/Cs/HandleCsAdventureTakeRewardAll.cs: batch claim reward
