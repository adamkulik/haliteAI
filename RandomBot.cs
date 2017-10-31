using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
/// <summary>
/// Sample C# bot using the starter kit
/// </summary>
public class MyBot {

    /// <summary>
    /// Setting the name of the bot (be nice!)
    /// </summary>
    public const string RandomBotName = "Mkbewe";

    /// <summary>
    /// Entry point for bot execution in C#ß
    /// </summary>
    public static void Main (string[] args) {
        //while (!Debugger.IsAttached);

        // Intialize the game with the name of your bot
        var hlt = Halite.Initialize (RandomBotName);
        // Create the Game Map
        var map = new GameMap (hlt.Item2);
        // Set up logging
        Log.Setup ("RandomC#Bot" + hlt.Item1 + ".log", LogingLevel.User);        
        // Intialize a command queue to store all the commands per turn
        var commands = new List<String> ();
        double[,] planetDistances = InitAnalyze(map, hlt.Item1);
        double[] myDistances = MyAnalyze(map, hlt.Item1);
       // Random random = new Random();
        List<int> safePlanetIds = FindSafePlanets(planetDistances, myDistances, map.Players.Count -1);

        Dictionary <int,List<int>> planetsToShips = InitDict(map, safePlanetIds, hlt.Item1);

        int shipsDone = 1;
        int turnCount = 0;
        // Game loop
        var player = map.Players.FirstOrDefault(aplayer => aplayer.Id == hlt.Item1);
        while (true) {
            // Make sure commands are cleared prior to each turn
            commands.Clear ();
            // Update your map
            map.Update ();

            if (shipsDone < player.Ships.Count)
            {
                shipsDone += EarlyGameStrat(map, commands, hlt.Item1, planetsToShips, turnCount);
            }
            else
            {

                LateLateGameStrat(map, commands, hlt.Item1);
            }
            if(turnCount == 0)
            {
                safePlanetIds = FindSafePlanets(planetDistances, myDistances, map.Players.Count - 1);

                planetsToShips = InitDict(map, safePlanetIds, hlt.Item1);
            }
            
            // Get your player info
            //  var myplayer = map.Players.FirstOrDefault (player => player.Id == hlt.Item1);
            // Now do the following for each ship that is owned by you


            // You are now done with the commands to your ships Fleet Admiral, 
            // lets get our battle computers to execute your commands
            turnCount++;
            Halite.SendCommandQueue (commands);
        }
    }

