using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;
using Pathfinding.Serialization.JsonFx;

/**
 * The class used to represent a room
 **/
[Serializable]
public class RoomData {
	public int columns;			//the width of the room 
	public int rows;			//the height of the room 
	public int[] entrance;
	public int[] exit;
	public int[] floorTiles;	//the prefab indexes of the floor tiles
	public int[] wallTiles;		//the prefab indexes of the floor tiles
	public int[] upperObjects;
	public int[] lowerObjects;

    public RoomData(int columns, int rows, bool preview = false)
    {
        this.columns = columns;
        this.rows = rows;
        this.entrance = new int[] { columns / 2, 0 };
        this.exit = new int[] { columns / 2, rows - 1 };

        this.floorTiles = new int[columns * rows];
        this.wallTiles = new int[columns * rows];
        this.upperObjects = new int[columns * rows];
        this.lowerObjects = new int[columns * rows];

        if (preview == false)
        {
            for (int i = 0; i < columns * rows; i++)
            {
                this.floorTiles[i] = 1;
            }
        }
        else
        {
            for (int i = 0; i < columns * rows; i++)
            {
                this.floorTiles[i] = -1;
                this.upperObjects[i] = -1;
                this.lowerObjects[i] = -1;
                this.wallTiles[i] = -1;
            }
        }
    }

	public RoomData(string type)
	{
		this.columns = 9;
		this.rows = 9;
		this.entrance = new int[] { -1, -1 };
		this.exit = new int[] { -1, -1 };
		if (type == "LevelStart") {
			this.entrance = new int[] { columns / 2, rows / 2 };
			this.exit = new int[] { columns / 2, rows - 1 };
		} else if (type == "LevelEnd") {
			this.entrance = new int[] { columns / 2, 0 };
			this.exit = new int[] { columns / 2, rows - 1 };
		}
			
		this.floorTiles = new int[columns * rows];
		this.wallTiles = new int[columns * rows];
		this.upperObjects = new int[columns * rows];
		this.lowerObjects = new int[columns * rows];

		for (int i = 0; i < columns * rows; i++) {
			this.floorTiles[i] = 1;
		}
	}
		
    public RoomData(RoomData data)
    {
        this.rows = data.rows;
        this.columns = data.columns;
        this.entrance = new int[] { columns / 2, 0 };
        this.exit = new int[] { columns / 2, rows - 1 };
        this.floorTiles = (int[])data.floorTiles.Clone();
        this.wallTiles = (int[])data.wallTiles.Clone();
        this.upperObjects = (int[])data.upperObjects.Clone();
        this.lowerObjects = (int[])data.lowerObjects.Clone();
    }

    public RoomData (){
		this.columns = 0;
		this.rows = 0;
		this.entrance = new int[]{0,0};
		this.exit = new int[]{0,0};
		this.floorTiles = null;
		this.wallTiles = null;
	}
    public void add(RoomData data)
    {

        for (int i = 0; i < columns * rows; i++)
        {
            if (data.floorTiles[i] != -1)
            {
                this.floorTiles[i] = data.floorTiles[i];
				this.upperObjects[i] = 0;
				this.lowerObjects[i] = 0;
            }
            if (data.wallTiles[i] != -1)
            {
                this.wallTiles[i] = data.wallTiles[i];
            }
            if (data.upperObjects[i] != -1)
            {
                this.upperObjects[i] = data.upperObjects[i];
            }
            if (data.lowerObjects[i] != -1)
            {
                this.lowerObjects[i] = data.lowerObjects[i];
            }
        }
    }


    public void toString()
    {
        //for debug
        string floor = "floor: ";
        for (int i = 0; i < floorTiles.Length; i++)
        {
            floor += " " + floorTiles[i].ToString();
        }
        string wall = "wall: ";
        for (int i = 0; i < wallTiles.Length; i++)
        {
            wall += " " + wallTiles[i].ToString();
        }
        string upper = "upper: ";
        for (int i = 0; i < upperObjects.Length; i++)
        {
            upper += " " + upperObjects[i].ToString();
        }
        string lower = "lower: ";
        for (int i = 0; i < lowerObjects.Length; i++)
        {
            lower += " " + lowerObjects[i].ToString();
        }

        //Debug.Log(floor);
        //Debug.Log(wall);
        //Debug.Log(upper);
        Debug.Log(lower.Length);
        //Debug.Log("count: " + lowerObjects.Length);
    }
}



