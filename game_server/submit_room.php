<?php

    /*
      This script handles room submission POST requests and save the submitted room data to the database.
    */

    // save the room data in POST request to the database
    function submit_room() {
        include 'connect_database.php';

        if (!$_POST["data"]){
            die("Invalid request.");
        }
        $username = mysqli_real_escape_string($conn, $_POST["username"]);
        $difficulty = mysqli_real_escape_string($conn, $_POST["difficulty"]);
        $data = mysqli_real_escape_string($conn, $_POST["data"]);

        $query = "SELECT UserID FROM user WHERE Username = '$username'";
        $result = mysqli_query($conn, $query);
        $row = mysqli_fetch_array($result);
        $userID = $row["UserID"];

        $query = "INSERT INTO room(BuilderID, BaseDifficulty, Data) values ($userID, $difficulty, '{$data}')";
        $result = mysqli_query($conn, $query);
        if (!$result) {
            die(mysqli_error($conn));
        }

        $query = "SELECT LAST_INSERT_ID() AS NewRoomID;";
        $result = mysqli_query($conn, $query);
        if (!$result) {
            die(mysqli_error($conn));
        }
        $row = mysqli_fetch_array($result);
        echo "Submission succeeded. New room ID: " . $row["NewRoomID"];


        mysqli_close($conn);
    }


    submit_room();


?>