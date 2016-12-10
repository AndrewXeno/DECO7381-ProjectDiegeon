using UnityEngine;
using System.Collections;

public class Utils {

	// Convert 2D coordinates to 1D array index
	public static int coordsToIdx(int x, int y, int columns){
		return y * columns + x;
	}

	// Convert 1D array index to 2D coordinates 
	public static void idxToCoords(int idx, int columns, out int x, out int y){
		y = idx / columns;
		x = idx % columns;
	}
}
