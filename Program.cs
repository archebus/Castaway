using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static Camp;
using static Character;

public class Program
{
    static void Main()
    {

        Game game = new Game();

        Character player = new Character();
        player.GenerateRolls();

        Camp camp = new Camp();
        Supplies[] supplies = new Supplies[2];
        camp.InitializeStructures();
        Supplies.InitSupplies(supplies);

        Narrative.Awakening(player, game, camp);
        Narrative.BeachWreckage(player, game, camp);
        Narrative.TakingStock(player, game, camp);

        while (player.IsAlive)
        {
            while (game.Time != TimeOfDay.Night)
            {
                Narrative.AtCamp(player, game, camp);
                Narrative.LaunchVerbNarrative(Narrative.GetCommand(), player, game, camp);
            }
            //Narrative.NightFalls();
            //game.Days++;
        }
        Console.ReadKey(true);
    }
}

public class Game
{
    public int Days { get; set; }
    public int Weather { get; set; }
    public TimeOfDay Time {  get; set; }

    public Game()
    {
        Days = 1;
        Weather = 5;
        Time = TimeOfDay.Morning;
    }

    public void MainScreen(Character player, Camp camp)
    {

        string green = "\u001b[1;32m";
        string white = "\u001b[0m";
        string red = "\u001b[31m";
        string bold = "\u001b[1m";

        string Line1 = $"┌─────────┬────────────────────┬─────────────────────────────┬────────────────┬─────────────────────────┬──────────────┐";
        string Line2 = $"│ -STATS- │   -AFFLICTIONS-    │       -CONSTRUCTIONS-       │   -SUPPLIES-   │       -INVENTORY-       │   -RECORDS-  │";
        string Line3 = $"│ {bold}AGI:{white} " + (player.Agility < 0 ? "" : "+")+ $"{player.Agility} │{red}{PrintAffliction(player.Afflictions[0])}{white}│{PrintStructure(camp.Structures[0])}│ Food  :{PrintFood(camp)} days │{PrintInventory(player.Inventory[0])}│ Day {PrintGameDays(Days)}      │";
        string Line4 = $"│ {bold}PRE:{white} " + (player.Presence < 0 ? "" : "+")+ $"{player.Presence} │{red}{PrintAffliction(player.Afflictions[1])}{white}│{PrintStructure(camp.Structures[1])}│ Water :{PrintWater(camp)} days │{PrintInventory(player.Inventory[1])}│              │";
        string Line5 = $"│ {bold}STR:{white} " + (player.Strength < 0 ? "" : "+") + $"{player.Strength} │{red}{PrintAffliction(player.Afflictions[2])}{white}│{PrintStructure(camp.Structures[2])}│ -------------- │{PrintInventory(player.Inventory[2])}│              │";
        string Line6 = $"│ {bold}TOU:{white} " + (player.Toughness < 0 ? "" : "+") + $"{player.Toughness} │{red}{PrintAffliction(player.Afflictions[3])}{white}│{PrintStructure(camp.Structures[3])}│ Lumber:        │{PrintInventory(player.Inventory[3])}│              │";
        string Line7 = $"├─────────┴────────────────────┤{PrintStructure(camp.Structures[4])}│ Cloth:         │{PrintInventory(player.Inventory[4])}│              │";
        string Line8 = $"│ Health: {green}{HealthPrint()}{white} │{PrintStructure(camp.Structures[5])}│                │{PrintInventory(player.Inventory[5])}│              │";
        string Line9 = $"│ Fatigue:{red}{FatiguePrint()}{white} │{PrintStructure(camp.Structures[6])}│                │{PrintInventory(player.Inventory[6])}│              │";
        string Line10 = $"├──────────────────────────────┤{PrintStructure(camp.Structures[7])}│                │{PrintInventory(player.Inventory[7])}│              │";
        string Line11 = $"│ WPN: [D{player.EquippedWeapon.Damage}] {WeaponPrint()}│                             │                │                         │              │";
        string Line12 = $"│ ARM: [{player.PlayerArmor.Tier}] {ArmorPrint()}│                             │                │                         │              │";
        string Line13 = $"└──────────────────────────────┴─────────────────────────────┴────────────────┴─────────────────────────┴──────────────┘";

        Console.Clear();
        Console.WriteLine(Line1);
        Console.WriteLine(Line2);
        Console.WriteLine(Line3);
        Console.WriteLine(Line4);
        Console.WriteLine(Line5);
        Console.WriteLine(Line6);
        Console.WriteLine(Line7);
        Console.WriteLine(Line8);
        Console.WriteLine(Line9);
        Console.WriteLine(Line10);
        Console.WriteLine(Line11);
        Console.WriteLine(Line12);
        Console.WriteLine(Line13);

        string HealthPrint()
        {
            StringBuilder healthString = new StringBuilder();
            int spaceRemaining = 10 - player.Health;
            for (int i = 0; i < player.Health; i++)
            {
                healthString.Append(" \u2588"); 
            }
            for (int i = 0; i < spaceRemaining; i++)
            {
                healthString.Append("  ");
            }

            return healthString.ToString();
        }

        string FatiguePrint()
        {
            StringBuilder fatigueString = new StringBuilder();
            int spaceRemaining = 10 - player.Fatigue;
            for (int i = 0; i < player.Fatigue; i++)
            {
                fatigueString.Append(" \u2588");
            }
            for (int i = 0; i < spaceRemaining; i++)
            {
                fatigueString.Append("  ");
            }

            return fatigueString.ToString();
        }

        string OriginPrint()
        {
            StringBuilder originString = new StringBuilder();
            string origin = player.Origin.Name;
            int length = origin.Length;
            int spaceRemaining = 10 - length;
            originString.Append(" " + origin + " ");
            for (int i = 0; i < spaceRemaining; i++)
            {
                originString.Append("─");
            }

            return originString.ToString();
        }

        string WeaponPrint()
        {
            StringBuilder weaponString = new StringBuilder();
            string weapon = player.EquippedWeapon.Name;
            int length = weapon.Length;
            int spaceRemaining = 19 - length;
            weaponString.Append(weapon);
            for (int i = 0; i < spaceRemaining; i++)
            {
                weaponString.Append(" ");
            }

            return weaponString.ToString();
        }

        string ArmorPrint()
        {
            StringBuilder armorString = new StringBuilder();
            string armor = player.PlayerArmor.Name;
            int length = armor.Length;
            int spaceRemaining = 20 - length;
            armorString.Append(armor);
            for (int i = 0; i < spaceRemaining; i++)
            {
                armorString.Append(" ");
            }

            return armorString.ToString();
        }

        string PrintAffliction(Affliction affliction)
        {
            StringBuilder afflictionString = new StringBuilder();
            int spaceRemaining = 18 - (affliction.Name.Length + affliction.Severity);
            afflictionString.Append(" ");
            if (affliction.Severity == 0)
            {
                return "                    ";
            }
            else
            {
                afflictionString.Append(affliction.Name);
                afflictionString.Append(" ");
                for (int i = 0; i < affliction.Severity; i++)
                {
                    afflictionString.Append("I");
                }
                for (int i = 0; i < spaceRemaining; i++)
                {
                    afflictionString.Append(" ");
                }
                return afflictionString.ToString();
            }
        }

        string PrintStructure(Structure structure)
        {
            StringBuilder structureString = new StringBuilder();
            string quality;
            if (structure.Built)
            {
                if (structure.Quality == 2)
                {
                    quality = "[+]";
                }
                else if (structure.Quality == 1)
                {
                    quality = "[-]";
                }
                else
                {
                    quality = "[ ]";
                }

                int spaceRemaining = 28 - (structure.Name.Length + quality.Length);

                structureString.Append(quality);
                structureString.Append(" ");
                structureString.Append(structure.Name);

                for (int i = 0; i < spaceRemaining; i++)
                {
                    structureString.Append(" ");
                }
            }
            else
            {
                structureString.Append("                             ");
            }

            return structureString.ToString();
        }

        string PrintFood(Camp camp)
        {
            if (camp.Food >= 10)
            {
                return $"{camp.Food}";
            }
            else return $" {camp.Food}";
        }

        string PrintWater(Camp camp)
        {
            if (camp.Water >= 10)
            {
                return $"{camp.Water}";
            }
            else return $" {camp.Water}";
        }

        string PrintInventory(Item inventory)
        {
            StringBuilder inventoryString = new StringBuilder();
            string dim = "\u001b[2m";
            string reset = "\u001b[0m";
            int spaceRemaining;

            if (inventory == null || inventory.Name == null)
            {
                return $" * {dim}Empty{reset}                 ";
            }
            else
            {
                if (Math.Abs(inventory.Count).ToString().Length > 1)
                {
                    spaceRemaining = 19 - inventory.Name.Length;
                }
                else
                {
                    spaceRemaining = 20 - inventory.Name.Length;
                }

                inventoryString.Append(" ");
                inventoryString.Append(inventory.Count);
                inventoryString.Append(" - ");
                inventoryString.Append(inventory.Name);

                for (int i = 0; i < spaceRemaining; i++)
                {
                    inventoryString.Append(" ");
                }
                return inventoryString.ToString();
            }
        }

        string PrintGameDays(int days)
        {
            if (days >= 10)
            {
                return $" {days}";
            }
            else return $" {days} ";
        }
    }

}

