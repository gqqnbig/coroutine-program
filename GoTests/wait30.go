package main

// Importing fmt and time 
import (
	"fmt"
	"time"
)

// Main function 
func main() {

	// Calling Sleep method
	time.Sleep(30 * time.Second)

	// Printed after sleep is over
	fmt.Println("Sleep Over.....")
}