    public static double[,] InitAnalyze(GameMap map, int id)
    {
        map.Update();
        double[,] returnedDouble = new double[map.Players.Count - 1, map.Planets.Count];
        //int lengthTrace;

      

            for (int k = 0; k < map.Players.Count - 1; k++)
            {
                for (int j = 0; j < map.Planets.Count; j++)
                {
                    returnedDouble[k, j] = 999999.0;
                }
            }

            // var myplayer = map.Players.FirstOrDefault(player => player.Id == id);
            var otherplayers = map.Players.FindAll(players => players.Id != id);
            int i = 0;

            foreach (var player in otherplayers)
            {
                foreach (var ship in player.Ships)
                {
                    int j = 0;
                    foreach (var planet in map.Planets)
                    {
                        var point = ship.GetClosestPointToEntity(planet);
                        double a = ship.GetDistance(point);
                        double score = a / Constants.MaxSpeed;
                        if (score < returnedDouble[i, j]) returnedDouble[i, j] = score;
                        j++;


                    }
                }
                i++;

            }
        
     /*   catch(OverflowException e)
        {
           // Console.Error.WriteLine(otherplayers.Length);
          //  Console.Error.WriteLine(map.Planets.Length);
            Console.Error.WriteLine(e.StackTrace);
        }*/

            return returnedDouble;


    }
    public static double[] MyAnalyze(GameMap map, int id)
    {
        double[] returnedDouble = new double[map.Planets.Count];
        var myplayer = map.Players.FirstOrDefault(player => player.Id == id);
     
        foreach (var ship in myplayer.Ships)
        {
            int i = 0;
            foreach(var planet in map.Planets)
            {
                var point = ship.GetClosestPointToEntity(planet);
                double a = ship.GetDistance(point);
                double score = a / Constants.MaxSpeed;
                if (score < returnedDouble[i]) returnedDouble[i] = score;
                i++;
            }
        }
        return returnedDouble;

    }
    public static List<int> FindSafePlanets(double[,] enemyDistances, double[] myDistances, int enemyCount)
    {
        List<int> returnedInts = new List<int>();
        int[] checkTable = new int[myDistances.Length];
        for(int i = 0; i<enemyCount; i++)
        {
            for(int j = 0; j<myDistances.Length; j++)
            {
                if (enemyDistances[i, j] > myDistances[j])
                    checkTable[j]++;
            }
        }
        for(int i = 0; i< myDistances.Length; i++)
        {
            if (checkTable[i] == enemyCount) returnedInts.Add(i);
        }

        return returnedInts;
    }
    public static Dictionary <int, List<int>> InitDict(GameMap map, List<int> safePlanetIds, int id)
    {
        Dictionary<int, List<int>> returnedDict = new Dictionary<int, List<int>>();
        var myplayer = map.Players.FirstOrDefault(player => player.Id == id);
        var ships = myplayer.Ships;
        for (int i = 0; i < safePlanetIds.Count; i++)
        {
            returnedDict.Add(safePlanetIds.ElementAt(i), new List<int>());
        }
        if (ships.Count > safePlanetIds.Count)
        {
            for(int i = 0; i <safePlanetIds.Count; i++)
            {
                safePlanetIds.Sort(delegate (int a, int b)
                {
                    //var myplayer = map.Players.FirstOrDefault(aplayer => aplayer.Id == id);
                    var ship = myplayer.Ships.ElementAt(i);
                    var planetA = map.Planets.ElementAt(a);
                    var planetB = map.Planets.ElementAt(b);
                    var pointA = ship.GetClosestPointToEntity(planetA);
                    var pointB = ship.GetClosestPointToEntity(planetB);
                    double compA = ship.GetDistance(pointA);
                    double compB = ship.GetDistance(pointB);
                    if (compA > compB) return 1;
                    else if (compA == compB) return 0;
                    else return -1;

                });
                returnedDict[safePlanetIds.ElementAt(i)].Add(ships.ElementAt(i).EntityInfo.Id);
            }
            int diff = ships.Count - safePlanetIds.Count;
            safePlanetIds.Sort(delegate (int a, int b)
            {
                var ship = myplayer.Ships.ElementAt(0);
                var planetA = map.Planets.ElementAt(a);
                var planetB = map.Planets.ElementAt(b);
                double compA = planetA.Radius;
                double compB = planetB.Radius;
                if (compA > compB) return -1;
                else if (compA == compB) return 0;
                else return 1;

            });
            for(int i = diff; i > 0; i--)
            {
                returnedDict[safePlanetIds.ElementAt(i)].Add(ships.ElementAt(safePlanetIds.Count + diff - i ).EntityInfo.Id);
            }


        }
        else
        {

            for(int i = 0; i < ships.Count; i++)
            {
                safePlanetIds.Sort(delegate (int a, int b)
                {
                    //var myplayer = map.Players.FirstOrDefault(aplayer => aplayer.Id == hlt.Item1);
                    var ship = myplayer.Ships.ElementAt(i);
                    var planetA = map.Planets.ElementAt(a);
                    var planetB = map.Planets.ElementAt(b);
                    var pointA = ship.GetClosestPointToEntity(planetA);
                    var pointB = ship.GetClosestPointToEntity(planetB);
                    double compA = ship.GetDistance(pointA);
                    double compB = ship.GetDistance(pointB);
                    if (compA > compB) return 1;
                    else if (compA == compB) return 0;
                    else return -1;

                });
                returnedDict[safePlanetIds.ElementAt(i)].Add(ships.ElementAt(i).EntityInfo.Id);
            }
        }
        return returnedDict;




    }
    public static int EarlyGameStrat(GameMap map, List<String> commands, int id, Dictionary<int, List<int>> safePlanetIds, int turnCount)
    {
        int shipsDone = 0;
        var myplayer = map.Players.FirstOrDefault(player => player.Id == id);
        // List<int> shipsMoved = new List<int>();
        // If the ship is already docked, skip the loop and start with the next ship
        // int i = 0;
        // foreach(var ship in myplayer.Ships)
        //   {
        //If the ship does not have any planet assigned, we'll choose a random planet
        //   if(!safePlanetIds.ContainsValue(i))
        //  {

        // }
        // }
        if (turnCount == 0)
        {
            double angle = 60;
            myplayer.Ships.OrderBy(x => x.Position.X);
            Ship ship = myplayer.Ships.ElementAt(2);
            commands.Add(ship.Move(Convert.ToInt32(Constants.MaxSpeed * 0.7), Convert.ToInt32(angle)));
            angle += 60;
            ship = myplayer.Ships.ElementAt(1);
            commands.Add(ship.Move(Convert.ToInt32(Constants.MaxSpeed * 0.7), Convert.ToInt32(angle)));
            angle += 60;
            ship = myplayer.Ships.ElementAt(0);
            commands.Add(ship.Move(Convert.ToInt32(Constants.MaxSpeed * 0.7), Convert.ToInt32(angle)));
            return 0;
        }
        else
        {
            foreach (var ShipsToPlanets in safePlanetIds)
            {

                var planet = map.Planets.ElementAt(ShipsToPlanets.Key);
                var ships = ShipsToPlanets.Value;
                foreach (int shipIndex in ships)
                {
                    Ship ship;
                    /* if(shipsMoved.Contains(shipIndex))
                     {
                         continue;
                     }*/
                    try
                    {
                        ship = myplayer.Ships.First(x => x.EntityInfo.Id == shipIndex);
                    }
                    catch (Exception e)
                    {
                        shipsDone++;
                        continue;
                    }



                    if (ship.DockingStatus != DockingStatus.undocked)
                    {
                        continue;

                    }

                    if (ship.CanDock(planet))
                    {
                        commands.Add(ship.Dock(planet));
                        // shipsMoved.Add(shipIndex);
                        shipsDone++;
                        continue;

                    }
                    else
                    {
                        var angle = ship.GetAngle(planet.Position);
                        angle *= Math.PI * 180;
                        var entityPoint = ship.GetClosestPointToEntity(planet);
                        // Since we have the point, lets try navigating to it. 
                        // Our pathfinding algorthm takes care of going around obstsancles for you.
                        string navigatecommand = "";


                        navigatecommand = ship.Navigate(entityPoint, map, Constants.MaxSpeed, true, 200);





                        // Lets check If we were able to find a route to the point
                        if (!string.IsNullOrEmpty(navigatecommand))
                        {
                            // shipsMoved.Add(shipIndex);// Looks like we found a way, let add this to our command queue
                            commands.Add(navigatecommand);

                        }



                        continue;
                    }


                }
            }
        }
        return shipsDone;
     }



        
    
