using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Terraria.ID;


/*Things to do:
 * Add Undo
 * Fix frameX issue with platforms
 * 
 * Notes:
 * Max wire length removed in 1.2.3
*/


namespace PointTeleporter
{
        [ApiVersion(1, 16)]
        public class PointTeleporter : TerrariaPlugin
        {
            public PointTeleporter(Main game)
                : base(game)
            {
            }
            public override Version Version
            {
                get { return new Version("0.4.0"); }
            }
            public override string Name
            {
                get { return "Point Teleporter"; }
            }
            public override string Author
            {
                get { return "Panini"; }
            }
            public override string Description
            {
                get { return "Allows creation of a wired teleporter from Point A to point B"; }
            }

            public override void Initialize()
            {
                Commands.ChatCommands.Add(new Command("pointtp.admin", commandPoint, "tpwire"));
            }

            //Declare Public Variables
            public static string PointErrorMessage = "Syntax error, Command: /tpwire (1/2), /tpwire <color>, /tpwire clear";

            public void commandPoint(CommandArgs e)
            {
                //Check if no parameters
                if (e.Parameters.Count == 0)
                {
                    e.Player.SendErrorMessage(PointErrorMessage);
                    return;
                }
                else if (e.Parameters[0] != null)
                {
                    e.Parameters[0] = e.Parameters[0].ToLower();
                }


                //Declare Variables
                int point = 0;
                int wireCheck = 0;

                Tile Block0 = new Tile();
                Block0.type = TileID.Glass;
                Tile Block1 = new Tile();
                Block1.type = TileID.Glass;
                

                //Pass second parameter as string. If no parameter, leave null
                //string color = e.Parameters.Count > 1 ? e.Parameters[1] : null;

                //TPset 1/2 Command
                //Figure out what point player wants to set, either 1 or 2
                if (int.TryParse(e.Parameters[0], out point) && point == 1 || point == 2)
                {
                    //Grab point A and point B for use
                    e.Player.SendInfoMessage("Tap a block to set teleporter point {0}", point);
                    e.Player.AwaitingTempPoint = point;
                }

                //TPwire Color Command
                else if (e.Parameters[0].Equals("red") || e.Parameters[0].Equals("blue") || e.Parameters[0].Equals("green"))
                {
                    //Check if point data is available on both Point A and Point B. If not, warn
                    if (e.Player.TempPoints[0] == Point.Zero || e.Player.TempPoints[1] == Point.Zero)
                    {
                        e.Player.SendErrorMessage(PointErrorMessage);
                    }

                    else
                    {
                        //Put points into variables and change point to center of teleporter
                        int X0 = e.Player.TempPoints[0].X;
                        int X1 = e.Player.TempPoints[1].X;
                        int Y0 = e.Player.TempPoints[0].Y - 1;
                        int Y1 = e.Player.TempPoints[1].Y - 1;


                        //Credits to MarioE for this logic
                        //Find which point is left and right, set to variable
                        int minX = Math.Min(X0, X1);
                        int maxX = Math.Max(X0, X1);


                        //Set wire for X direction
                        if (e.Parameters[0].Equals("red"))
                        {
                            for (int i = minX; i <= maxX; i++)
                            {
                                Main.tile[i, Y0].wire(true);
                            }
                        }
                        else if (e.Parameters[0].Equals("blue"))
                        {
                            for (int i = minX; i <= maxX; i++)
                            {
                                Main.tile[i, Y0].wire2(true);
                            }
                        }
                        else if (e.Parameters[0].Equals("green"))
                        {
                            for (int i = minX; i <= maxX; i++)
                            {
                                Main.tile[i, Y0].wire3(true);
                            }
                        }


                        //Find which point is lower and higher, set to variable
                        int minY = Math.Min(Y0, Y1);
                        int maxY = Math.Max(Y0, Y1);

                        
                        //Set wire for Y direction
                        if (e.Parameters[0].Equals("red"))
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                Main.tile[X1, j].wire(true);
                                if (j == maxY)
                                {
                                    Main.tile[X0, Y0 - 1].wire(true);
                                    Main.tile[X1, Y1 - 1].wire(true);
                                }
                            }
                        }
                        else if (e.Parameters[0].Equals("blue"))
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                Main.tile[e.Player.TempPoints[1].X, j].wire2(true);
                                if (j == maxY)
                                {
                                    Main.tile[X0, Y0 - 1].wire2(true);
                                    Main.tile[X1, Y1 - 1].wire2(true);
                                }
                            }
                        }
                        else if (e.Parameters[0].Equals("green"))
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                Main.tile[e.Player.TempPoints[1].X, j].wire3(true);
                                if (j == maxY)
                                {
                                    Main.tile[X0, Y0 - 1].wire3(true);
                                    Main.tile[X1, Y1 - 1].wire3(true);
                                }
                            }
                        }

                        //Reuse variables, place point back on original spot
                        //--------------------------------------------------
                        Y0 = e.Player.TempPoints[0].Y; // from int Y0 = e.Player.TempPoints[0].Y - 1;
                        Y1 = e.Player.TempPoints[1].Y; // from int Y1 = e.Player.TempPoints[1].Y - 1;
                        //--------------------------------------------------


                        //Get block type and store it to place under teleporter later. Check if it isn't passable (i.e. a tree or sunflower)
                        //Tile0 - Storing -
                        if (!WorldGen.SolidTile(X0, Y0) && Main.tile[X0, Y0].type != TileID.Platforms || Main.tile[X0, Y0].type == TileID.Teleporter)
                        {
                            Block0.type = TileID.Glass;
                        }
                        else
                        {
                            Block0.type = Main.tile[X0, Y0].type;
                            Block0.color(Main.tile[X0, Y0].color());
                        }

                        //Tile1 - Storing -
                        if (!WorldGen.SolidTile(X1, Y1) && Main.tile[X1, Y1].type != TileID.Platforms || Main.tile[X1,Y1].type == TileID.Teleporter)
                        {
                            Block1.type = TileID.Glass;
                        }
                        else
                        {
                            Block1.type = Main.tile[X1, Y1].type;
                            Block0.color(Main.tile[X1, Y1].color());
                        }

                        //Check if tile is a platform
                        if (Block0.type == TileID.Platforms)
                        {
                            //Block0.frameX = Main.tile[X0, Y0].frameX;//------frameX Not functioning, need to debug
                            Block0.frameY = Main.tile[X0, Y0].frameY;
                            if (Block1.type == TileID.Platforms)
                            {
                                //Block1.frameX = Main.tile[X1, Y1].frameX;//------frameX Not functioning, need to debug
                                Block1.frameY = Main.tile[X1, Y1].frameY;
                            }
                        }


                        //Destroy three blocks and place stored blocks without other parameters (i.e. slopes, grass on top). 
                        //Three blocks = type of block found at point found above.
                        for (int i = -1; i <= 1; i++)
                        {
                            for (int j = 0; j <= 4; j++)
                            {
                                WorldGen.KillTile(X0 + i, Y0 - j, noItem: true);
                                WorldGen.KillTile(X1 + i, Y1 - j, noItem: true);
                            }
                            //Set block and paint color to tiles
                            Main.tile[X0 + i, Y0].type = Block0.type;
                            Main.tile[X1 + i, Y1].type = Block1.type;
                            Main.tile[X0 + i, Y0].color(Block0.color());
                            Main.tile[X1 + i, Y1].color(Block1.color());


                            //Check if tile is a platform
                            if (Block0.type == TileID.Platforms)
                            {
                                Main.tile[X0 + i, Y0].frameX = Block0.frameX;
                                Main.tile[X0 + i, Y0].frameY = Block0.frameY;
                                if (Block1.type == TileID.Platforms)
                                {
                                    Main.tile[X1 + i, Y1].frameX = Block1.frameX;
                                    Main.tile[X1 + i, Y1].frameY = Block1.frameY;
                                }
                            }

                            //"Place" blocks
                            Main.tile[X0 + i, Y0].active(true);
                            Main.tile[X1 + i, Y1].active(true);

                        }
                        

                        //Find points and place teleporter on point if three flat blocks are available
                        WorldGen.Place3x1(X0, Y0 - 1, TileID.Teleporter);
                        WorldGen.Place3x1(X1, Y1 - 1, TileID.Teleporter);

                        //Place switches on teleporters
                        WorldGen.PlaceTile(X0, Y0 - 2, TileID.Switches);
                        WorldGen.PlaceTile(X1, Y1 - 2, TileID.Switches);


                        //Reset points and show player amount of wire used.
                        wireCheck = Math.Abs(Y1 - Y0) + Math.Abs(X1 - X0) + 2;
                        e.Player.SendSuccessMessage("{0} teleporter setup and wired! Wire used: {1} +/- 2", e.Parameters[0].ToUpper(), wireCheck);
                        e.Player.TempPoints[0] = Point.Zero;
                        e.Player.TempPoints[1] = Point.Zero;


                        //Mark all sections to re-send tile data
                        Netplay.ResetSections();
                    }
                }
                
                    
                //Clear command
                else if (e.Parameters[0].Equals("clear"))
                {
                    if (e.Player.TempPoints[0] == Point.Zero || e.Player.TempPoints[1] == Point.Zero)
                    {
                        e.Player.SendWarningMessage("Points are not set, use /tpwire 1/2");
                    }
                    else
                    {
                        int minX = Math.Min(e.Player.TempPoints[0].X, e.Player.TempPoints[1].X);
                        int maxX = Math.Max(e.Player.TempPoints[0].X, e.Player.TempPoints[1].X);

                        int minY = Math.Min(e.Player.TempPoints[0].Y, e.Player.TempPoints[1].Y);
                        int maxY = Math.Max(e.Player.TempPoints[0].Y, e.Player.TempPoints[1].Y);

                        //Clear all wires
                        for (int h = minX; h <= maxX; h++)
                        {
                            for (int k = minY; k <= maxY; k++)
                            {
                                Main.tile[h, k].wire(false);
                                Main.tile[h, k].wire2(false);
                                Main.tile[h, k].wire3(false);
                            }
                        }

                        //Reset tile data
                        Netplay.ResetSections();

                        //Done =)
                        e.Player.SendSuccessMessage("Wire cleared.");
                    }
                }

                else
                {
                    e.Player.SendErrorMessage(PointErrorMessage);
                }
            }
        }
    }