public static class Narrative
{
    public static string[] GetCommand()
    {
        Print($"\n\nWhat would you like to do?");
        Inform(" [Verb + Noun] ");
        string input = Console.ReadLine();

        string[] inputParts = input.Split(' ');
        if (inputParts.Length != 2)
        {
            Print("\nInvalid input. Please enter 1 verb and 1 noun.");
            return null;
        }

        return inputParts;
    }
    public static void LaunchVerbNarrative(string[] inputParts, Character player, Game game, Camp camp)
    {
        if (inputParts != null)
        {
            switch (inputParts[0].ToLower())
            {
                case "explore":
                case "investigate":
                case "search":
                case "scout":
                case "navigate":
                case "wander":
                case "travel":
                case "move":
                case "hike":
                case "trek":
                case "roam":
                    Explore(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "forage":
                case "scavange":
                case "gather":
                case "collect":
                case "harvest":
                case "obtain":
                case "find":
                case "discover":
                    Gather(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "hunt":
                    Hunt(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "trap":
                    Trap(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "fish":
                    Fish(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "build":
                case "craft":
                case "construct":
                case "create":
                case "make":
                case "assemble":
                    Craft(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "drop":
                case "deposit":
                    Supply(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "cook":
                case "prepare":
                case "bake":
                case "roast":
                case "grill":
                case "fry":
                case "boil":
                case "stew":
                    Cook(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "heal":
                case "mend":
                case "treat":
                case "bandage":
                    Heal(inputParts[1].ToLower(), player, game, camp);
                    break;
                case "rest":
                case "relax":
                case "unwind":
                case "recline":
                case "nap":
                case "sleep":
                case "breathe":
                case "reflect":
                    Rest(inputParts[1].ToLower(), player, game, camp);
                    break;
                default: Print("Unrecognised command."); break;
            }
        }
    }
    public static void Explore(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to explore, specifically {noun}."); Console.ReadKey(true);
    }
    public static void Gather(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to gather, specifically {noun}."); Console.ReadKey(true);
    }
    public static void Hunt(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to hunt, specifically {noun}."); Console.ReadKey(true);
    }
    public static void Trap(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to trap, specifically {noun}."); Console.ReadKey(true);
    }
    public static void Fish(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to fish, specifically {noun}."); Console.ReadKey(true);
    }
    public static void Craft(string noun, Character player, Game game, Camp camp)
    {

        game.MainScreen(player, camp);

        Print($"\nYou set out to craft a {noun}."); Console.ReadKey(true);
        switch (noun)
        {
            case "fire":

                int modifier = 0;
                Print("\n\nLooking about the beach there is plenty of wood from the wreckage strewn about.\n\n"); Console.ReadKey(true);

                Result result1 = GameRoll(player, Skill.Presence, 6, 0);

                if (result1 == Result.CritSuccess)
                {
                    Print("\n\nCRIT SUCCESS, you've found an excellent assortment of dried wood and tinder.");
                    Inform("\n\nThanks to your crit success, the dried wood will assist in starting a fire."); Console.ReadKey(true);

                    modifier = 6;

                }
                else if (result1 == Result.Success)
                {
                    Print("\n\nSUCCESS, you've found some adequate wood lying about.");
                    Inform("\n\nThanks to your success, the wood will assist in starting a fire."); Console.ReadKey(true);

                    modifier = 3;

                    game.MainScreen(player, camp);
                }
                else
                {
                    Print("\n\nFAILURE\n\nYou're search finds little but sea soaked logs on the beach, nothing dry."); Console.ReadKey(true);
                    modifier = -4;
                }

                Print("\n\nYou gather what wood you could find and crouch down at your campsite.\n\n");
                
                Result result2 = GameRoll(player, Skill.Presence, 10, modifier);

                if (result2 == Result.CritSuccess)
                {
                    Print("\n\nCRIT SUCCESS, with little work the fire in front of you springs to life.");
                    Print("\n\nYou pile some stones around the fire to make it a a more permanent fixture of your camp."); Console.ReadKey(true);

                    camp.Structures[0].Quality = 2;
                    camp.Structures[0].Built = true;

                }
                else if (result2 == Result.Success)
                {
                    Print("\n\nSUCCESS, with some effort a fire sputters to life in the small space in front of you.");
                    Print("\n\nYou pile some stones around the fire to make it a a more permanent fixture of your camp."); Console.ReadKey(true);

                    camp.Structures[0].Quality = 1;
                    camp.Structures[0].Built = true;
                }
                else
                {
                    Print("\n\nFAILURE\n\nThe fire sputters and your efforts fail to ignite the wood you have collected."); Console.ReadKey(true);
                }

                if (!player.Inventory.Any(item => item != null && item.Name == "Tinderbox"))
                {
                    Print("\n\nWithout tools, working on this fire was hard work");
                    TestFatigue(player, game, camp);
                }
                game.Time++;

                break;
        }
    }
    public static void Supply(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to supply, specifically {noun}."); Console.ReadKey(true);
    }
    public static void Cook(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to prepare food, specifically {noun}."); Console.ReadKey(true);
    }
    public static void Heal(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to heal, specifically {noun}."); Console.ReadKey(true);
    }
    public static void Rest(string noun, Character player, Game game, Camp camp)
    {
        Print($"\nI've detected you want to rest, specifically {noun}."); Console.ReadKey(true);
    }

    public static void Awakening(Character player, Game game, Camp camp)
    {
        DramaPrint("\nYou wash ashore...");
        Thread.Sleep(1000);
        Inform("\n\nPress any key to continue whenever text pauses..."); Console.ReadKey(true);
        Print("\n\nThe crashing waves lap at your feet."); Console.ReadKey(true);
        Print("\n\nSalt and sand encrust your clothes and the sun beats down on your back."); Console.ReadKey(true);
        Print("\n\nYour eyes strain against the sun, you lie on the golden sand of a large island ..."); Console.ReadKey(true);
        Print("\n\nSomewhere..."); Console.ReadKey(true);

        game.MainScreen(player, camp);

        player.GenOrigin();

        Print("\nYour head throbs. You cough and splutter sea water as you're faculties return."); Console.ReadKey(true);
        Print("\n\nYou try to recall something of your past... you were once a ... [D10 Origin] "); SimulateRoll(); Print($"{player.Origin.Name}"); Console.ReadKey(true);
        Print($"\n\n{player.Origin.Description}"); Console.ReadKey(true);
        Print($"\n\n{player.Origin.Trait}"); Console.ReadKey(true);
        Inform($"\n\nYour origin gives your character some flavour, the trait will popup in special moments of benefit."); Console.ReadKey(true);

        game.MainScreen(player, camp);

        player.GenAbilityScores();

        Print("\nYou are alone here, and have only yourself to rely on. Hopefully your 'self' measures up ... [CORE STATS]"); Console.ReadKey(true);
        Inform($"\n\nDefend ... Balance ... Swim ... Flee");
        Print("\nAgility:   "); SimulateRoll(); Print($"{player.AbilityRolls[0]} - "); SimulateRoll(); Print($"{player.AbilityRolls[1]} = "); Print((player.Agility < 0 ? "" : "+") + $"{player.Agility} - {player.AgilityDescription}"); Console.ReadKey(true);
        Inform($"\n\nPerceive ... Aim ... Charm ... Explore");
        Print("\nPresence:  "); SimulateRoll(); Print($"{player.AbilityRolls[2]} - "); SimulateRoll(); Print($"{player.AbilityRolls[3]} = "); Print((player.Presence < 0 ? "" : "+") + $"{player.Presence} - {player.PresenceDescription}"); Console.ReadKey(true);
        Inform($"\n\nCrush ... Lift ... Strike ... Grapple");
        Print("\nStrength:  "); SimulateRoll(); Print($"{player.AbilityRolls[4]} - "); SimulateRoll(); Print($"{player.AbilityRolls[5]} = "); Print((player.Strength < 0 ? "" : "+") + $"{player.Strength} - {player.StrengthDescription}"); Console.ReadKey(true);
        Inform($"\n\nResist Elements ... Resist Afflictions ...");
        Print("\nToughness: "); SimulateRoll(); Print($"{player.AbilityRolls[6]} - "); SimulateRoll(); Print($"{player.AbilityRolls[7]} = "); Print((player.Toughness < 0 ? "" : "+") + $"{player.Toughness} - {player.ToughnessDescription}"); Console.ReadKey(true);

        game.MainScreen(player, camp);

        player.GenFatigue();

        Print("\nAs you get to your feet, the fatigue hits you... [D6 - TOU = FATIGUE]"); Console.ReadKey(true);
        Print("\n\nFatigue:"); for (int i = 0; i < player.Fatigue; i++) { DramaPrint(" \u2588 "); } Print($"[{player.Fatigue}]"); Console.ReadKey(true);
        Inform($"\n\nFatigue is checked when doing hard things, the worse your fatigue, the harder hard things are ....");
        Inform($"\n\nYour toughness [{player.Toughness}] impacted your fatigue roll [{player.AbilityRolls[10]}]");

        Console.ReadKey(true);

        game.MainScreen(player, camp);

        player.GenHealth();

        Print("\nAs you take your first step something stings... You check for injuries: [D8 + TOU = HEALTH]"); Console.ReadKey(true);
        Print("\n\nHealth:    "); SimulateRoll(); Print($"{player.AbilityRolls[9]} + {player.Toughness} = {player.Health} | "); Console.ReadKey(true);
        string healthDesc = (player.Health >= 3 ? (player.Health >= 7 ? "Only some minor scrapes" : "Some worrying bruises...") : "You're barely standing."); Print(healthDesc); Console.ReadKey(true);
        Inform($"\n\nMax health is 10, this is the state of your body after landing on the island.\nLose all your health and you become broken."); Console.ReadKey(true);

        game.MainScreen(player, camp);

        player.GenArmor();

        Print("\nYou pat yourself down... and realise thankfully you're still wearing your outfit ...[D4 ARMOR]"); Console.ReadKey(true);
        Print("\n\nArmor:     "); SimulateRoll(); Print($"{player.PlayerArmor.Tier} | {player.PlayerArmor.Name} - {player.PlayerArmor.Description}"); Console.ReadKey(true);
        Inform($"\n\nArmor directly reduces damage dependant on it's tier [0 - 3].\nBut higher tier armor wears you out and makes it harder to move."); Console.ReadKey(true);
        Print($"\n\n[Extra Fatigue From Armor] = {player.PlayerArmor.Tier}");
        player.Fatigue = player.Fatigue + player.PlayerArmor.Tier; Console.ReadKey(true);
    }

    public static void BeachWreckage(Character player, Game game, Camp camp)
    {
        game.MainScreen(player, camp);

        Print("\nBefore you, is what remains of the wreck of your vessel scattered in the sand.");
        Print("\n\nYou have only what you can find on the beach in front of you and the waterlogged clothes on your back.");

        player.GenWeapon();

        Print($"\n\nYou see a weapon lying on the ground ... [D10 Random Weapon] "); Console.ReadKey(true); 
        SimulateRoll(); Print($"{player.EquippedWeapon.Name}"); Console.ReadKey(true);
        Print($"\n\nYou pick up the {player.EquippedWeapon.Name}, and hope you wont have to use it."); Console.ReadKey(true);

        game.MainScreen(player, camp);

        Print("\nYou decide to search the wreckage of your former vessel for more items.\n\nThere is wreckage strewn about the beach.\n\n");

        Result result = GameRoll(player, Skill.Presence, 6, 0);

        if (result == Result.CritSuccess)
        {
            Print("SUCCESS, you've found several things within the sand.\n\n");
            Item wreckItem1 = WreckageLoot(Roll(3));
            Item wreckItem2 = WreckageLoot(Roll(3));

            Print($"You find a {wreckItem1.Name} lying under some debris.\n\n");

            player.AddToInventory(wreckItem1);

            game.MainScreen(player, camp);

            Print($"You also find a {wreckItem2.Name} buried in the sand.\n\n");

            player.AddToInventory(wreckItem2);

            game.MainScreen(player, camp);
        }
        else if (result == Result.Success)
        {
            Print("SUCCESS, you've found something in the sand.\n\n");
            Item wreckItem1 = WreckageLoot(Roll(3));

            Print($"You find a {wreckItem1.Name} lying under some debris.\n\n");

            player.AddToInventory(wreckItem1);

            game.MainScreen(player, camp);
        }
        else
        {
            Print("FAILURE\n\nYou search the wreckage but find nothing of use."); Console.ReadKey(true);
        }
    }

    public static void TakingStock(Character player, Game game, Camp camp)
    {
        game.MainScreen(player, camp);

        Print("\nYou trudge up the beach, ringing out your water logged clothes."); Console.ReadKey(true);
        Print("\n\nYou are standing on the edge of a vast tropical jungle that covers most of the island."); Console.ReadKey(true);
        Print("\n\nBehind you, into the water, is a coral reef."); Console.ReadKey(true);
        Inform("\n\nYou are going to need food, water, and shelter to survive."); Console.ReadKey(true);
        Inform("\n\nThis game works by letting you input a verb and a noun to direct your character."); Console.ReadKey(true);
        Inform("\n\nWhile at camp you could say 'cook food' or 'explore jungle' to start actions."); Console.ReadKey(true);
        Inform("\n\nExperiment with combinations, and do your best to survive."); Console.ReadKey(true);
    }

    public static void AtCamp(Character player, Game game, Camp camp)
    {
        game.MainScreen(player, camp);

        Print($"\nIt's {game.Time}.");
        Print($"\n\nYou are standing at your camp.\n\n{GetWeatherDesc(game)}");
    }

    public static Item WreckageLoot(int table)
    {

        Item sand = new Item("Some Sand", "Sand is all you seem to find.... there's lots of it.", 1);
        
        Container sling = new Container("Sling", "A modest sling satchel you can use to carry items.", 1, 6);
        Container sack = new Container("Potato Sack", "Once held potatoes, but now it can hold your stuff!", 1, 8);
        Container trunk = new Container("Trunk", "A travel trunk that can be strapped to your back.", 1, 12);
        Container chest = new Container("Backpack", "A solid backpack to keep your items safe.", 1, 15);

        Item rope = new Item("Rope", "30ft of Rope", 30);
        Item cloth = new Item("A Sail Cloth", "Recovered sails from a wreckage.", 1);
        Consumable rum = new Consumable("Rum Cask", "Could sterilize infection.", 1, false, true);
        Item gunpowder = new Item("Gunpowder Keg", "A gunpowder keg, could do some damage.", 1);
        Consumable limes = new Consumable("Limes", "A case of 12 limes, will stave off scurvy.", Roll(12), true, false);
        Consumable pork = new Consumable("Salted Pork", "Delicious, and lasts a while.", Roll(4), true, false);
        Consumable lard = new Consumable("Sack of Lard", "A sack of lard, edible in a pinch.", Roll(3), true, false);
        Consumable water = new Consumable("Water Barrel", "Clean drinking water. You'll need this.", Roll(8) + Roll(8), false, true);
        Item wood = new Item("Wooden Plans", "Sturdy wooden planks, pulled from the wreckage.", Roll(12));
        Item show = new Item("Lead Shot", "Little spheres of lead, deadly at high velocity.", Roll(6) + Roll(6));
        Item sewing = new Item("Sewing Kit", "A kit of needle and thread.", Roll(4) + Roll(4));
        Item anitvenom = new Item("Anti-Venom", "Bottle of Anti-vemon", Roll(4));

        Consumable cigar = new Consumable("Cigars", "A case of cigars, great cure for cabin fever.", Roll(6), false, false);
        Item surgeon = new Item("Surgeons Kit", "Can help you recover from infection.", Roll(4));
        Item tinder = new Item("Tinder Box", "Making a fire just got a lot easier.", 1);
        Item knife = new Item("Pocket Knife", "Can help with small crafts.", 1);
        Item net = new Item("Fishing Net", "Maybe you can catch some fish with this?", 1);
        Item compass = new Item("Compass", "Navigation just got a lot easier..", 1);
        Item journal = new Item("Journalling Kit", "Maybe you can write down some of your thoughts?", 1);
        Item mallet = new Item("A mallet", "This could help with crafting.", 1);
        Item saw = new Item("A saw", "For ship repair, useful on an island too.", 1);
        Weapon axe = new Weapon("Axe", "What a find! Look out trees...", 1, 8, false, true);

        switch (table)
        {
            case 1:
                {
                    int roll = Roll(4);
                    switch (roll)
                    {
                        case 1: return sling;
                        case 2: return sack;
                        case 3: return trunk;
                        case 4: return chest;
                        default: return sling;
                    }
                }
            case 2:
                {
                    int roll = Roll(12);
                    switch (roll)
                    {
                        case 1: return rope;
                        case 2: return cloth;
                        case 3: return rum;
                        case 4: return gunpowder;
                        case 5: return limes;
                        case 6: return pork;
                        case 7: return lard;
                        case 8: return water;
                        case 9: return wood;
                        case 10: return show;
                        case 11: return sewing;
                        case 12: return anitvenom;
                        default: return rope;
                    }
                }
            case 3:
                {
                    int roll = Roll(10);
                    switch (roll)
                    {
                        case 1: return cigar;
                        case 2: return surgeon;
                        case 3: return tinder;
                        case 4: return knife;
                        case 5: return net;
                        case 6: return compass;
                        case 7: return journal;
                        case 8: return mallet;
                        case 9: return saw;
                        case 10: return axe;
                        default: return cigar;
                    }
                }
            default: return sling;
        }
    }

    public static string GetWeatherDesc(Game game)
    {
        switch (game.Weather)
        {
            case 1: return "A tropical storm, it's cold and miserable.";
            case 2: return "It's raining. ";
            case 3: return "A slight patter of rain, at least it's cool.";
            case 4: return "Rolling clouds overheard, providing some shade.";
            case 5: return "Skies are blue, there is a slight ocean breeze.";
            case 6: return "The sun is bright, the humidity is heavy.";
            case 7: return "The golden sand is a haze of heat, the sun beats down on you.";
            case 8: return "The sun is relentless, it will be dangerous in the open today.";
        }
        return "ERROR GETTING DESCRIPTION";
    }

    public static void Print(string input)
    {
        string green = "\u001b[1;32m";
        string white = "\u001b[0m";
        string red = "\u001b[31m";
        string bold = "\u001b[1m";

        input = input.Replace("Agility", $"{green}Agility{white}");
        input = input.Replace("Presence", $"{green}Presence{white}");
        input = input.Replace("Strength", $"{green}Strength{white}");
        input = input.Replace("Toughness", $"{green}Toughness{white}");
        input = input.Replace("Power", $"{green}Power{white}");
        input = input.Replace("Omens", $"{green}Omens{white}");
        input = input.Replace("Armor", $"{green}Armor{white}");
        input = input.Replace("Health", $"{green}Health{white}");

        input = input.Replace("Affliction", $"{red}Affliction{white}");
        input = input.Replace("Fatigue", $"{red}Fatigue{white}");
        input = input.Replace("Cabin Fever", $"{red}Cabin Fever{white}");

        input = input.Replace("Weather", $"{bold}Weather{white}");
        input = input.Replace("Navigate", $"{bold}Navigate{white}");
        input = input.Replace("Analyze", $"{bold}Analyze{white}");
        input = input.Replace("Shelter", $"{bold}Shelter{white}");
        input = input.Replace("Escape Plan", $"{bold}Escape Plan{white}");
        input = input.Replace("Craft", $"{bold}Craft{white}");

        input = input.Replace("Mortician", $"{bold}Mortician{white}");
        input = input.Replace("Soldier", $"{bold}Soldier{white}");
        input = input.Replace("Sawbones", $"{bold}Sawbones{white}");
        input = input.Replace("Smuggler", $"{bold}Smuggler{white}");
        input = input.Replace("Starving Artist", $"{bold}Starving Artist{white}");
        input = input.Replace("Explorer", $"{bold}Explorer{white}");
        input = input.Replace("Stowaway", $"{bold}Stowaway{white}");
        input = input.Replace("Prisoner", $"{bold}Prisoner{white}");

        input = input.Replace("Search", $"{bold}Search{white}");

        char[] characters = input.ToCharArray();
        int stringLength = input.Length;
        Random rnd = new Random();

        for (int i = 0; i < stringLength; i++)
        {
            int ranSleep = rnd.Next(15, 60);
            Thread.Sleep(ranSleep);
            Console.Write($"{characters[i]}");
        }
    }

    public static void DramaPrint(string input)
    {
        string green = "\u001b[1;32m";
        string white = "\u001b[0m";
        string red = "\u001b[31m";
        string bold = "\u001b[1m";

        char[] characters = input.ToCharArray();
        int stringLength = input.Length;
        Random rnd = new Random();

        for (int i = 0; i < stringLength; i++)
        {
            int ranSleep = rnd.Next(100, 500);
            Thread.Sleep(ranSleep);
            Console.Write($"{characters[i]}");
        }
    }

    public static void Inform(string input)
    {
        string dim = "\u001b[2m";
        string reset = "\u001b[0m";

        char[] characters = input.ToCharArray();
        int stringLength = input.Length;
        Random rnd = new Random();

        for (int i = 0; i < stringLength; i++)
        {
            int ranSleep = rnd.Next(15, 60);
            Thread.Sleep(ranSleep);
            Console.Write($"{dim}{characters[i]}{reset}");
        }
    }

    private static void SimulateRoll()
    {
        Random rnd = new Random();

        for (int i = 0; i < 50; i++)
        {
            int ranSleep = rnd.Next(10, 40);
            Console.Write(RandomChar(rnd));
            Thread.Sleep(ranSleep);
            Console.Write("\b");
        }

        static char RandomChar(Random rnd)
        {
            int ranChar = rnd.Next(97, 123);
            return (char)ranChar;
        }
    }

    private static int Roll(int roll)
    {
        Random random = new Random();
        return random.Next(1, (roll + 1));
    }

    private static void TestFatigue(Character player, Game game, Camp camp)
    {
        Print("\n\nYour fatigue is being tested with this task.");
        int roll = Roll(10);
        if (roll >= player.Fatigue)
        {
            Print("\n\nThe sweat drips from your brow, but you still feel strong.");
        }
        else
        {
            Print("\n\nYou rise from your task, tired."); Inform("Your fatigue has increased.");
            player.Fatigue++;
        }
    }

    public static Result GameRoll(Character character, Skill skill, int difficulty, int modifier)
    {
        switch (skill)
        {
            case Skill.Agility:
            {
                    Print($"[Agility Check DR {difficulty}] | ");
                    Console.ReadKey(true);
                    SimulateRoll();
                    int roll = Roll(20);
                    int result = roll + character.Agility + modifier;
                    Print($"{roll} + {character.Agility} + {modifier} = {result} vs {difficulty} ");
                    if (roll == 20) { return Result.CritSuccess; }
                    else if (roll == 1) { return Result.CritFail; }
                    else if (roll >= difficulty) { return Result.Success; }
                    else if (roll < difficulty) { return Result.Fail; }
                    else { Console.WriteLine(" Something went wrong..."); return Result.CritFail; }
            }
            case Skill.Presence:
                {
                    Print($"[Presence Check DR {difficulty}] | ");
                    Console.ReadKey(true);
                    SimulateRoll();
                    int roll = Roll(20);
                    int result = roll + character.Presence + modifier;
                    Print($"{roll} + {character.Presence} + {modifier} = {result} vs {difficulty} ");
                    if (roll == 20) { return Result.CritSuccess; }
                    else if (roll == 1) { return Result.CritFail; }
                    else if (roll >= difficulty) { return Result.Success; }
                    else if (roll < difficulty) { return Result.Fail; }
                    else { Console.WriteLine(" Something went wrong..."); return Result.CritFail; }
                }
            case Skill.Strength:
                {
                    Print($"[Strength Check DR {difficulty}] | ");
                    Console.ReadKey(true);
                    SimulateRoll();
                    int roll = Roll(20);
                    int result = roll + character.Strength + modifier;
                    Print($"{roll} + {character.Strength} + {modifier} = {result} vs {difficulty} ");
                    if (roll == 20) { return Result.CritSuccess; }
                    else if (roll == 1) { return Result.CritFail; }
                    else if (roll >= difficulty) { return Result.Success; }
                    else if (roll < difficulty) { return Result.Fail; }
                    else { Console.WriteLine(" Something went wrong..."); return Result.CritFail; }
                }
            case Skill.Toughness:
                {
                    Print($"[Toughness Check DR {difficulty}] | ");
                    Console.ReadKey(true);
                    SimulateRoll();
                    int roll = Roll(20);
                    int result = roll + character.Toughness + modifier;
                    Print($"{roll} + {character.Toughness} + {modifier} = {result} vs {difficulty} ");
                    if (roll == 20) { return Result.CritSuccess; }
                    else if (roll == 1) { return Result.CritFail; }
                    else if (roll >= difficulty) { return Result.Success; }
                    else if (roll < difficulty) { return Result.Fail; }
                    else { Console.WriteLine(" Something went wrong..."); return Result.CritFail; }
                }
            default: return Result.CritFail;
        }
    }
}

public class Character
{
    public int[]? AbilityRolls { get; set; }
    public int Agility { get; set; }
    public string AgilityDescription { get; set; }
    public int Presence { get; set; }
    public string PresenceDescription { get; set; }
    public int Strength { get; set; }
    public string StrengthDescription { get; set; }
    public int Toughness { get; set; }
    public string ToughnessDescription { get; set; }
    public PlayerOrigin Origin { get; set; }
    public Armor PlayerArmor { get; set; }
    public Item[] Inventory { get; set; }
    public int Health { get; set; }
    public int Fatigue { get; set; }
    public Weapon EquippedWeapon { get; set; }
    public Affliction[] Afflictions { get; set; }
    public bool IsAlive {  get; set; }

    public Character()
    {
        Agility = 0;
        Presence = 0;
        Strength = 0;
        Toughness = 0;

        Fatigue = 0;
        Health = 0;

        IsAlive = true;

        AgilityDescription = string.Empty;
        PresenceDescription = string.Empty;
        StrengthDescription = string.Empty;
        ToughnessDescription = string.Empty;

        Origin = new PlayerOrigin("None","none","none");

        PlayerArmor = new Armor("None", "none", 0, 0);

        GenInventory();

        GenAfflictions();

        Fatigue = 0;

        EquippedWeapon = new Weapon("Fists", "Your trusty hands, curled into a fist.", 1, 2, false, false);
    }

    public void GenerateRolls()
    {
        AbilityRolls = new int[11];
        Random random = new Random();

        AbilityRolls[0] = Roll(4); // Rolls for the 4 primary stats.
        AbilityRolls[1] = Roll(4);
        AbilityRolls[2] = Roll(4);
        AbilityRolls[3] = Roll(4);
        AbilityRolls[4] = Roll(4);
        AbilityRolls[5] = Roll(4);
        AbilityRolls[6] = Roll(4);
        AbilityRolls[7] = Roll(4);

        AbilityRolls[8] = Roll(2); // Roll for Omens.

        AbilityRolls[9] = Roll(8); // Roll for Health.

        AbilityRolls[10] = Roll(6); // Roll for Fatigue.
    }

    public void GenOrigin()
    {
        Origin = PlayerOrigin.RandomOrigin();
    }

    public void GenAbilityScores()
    {
        Agility = AbilityRolls[0] - AbilityRolls[1];
        Presence = AbilityRolls[2] - AbilityRolls[3];
        Strength = AbilityRolls[4] - AbilityRolls[5];
        Toughness = AbilityRolls[6] - AbilityRolls[7];

        AgilityDescription = GetAgilityDesc(Agility);
        PresenceDescription = GetPresenceDesc(Presence);
        StrengthDescription = GetStrengthDesc(Strength);
        ToughnessDescription = GetToughDesc(Toughness);
    }

    public void GenArmor()
    {
        PlayerArmor = Armor.RandomArmor();
    }

    public void GenWeapon()
    {
        EquippedWeapon = Weapon.RandomWeapon();
    }

    public void GenHealth()
    {
        Health = AbilityRolls[9] + Toughness;
        if (Health <= 0) { Health = 1; }
        if (Health > 10) { Health = 10; }
    }

    public void GenFatigue()
    {
        Fatigue = AbilityRolls[10] - Toughness;
        if (Fatigue <= 0) { Fatigue = 1; }
        if (Fatigue > 6) { Fatigue = 6; }
    }

    public void GenInventory()
    {
        int inventorySize = 8;
        Inventory = new Item[inventorySize];
    }

    public void GenAfflictions()
    {
        Afflictions = new Affliction[4];
        Afflictions[0] = new Affliction("Sunburn");
        Afflictions[1] = new Affliction("Dehydration");
        Afflictions[2] = new Affliction("Starvation");
        Afflictions[3] = new Affliction("Infection");
    }

    private int Roll(int roll)
    {
        Random random = new Random();
        return random.Next(1, (roll + 1));
    }

    public void AddToInventory(Item item)
    {
        Console.Write($"Pickup {item.Name}? [y] [n] ");
        string input = Console.ReadLine();
        if (input.ToLower() == "y")
        {
            int filledCount = Inventory.Count(slot => slot != null);
            int totalCount = Inventory.Length;

            if (item is Weapon)
            {
                Console.Write($"\n\nWould you also like to equip {item.Name}?");
                input = Console.ReadLine();
                if (input.ToLower() == "y")
                {
                    EquippedWeapon = (Weapon)item;
                    Console.Write($"\n\nYou are now wielding {item.Name}");
                }
            }
            else
            {
                if (filledCount < totalCount)
                {
                    for (int i = 0; i < Inventory.Length; i++)
                    {
                        if (Inventory[i] == null)
                        {
                            Inventory[i] = item;
                            Console.Write($"\nPicked up {item.Name}. Inventory: {filledCount + 1} / {totalCount}.\n\n");
                            break;
                        }
                    }
                }
                else
                {
                    Console.Write($"\nNo room for {item.Name} in the inventory (Inventory full).\n\n");
                }
            }
        }
        else
        {
            Console.Write($"\nLeaving {item.Name} behind.\n\n");
        }
    }

    public void AddAffliction(Affliction affliction)
    {
        affliction.Severity += 1;
        affliction.Has = true;
        Console.Write($"You are now suffering from {affliction.Name}.\n\n");
        Console.Write(affliction.Severity);
    }

    static string GetAgilityDesc(int stat)
    {
        string description = "";

        switch (stat)
        {
            case -3:
                description = "You move as if your shoes are filled with lead.";
                break;
            case -2:
                description = "Your reactions are sluggish.";
                break;
            case -1:
                description = "Slower than most.";
                break;
            case 0:
                description = "Your neither fast or slow.";
                break;
            case 1:
                description = "You're quicker than most.";
                break;
            case 2:
                description = "You move with grace and speed.";
                break;
            case 3:
                description = "You're nimble, like a cat.";
                break;
            default:
                description = "Invalid agility value.";
                break;
        }

        return description;
    }

    static string GetPresenceDesc(int stat)
    {
        string description = "";

        switch (stat)
        {
            case -3:
                description = "You miss everything.";
                break;
            case -2:
                description = "You're rarely paying attention.";
                break;
            case -1:
                description = "Little things sometimes slip your notice.";
                break;
            case 0:
                description = "You catch some things, but miss other things.";
                break;
            case 1:
                description = "You pick up on small details.";
                break;
            case 2:
                description = "You rarely miss things.";
                break;
            case 3:
                description = "No detail escapes your attention.";
                break;
            default:
                description = "Invalid presence value.";
                break;
        }

        return description;
    }

    static string GetStrengthDesc(int stat)
    {
        string description = "";

        switch (stat)
        {
            case -3:
                description = "A soft breeze could blow you over.";
                break;
            case -2:
                description = "Skinny and weak.";
                break;
            case -1:
                description = "A tad smaller than most.";
                break;
            case 0:
                description = "Of average build.";
                break;
            case 1:
                description = "Fairly fit.";
                break;
            case 2:
                description = "Athletic and broad.";
                break;
            case 3:
                description = "Rippling and powerful.";
                break;
            default:
                description = "Invalid presence value.";
                break;
        }

        return description;
    }

    static string GetToughDesc(int stat)
    {
        string description = "";

        switch (stat)
        {
            case -3:
                description = "Always ill with something.";
                break;
            case -2:
                description = "Often unwell.";
                break;
            case -1:
                description = "A tad weaker than most.";
                break;
            case 0:
                description = "Can shake off the common cold.";
                break;
            case 1:
                description = "Of good consitution.";
                break;
            case 2:
                description = "Rarely ill.";
                break;
            case 3:
                description = "The pinnacle of health.";
                break;
            default:
                description = "Invalid presence value.";
                break;
        }

        return description;
    }

    public class PlayerOrigin
    {
        public string? Name { get; }
        public string? Description { get; }
        public string? Trait { get; }


        public PlayerOrigin() { }

        public PlayerOrigin(string name, string description, string trait)
        {
            Name = name;
            Description = description;
            Trait = trait;
        }

        public static PlayerOrigin RandomOrigin()
        {

            Random random = new Random();
            int roll = random.Next(1, 11);

            PlayerOrigin mortician = new PlayerOrigin("Mortician",
                @"You are comfortable with death, and are skilled stitching closed wounds.",
                @"When you heal at camp you roll twice and take the higher result.");

            PlayerOrigin soldier = new PlayerOrigin("Pirate",
                @"You've sailed the seven seas, looted and plundered to your black hearts content.",
                @"In battle you intimidate your foes, on the first round of combat you roll twice to hit.");

            PlayerOrigin sawbones = new PlayerOrigin("Sawbones",
                @"Some call you a quack, but really they just don't understand your art.",
                @"When attempting to heal at camp you may also attempt to heal an affliction.");

            PlayerOrigin smuggler = new PlayerOrigin("Smuggler",
                @"With deep pockets and a compulsion to let nothing go un-pilfered.",
                "You roll twice on the loot table when taking from the dead.");

            PlayerOrigin keeper = new PlayerOrigin("Lighthouse Keeper",
                @"Familiar with all forms of Weather watching, you can always see what’s on the horizon.",
                "You may re-roll the weather each morning, if you're unhappy with the first result.");

            PlayerOrigin artist = new PlayerOrigin("Starving Artist",
                @"Accustomed to a life of small means, you know how to stretch your rations.",
                @"When you craft or prepare food, it costs you 1 less ration than others.");

            PlayerOrigin explorer = new PlayerOrigin("Explorer",
                @"You are renowned for your many discoveries about far off lands.",
                @"Your first explore roll you get two rolls, and take the higher result.");

            PlayerOrigin stowaway = new PlayerOrigin("Stowaway",
                @"You don’t belong here. Not that you belong anywhere, really. Maybe this place isn't so bad?",
                @"Your positive outlook means you benefit from one comfort level higher than your Shelter’s current level.

But you get comfy easily, any escape plans have a higher DC than normal.");

            PlayerOrigin prisoner = new PlayerOrigin("Prisoner",
                @"You’ve been in and out of every jail imaginable. ",
                "Whenever you Craft or work on an Escape Plan you can roll twice and take the better number.");

            switch (roll)
            {
                case 1: return mortician;
                case 2: return soldier;
                case 3: return sawbones;
                case 4: return smuggler;
                case 5: return keeper;
                case 6: return artist;
                case 7: return explorer;
                case 8: return stowaway;
                case 9: return artist;
                case 10: return prisoner;
                default: return mortician;
            }
        }
    }

    public class Affliction
    {
        public string Name { get; set; }
        public int Severity { get; set; }
        public bool Has { get; set; }

        public Affliction(string name)
        {
            Name = name;
            Severity = 0;
            Has = false;
        }
    }

    public class Gen
    {

    }
}

public class Item
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Count { get; set; }

    public Item(string name, string description, int count)
    {
        Name = name;
        Description = description;
        Count = count;
    }
}

public class Armor : Item
{
    public int Tier { get; private set; }

    public Armor(string name, string description, int count, int tier) : base(name, description, count)
    {
        Tier = tier;
    }

    public static Armor RandomArmor()
    {

        Armor shirt = new Armor("Linen Shirt", "A bare shirt, it provides no protection from the sun.", 1 , 0);

        Armor frock = new Armor("Frock Coat", "A nice coat, that offers some protection from the elements.", 1, 1);
        Armor jacket = new Armor("Riding Jacket", "A scuffed riding jacket, modest protection from danger.", 1, 1);

        Armor hide = new Armor("Hide Jacket", "The skin of some unfortunate creature, good defense.", 1, 2);
        Armor gambeson = new Armor("Quilted Gambeson", "A military gambeson, though it's seen better years.", 1, 2);

        Armor cuirass = new Armor("Plate Cuirass", "It's amazing you didn't drown...", 1, 3);
        Armor scale = new Armor("Scaled Leather", "Interlocking scales hidden beneath leather, excellent defense.", 1, 3);

        Random random = new Random();
        int roll = random.Next(1, 5);

        switch (roll)
        {
            case 1: return shirt;
            case 2:
                int innerRoll = random.Next(1, 3);
                if (innerRoll == 1)
                {
                    return frock;
                }
                else return jacket;
            case 3:
                innerRoll = random.Next(1, 3);
                if (innerRoll == 1)
                {
                    return hide;
                }
                else return gambeson;
            case 4:
                innerRoll = random.Next(1, 3);
                if (innerRoll == 1)
                {
                    return cuirass;
                }
                else return scale;
            default: return shirt;
        }
    }
}

public class Weapon : Item
{
    public int Damage { get; set; }
    public bool Broken { get; set; }
    public bool isTool { get; set; }

    public Weapon(string name, string description, int count, int damage, bool broken, bool isTool) : base(name, description, count)
    {
        Damage = damage;
        Broken = broken;
    }

    public static Weapon RandomWeapon()
    {
        {
            Weapon coconut = new Weapon("Coconut", "Easy to throw, also easy to break.", 1, 4, false, false);
            Weapon knife = new Weapon("Knife", "Rusty, but will get the job done.", 1, 4, false, true);
            Weapon hammer = new Weapon("Deck Hammer", "Was probably once used for repairs.", 1, 4, false, true);
            Weapon cudgel = new Weapon("Cudgel", "Little more than a stick.", 1, 4, false, false);
            Weapon machete = new Weapon("Machete", "Useful in the thick jungle.", 1, 6, false, true);
            Weapon hatchet = new Weapon("Hatchet", "Lucky find, could fell some trees with this.", 1, 6, false, true);
            Weapon sling = new Weapon("Sling", "Easy to throw, also easy to break.", 1, 6, false, false);
            Weapon cutless = new Weapon("Cutlass", "Easy to throw, also easy to break.", 1, 4, false, false);
            Weapon pistol = new Weapon("Flintlock Pistol", "Easy to throw, also easy to break.", 1, 4, false, false);
            Weapon rifle = new Weapon("Flintlock Rifle", "Easy to throw, also easy to break.", 1, 4, false, false);

            Random random = new Random();
            int roll = random.Next(1, 11);

            switch (roll)
            {
                case 1: return coconut;
                case 2: return knife;
                case 3: return hammer;
                case 4: return cudgel;
                case 5: return machete;
                case 6: return hatchet;
                case 7: return sling;
                case 8: return cutless;
                case 9: return pistol;
                case 10: return rifle;
                default: return coconut;
            }
        }
    }
}

public class Container : Item
{
    public int Capacity { get; set; }

    public Container(string name, string description, int count, int capacity) : base(name, description, count)
    {
        Capacity = capacity;
    }
}

public class Consumable : Item 
{
    public bool Satiates { get; set; }
    public bool Quenches { get; set; }

    public Consumable(string name, string description, int count, bool satiates, bool quenches) : base(name, description, count)
    {
        Satiates = satiates;
        Quenches = quenches;
    }
}

public class Camp
{
    public int Comfort { get; set; }
    public Structure[] Structures { get; private set; }
    public int Food { get; set; }
    public int Water { get; set; }

    public Camp()
    {
        Comfort = 0;
        Structures = new Structure[8];
        Food = 0;
        Water = 0;
    }

    public void InitializeStructures()
    {
        Structures = new Structure[]
        {
            new Structure("Fire", "A nice fire, sure to ward away danger", 0),
            new Structure("Raised Platform", "Ignore invading insects", 0),
            new Structure("Walls or Fence", "Ignore predator attacks", 0),
            new Structure("Wards of Skulls and Bone", "Ignore stolen resources", 0),
            new Structure("Canopy", "Fire doesn’t die due to bad weather, provides shade", 0),
            new Structure("Storage", "Protects stored resources from theft and spoiling", 0),
            new Structure("Rain Catcher", "Provides 1d4 days of fresh water each night it rains", 0),
            new Structure("Hammock or Bedding", "", 0),
            new Structure("Decorations", "", 0)
        };
    }

    public void Build(Structure Structure, int quality)
    {
        Structure.Built = true;
        Structure.Quality = quality;
    }

    public void UpdateComfort()
    {
        Comfort = 0;
        foreach (Structure structure in Structures)
        {
            if (structure.Built)
            {
                Comfort++;
            }
        }
    }

    public class Supplies
    {
        public string Name {  get; set; }
        public int Count {  get; set; }

        public Supplies(string name, int count)
        {
            Name = name;
            Count = count;
        }

        public static void InitSupplies(Supplies[] supplies)
        {
            supplies[0] = new Supplies("Lumber", 0);
            supplies[1] = new Supplies("Cloth", 0);
        }

    }
}

public class Structure
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Built {  get; set; }
    public int Quality {  get; set; }

    public Structure(string name, string description, int quality)
    {
        Name = name;
        Description = description;
        Built = false;
        Quality = quality;
    }
}

public class Enemy
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Disposition Disposition {  get; set; }
    public int Health { get; set; }
    public int Morale { get; set; }
    public Weapon Weapon { get; set; }
    public int Rations { get; set; }
    public bool IsAlive {  get; set; }

    public Enemy(string name, string description, int health, int morale, Weapon weapon, int rations)
    {
        Name = name;
        Description = description;
        Health = health;
        Morale = morale;
        Weapon = weapon;
        Rations = rations;
        IsAlive = true;
    }
}

//public class EnemyEncounter
//{
//    public static void CheckDisposition(Enemy enemy)
//    {
//        Narrative.Print($"A {enemy.Name} is lurking in this area.");
//    }
    
//    public static void StartCombat(Character player, Enemy enemy, Game game, Camp camp)
//    {
//        game.MainScreen(player, camp);

//        Console.WriteLine($"A wild {enemy.Name} appears!");

//        while (player.IsAlive && enemy.IsAlive)
//        {
//            PlayerTurn(player, enemy);

//            if (!enemy.IsAlive)
//            {
//                Console.WriteLine($"You defeated the {enemy.Name}!");
//                break;
//            }

//            EnemyTurn(player, enemy);

//            if (!player.IsAlive)
//            {
//                Console.WriteLine($"Oh no! You have been defeated by the {enemy.Name}!");
//                break;
//            }
//        }
//    }

//    private static void PlayerTurn(Character player, Enemy enemy)
//    {
//        Console.WriteLine($"Your turn:");
//        // Implement player's actions, such as choosing an attack and dealing damage
//        int damageDealt = player.Attack();
//        enemy.TakeDamage(damageDealt);
//        Console.WriteLine($"You dealt {damageDealt} damage to the {enemy.Name}.");
//    }

//    private static void EnemyTurn(Character player, Enemy enemy)
//    {
//        Console.WriteLine($"{enemy.Name}'s turn:");
//        // Implement enemy's actions, such as choosing an attack and dealing damage
//        int damageDealt = enemy.Attack();
//        player.TakeDamage(damageDealt);
//        Console.WriteLine($"{enemy.Name} dealt {damageDealt} damage to {player.Name}.");
//    }
//}

public enum TimeOfDay
{
    Morning,
    Midday,
    Afternoon,
    Night,
}

public enum Result
{
    CritFail,
    Fail,
    Success,
    CritSuccess,
}

public enum Skill
{
    Agility,
    Presence,
    Strength,
    Toughness,
}

public enum Disposition
{
    Fight,
    Flight,
    Anger,
    Indifferent,
    Dubious,
    Friendly,
}