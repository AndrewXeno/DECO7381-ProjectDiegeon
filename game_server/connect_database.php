<?php
    /*
      This script connects the game database with predefined database configuration, 
      and should be included in every script that interacts with the database.
      Note: Database configuration containing database link credential is defined in database_config.php and it is NOT stored in the repository. 
      This file should be included in the same folder when deploying to the server.
    */

    include 'database_config.php';
    $conn = new mysqli($servername, $dbusername, $dbpassword, $dbname);

    if ($conn->connect_error) {
        die("Connection failed: " . $conn->connect_error);
    }
?>