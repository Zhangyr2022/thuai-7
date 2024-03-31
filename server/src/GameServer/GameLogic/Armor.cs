namespace GameServer.GameLogic;

/// <summary>
/// Factory for converting between items and armors.
/// </summary>
public class ArmorFactory
{
    /// <summary>
    /// Create armor from an item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IArmor CreateFromItem(IItem item)
    {
        if (item.Kind != IItem.ItemKind.Armor)
        {
            throw new ArgumentException($"Item kind {item.Kind} is not an armor.");
        }

        int maxHealth = item.ItemSpecificName switch
        {
            // TODO: Create a mapping from item specific id to max health
            _ => throw new ArgumentException($"Item specific id {item.ItemSpecificName} is not valid for armor.")
        };
        return new Armor(item.ItemSpecificName, maxHealth);
    }

    public static IItem ToItem(IArmor armor, int count)
    {
        return new Item(IItem.ItemKind.Armor, armor.ItemSpecificName, count);

    }
}

/// <summary>
/// Armor can be worn by a player to protect them from damage.
/// </summary>
public class Armor : IArmor
{
    public string ItemSpecificName { get; }
    public int Health { get; private set; }
    public int MaxHealth { get; }

    public Armor(string itemSpecificName, int maxHealth)
    {
        ItemSpecificName = itemSpecificName;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public int Hurt(int Damage)
    {
        if (Health > Damage)
        {
            Health -= Damage;
            return 0;
        }
        else
        {
            Health = 0;
            return (Damage - Health);
        }
    }
}
