<?php
date_default_timezone_set('Australia/Brisbane');

function validateLogin($username, $password) {
	require 'includes/connect_database.php';
	try {
		$stmt = $conn->prepare('SELECT * FROM user WHERE username = ? and EncryptedPassword = SHA2(CONCAT(?, salt), 512)');
		$stmt->bind_param('ss', $username, $password);
		$stmt->execute();
		$stmt->store_result();
		$numberofrows = $stmt->num_rows;
		if ($numberofrows <= 0) {
			return false;
		} else {
			return true;
		}
	} catch (Exception $e) {
		echo "Error: " . $e->getMessage();
	}
}

function hasPermission() {
	session_start();
	if (isset($_SESSION['isUser'])) {
		return true;
	} else {
		return false;
	}
}

// INSERT INTO user(Username, Salt, EncryptedPassword, ExploreScore, BuildScore) values ("6bit", "a334e7b6d105cea8af242a0c7f83e61105d6b3bf734f482c76e6c21d1b7cf7d6", SHA2(CONCAT("6bitTest", "a334e7b6d105cea8af242a0c7f83e61105d6b3bf734f482c76e6c21d1b7cf7d6"), 512), 0, 0);

?>
