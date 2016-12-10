<?php

    /*
      This script randomly generates level data basing on the level number and room difficulties, and sent the data back to the client.
    */
   
    // Generate random level data based on the given level number (from $_POST)
    function load_level() {
        include 'connect_database.php';
        $MAX_LEVEL=10;

        $level = (int)$_POST["level"];
		$query = "SELECT COUNT(*) AS Total from room";
        $result = mysqli_query($conn, $query);
		$row = mysqli_fetch_array($result);

		$total=$row["Total"];
		$intervalStart = (int)($total * ($level-1) / $MAX_LEVEL);
		$intervalCount = max(3, (int)($total/$MAX_LEVEL)) ;

		if ($level>$MAX_LEVEL){
			$query = "SELECT Data from room ORDER BY BaseDifficulty";
		} else {
			$query = "SELECT Data from room ORDER BY BaseDifficulty LIMIT $intervalStart, $intervalCount";
		}
        $result = mysqli_query($conn, $query);
        if (!$result) {
            die(mysqli_error($conn));
        }
        mysqli_close($conn);
        $allRooms = array();
        $i=0;
        while ($row = mysqli_fetch_array($result)){
        	$allRooms[$i]=$row["Data"];
        	$i++;
        }

        $roomCount = $i;
        $roomNeeded = min($level +2, $roomCount);

        // randomly select rooms from retrieved rooms
        $roomIdxs = range(0, $roomCount-1);
        shuffle ($roomIdxs); 
        $selectedRooms= array();
        for ($i=0;$i<$roomNeeded;$i++){
        	$roomIdx = $roomIdxs[$i];
        	$selectedRooms[$i]=$allRooms[$roomIdx];
        }
        echo json_encode($selectedRooms);
    }
    load_level();
?>