    public static void LateGameStrat(GameMap map, List<String> commands, int id)
    {
        var myplayer = map.Players.FirstOrDefault(player => player.Id == id);

        foreach (var ship in myplayer.Ships)
        {
            // If the ship is already docked, skip the loop and start with the next ship
            if (ship.DockingStatus != DockingStatus.undocked)
            {
                continue;
            }
            map.Planets.Sort(delegate (Planet a, Planet b)
                {
                    var pointA = ship.GetClosestPointToEntity(a);
                    var pointB = ship.GetClosestPointToEntity(b);
                    double compA = ship.GetDistance(pointA);
                    double compB = ship.GetDistance(pointB);
                    if (compA > compB) return 1;
                    else if (compA == compB) return 0;
                    else return -1;

                });
            // Since the ship is not docked, lets checkout whast the planets are doing
            foreach (var planet in map.Planets)
            {

                if (ship.CanDock(planet) && planet.DockedShips.Count < planet.DockingSpots)
                {
                    // Sweet, you can dock. Lets add the dock command to your queue
                    commands.Add(ship.Dock(planet));
                    break;
                }
                // If the planet is owned, lets not bother attacking it or going near it.
                 if (planet.isOwned())
                 {
                     continue;
                 }

                 // If you are close enough to the planet you can dock and produce more ships.
                 // lets try that out now
                 else
                 {
                    // Not close enough to dock.
                    // So lets find the closest point in the planet relative to the current ships location
                    var angle = ship.GetAngle(planet.Position);
                    angle *= Math.PI * 180;
                var entityPoint = ship.GetClosestPointToEntity(planet);
                    // Since we have the point, lets try navigating to it. 
                    // Our pathfinding algorthm takes care of going around obstsancles for you.

                    var navigatecommand = ship.Navigate(entityPoint, map, Constants.MaxSpeed, true, 150);
                    // Lets check If we were able to find a route to the point
                    if (!string.IsNullOrEmpty(navigatecommand))
                    {
                        // Looks like we found a way, let add this to our command queue
                        commands.Add(navigatecommand);
                    }

                    break;
                }
            }
        }
    }
    public static void LateLateGameStrat(GameMap map, List<String> commands, int id)
    {
        Random random = new Random();
        var myplayer = map.Players.FirstOrDefault(player => player.Id == id);

        foreach (var ship in myplayer.Ships)
        {
            // If the ship is already docked, skip the loop and start with the next ship
            if (ship.DockingStatus != DockingStatus.undocked)
            {
                continue;
            }
            map.Planets.Sort(delegate (Planet a, Planet b)
            {
                var pointA = ship.GetClosestPointToEntity(a);
                var pointB = ship.GetClosestPointToEntity(b);
                double compA = ship.GetDistance(pointA);
                double compB = ship.GetDistance(pointB);
                if (compA > compB) return 1;
                else if (compA == compB) return 0;
                else return -1;

            });
            // Since the ship is not docked, lets checkout whast the planets are doing
            foreach (var planet in map.Planets)
            {
                if (ship.CanDock(planet) && planet.DockedShips.Count < planet.DockingSpots && (planet.EntityInfo.Owner == id || !planet.isOwned()))
                {
                    // Sweet, you can dock. Lets add the dock command to your queue
                    commands.Add(ship.Dock(planet));
                    break;
                }
                

                // If the planet is owned (by y, lets not bother attacking it or going near it.
             
                // If you are close enough to the planet you can dock and produce more ships.
                else if(planet.EntityInfo.Owner == id)
                {
                    continue;
                }
                else
                {

                    // Not close enough to dock.
                    // So lets find the closest point in the planet relative to the current ships location
                    var angle = ship.GetAngle(planet.Position);
                    angle *= Math.PI * 180;
                    Entity targetedShip = null;
                    bool attackMode = false;
                    if (planet.DockedShips.Count > 0)
                    {
                        int a = random.Next() % planet.DockedShips.Count;
                        targetedShip = map.Ships.Find(x => x.EntityInfo.Id == planet.DockedShips.ElementAt(a));
                        attackMode = true;
                    }
                    if(targetedShip == null)
                    {
                        targetedShip = planet;
                    }
                    Position entityPoint;
                        entityPoint = ship.GetClosestPointToEntity(targetedShip);
                    String navigatecommand;
                    if (attackMode)
                    {
                        if (map.GetObstaclesBetween(ship, entityPoint,id, false).Count == 0)
                        {
                            entityPoint = ship.GetClosestPointToEntity(targetedShip, 0);

                            navigatecommand = ship.Navigate(entityPoint, map, Constants.MaxSpeed, false, 150, false);
                        }
                        else
                            navigatecommand = ship.Navigate(entityPoint, map, Constants.MaxSpeed, true, 150, false);
                    }
                    else
                    {
                        navigatecommand = ship.Navigate(entityPoint, map, Constants.MaxSpeed, true, 150);
                        // Lets check If we were able to find a route to the point
        
                    }
                    // Since we have the point, lets try navigating to it. 
                    // Our pathfinding algorthm takes care of going around obstsancles for you.

                    // Lets check If we were able to find a route to the point
                    if (!string.IsNullOrEmpty(navigatecommand))
                    {
                        // Looks like we found a way, let add this to our command queue
                        commands.Add(navigatecommand);
                        break;
                    }
                }

            }
        }
    }

}