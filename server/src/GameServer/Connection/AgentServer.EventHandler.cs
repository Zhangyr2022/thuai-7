using GameServer.GameController;
using GameServer.GameLogic;

namespace GameServer.Connection;

public partial class AgentServer
{
    public static int TickCount = 0;
    public void HandleAfterGameTickEvent(object? sender, AfterGameTickEventArgs e)
    {
        List<MapMessage.Wall> walls = new();
        List<SuppliesMessage.Supply> supplies = new();

        // Add walls and supplies
        for (int x = 0; x < e.GameMap.Width; x++)
        {
            for (int y = 0; y < e.GameMap.Height; y++)
            {
                // Add walls
                if (e.GameMap.MapChunk[x, y].IsWall == true)
                {
                    walls.Add(
                        new MapMessage.Wall
                        {
                            Position = new MapMessage.Wall.WallPositions
                            {
                                X = x,
                                Y = y
                            }
                        }
                    );
                }

                // Add supplies
                foreach (IItem item in e.GameMap.MapChunk[x, y].Items)
                {
                    supplies.Add(
                        new SuppliesMessage.Supply
                        {
                            Name = item.ItemSpecificName,
                            PositionOfSupply = new SuppliesMessage.Supply.Position
                            {
                                X = x,
                                Y = y
                            },
                            Numb = item.Count
                        }
                    );
                }
            }
        }

        List<PlayersInfoMessage.Player> players = new();

        // Add players
        foreach (Player player in e.AllPlayers)
        {
            List<PlayersInfoMessage.Player.Item> inventory = new();

            List<PlayersInfoMessage.Player.FirearmInfo> weaponSlot = new();

            // Add inventory
            foreach (IItem item in player.PlayerBackPack.Items)
            {
                inventory.Add(
                    new PlayersInfoMessage.Player.Item
                    {
                        Name = item.ItemSpecificName,
                        Num = item.Count
                    }
                );
            }

            foreach (IWeapon _weapon in player.WeaponSlot)
            {
                weaponSlot.Add(
                    new PlayersInfoMessage.Player.FirearmInfo
                    {
                        Name = _weapon.Name,
                        Distance = _weapon.Range
                    }
                )
            }


            // Add player
            players.Add(
                new PlayersInfoMessage.Player
                {
                    PlayerId = player.PlayerId,
                    Armor = (player.PlayerArmor is not null) ? player.PlayerArmor.ItemSpecificName : "NO_ARMOR",
                    Current_armor_health = player.PlayerArmor.health,
                    Health = player.Health,
                    Speed = player.Speed,
                    Firearm = new PlayersInfoMessage.Player.FirearmInfo
                    {
                        Name = player.PlayerWeapon.Name,
                        Distance = player.PlayerWeapon.Range
                    },
                    Firearms_pool = new(weaponSlot),
                    Position = new PlayersInfoMessage.Player.PositionInfo
                    {
                        X = player.PlayerPosition.x,
                        Y = player.PlayerPosition.y
                    },
                    Inventory = new(inventory)
                }
            );
        }

        // Append map message, supplies message, players info message, and safe zone message to _messageToPublish
        if (TickCount % 100 == 0)
        {
            _messageToPublish.Enqueue(
                new MapMessage
                {
                    Length = e.GameMap.Height,
                    Walls = new(walls)
                }
            );
        }
        _messageToPublish.Enqueue(
            new SuppliesMessage
            {
                Supplies = new(supplies)
            }
        );
        _messageToPublish.Enqueue(
            new PlayersInfoMessage
            {
                Players = new(players)
            }
        );
        _messageToPublish.Enqueue(
            new SafeZoneMessage
            {
                CenterOfCircle = new SafeZoneMessage.Center
                {
                    X = e.GameMap.SafeZone.Center.x,
                    Y = e.GameMap.SafeZone.Center.y
                },
                Radius = e.GameMap.SafeZone.Radius
            }
        );

        TickCount++;
    }

    public void HandleAfterPlayerConnectEvent(object? sender, AfterPlayerConnect e)
    {
        // Remove all items whose value is e.Token
        List<Guid> keys = [];
        foreach (KeyValuePair<Guid, string> pair in _socketTokens)
        {
            if (pair.Value == e.Token)
            {
                keys.Add(pair.Key);
            }
        }
        foreach (Guid key in keys)
        {
            _socketTokens.TryRemove(key, out _);
        }

        _socketTokens.AddOrUpdate(e.SocketId, e.Token, (key, oldValue) => e.Token);

        Publish(
            new PlayerIdMessage() { PlayerId = e.PlayerId },
            e.Token
        );
    }
}
